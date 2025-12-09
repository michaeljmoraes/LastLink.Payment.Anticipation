using LastLink.Payment.Anticipation.Domain.Entities;
using LastLink.Payment.Anticipation.Domain.Repositories;
using LastLink.Payment.Anticipation.Domain.Enums;
using LastLink.Payments.Anticipation.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;


namespace LastLink.Payments.Anticipation.Infrastructure.Repositories
{
    /// <summary>
    /// Implements persistence operations for AnticipationRequest using EF Core.
    ///
    /// This class serves as the Infrastructure-level adapter in the Clean Architecture
    /// structure, fulfilling the repository contract defined in the Domain layer.
    ///
    /// Responsibilities:
    /// - Execute queries with consistent filtering rules
    /// - Persist domain aggregates without leaking EF-specific behavior to the Domain
    /// - Enforce separation of concerns between domain invariants and data access
    /// </summary>
    public sealed class AnticipationRequestRepository : IAnticipationRequestRepository
    {
        private readonly AnticipationDbContext _context;

        public AnticipationRequestRepository(AnticipationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Persists a new anticipation request to the database.
        /// Domain invariants must already have been validated before insertion.
        /// </summary>
        public async Task AddAsync(AnticipationRequest request, CancellationToken cancellationToken = default)
        {
            await _context.AnticipationRequests.AddAsync(request, cancellationToken);
        }

        /// <summary>
        /// Checks if the specified creator has an existing pending request.
        /// This enforces the rule: "Only one pending request per creator".
        /// </summary>
        public async Task<bool> HasPendingRequestAsync(Guid creatorId, CancellationToken cancellationToken = default)
        {
            return await _context.AnticipationRequests
                .AnyAsync(x => x.CreatorId == creatorId &&
                               x.Status == AnticipationStatus.Pending,
                               cancellationToken);
        }

        /// <summary>
        /// Retrieves a single anticipation request by ID.
        /// Returns null if the request does not exist.
        /// </summary>
        public async Task<AnticipationRequest?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.AnticipationRequests
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        /// <summary>
        /// Retrieves all anticipation requests belonging to the specified creator.
        /// The results are ordered by request date (newest first) for convenience in UI/API.
        /// </summary>
        public async Task<IReadOnlyList<AnticipationRequest>> GetByCreatorAsync(Guid creatorId, CancellationToken cancellationToken = default)
        {
            return await _context.AnticipationRequests
                .Where(x => x.CreatorId == creatorId)
                .OrderByDescending(x => x.RequestedAt)
                .ToListAsync(cancellationToken);
        }

        /// <summary>
        /// Commits pending changes to the database.
        /// Fulfills the unit-of-work pattern exposed at the repository boundary.
        /// </summary>
        public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            await _context.SaveChangesAsync(cancellationToken);
        }

        /// <summary>
        /// Deletes all anticipation requests belonging to the specified creator.
        /// 
        /// This method is intended exclusively for automated tests and Postman workflows.
        /// It must NOT be exposed in production APIs or business processes.
        /// </summary>
        public async Task CleanupAsync(Guid creatorId, CancellationToken cancellationToken = default)
        {
            var items = await _context.AnticipationRequests
                .Where(x => x.CreatorId == creatorId)
                .ToListAsync(cancellationToken);

            if (items.Count == 0)
                return;

            _context.AnticipationRequests.RemoveRange(items);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
