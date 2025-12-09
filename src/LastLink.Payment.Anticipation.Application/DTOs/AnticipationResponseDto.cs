using LastLink.Payment.Anticipation.Domain.Enums;

namespace LastLink.Payment.Anticipation.Application.DTOs
{
    /// <summary>
    /// Represents the data returned to clients consuming anticipation operations.
    /// This DTO acts as an output boundary for application-level workflows,
    /// exposing only the information relevant to API consumers while keeping
    /// domain internals encapsulated.
    ///
    /// It reflects the lifecycle of an anticipation request and the calculated
    /// financial values derived from the domain logic.
    /// </summary>
    public sealed class AnticipationResponseDto
    {
        /// <summary>
        /// Unique identifier of the anticipation request.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Identifier of the creator who initiated the anticipation.
        /// </summary>
        public Guid CreatorId { get; set; }

        /// <summary>
        /// Total gross amount requested for anticipation before applying fees.
        /// </summary>
        public decimal GrossAmount { get; set; }

        /// <summary>
        /// Net amount that the creator will receive after fee deductions.
        /// This value is computed by the domain and returned as-is.
        /// </summary>
        public decimal NetAmount { get; set; }

        /// <summary>
        /// Fee percentage applied to the anticipation (e.g., 0.05 for 5%).
        /// Included for transparency and auditability.
        /// </summary>
        public decimal FeeRate { get; set; }

        /// <summary>
        /// Timestamp indicating when the anticipation request was created.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Current lifecycle status of the anticipation request.
        /// </summary>
        public AnticipationStatus Status { get; set; }

        /// <summary>
        /// Timestamp representing when the anticipation request was approved or rejected.
        /// Null while the request remains pending.
        /// </summary>
        public DateTime? DecisionAt { get; set; }
    }
}
