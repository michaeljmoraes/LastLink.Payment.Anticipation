using System;

namespace LastLink.Payment.Anticipation.Domain.Enums
{
    /// <summary>
    /// Defines the lifecycle states of an anticipation request within the LastLink Payment domain.
    ///
    /// The status reflects the operational flow of an anticipation request:
    /// - <see cref="Pending"/>: The request has been created and awaits a business decision.
    /// - <see cref="Approved"/>: The request has been validated and the payout is authorized.
    /// - <see cref="Rejected"/>: The request was evaluated and denied, preventing payout.
    ///
    /// Enforcing explicit lifecycle states ensures domain consistency and prevents
    /// invalid transitions at the application and persistence layers.
    /// </summary>
    public enum AnticipationStatus
    {
        /// <summary>
        /// The request is awaiting approval or rejection.
        /// No financial action is permitted in this state.
        /// </summary>
        Pending = 0,

        /// <summary>
        /// The request has been approved and is eligible for payout.
        /// This represents a terminal state in the anticipation lifecycle.
        /// </summary>
        Approved = 1,

        /// <summary>
        /// The request was rejected during validation or manual review.
        /// This is also a terminal state and prevents further updates.
        /// </summary>
        Rejected = 2
    }
}
