using NUnit.Framework;
using System;

using LastLink.Payment.Anticipation.Domain.Entities;
using LastLink.Payment.Anticipation.Domain.Enums;
using LastLink.Payment.Anticipation.Domain.Exceptions;

namespace LastLink.Payment.Anticipation.Tests.Domain
{
    /// <summary>
    /// Domain Test Suite — AnticipationRequest Aggregate
    ///
    /// This suite validates:
    /// - Aggregate invariants enforced at creation time
    /// - Permitted and illegal state transitions
    /// - Financial calculations (fee + net value)
    /// - Protection against invalid operations (DDD defensive modeling)
    ///
    /// Mapping:
    ///   D1.x → Initialization / Invariants
    ///   D2.x → State transitions (Approve / Reject)
    ///   D3.x → Fee calculation rules
    ///
    /// Ensures traceability between:
    ///   Business Rule → Domain Invariant → Domain Test → Application Service Test → API Scenario
    /// </summary>
    [TestFixture]
    public class AnticipationRequestTests
    {
        private readonly Guid ValidCreatorId = Guid.NewGuid();

        // ============================================================================
        // Domain Scenario D1.1 — Creator validation
        // ============================================================================

        /// <summary>
        /// D1.1 — Creator identity cannot be empty.
        /// GIVEN an empty creator id
        /// WHEN constructing an anticipation request
        /// THEN the domain must reject the operation.
        /// </summary>
        [Test, Category("DomainRules")]
        public void Create_ShouldThrow_WhenCreatorIdIsEmpty()
        {
            Assert.Throws<DomainException>(() =>
                AnticipationRequest.Create(Guid.Empty, 150)
            );
        }

        // ============================================================================
        // Domain Scenario D1.2 — Minimum amount rule
        // ============================================================================

        /// <summary>
        /// D1.2 — Minimum amount must be ≥ 100.
        /// GIVEN a requested amount below 100
        /// WHEN creating the entity
        /// THEN a DomainException must be thrown.
        /// </summary>
        [Test, Category("DomainRules")]
        public void Create_ShouldThrow_WhenAmountBelowMinimum()
        {
            Assert.Throws<DomainException>(() =>
                AnticipationRequest.Create(ValidCreatorId, 50)
            );
        }

        // ============================================================================
        // Domain Scenario D1.3 — Initialization defaults
        // ============================================================================

        /// <summary>
        /// D1.3 — Ensure default initialization values.
        /// GIVEN valid parameters
        /// WHEN Create() initializes the aggregate
        /// THEN defaults must be applied consistently.
        /// </summary>
        [Test, Category("DomainRules")]
        public void Create_ShouldInitialize_WithCorrectDefaultValues()
        {
            var entity = AnticipationRequest.Create(ValidCreatorId, 200);

            Assert.Multiple(() =>
            {
                Assert.That(entity.Status, Is.EqualTo(AnticipationStatus.Pending));
                Assert.That(entity.FeePercentage, Is.EqualTo(0.05m));
                Assert.That(entity.NetAmount, Is.EqualTo(200 - (200 * 0.05m)));
                Assert.That(entity.DecisionAt, Is.Null);
                Assert.That(entity.CreatorId, Is.EqualTo(ValidCreatorId));
            });
        }

        // ============================================================================
        // Domain Scenario D2.1 — Approval transition
        // ============================================================================

        /// <summary>
        /// D2.1 — Approving is allowed only from Pending.
        /// GIVEN a pending anticipation
        /// WHEN Approve() is invoked
        /// THEN status must become Approved and timestamp assigned.
        /// </summary>
        [Test, Category("StateTransitions")]
        public void Approve_ShouldSetStatusAndDecisionTimestamp()
        {
            var entity = AnticipationRequest.Create(ValidCreatorId, 200);

            entity.Approve();

            Assert.Multiple(() =>
            {
                Assert.That(entity.Status, Is.EqualTo(AnticipationStatus.Approved));
                Assert.That(entity.DecisionAt, Is.Not.Null);
            });
        }

        // ============================================================================
        // Domain Scenario D2.2 — Illegal approval
        // ============================================================================

        /// <summary>
        /// D2.2 — Approval after leaving Pending must fail.
        /// GIVEN the request has already transitioned
        /// WHEN Approve() is invoked again
        /// THEN a DomainException must be thrown.
        /// </summary>
        [Test, Category("StateTransitions")]
        public void Approve_ShouldThrow_WhenStatusNotPending()
        {
            var entity = AnticipationRequest.Create(ValidCreatorId, 200);
            entity.Approve();

            Assert.Throws<DomainException>(() => entity.Approve());
        }

        // ============================================================================
        // Domain Scenario D2.3 — Reject transition
        // ============================================================================

        /// <summary>
        /// D2.3 — Rejecting is allowed only from Pending.
        /// GIVEN a pending anticipation
        /// WHEN Reject() is invoked
        /// THEN status must become Rejected and timestamp assigned.
        /// </summary>
        [Test, Category("StateTransitions")]
        public void Reject_ShouldSetStatusAndDecisionTimestamp()
        {
            var entity = AnticipationRequest.Create(ValidCreatorId, 200);

            entity.Reject();

            Assert.Multiple(() =>
            {
                Assert.That(entity.Status, Is.EqualTo(AnticipationStatus.Rejected));
                Assert.That(entity.DecisionAt, Is.Not.Null);
            });
        }

        // ============================================================================
        // Domain Scenario D2.4 — Illegal rejection
        // ============================================================================

        /// <summary>
        /// D2.4 — Rejecting after leaving Pending must fail.
        /// GIVEN an already-decided anticipation
        /// WHEN Reject() is invoked
        /// THEN the domain must block the transition.
        /// </summary>
        [Test, Category("StateTransitions")]
        public void Reject_ShouldThrow_WhenStatusNotPending()
        {
            var entity = AnticipationRequest.Create(ValidCreatorId, 200);
            entity.Approve();

            Assert.Throws<DomainException>(() => entity.Reject());
        }

        // ============================================================================
        // Domain Scenario D3.1 — Fee recalculation
        // ============================================================================

        /// <summary>
        /// D3.1 — Recalculate financials using a new fee rule.
        /// GIVEN an anticipation entity
        /// WHEN RecalculateNetAmount() applies a new fee
        /// THEN FeePercentage and NetAmount must be updated.
        /// </summary>
        [Test, Category("FinancialCalculations")]
        public void RecalculateNetAmount_ShouldRecomputeValues_WhenValidFee()
        {
            var entity = AnticipationRequest.Create(ValidCreatorId, 200);

            entity.RecalculateNetAmount(0.10m);

            Assert.Multiple(() =>
            {
                Assert.That(entity.FeePercentage, Is.EqualTo(0.10m));
                Assert.That(entity.NetAmount, Is.EqualTo(200 - (200 * 0.10m)));
            });
        }

        // ============================================================================
        // Domain Scenario D3.2 — Invalid fee rule
        // ============================================================================

        /// <summary>
        /// D3.2 — Fee must be between 0 and 1.
        /// GIVEN an invalid fee value
        /// WHEN RecalculateNetAmount() executes
        /// THEN DomainException must be thrown.
        /// </summary>
        [Test, Category("FinancialCalculations")]
        public void RecalculateNetAmount_ShouldThrow_WhenFeeIsInvalid()
        {
            var entity = AnticipationRequest.Create(ValidCreatorId, 200);

            Assert.Throws<DomainException>(() => entity.RecalculateNetAmount(-1));
            Assert.Throws<DomainException>(() => entity.RecalculateNetAmount(2));
        }
    }
}
