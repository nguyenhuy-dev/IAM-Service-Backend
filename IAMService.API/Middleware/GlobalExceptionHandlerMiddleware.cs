using FluentValidation;
using IAMService.Application.Exceptions;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace IAMService.API.Middleware
{
    /// <summary>
    ///     The global exception handler middleware class
    /// </summary>
    public class GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger)
    {

        /// <summary>
        ///     Invokes the context
        /// </summary>
        /// <param name="context">The context</param>
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        /// <summary>
        ///     Handles the exception using the specified context
        /// </summary>
        /// <param name="context">The context</param>
        /// <param name="exception">The exception</param>
        private async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";
            var response = new ErrorResponse();

            switch (exception)
            {
                case ValidationException validationException:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = "Validation failed";
                    response.Errors = validationException.Errors
                        .Select(e => new ErrorDetail
                        {
                            Field = e.PropertyName,
                            Message = e.ErrorMessage
                        })
                        .ToList();

                    logger.LogWarning(
                        validationException,
                        "Validation error: {ValidationErrors}",
                        string.Join(", ", response.Errors.Select(e => $"{e.Field}: {e.Message}"))
                    );
                    break;

                case NotFoundException notFoundException:
                    context.Response.StatusCode = (int)HttpStatusCode.NotFound;
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    response.Message = notFoundException.Message;

                    logger.LogWarning(notFoundException, "Resource not found");
                    break;

                case BusinessRuleViolationException businessException:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = businessException.Message;

                    logger.LogWarning(businessException, "Business rule violation");
                    break;

                case UnauthorizedAccessException unauthorizedException:
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    response.Message = "Unauthorized access";

                    logger.LogWarning(unauthorizedException, "Unauthorized access attempt");
                    break;
                case ForbiddenAccessException forbiddenException:
                    context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    response.StatusCode = (int)HttpStatusCode.Forbidden;
                    response.Message = "User does not have permission to access this resource.";

                    logger.LogWarning(forbiddenException, "Forbidden access attempt");
                    break;
                case InvalidOperationException invalidOp:
                    context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    response.Message = invalidOp.Message;
                    break;

                default:
                    context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    response.Message = "An error occurred while processing your request";

                    logger.LogError(
                        exception,
                        "Unhandled exception: {ExceptionType} - {Message}",
                        exception.GetType().Name,
                        exception.Message
                    );
                    break;
            }

            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            await context.Response.WriteAsync(jsonResponse);
        }
    }

    /// <summary>
    ///     Standard error response model
    /// </summary>
    public class ErrorResponse
    {
        /// <summary>
        ///     Gets or sets the value of the status code
        /// </summary>
        public int StatusCode { get; set; }
        /// <summary>
        ///     Gets or sets the value of the message
        /// </summary>
        public string Message { get; set; } = string.Empty;
        /// <summary>
        ///     Gets or sets the value of the errors
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public List<ErrorDetail>? Errors { get; set; }
    }

    /// <summary>
    ///     Detailed error information for validation failures
    /// </summary>
    public class ErrorDetail
    {
        /// <summary>
        ///     Gets or sets the value of the field
        /// </summary>
        public string Field { get; set; } = string.Empty;
        /// <summary>
        ///     Gets or sets the value of the message
        /// </summary>
        public string Message { get; set; } = string.Empty;
    }

}
