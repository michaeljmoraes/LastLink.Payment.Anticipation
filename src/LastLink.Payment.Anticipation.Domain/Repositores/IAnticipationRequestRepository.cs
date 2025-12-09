using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LastLink.Payment.Anticipation.Domain.Entities;

namespace LastLink.Payment.Anticipation.Domain.Repositories
{
    /// <summary>
    /// Defines the persistence contract for managing anticipation requests.
    ///
    /// As part of the Domain Layer, this interface abstracts away any storage
    /// mechanism (SQL, NoSQL, in-memory, or distributed cache), ensuring
    /// that the domain model remains persistence-agnostic.
    ///
    /// Application services rely on this contract to enforce business rules such as:
    /// - Ensuring a creator cannot have more than one pending request.
    /// - Retrieving historical requests for listing and auditing.
    /// - Persisting approval or rejection decisions.
    ///
    /// Implementations must guarantee transactional integrity and respect
    /// the invariants upheld by the <see cref="AnticipationRequest"/> aggregate.
    /// </summary>
    public interface IAnticipationRequestRepository
    {
        /// <summary>
        /// Checks whether the specified creator currently has a pending anticipation request.
        /// Used to enforce the business rule: "A creator cannot have more than one pending request."
        /// </summary>
        Task<bool> HasPendingRequestAsync(Guid creatorId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Persists a new anticipation request into the underlying storage.
        /// </summary>
        Task AddAsync(AnticipationRequest request, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a specific anticipation request by its unique identifier.
        /// Returns null if the request does not exist.
        /// </summary>
        Task<AnticipationRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves all anticipation requests associated with a given creator.
        /// Useful for listing, dashboards, and operational visibility.
        /// </summary>
        Task<IReadOnlyList<AnticipationRequest>> GetByCreatorAsync(Guid creatorId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Commits any pending changes to the storage mechanism.
        /// Infrastructure implementations may treat this as:
        /// - EF Core SaveChanges
        /// - Unit of Work commit
        /// - Transactional boundary flush
        /// </summary>
        Task SaveChangesAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Removes all anticipation requests for the given creator.
        /// Intended exclusively for test automation scenarios such as
        /// Postman collections or integration test setup.
        ///
        /// This method must NOT be used in production workflows.
        /// </summary>
        Task CleanupAsync(Guid creatorId, CancellationToken cancellationToken = default);
    }
}
