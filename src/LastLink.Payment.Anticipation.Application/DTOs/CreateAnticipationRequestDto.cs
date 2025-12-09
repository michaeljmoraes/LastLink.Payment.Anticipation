using System;

namespace LastLink.Payment.Anticipation.Application.DTOs
{
    /// <summary>
    /// Represents the input data required to create a new anticipation request.
    ///
    /// This DTO acts as a boundary model for the application layer,
    /// receiving information from API or external clients and forwarding
    /// it to the use case responsible for creating anticipation requests.
    ///
    /// Domain rules such as minimum amount validation, fee calculation,
    /// and lifecycle initialization are enforced exclusively by the domain model.
    /// </summary>
    public sealed class CreateAnticipationRequestDto
    {
        /// <summary>
        /// Identifier of the creator requesting the anticipation.
        /// Must refer to a valid platform user.
        /// </summary>
        public Guid CreatorId { get; set; }

        /// <summary>
        /// Gross amount the creator wishes to anticipate.
        /// Domain validation will ensure it meets the minimum threshold.
        /// </summary>
        public decimal GrossAmount { get; set; }

        /// <summary>
        /// Optional timestamp representing when the request was initiated.
        /// If omitted, the application layer typically assigns DateTime.UtcNow.
        /// </summary>
        public DateTime CreatedAt { get; set; }
    }
}
