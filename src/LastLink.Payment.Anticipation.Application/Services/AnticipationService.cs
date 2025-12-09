using LastLink.Payment.Anticipation.Application.DTOs;
using LastLink.Payment.Anticipation.Application.Interfaces;
using LastLink.Payment.Anticipation.Domain.Entities;
using LastLink.Payment.Anticipation.Domain.Exceptions;
using LastLink.Payment.Anticipation.Domain.Repositories;

namespace LastLink.Payment.Anticipation.Application.Services
{
    /// <summary>
    /// Orchestrates the anticipation workflow at the application layer.
    /// 
    /// Responsibilities:
    /// - Enforce workflow consistency (ordering of validations)
    /// - Delegate business rule validation to the domain model
    /// - Interact with the repository abstraction for persistence
    /// - Return boundary DTOs to the API layer
    ///
    /// This service must not contain business rules; those belong
    /// exclusively to the Domain (AnticipationRequest aggregate).
    /// </summary>
    public sealed class AnticipationService : IAnticipationService
    {
        private readonly IAnticipationRequestRepository _repository;

        public AnticipationService(IAnticipationRequestRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// Creates a new anticipation request and persists it.
        /// 
        /// Workflow sequence:
        /// 1 — Validate minimum amount early for predictable error behavior
        /// 2 — Enforce "one pending request per creator" rule
        /// 3 — Create the domain aggregate enforcing all invariants
        /// </summary>
        public async Task<AnticipationResponseDto> CreateAsync(CreateAnticipationRequestDto dto)
        {
            // 1 — Minimum amount validation (high-priority rule)
            DomainException.ThrowIf(dto.GrossAmount < 100,
                "Requested amount must be at least ¤100.00.");

            // 2 — Business consistency: no more than one pending request per creator
            var hasPending = await _repository.HasPendingRequestAsync(dto.CreatorId);

            DomainException.ThrowIf(hasPending,
                "Creator already has a pending anticipation request.");

            // 3 — Domain aggregate creation (invariants enforced inside the Domain)
            var entity = AnticipationRequest.Create(
                dto.CreatorId,
                dto.GrossAmount,
                dto.CreatedAt
            );

            await _repository.AddAsync(entity);
            await _repository.SaveChangesAsync();

            return ToResponse(entity);
        }

        /// <summary>
        /// Retrieves all anticipation requests belonging to a given creator.
        /// Ensures Domain → Application → API mapping without leaking internals.
        /// </summary>
        public async Task<IReadOnlyList<AnticipationResponseDto>> GetByCreatorAsync(Guid creatorId)
        {
            var list = await _repository.GetByCreatorAsync(creatorId);
            return list.Select(ToResponse).ToList();
        }

        /// <summary>
        /// Updates the status of an anticipation request.
        /// Only pending requests may transition to Approved or Rejected.
        /// DomainExceptions are thrown if rules are violated.
        /// </summary>
        public async Task<AnticipationResponseDto> UpdateStatusAsync(Guid id, bool approve)
        {
            var entity = await _repository.GetByIdAsync(id)
                ?? throw new DomainException("Anticipation request not found.");

            if (approve)
                entity.Approve();
            else
                entity.Reject();

            await _repository.SaveChangesAsync();
            return ToResponse(entity);
        }

        /// <summary>
        /// Performs a fee and net amount calculation without persisting the request.
        /// This enables UI preview/estimation scenarios.
        /// 
        /// The simulation uses Guid.Empty intentionally, as simulations do not
        /// belong to a real creator and should not imply ownership.
        /// </summary>
        public Task<AnticipationResponseDto> SimulateAsync(decimal grossAmount)
        {
            var simulated = AnticipationRequest.Create(Guid.NewGuid(), grossAmount, DateTime.UtcNow);
            return Task.FromResult(ToResponse(simulated));
        }

        /// <summary>
        /// Maps a domain entity to a response DTO.
        /// Ensures API layers do not directly depend on the domain model.
        /// </summary>
        private static AnticipationResponseDto ToResponse(AnticipationRequest entity)
        {
            return new AnticipationResponseDto
            {
                Id = entity.Id,
                CreatorId = entity.CreatorId,
                GrossAmount = entity.GrossAmount,
                NetAmount = entity.NetAmount,
                FeeRate = entity.FeePercentage,
                CreatedAt = entity.RequestedAt,
                Status = entity.Status,
                DecisionAt = entity.DecisionAt
            };
        }

        /// <summary>
        /// Clears anticipation requests for a given creator.
        /// Intended for test automation (Postman / Integration Tests).
        /// </summary>
        public async Task CleanupAsync(Guid creatorId)
        {
            await _repository.CleanupAsync(creatorId);
        }
    }
}
