using IAMService.API.Common;
using IAMService.API.Middleware;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Text.Json;
using IAMService.Application.Exceptions;

namespace IAMService.API.Test
{
    [TestFixture]
    public class GlobalExceptionHandlerMiddlewareTests
    {
        private GlobalExceptionHandlerMiddleware _middleware;
        private RequestDelegate _next;
        private DefaultHttpContext _httpContext;
        private ILogger<GlobalExceptionHandlerMiddleware> _logger;

        [SetUp]
        public void SetUp()
        {
            _next = Substitute.For<RequestDelegate>();
            _logger = Substitute.For<ILogger<GlobalExceptionHandlerMiddleware>>();
            _middleware = new GlobalExceptionHandlerMiddleware(_next, _logger);
            _httpContext = new DefaultHttpContext();
            _httpContext.Response.Body = new MemoryStream();
        }

        #region Success Path Tests

        [Test]
        public async Task InvokeAsync_ShouldCallNextDelegate_WhenNoExceptionIsThrown()
        {
            // Arrange
            _next.Invoke(_httpContext).Returns(Task.CompletedTask);

            // Act
            await _middleware.InvokeAsync(_httpContext);

            // Assert
            await _next.Received(1).Invoke(_httpContext);
            Assert.That(_httpContext.Response.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
        }

        #endregion

        #region KeyNotFoundException Tests

        [Test]
        public async Task InvokeAsync_ShouldReturn500_WhenKeyNotFoundExceptionIsThrown()
        {
            // Arrange
            var exceptionMessage = "User not found";
            _next.Invoke(_httpContext).Returns(Task.FromException(new KeyNotFoundException(exceptionMessage)));

            // Act
            await _middleware.InvokeAsync(_httpContext);

            // Assert
            Assert.That(_httpContext.Response.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
            Assert.That(_httpContext.Response.ContentType, Is.EqualTo("application/json"));

            var response = await GetResponseBody();
            Assert.That(response.Message, Does.Contain("error").IgnoreCase);
        }

        [Test]
        public async Task InvokeAsync_ShouldLogError_WhenKeyNotFoundExceptionIsThrown()
        {
            var exceptionMessage = "Resource not found";
            var exception = new KeyNotFoundException(exceptionMessage);
            _next.Invoke(_httpContext).Returns(Task.FromException(exception));

            await _middleware.InvokeAsync(_httpContext);

            _logger.Received(1).Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                exception,
                Arg.Any<Func<object, Exception?, string>>());
        }

        #endregion

        #region ValidationException Tests (FluentValidation)

        [Test]
        public async Task InvokeAsync_ShouldReturn400_WhenFluentValidationExceptionIsThrown()
        {
            // Arrange
            var failures = new List<FluentValidation.Results.ValidationFailure>
            {
                new FluentValidation.Results.ValidationFailure("Email", "Email is required"),
                new FluentValidation.Results.ValidationFailure("Email", "Email format is invalid"),
                new FluentValidation.Results.ValidationFailure("PhoneNumber", "Phone number must be 10 digits")
            };
            var validationException = new ValidationException(failures);
            _next.Invoke(_httpContext).Returns(Task.FromException(validationException));

            // Act
            await _middleware.InvokeAsync(_httpContext);

            // Assert
            Assert.That(_httpContext.Response.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
            Assert.That(_httpContext.Response.ContentType, Is.EqualTo("application/json"));

            var response = await GetResponseBody();
            Assert.That(response.Message, Does.Contain("validation").IgnoreCase);
            Assert.That(response.Errors, Is.Not.Null);
            Assert.That(response.Errors.Count, Is.GreaterThan(0));
        }

        [Test]
        public async Task InvokeAsync_ShouldIncludeAllValidationErrors_WhenFluentValidationExceptionIsThrown()
        {
            // Arrange
            var failures = new List<FluentValidation.Results.ValidationFailure>
            {
                new FluentValidation.Results.ValidationFailure("Email", "Email is required"),
                new FluentValidation.Results.ValidationFailure("Password", "Password must be at least 8 characters")
            };
            var validationException = new ValidationException(failures);
            _next.Invoke(_httpContext).Returns(Task.FromException(validationException));

            // Act
            await _middleware.InvokeAsync(_httpContext);

            // Assert
            var response = await GetResponseBody();
            var emailError = response.Errors.FirstOrDefault(e => e.Field == "Email");
            var passwordError = response.Errors.FirstOrDefault(e => e.Field == "Password");

            Assert.That(emailError, Is.Not.Null);
            Assert.That(passwordError, Is.Not.Null);
            Assert.That(emailError!.Message, Is.EqualTo("Email is required"));
            Assert.That(passwordError!.Message, Is.EqualTo("Password must be at least 8 characters"));
        }

        [Test]
        public async Task InvokeAsync_ShouldHandleValidationExceptionWithEmptyErrors()
        {
            // Arrange
            var failures = new List<FluentValidation.Results.ValidationFailure>();
            var validationException = new ValidationException(failures);
            _next.Invoke(_httpContext).Returns(Task.FromException(validationException));

            // Act
            await _middleware.InvokeAsync(_httpContext);

            // Assert
            Assert.That(_httpContext.Response.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));

            var response = await GetResponseBody();
            Assert.That(response.Errors, Is.Not.Null);
        }

        #endregion

        #region UnauthorizedAccessException Tests

        [Test]
        public async Task InvokeAsync_ShouldReturn401_WhenUnauthorizedAccessExceptionIsThrown()
        {
            var exceptionMessage = "Invalid credentials";
            _next.Invoke(_httpContext).Returns(Task.FromException(new UnauthorizedAccessException(exceptionMessage)));

            await _middleware.InvokeAsync(_httpContext);

            Assert.That(_httpContext.Response.StatusCode, Is.EqualTo(StatusCodes.Status401Unauthorized));
            var response = await GetResponseBody();
            Assert.That(response.Message, Is.EqualTo("Unauthorized access")); // fixed message
        }


        #endregion

        #region InvalidOperationException Tests

        [Test]
        public async Task InvokeAsync_ShouldReturn500_WhenInvalidOperationExceptionIsThrown()
        {
            var exceptionMessage = "Invalid request parameters";
            _next.Invoke(_httpContext).Returns(Task.FromException(new InvalidOperationException(exceptionMessage)));

            await _middleware.InvokeAsync(_httpContext);

            Assert.That(_httpContext.Response.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
        }

        #endregion

        #region ArgumentException Tests

        [Test]
        public async Task InvokeAsync_ShouldReturn500_WhenArgumentExceptionIsThrown()
        {
            var exceptionMessage = "Email already exists";
            _next.Invoke(_httpContext).Returns(Task.FromException(new ArgumentException(exceptionMessage)));

            await _middleware.InvokeAsync(_httpContext);

            Assert.That(_httpContext.Response.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
        }

        #endregion

        #region General Exception Tests

        [Test]
        public async Task InvokeAsync_ShouldReturn500_WhenUnhandledExceptionIsThrown()
        {
            // Arrange
            var exceptionMessage = "An unexpected error occurred";
            _next.Invoke(_httpContext).Returns(Task.FromException(new Exception(exceptionMessage)));

            // Act
            await _middleware.InvokeAsync(_httpContext);

            // Assert
            Assert.That(_httpContext.Response.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
            Assert.That(_httpContext.Response.ContentType, Is.EqualTo("application/json"));

            var response = await GetResponseBody();
            Assert.That(response.Message, Is.EqualTo("An error occurred while processing your request"));
        }

        [Test]
        public async Task InvokeAsync_ShouldLogError_WhenUnhandledExceptionIsThrown()
        {
            // Arrange
            var exception = new Exception("Critical error");
            _next.Invoke(_httpContext).Returns(Task.FromException(exception));

            // Act
            await _middleware.InvokeAsync(_httpContext);

            // Assert
            _logger.Received(1).Log(
                LogLevel.Error,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                exception,
                Arg.Any<Func<object, Exception?, string>>());
        }

        [Test]
        public async Task InvokeAsync_ShouldReturn500_WhenNullReferenceExceptionIsThrown()
        {
            // Arrange
            _next.Invoke(_httpContext).Returns(Task.FromException(new NullReferenceException()));

            // Act
            await _middleware.InvokeAsync(_httpContext);

            // Assert
            Assert.That(_httpContext.Response.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));

            var response = await GetResponseBody();
            Assert.That(response.Message, Is.EqualTo("An error occurred while processing your request"));
        }

        [Test]
        public async Task InvokeAsync_ShouldReturn500_WhenDivideByZeroExceptionIsThrown()
        {
            // Arrange
            var exception = new DivideByZeroException("Division by zero");
            _next.Invoke(_httpContext).Returns(Task.FromException(exception));

            // Act
            await _middleware.InvokeAsync(_httpContext);

            // Assert
            Assert.That(_httpContext.Response.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
        }

        #endregion

        #region Edge Cases

        [Test]
        public async Task InvokeAsync_ShouldHandleEmptyExceptionMessage()
        {
            _next.Invoke(_httpContext).Returns(Task.FromException(new KeyNotFoundException("")));

            await _middleware.InvokeAsync(_httpContext);

            Assert.That(_httpContext.Response.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
        }

        [Test]
        public async Task InvokeAsync_ShouldHandleNullExceptionMessage()
        {
            _next.Invoke(_httpContext).Returns(Task.FromException(new KeyNotFoundException()));

            await _middleware.InvokeAsync(_httpContext);

            Assert.That(_httpContext.Response.StatusCode, Is.EqualTo(StatusCodes.Status500InternalServerError));
        }

        [Test]
        public async Task InvokeAsync_ShouldSetCorrectContentType_ForAllExceptions()
        {
            // Arrange
            _next.Invoke(_httpContext).Returns(Task.FromException(new KeyNotFoundException("Test")));

            // Act
            await _middleware.InvokeAsync(_httpContext);

            // Assert
            Assert.That(_httpContext.Response.ContentType, Is.EqualTo("application/json"));
        }

        [Test]
        public async Task InvokeAsync_ShouldHandleMultipleValidationErrorsForSameField()
        {
            // Arrange
            var failures = new List<FluentValidation.Results.ValidationFailure>
            {
                new FluentValidation.Results.ValidationFailure("Email", "Email is required"),
                new FluentValidation.Results.ValidationFailure("Email", "Email format is invalid"),
                new FluentValidation.Results.ValidationFailure("Email", "Email must be unique")
            };
            var validationException = new ValidationException(failures);
            _next.Invoke(_httpContext).Returns(Task.FromException(validationException));

            // Act
            await _middleware.InvokeAsync(_httpContext);

            // Assert
            var response = await GetResponseBody();
            var emailErrors = response.Errors.Where(e => e.Field == "Email").ToList();

            Assert.That(emailErrors.Count, Is.EqualTo(3));
        }

        [Test]
        public async Task InvokeAsync_ShouldNotExposeInternalExceptionDetails_For500Errors()
        {
            // Arrange
            var exception = new Exception("Internal database connection string: server=prod;password=secret123");
            _next.Invoke(_httpContext).Returns(Task.FromException(exception));

            // Act
            await _middleware.InvokeAsync(_httpContext);

            // Assert
            var response = await GetResponseBody();
            Assert.That(response.Message, Does.Not.Contain("password"));
            Assert.That(response.Message, Does.Not.Contain("secret"));
        }

        #endregion

        #region Helper Methods  

        private async Task<ErrorResponse> GetResponseBody()
        {
            _httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(_httpContext.Response.Body);
            var responseBody = await reader.ReadToEndAsync();
            return JsonSerializer.Deserialize<ErrorResponse>(responseBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            })!;
        }

        #endregion
        #region NotFoundException Tests

        [Test]
        public async Task InvokeAsync_ShouldReturn404_WhenNotFoundExceptionIsThrown()
        {
            // Arrange
            var exceptionMessage = "User not found";
            var exception = new NotFoundException(exceptionMessage);
            _next.Invoke(_httpContext).Returns(Task.FromException(exception));

            // Act
            await _middleware.InvokeAsync(_httpContext);

            // Assert
            Assert.That(_httpContext.Response.StatusCode, Is.EqualTo(StatusCodes.Status404NotFound));
            var response = await GetResponseBody();
            Assert.That(response.Message, Is.EqualTo(exceptionMessage));
            _logger.Received(1).Log(
                LogLevel.Warning,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                exception,
                Arg.Any<Func<object, Exception?, string>>());
        }

        #endregion

        #region BusinessRuleViolationException Tests

        [Test]
        public async Task InvokeAsync_ShouldReturn400_WhenBusinessRuleViolationExceptionIsThrown()
        {
            // Arrange
            var exceptionMessage = "Business rule violated";
            var exception = new BusinessRuleViolationException(exceptionMessage);
            _next.Invoke(_httpContext).Returns(Task.FromException(exception));

            // Act
            await _middleware.InvokeAsync(_httpContext);

            // Assert
            Assert.That(_httpContext.Response.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));
            var response = await GetResponseBody();
            Assert.That(response.Message, Is.EqualTo(exceptionMessage));
            _logger.Received(1).Log(
                LogLevel.Warning,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                exception,
                Arg.Any<Func<object, Exception?, string>>());
        }

        #endregion

        #region ForbiddenAccessException Tests

        [Test]
        public async Task InvokeAsync_ShouldReturn403_WhenForbiddenAccessExceptionIsThrown()
        {
            // Arrange
            var exception = new ForbiddenAccessException("Access denied");
            _next.Invoke(_httpContext).Returns(Task.FromException(exception));

            // Act
            await _middleware.InvokeAsync(_httpContext);

            // Assert
            Assert.That(_httpContext.Response.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));
            var response = await GetResponseBody();
            Assert.That(response.Message, Is.EqualTo("User does not have permission to access this resource."));
            _logger.Received(1).Log(
                LogLevel.Warning,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                exception,
                Arg.Any<Func<object, Exception?, string>>());
        }

        #endregion

    }
}