using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LastLink.Payment.Anticipation.Application.DTOs;

namespace LastLink.Payment.Anticipation.Application.Interfaces
{
    /// <summary>
    /// Defines the application-layer contract for managing anticipation workflows.
    ///
    /// This interface represents the set of use cases exposed by the anticipation
    /// module. It orchestrates domain operations, enforces workflow rules, and
    /// returns DTOs tailored for API or external consumers.
    ///
    /// All business invariants (amount validation, lifecycle transitions, fee rules)
    /// are enforced by the domain model, while this service coordinates the flow
    /// between input DTOs, domain aggregates, and persistence abstractions.
    /// </summary>
    public interface IAnticipationService
    {
        /// <summary>
        /// Creates a new anticipation request based on client-provided data.
        /// Executes domain validation, calculates net values, and persists the request.
        /// </summary>
        Task<AnticipationResponseDto> CreateAsync(CreateAnticipationRequestDto dto);

        /// <summary>
        /// Retrieves all anticipation requests belonging to the specified creator.
        /// Used primarily for listing, dashboards, and user history.
        /// </summary>
        Task<IReadOnlyList<AnticipationResponseDto>> GetByCreatorAsync(Guid creatorId);

        /// <summary>
        /// Updates the status of an existing anticipation request.
        /// The boolean flag indicates whether the request should be approved
        /// (true) or rejected (false).
        ///
        /// The domain restricts transitions to Pending → Approved/Rejected only.
        /// </summary>
        Task<AnticipationResponseDto> UpdateStatusAsync(Guid id, bool approve);

        /// <summary>
        /// Performs a fee and net amount calculation without persisting a request.
        /// Used for preview/simulation scenarios before the user confirms submission.
        /// </summary>
        Task<AnticipationResponseDto> SimulateAsync(decimal grossAmount);

        /// <summary>
        /// Removes all anticipation requests for the specified creator.
        /// Intended exclusively for automated testing scenarios (e.g., Postman flows).
        /// </summary>
        Task CleanupAsync(Guid creatorId);
    }
}
