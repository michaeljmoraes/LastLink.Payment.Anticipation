using LastLink.Payment.Anticipation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace LastLink.Payments.Anticipation.Infrastructure.Configurations
{
    /// <summary>
    /// EF Core configuration for the AnticipationRequest aggregate.
    /// 
    /// This mapping ensures:
    /// - Proper table naming and column types for financial accuracy
    /// - Explicit precision for monetary fields
    /// - Enum storage as integers for lightweight persistence
    /// - Enforcement of required fields aligned with domain invariants
    /// 
    /// This configuration preserves domain integrity at the database level
    /// and prepares the model for future evolutions (e.g., multi-gateway storage).
    /// </summary>
    public sealed class AnticipationRequestConfiguration : IEntityTypeConfiguration<AnticipationRequest>
    {
        public void Configure(EntityTypeBuilder<AnticipationRequest> builder)
        {
            // Table name
            builder.ToTable("AnticipationRequests");

            // Primary key
            builder.HasKey(x => x.Id);

            // Required fields
            builder.Property(x => x.CreatorId)
                .IsRequired();

            // Monetary fields with explicit precision (important for financial correctness)
            builder.Property(x => x.GrossAmount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(x => x.NetAmount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            // Fee stored as decimal (0.05 = 5%)
            builder.Property(x => x.FeePercentage)
                .HasColumnType("decimal(5,4)") // up to 99.99% if necessary
                .IsRequired();

            // Enum stored as integer (recommended for performance + clarity)
            builder.Property(x => x.Status)
                .IsRequired();

            // Request timestamp
            builder.Property(x => x.RequestedAt)
                .IsRequired();

            // Optional field for decision timestamp
            builder.Property(x => x.DecisionAt)
                .IsRequired(false);
        }
    }
}
