using LastLink.Payment.Anticipation.Domain.Exceptions;
using LastLink.Payment.Anticipation.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace LastLink.Payment.Anticipation.Api.Filters
{
    /// <summary>
    /// Global exception filter responsible for translating thrown exceptions
    /// into a standardized API response format.
    ///
    /// This filter ensures that:
    /// • DomainException → HTTP 400 (Business rule violation)
    /// • Any other exception → HTTP 500 (Unexpected internal error)
    ///
    /// All responses follow the unified ApiResponse envelope to guarantee
    /// predictable behavior for client applications and automated tests.
    /// </summary>
    public class ApiExceptionFilter : IExceptionFilter
    {
        /// <summary>
        /// Captures exceptions thrown during request execution and transforms
        /// them into structured HTTP responses.
        ///
        /// The filter does not rethrow exceptions and fully handles the response,
        /// ensuring a consistent error-handling strategy across all controllers.
        /// </summary>
        /// <param name="context">The context containing exception and request details.</param>
        public void OnException(ExceptionContext context)
        {
            string message = context.Exception switch
            {
                DomainException ex => ex.Message,
                _ => "Internal server error."
            };

            var result = new ApiResponse<string>
            {
                Success = false,
                Error = message
            };

            context.Result = new ObjectResult(result)
            {
                StatusCode = context.Exception is DomainException ? 400 : 500
            };
        }
    }
}
