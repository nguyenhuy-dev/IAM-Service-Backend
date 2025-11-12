namespace IAMService.Application.Exceptions
{
    /// <summary>
    ///     Exception thrown when login credentials (email/password) are incorrect. (Maps to HTTP 401)
    /// </summary>
    public class InvalidCredentialsException : Exception
    {
        public InvalidCredentialsException(string message = "Invalid Email or Password.") : base(message) { }
    }
}
