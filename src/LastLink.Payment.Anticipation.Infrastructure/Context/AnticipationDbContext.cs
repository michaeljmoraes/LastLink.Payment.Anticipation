using LastLink.Payment.Anticipation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace LastLink.Payment.Anticipation.Infrastructure.Context
{
    /// <summary>
    /// Represents the EF Core database context for the anticipation module.
    ///
    /// Responsibilities:
    /// - Expose DbSets used by the infrastructure layer
    /// - Apply entity configurations defined in the assembly
    /// - Act as the persistence boundary for the application layer
    ///
    /// This DbContext is intentionally minimal, ensuring the domain remains
    /// persistence-agnostic while supporting both SQLite and InMemory providers
    /// based on environment or deployment requirements.
    /// </summary>
    public sealed class AnticipationDbContext : DbContext
    {
        /// <summary>
        /// Table representing persisted anticipation requests.
        /// Mapped via AnticipationRequestConfiguration.
        /// </summary>
        public DbSet<AnticipationRequest> AnticipationRequests { get; set; }

        public AnticipationDbContext(DbContextOptions<AnticipationDbContext> options)
            : base(options)
        {
        }

        /// <summary>
        /// Applies all IEntityTypeConfiguration classes found in the assembly.
        /// This ensures clean separation of concerns and allows the domain model
        /// to remain free of persistence annotations.
        ///
        /// Using ApplyConfigurationsFromAssembly promotes a scalable approach,
        /// enabling additional configurations (e.g., for gateways or audit logs)
        /// without modifying the DbContext class.
        /// </summary>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            base.OnModelCreating(modelBuilder);
        }
    }
}
