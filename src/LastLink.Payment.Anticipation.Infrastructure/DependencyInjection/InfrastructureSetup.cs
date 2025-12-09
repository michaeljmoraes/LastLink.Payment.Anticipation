using LastLink.Payment.Anticipation.Application.Interfaces;
using LastLink.Payment.Anticipation.Application.Services;
using LastLink.Payment.Anticipation.Domain.Repositories;
using LastLink.Payment.Anticipation.Infrastructure.Context;
using LastLink.Payment.Anticipation.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LastLink.Payment.Anticipation.Infrastructure
{
    /// <summary>
    /// Provides extension methods to configure and register infrastructure-level
    /// components such as the database context and repository implementations.
    ///
    /// This class isolates infrastructure wiring from the rest of the application,
    /// keeping dependency graph configuration consistent with Clean Architecture.
    /// </summary>
    public static class InfrastructureSetup
    {
        /// <summary>
        /// Registers database providers and concrete repository implementations.
        ///
        /// Notes:
        /// - Uses SQLite as the storage provider for demonstration and local execution.
        /// - Registers only infrastructure components; application services should be
        ///   registered in the application layer to maintain proper separation of concerns.
        /// </summary>
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
        {
            // Database registration (SQLite)
            services.AddDbContext<AnticipationDbContext>(options =>
                options.UseSqlite(connectionString));

            // Repository implementations
            services.AddScoped<IAnticipationRequestRepository, AnticipationRequestRepository>();
            services.AddScoped<IAnticipationService, AnticipationService>();

            return services;
        }
    }
}
