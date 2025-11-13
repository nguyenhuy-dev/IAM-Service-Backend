namespace IAMService.API.Common
{
    /// <summary>
    ///     The api response class
    /// </summary>
    public class ApiResponse<T>
    {
        /// <summary>
        ///     The HTTP status code.
        /// </summary>
        public int StatusCode { get; set; }

        /// <summary>
        ///     A developer-friendly message.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        ///     The actual data payload of the response.
        /// </summary>
        public T? Data { get; set; }

        /// <summary>
        ///     Creates a new success response.
        /// </summary>
        /// <param name="data">The data payload.</param>
        /// <param name="message">An optional success message.</param>
        /// <param name="statusCode">The HTTP status code, defaults to 200 (OK).</param>
        /// <returns>A new ApiResponse instance.</returns>
        public static ApiResponse<T> Success(T data, string message = "Request successful", int statusCode = 200)
        {
            return new ApiResponse<T>
            {
                StatusCode = statusCode,
                Message = message,
                Data = data
            };
        }
    }
}
