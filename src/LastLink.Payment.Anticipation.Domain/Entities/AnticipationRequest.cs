using System;
using LastLink.Payment.Anticipation.Domain.Enums;
using LastLink.Payment.Anticipation.Domain.Exceptions;

namespace LastLink.Payment.Anticipation.Domain.Entities
{
    /// <summary>
    /// Aggregate root representing a creator's anticipation request.
    /// 
    /// This entity encapsulates all domain rules related to the anticipation lifecycle:
    /// - Minimum amount validation
    /// - Automatic fee calculation (fixed 5% as per business rules)
    /// - Enforced state transitions (Pending → Approved/Rejected)
    ///
    /// As part of the core domain for the LastLink Payment Project,
    /// this aggregate ensures business integrity by preventing invalid
    /// states and centralizing all domain invariants under a single model.
    /// </summary>
    public sealed class AnticipationRequest
    {
        /// <summary>
        /// Minimum gross amount allowed for creating an anticipation request.
        /// This prevents low-value operations that are not economically viable
        /// for the platform or the creator.
        /// </summary>
        private const decimal MinimumAllowedAmount = 100m;

        /// <summary>
        /// Default fee percentage applied to all anticipation requests.
        /// The current challenge defines a fixed 5% rate, but future versions
        /// may require dynamic or gateway-driven fees.
        /// </summary>
        private const decimal DefaultFeePercentage = 0.05m;

        /// <summary>
        /// Unique identifier of the anticipation request (Aggregate Root key).
        /// </summary>
        public Guid Id { get; private set; }

        /// <summary>
        /// Unique identifier of the creator requesting the anticipation.
        /// This binds the operation to the creator's financial account.
        /// </summary>
        public Guid CreatorId { get; private set; }

        /// <summary>
        /// Total gross amount requested for anticipation before fees.
        /// </summary>
        public decimal GrossAmount { get; private set; }

        /// <summary>
        /// Fee percentage applied to the anticipation. Stored to support
        /// historical integrity and potential recalculations in future releases.
        /// </summary>
        public decimal FeePercentage { get; private set; }

        /// <summary>
        /// Net amount released to the creator after deducting fees.
        /// Calculated at creation and whenever fee rules change.
        /// </summary>
        public decimal NetAmount { get; private set; }

        /// <summary>
        /// Date/time (UTC) when the anticipation request was created.
        /// </summary>
        public DateTime RequestedAt { get; private set; }

        /// <summary>
        /// Current state of the anticipation request.
        /// Ensures the request follows the defined lifecycle transitions.
        /// </summary>
        public AnticipationStatus Status { get; private set; }

        /// <summary>
        /// Date/time (UTC) when the anticipation request was approved or rejected.
        /// Null if the request is still pending.
        /// </summary>
        public DateTime? DecisionAt { get; private set; }

        /// <summary>
        /// Required by EF Core / ORMs. Should never be used manually.
        /// </summary>
        private AnticipationRequest() { }

        /// <summary>
        /// Internal constructor that enforces domain invariants.
        /// </summary>
        private AnticipationRequest(Guid creatorId, decimal grossAmount, DateTime requestedAt)
        {
            ValidateRequestedAmount(grossAmount);

            Id = Guid.NewGuid();
            CreatorId = creatorId;
            GrossAmount = grossAmount;
            FeePercentage = DefaultFeePercentage;
            NetAmount = CalculateNetAmount(grossAmount, DefaultFeePercentage);
            RequestedAt = requestedAt;
            Status = AnticipationStatus.Pending;
            DecisionAt = null;
        }

        /// <summary>
        /// Factory method for creating a new anticipation request.
        /// Centralizes all business rules that must be executed during creation.
        /// </summary>
        public static AnticipationRequest Create(Guid creatorId, decimal requestedAmount, DateTime? requestedAt = null)
        {
            DomainException.ThrowIf(creatorId == Guid.Empty,
                "CreatorId must be a valid GUID.");

            var effectiveDate = requestedAt ?? DateTime.UtcNow;
            return new AnticipationRequest(creatorId, requestedAmount, effectiveDate);
        }

        /// <summary>
        /// Approves the anticipation request.
        /// Only requests in the Pending state may be approved.
        /// </summary>
        public void Approve()
        {
            DomainException.ThrowIf(Status != AnticipationStatus.Pending,
                "Only pending requests can be approved.");

            Status = AnticipationStatus.Approved;
            DecisionAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Rejects the anticipation request.
        /// Only requests in the Pending state may be rejected.
        /// </summary>
        public void Reject()
        {
            DomainException.ThrowIf(Status != AnticipationStatus.Pending,
                "Only pending requests can be rejected.");

            Status = AnticipationStatus.Rejected;
            DecisionAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Recalculates the net amount using a new fee percentage.
        /// This protects future extensibility where fee rules may vary
        /// based on gateways, risk scoring, contractual agreements, etc.
        /// </summary>
        public void RecalculateNetAmount(decimal newFeePercentage)
        {
            DomainException.ThrowIf(newFeePercentage < 0 || newFeePercentage > 1,
                "Fee percentage must be between 0 and 1 (0% - 100%).");

            FeePercentage = newFeePercentage;
            NetAmount = CalculateNetAmount(GrossAmount, newFeePercentage);
        }

        /// <summary>
        /// Ensures that the gross value respects the minimum amount required by business policy.
        /// </summary>
        private static void ValidateRequestedAmount(decimal amount)
        {
            DomainException.ThrowIf(amount < MinimumAllowedAmount,
                $"Requested amount must be at least {MinimumAllowedAmount:C}.");
        }

        /// <summary>
        /// Computes the final amount the creator will receive after deducting fees.
        /// </summary>
        private static decimal CalculateNetAmount(decimal amount, decimal feePercentage)
        {
            var fee = amount * feePercentage;
            return amount - fee;
        }
    }
}
