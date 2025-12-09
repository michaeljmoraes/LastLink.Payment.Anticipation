namespace LastLink.Payment.Anticipation.Api.Models
{
    /// <summary>
    /// Represents a standardized API response envelope used across the entire
    /// Anticipation API. Ensures consistent structure for both success and
    /// error flows, simplifying client-side handling and automated E2E testing.
    ///
    /// The response model enforces:
    /// • Predictable shape for all endpoints  
    /// • Separation of success data vs. error messaging  
    /// • Strong typing for the returned payload  
    ///
    /// This envelope is used by controllers and filters to unify the external API contract.
    /// </summary>
    /// <typeparam name="T">Type of the data returned in successful responses.</typeparam>
    public sealed class ApiResponse<T>
    {
        /// <summary>
        /// Indicates whether the request was processed successfully.
        /// </summary>
        public bool Success { get; init; }

        /// <summary>
        /// Response payload returned in case of a successful operation.
        /// When <see cref="Success"/> is false, this value will be null.
        /// </summary>
        public T? Data { get; init; }

        /// <summary>
        /// Error message returned when <see cref="Success"/> is false.
        /// Represents business rule violations or unexpected system errors.
        /// </summary>
        public string? Error { get; init; }

        /// <summary>
        /// Creates a successful API response wrapping the given payload.
        /// </summary>
        public static ApiResponse<T> Ok(T data)
            => new ApiResponse<T> { Success = true, Data = data };

        /// <summary>
        /// Creates a failed API response encapsulating the error description.
        /// </summary>
        public static ApiResponse<T> Fail(string error)
            => new ApiResponse<T> { Success = false, Error = error };
    }
}
