using LastLink.Payment.Anticipation.Application.DTOs;
using LastLink.Payment.Anticipation.Application.Services;
using LastLink.Payment.Anticipation.Domain.Entities;
using LastLink.Payment.Anticipation.Domain.Enums;
using LastLink.Payment.Anticipation.Domain.Exceptions;
using LastLink.Payment.Anticipation.Domain.Repositories;
using Moq;

namespace LastLink.Payments.Anticipation.Tests.Application
{
    /// <summary>
    /// Application Layer Unit Test Suite — AnticipationService
    ///
    /// This suite validates:
    /// - Workflow orchestration at the Application layer
    /// - Correct delegation to Domain invariants
    /// - Enforcement of business rules required by V1 MVP
    /// - Correct mapping Domain → DTO
    /// - No leakage of business rules outside the Domain model
    ///
    /// Scenarios:
    ///   1.x → Creation workflow
    ///   2.x → Pending constraint enforcement
    ///   3.x → Minimum amount validation
    ///   4.x → State transitions (Approve/Reject)
    ///   5.x → Simulation without persistence
    ///
    /// Ensures full traceability: Business Rule → Domain → Service → Test
    /// </summary>
    [TestFixture]
    public class AnticipationServiceTests
    {
        private Mock<IAnticipationRequestRepository> _repo;
        private AnticipationService _service;

        private readonly Guid CreatorId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6");

        [SetUp]
        public void Setup()
        {
            _repo = new Mock<IAnticipationRequestRepository>(MockBehavior.Strict);
            _service = new AnticipationService(_repo.Object);
        }

        // ============================================================================
        // Scenario 1.1 — Create a valid anticipation request
        // ============================================================================

        /// <summary>
        /// Scenario 1.1 — Valid creation flow.
        /// GIVEN a valid CreateAnticipationRequestDto
        /// AND no pending requests exist for the creator
        /// WHEN CreateAsync executes
        /// THEN the request must be persisted with correct financial calculations
        /// AND assigned Pending status.
        /// </summary>
        [Test, Category("ApplicationFlow")]
        public async Task CreateAsync_ShouldCreatePendingRequest_WhenValid()
        {
            var dto = new CreateAnticipationRequestDto
            {
                CreatorId = CreatorId,
                GrossAmount = 500,
                CreatedAt = DateTime.UtcNow
            };

            _repo.Setup(r => r.HasPendingRequestAsync(dto.CreatorId, default))
                 .ReturnsAsync(false);

            _repo.Setup(r => r.AddAsync(It.IsAny<AnticipationRequest>(), default))
                 .Returns(Task.CompletedTask);

            _repo.Setup(r => r.SaveChangesAsync(default))
                 .Returns(Task.CompletedTask);

            var result = await _service.CreateAsync(dto);

            Assert.Multiple(() =>
            {
                Assert.That(result.Status, Is.EqualTo(AnticipationStatus.Pending));
                Assert.That(result.FeeRate, Is.EqualTo(0.05m));
                Assert.That(result.NetAmount, Is.EqualTo(500 - (500 * 0.05m)));
                Assert.That(result.CreatorId, Is.EqualTo(CreatorId));
            });

            _repo.VerifyAll();
        }

        // ============================================================================
        // Scenario 1.2 — Listing requests
        // ============================================================================

        /// <summary>
        /// Scenario 1.2 — Listing.
        /// GIVEN anticipation requests belonging to a creator
        /// WHEN GetByCreatorAsync executes
        /// THEN they must be returned mapped to DTOs.
        /// </summary>
        [Test, Category("ApplicationFlow")]
        public async Task GetByCreatorAsync_ShouldReturnMappedDtos()
        {
            var list = new List<AnticipationRequest>
            {
                AnticipationRequest.Create(CreatorId, 300, DateTime.UtcNow)
            };

            _repo.Setup(r => r.GetByCreatorAsync(CreatorId, default))
                 .ReturnsAsync(list);

            var result = await _service.GetByCreatorAsync(CreatorId);

            Assert.That(result, Has.Count.EqualTo(1));
            Assert.That(result[0].GrossAmount, Is.EqualTo(300));
            Assert.That(result[0].CreatorId, Is.EqualTo(CreatorId));

            _repo.VerifyAll();
        }

        // ============================================================================
        // Scenario 1.3 — Approve request
        // ============================================================================

        /// <summary>
        /// Scenario 1.3 — Approve transition.
        /// GIVEN a pending anticipation
        /// WHEN UpdateStatusAsync executes with approve = true
        /// THEN the request must become Approved and DecisionAt must be populated.
        /// </summary>
        [Test, Category("StateTransitions")]
        public async Task UpdateStatusAsync_ShouldApprove_WhenApproveIsTrue()
        {
            var id = Guid.NewGuid();
            var entity = AnticipationRequest.Create(CreatorId, 400, DateTime.UtcNow);

            _repo.Setup(r => r.GetByIdAsync(id, default))
                 .ReturnsAsync(entity);

            _repo.Setup(r => r.SaveChangesAsync(default))
                 .Returns(Task.CompletedTask);

            var result = await _service.UpdateStatusAsync(id, approve: true);

            Assert.Multiple(() =>
            {
                Assert.That(result.Status, Is.EqualTo(AnticipationStatus.Approved));
                Assert.That(result.DecisionAt, Is.Not.Null);
            });

            _repo.VerifyAll();
        }

        // ============================================================================
        // Scenario 2.2 — Reject creation if pending exists
        // ============================================================================

        /// <summary>
        /// Scenario 2.2 — Pending request rule.
        /// GIVEN an existing pending anticipation
        /// WHEN CreateAsync executes
        /// THEN DomainException must be thrown.
        /// </summary>
        [Test, Category("BusinessOrchestration")]
        public void CreateAsync_ShouldThrow_WhenCreatorHasPending()
        {
            var dto = new CreateAnticipationRequestDto
            {
                CreatorId = CreatorId,
                GrossAmount = 300
            };

            _repo.Setup(r => r.HasPendingRequestAsync(CreatorId, default))
                 .ReturnsAsync(true);

            var ex = Assert.ThrowsAsync<DomainException>(() => _service.CreateAsync(dto));

            Assert.That(ex!.Message, Does.Contain("pending"));

            _repo.VerifyAll();
        }

        // ============================================================================
        // Scenario 3.1 — Minimum amount enforcement
        // ============================================================================

        /// <summary>
        /// Scenario 3.1 — GrossAmount < 100 must fail.
        /// GIVEN an invalid amount
        /// WHEN CreateAsync executes
        /// THEN DomainException must be thrown.
        /// </summary>
        [Test, Category("BusinessOrchestration")]
        public void CreateAsync_ShouldThrow_WhenAmountIsBelow100()
        {
            var dto = new CreateAnticipationRequestDto
            {
                CreatorId = CreatorId,
                GrossAmount = 50
            };

            var ex = Assert.ThrowsAsync<DomainException>(() => _service.CreateAsync(dto));

            Assert.That(ex!.Message, Does.Contain("100"));

            // No VerifyAll because CreateAsync stops early
        }

        // ============================================================================
        // Scenario 4.2 — Reject transition
        // ============================================================================

        /// <summary>
        /// Scenario 4.2 — Reject request.
        /// GIVEN a pending anticipation
        /// WHEN UpdateStatusAsync executes with approve = false
        /// THEN the request must become Rejected.
        /// </summary>
        [Test, Category("StateTransitions")]
        public async Task UpdateStatusAsync_ShouldReject_WhenApproveIsFalse()
        {
            var id = Guid.NewGuid();
            var entity = AnticipationRequest.Create(CreatorId, 300, DateTime.UtcNow);

            _repo.Setup(r => r.GetByIdAsync(id, default))
                 .ReturnsAsync(entity);

            _repo.Setup(r => r.SaveChangesAsync(default))
                 .Returns(Task.CompletedTask);

            var result = await _service.UpdateStatusAsync(id, approve: false);

            Assert.Multiple(() =>
            {
                Assert.That(result.Status, Is.EqualTo(AnticipationStatus.Rejected));
                Assert.That(result.DecisionAt, Is.Not.Null);
            });

            _repo.VerifyAll();
        }

        // ============================================================================
        // Scenario 5.1 — Simulation without persistence
        // ============================================================================

        /// <summary>
        /// Scenario 5.1 — Simulation flow.
        /// GIVEN a gross amount
        /// WHEN SimulateAsync executes
        /// THEN Domain rules must compute fee/net
        /// AND no repository method must be called.
        /// </summary>
        [Test, Category("FinancialSimulation")]
        public async Task SimulateAsync_ShouldReturnCorrectFinancials_AndNotTouchRepository()
        {
            var res = await _service.SimulateAsync(350);

            Assert.Multiple(() =>
            {
                Assert.That(res.Status, Is.EqualTo(AnticipationStatus.Pending));
                Assert.That(res.FeeRate, Is.EqualTo(0.05m));
                Assert.That(res.NetAmount, Is.EqualTo(350 - (350 * 0.05m)));
                Assert.That(res.Id, Is.Not.EqualTo(Guid.Empty));
            });

            _repo.VerifyNoOtherCalls();
        }

        // ============================================================================
        // Scenario — Cleanup
        // ============================================================================

        /// <summary>
        /// Cleanup Scenario — test automation support.
        /// GIVEN a creatorId
        /// WHEN CleanupAsync executes
        /// THEN repository cleanup must be invoked once.
        /// </summary>
        [Test, Category("ApplicationFlow")]
        public async Task CleanupAsync_ShouldCallRepository()
        {
            _repo.Setup(r => r.CleanupAsync(CreatorId, default))
                 .Returns(Task.CompletedTask);

            await _service.CleanupAsync(CreatorId);

            _repo.VerifyAll();
        }
    }
}
