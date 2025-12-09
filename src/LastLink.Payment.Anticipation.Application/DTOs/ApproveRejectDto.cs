using LastLink.Payment.Anticipation.Domain.Enums;

namespace LastLink.Payment.Anticipation.Application.DTOs
{
    /// <summary>
    /// Represents the input data required to approve or reject
    /// an existing anticipation request.
    ///
    /// This DTO acts as an application-layer command and is consumed
    /// by the use case responsible for transitioning a request from
    /// Pending to either Approved or Rejected.
    ///
    /// Only valid transitions are allowed by the domain model, and
    /// this DTO simply conveys the caller's intent.
    /// </summary>
    public sealed class ApproveRejectDto
    {
        /// <summary>
        /// The new status the anticipation request should transition to.
        /// Must be either Approved or Rejected. Pending is not allowed.
        /// </summary>
        public AnticipationStatus Status { get; set; }
    }
}
