using System;

namespace LastLink.Payment.Anticipation.Domain.Exceptions
{
    /// <summary>
    /// Represents a violation of a domain business rule within the anticipation context.
    ///
    /// This exception type is used exclusively at the Domain Layer to protect
    /// business invariants and ensure that invalid states cannot be created or persisted.
    ///
    /// By centralizing rule violations into a specific domain exception:
    /// - Domain logic becomes explicit and self-documenting.
    /// - Application services can uniformly handle business errors.
    /// - Infrastructure layers are prevented from leaking unrelated exceptions.
    ///
    /// This aligns with DDD principles, where the domain enforces correctness
    /// and rejects operations that conflict with business rules.
    /// </summary>
    public sealed class DomainException : Exception
    {
        /// <summary>
        /// Creates a new instance of <see cref="DomainException"/> with a specific message
        /// describing the business rule that was violated.
        /// </summary>
        public DomainException(string message) : base(message)
        {
        }

        /// <summary>
        /// Utility method that throws a <see cref="DomainException"/> if the given condition is met.
        ///
        /// This pattern simplifies invariant enforcement, making business rules
        /// easier to express directly within the domain model.
        ///
        /// Usage example:
        /// <code>
        /// DomainException.ThrowIf(amount < 100, "Amount must be at least 100.");
        /// </code>
        /// </summary>
        /// <param name="condition">The condition that determines whether the exception should be thrown.</param>
        /// <param name="message">The business rule description to include in the exception.</param>
        public static void ThrowIf(bool condition, string message)
        {
            if (condition)
            {
                throw new DomainException(message);
            }
        }
    }
}
