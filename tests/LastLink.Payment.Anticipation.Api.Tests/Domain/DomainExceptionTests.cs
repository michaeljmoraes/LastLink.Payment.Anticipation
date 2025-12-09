using NUnit.Framework;
using LastLink.Payment.Anticipation.Domain.Exceptions;

namespace LastLink.Payment.Anticipation.Tests.Domain
{
    /// <summary>
    /// Domain Test Suite — DomainException
    ///
    /// Purpose:
    /// - Validate the consistency of the domain-level exception mechanism.
    /// - Ensure error messages are propagated correctly.
    /// - Verify the behavior of the static helper ThrowIf, used extensively
    ///   across invariants and business rules in the AnticipationRequest aggregate.
    ///
    /// Test Scenarios:
    ///   DX.1 – Exception message exposure
    ///   DX.2 – ThrowIf throws when condition is true
    ///   DX.3 – ThrowIf does not throw when condition is false
    ///
    /// This suite ensures the foundational error-handling behavior of the domain
    /// layer is predictable, reliable, and aligned with DDD defensive modeling.
    /// </summary>
    [TestFixture]
    public class DomainExceptionTests
    {
        // ============================================================================
        // DX.1 – Exception message exposure
        // ============================================================================

        /// <summary>
        /// DX.1
        /// GIVEN a domain violation
        /// WHEN a DomainException is instantiated
        /// THEN the message provided must be exposed verbatim.
        /// </summary>
        [Test, Category("ErrorHandling")]
        public void DomainException_ShouldExposeMessage()
        {
            var ex = new DomainException("test message");

            Assert.That(ex.Message, Is.EqualTo("test message"));
        }

        // ============================================================================
        // DX.2 – ThrowIf should throw when condition is true
        // ============================================================================

        /// <summary>
        /// DX.2
        /// GIVEN a failing condition
        /// WHEN ThrowIf is executed with condition = true
        /// THEN a DomainException must be thrown to interrupt execution.
        /// </summary>
        [Test, Category("ErrorHandling")]
        public void ThrowIf_ShouldThrow_WhenConditionIsTrue()
        {
            Assert.Throws<DomainException>(() =>
                DomainException.ThrowIf(true, "condition failed")
            );
        }

        // ============================================================================
        // DX.3 – ThrowIf should NOT throw when condition is false
        // ============================================================================

        /// <summary>
        /// DX.3
        /// GIVEN a valid condition
        /// WHEN ThrowIf is executed with condition = false
        /// THEN no exception must be thrown and execution should proceed normally.
        /// </summary>
        [Test, Category("ErrorHandling")]
        public void ThrowIf_ShouldNotThrow_WhenConditionIsFalse()
        {
            Assert.DoesNotThrow(() =>
                DomainException.ThrowIf(false, "unused")
            );
        }
    }
}
