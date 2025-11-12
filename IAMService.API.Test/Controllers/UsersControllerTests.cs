using IAMService.API.Common;
using IAMService.API.Controllers;
using IAMService.API.Middleware;
using IAMService.Application.DTOs;
using IAMService.Application.Features.User.Commands.CreateUser;
using IAMService.Application.Features.User.Commands.DeleteUser;
using IAMService.Application.Features.User.Commands.UpdateUser;
using IAMService.Application.Features.User.Queries.ViewUserInformation;
using IAMService.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using NSubstitute;
using System.Security.Claims;
namespace IAMService.API.Test
{
    [TestFixture]
    public class UsersControllerTests
    {

        [SetUp]
        public void SetUp()
        {
            // Use NSubstitute to create a mock for ISender
            _sender = Substitute.For<ISender>();
            // Initialize the controller with the mock sender
            _controller = new UsersController(_sender);
            _token = CancellationToken.None;

            // Set up a default ControllerContext for actions that rely on HttpContext/User
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
        }
        private ISender _sender;
        private UsersController _controller;
        private CancellationToken _token;

        private void SetupControllerContext(Guid userId, string role)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()), // For robustness
                new Claim(ClaimTypes.Role, role),
                new Claim("role", role) // For robustness
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };
        }

        [Test]
        public async Task CreateUser_IsPatient_ShouldReturn201Created_WithPatientMessage()
        {
            // Arrange
            var command = new CreateUserCommand
            {
                Email = "patient@example.com",
                PhoneNumber = "0123456789",
                FullName = "Test Patient",
                IdentityNumber = "123456789012",
                Gender = "Female",
                Address = "123 Test St",
                DateOfBirth = "01/01/1990",
                Password = null,
                IsPatient = true,
                PrivilegeIds = null
            };
            var createdUser = new User
            {
                UserId = Guid.NewGuid(),
                Email = command.Email,
                FullName = command.FullName
            };
            var expectedDto = new UserDto
            {
                UserId = createdUser.UserId,
                Email = createdUser.Email,
                FullName = createdUser.FullName,
                PhoneNumber = command.PhoneNumber,
                Gender = command.Gender,
                IdentityNumber = command.IdentityNumber,
                Address = command.Address,
                Role = new RoleDto
                {
                    RoleId = 1,
                    RoleName = "Default Role",
                    RoleCode = "DEFAULT",
                    Description = "Default role description",
                    Privileges = new List<PrivilegeDto>()
                },
                DateOfBirth = DateOnly.ParseExact(command.DateOfBirth, "MM/dd/yyyy", null),
                Age = 33,
                IsPatient = false,
                NeedsVerification = true
            };

            _sender.Send(command, _token).Returns(expectedDto);

            // Act
            var result = await _controller.CreateUser(command, _token);

            // Assert
            var statusCodeResult = result as ObjectResult;
            Assert.That(statusCodeResult, Is.Not.Null);
            Assert.That(statusCodeResult.StatusCode, Is.EqualTo(StatusCodes.Status201Created));

            var apiResponse = statusCodeResult!.Value as ApiResponse<UserDto>;
            Assert.That(apiResponse, Is.Not.Null);
            Assert.That(apiResponse!.Data, Is.EqualTo(expectedDto));
            Assert.That(apiResponse.Message, Does.Contain("Patient account created successfully."));

            // Verify MediatR was called
            await _sender.Received(1).Send(command, _token);
        }

        [Test]
        public async Task CreateUser_IsEmployee_ShouldReturn201Created_WithEmployeeMessage()
        {
            // Arrange
            var command = new CreateUserCommand
            {
                Email = "test@example.com",
                PhoneNumber = "0123456789",
                FullName = "Test User",
                IdentityNumber = "123456789012",
                Gender = "Male",
                Address = "123 Test St",
                DateOfBirth = "01/01/1990",
                Password = "StrongP@ss123",
                IsPatient = false,
                PrivilegeIds = new[] { 1 }
            };
            var createdUser = new User
            {
                UserId = Guid.NewGuid(),
                Email = command.Email,
                FullName = command.FullName
            };
            var expectedDto = new UserDto
            {
                UserId = createdUser.UserId,
                Email = createdUser.Email,
                FullName = createdUser.FullName,
                PhoneNumber = command.PhoneNumber,
                Gender = command.Gender,
                IdentityNumber = command.IdentityNumber,
                Address = command.Address,
                Role = new RoleDto
                {
                    RoleId = 1,
                    RoleName = "Default Role",
                    RoleCode = "DEFAULT",
                    Description = "Default role description",
                    Privileges = new List<PrivilegeDto>()
                },
                DateOfBirth = DateOnly.ParseExact(command.DateOfBirth, "MM/dd/yyyy", null),
                Age = 33,
                IsPatient = false,
                NeedsVerification = true
            };

            _sender.Send(command, _token).Returns(expectedDto);

            // Act
            var result = await _controller.CreateUser(command, _token);

            // Assert
            var statusCodeResult = result as ObjectResult;
            Assert.That(statusCodeResult, Is.Not.Null);
            Assert.That(statusCodeResult.StatusCode, Is.EqualTo(StatusCodes.Status201Created));

            var apiResponse = statusCodeResult!.Value as ApiResponse<UserDto>;
            Assert.That(apiResponse, Is.Not.Null);
            Assert.That(apiResponse!.Data, Is.EqualTo(expectedDto));
            Assert.That(apiResponse.Message, Does.Contain("Employee account created successfully."));
        }

        [Test]
        public async Task UpdateUser_ShouldReturn400BadRequest_WhenDtoIsNull()
        {
            // Arrange
            var userId = Guid.NewGuid();

            // Act
            var result = await _controller.UpdateUser(userId, null!, _token);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult, Is.Not.Null);
            var errorResponse = badRequestResult!.Value as ErrorResponse;
            Assert.That(errorResponse!.Message, Is.EqualTo("Request body cannot be empty."));

            await _sender.DidNotReceiveWithAnyArgs().Send(Arg.Any<UpdateUserCommand>(), _token);
        }

        [Test]
        public async Task UpdateUser_ShouldReturn400BadRequest_WhenNoFieldsAreProvided()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var dto = new UpdateUserRequestDto(); // All properties are null/empty
            SetupControllerContext(userId, "User"); // Must be authenticated to reach this check

            // Act
            var result = await _controller.UpdateUser(userId, dto, _token);

            // Assert
            var badRequestResult = result as BadRequestObjectResult;
            Assert.That(badRequestResult, Is.Not.Null);
            var errorResponse = badRequestResult!.Value as ErrorResponse;
            Assert.That(errorResponse!.Message, Is.EqualTo("No fields were provided for update."));

            await _sender.DidNotReceiveWithAnyArgs().Send(Arg.Any<UpdateUserCommand>(), _token);
        }

        [Test]
        public async Task UpdateUser_ShouldReturn401Unauthorized_WhenUserIsNotAuthenticated()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var dto = new UpdateUserRequestDto { FullName = "New Name" };
            // Simulate unauthenticated user (default setup has a null or empty user principal)
            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity());

            // Act
            var result = await _controller.UpdateUser(userId, dto, _token);

            // Assert
            var unauthorizedResult = result as UnauthorizedObjectResult;
            Assert.That(unauthorizedResult, Is.Not.Null);
            var errorResponse = unauthorizedResult!.Value as ErrorResponse;
            Assert.That(errorResponse!.Message, Does.Contain("User is not authenticated"));

            await _sender.DidNotReceiveWithAnyArgs().Send(Arg.Any<UpdateUserCommand>(), _token);
        }

        [Test]
        public async Task UpdateUser_ShouldReturn403Forbidden_WhenNonAdminUpdatesAnotherUser()
        {
            // Arrange
            var targetUserId = Guid.NewGuid();
            var currentUserId = Guid.NewGuid(); // Different from target
            var dto = new UpdateUserRequestDto { FullName = "New Name" };
            SetupControllerContext(currentUserId, "RegularUser"); // Non-Admin/Manager role

            // Act
            var result = await _controller.UpdateUser(targetUserId, dto, _token);

            // Assert
            var forbiddenResult = result as ObjectResult;
            Assert.That(forbiddenResult, Is.Not.Null);
            Assert.That(forbiddenResult!.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));
            var errorResponse = forbiddenResult.Value as ErrorResponse;
            Assert.That(errorResponse!.Message, Does.Contain("You do not have permission"));

            await _sender.DidNotReceiveWithAnyArgs().Send(Arg.Any<UpdateUserCommand>(), _token);
        }

        [Test]
        public async Task UpdateUser_ShouldReturn200Ok_WhenOwnerUpdatesOwnUser()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var dto = new UpdateUserRequestDto { FullName = "Owner New Name" };
            var expectedResponse = new UserResponseDto { UserId = userId, FullName = dto.FullName };
            SetupControllerContext(userId, "RegularUser"); // Owner is allowed

            _sender.Send(Arg.Is<UpdateUserCommand>(c => c.UserId == userId), _token)
                .Returns(expectedResponse);

            // Act
            var result = await _controller.UpdateUser(userId, dto, _token);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            var apiResponse = okResult!.Value as ApiResponse<UserResponseDto>;
            Assert.That(apiResponse!.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(apiResponse.Message, Is.EqualTo("User updated successfully."));
            Assert.That(apiResponse.Data.UserId, Is.EqualTo(userId));

            // Verify MediatR was called with IsAdmin = false
            await _sender.Received(1).Send(Arg.Is<UpdateUserCommand>(c => c.UserId == userId && c.IsAdmin == false), _token);
        }

        [Test]
        public async Task UpdateUser_ShouldReturn200Ok_WhenAdminUpdatesAnotherUser()
        {
            // Arrange
            var targetUserId = Guid.NewGuid();
            var adminId = Guid.NewGuid();
            var dto = new UpdateUserRequestDto { Email = "admin.update@test.com" };
            var expectedResponse = new UserResponseDto { UserId = targetUserId, Email = dto.Email };
            SetupControllerContext(adminId, "Admin"); // Admin role

            _sender.Send(Arg.Is<UpdateUserCommand>(c => c.UserId == targetUserId), _token)
                .Returns(expectedResponse);

            // Act
            var result = await _controller.UpdateUser(targetUserId, dto, _token);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            var apiResponse = okResult!.Value as ApiResponse<UserResponseDto>;
            Assert.That(apiResponse!.StatusCode, Is.EqualTo(StatusCodes.Status200OK));

            // Verify MediatR was called with IsAdmin = true
            await _sender.Received(1).Send(Arg.Is<UpdateUserCommand>(c => c.UserId == targetUserId && c.IsAdmin == true), _token);
        }
        [Test]
        public async Task UpdateUser_WithOnlyEmail_ShouldPass()
        {
            // Arrange - Test single field update
            var userId = Guid.NewGuid();
            var dto = new UpdateUserRequestDto { Email = "newemail@test.com" };
            var expectedResponse = new UserResponseDto { UserId = userId, Email = dto.Email };
            SetupControllerContext(userId, "User");

            _sender.Send(Arg.Any<UpdateUserCommand>(), _token)
                .Returns(expectedResponse);

            // Act
            var result = await _controller.UpdateUser(userId, dto, _token);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
        }

        [Test]
        public async Task UpdateUser_WithOnlyPhoneNumber_ShouldPass()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var dto = new UpdateUserRequestDto { PhoneNumber = "0123456789" };
            var expectedResponse = new UserResponseDto { UserId = userId, PhoneNumber = dto.PhoneNumber };
            SetupControllerContext(userId, "User");

            _sender.Send(Arg.Any<UpdateUserCommand>(), _token)
                .Returns(expectedResponse);

            // Act
            var result = await _controller.UpdateUser(userId, dto, _token);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
        }

        [Test]
        public async Task UpdateUser_WithOnlyAddress_ShouldPass()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var dto = new UpdateUserRequestDto { Address = "New Address 123" };
            var expectedResponse = new UserResponseDto { UserId = userId, Address = dto.Address };
            SetupControllerContext(userId, "User");

            _sender.Send(Arg.Any<UpdateUserCommand>(), _token)
                .Returns(expectedResponse);

            // Act
            var result = await _controller.UpdateUser(userId, dto, _token);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
        }

        [Test]
        public async Task UpdateUser_WithOnlyGender_ShouldPass()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var dto = new UpdateUserRequestDto { Gender = true };
            var expectedResponse = new UserResponseDto { UserId = userId };
            SetupControllerContext(userId, "User");

            _sender.Send(Arg.Any<UpdateUserCommand>(), _token)
                .Returns(expectedResponse);

            // Act
            var result = await _controller.UpdateUser(userId, dto, _token);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
        }

        [Test]
        public async Task UpdateUser_WithOnlyDateOfBirth_ShouldPass()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var dto = new UpdateUserRequestDto { DateOfBirth = "06/15/1985" };
            var expectedResponse = new UserResponseDto { UserId = userId };
            SetupControllerContext(userId, "User");

            _sender.Send(Arg.Any<UpdateUserCommand>(), _token)
                .Returns(expectedResponse);

            // Act
            var result = await _controller.UpdateUser(userId, dto, _token);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
        }

        [Test]
        public async Task GetUserById_ShouldReturn401Unauthorized_WhenUserIsNotAuthenticated()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(new ClaimsIdentity());

            // Act
            var result = await _controller.GetUserById(userId, _token);

            // Assert
            var unauthorizedResult = result as UnauthorizedObjectResult;
            Assert.That(unauthorizedResult, Is.Not.Null);
            var errorResponse = unauthorizedResult!.Value as ErrorResponse;
            Assert.That(errorResponse!.Message, Does.Contain("Unauthorized: missing or invalid JWT."));
        }

        [Test]
        public async Task GetUserById_ShouldReturn200Ok_WhenUserIsFound()
        {
            // Arrange
            var targetUserId = Guid.NewGuid();
            SetupControllerContext(Guid.NewGuid(), "Admin"); // Set up an authenticated user (Admin to bypass permission checks easily for this test)

            var expectedResponse = new UserResponseDto { UserId = targetUserId, FullName = "Test User" };

            // Setup the sender to return the DTO
            _sender.Send(Arg.Is<ViewUserInformationQuery>(q => q.TargetUserId == targetUserId), _token)
                .Returns(expectedResponse);

            // Act
            var result = await _controller.GetUserById(targetUserId, _token);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            var apiResponse = okResult!.Value as ApiResponse<UserResponseDto>;
            Assert.That(apiResponse!.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(apiResponse.Data, Is.EqualTo(expectedResponse));

            // Verify MediatR was called
            await _sender.Received(1).Send(Arg.Is<ViewUserInformationQuery>(q => q.TargetUserId == targetUserId), _token);
        }

        [Test]
        public async Task GetUserById_ShouldReturn404NotFound_WhenUserIsNotFoundByMediatR()
        {
            // Arrange
            var targetUserId = Guid.NewGuid();
            SetupControllerContext(Guid.NewGuid(), "User"); // Set up an authenticated user

            // Setup the sender to return null
            _sender.Send(Arg.Is<ViewUserInformationQuery>(q => q.TargetUserId == targetUserId), _token)
                .Returns((UserResponseDto)null!);

            // Act
            var result = await _controller.GetUserById(targetUserId, _token);

            // Assert
            var notFoundResult = result as NotFoundObjectResult;
            Assert.That(notFoundResult, Is.Not.Null);
            var errorResponse = notFoundResult!.Value as ErrorResponse;
            Assert.That(errorResponse!.Message, Is.EqualTo("User not found or has been deleted."));
        }

        [Test]
        public async Task DeleteUser_ShouldReturn200Ok_WhenDeletionIsSuccessful()
        {
            // Arrange
            var userIdToDelete = Guid.NewGuid();

            // Setup the sender to return true (successful deletion)
            _sender.Send(Arg.Is<DeleteUserCommand>(c => c.UserId == userIdToDelete), _token)
                .Returns(true);

            // Act
            var result = await _controller.DeleteUser(userIdToDelete, _token);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            var apiResponse = okResult!.Value as ApiResponse<bool>;
            Assert.That(apiResponse!.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(apiResponse.Message, Is.EqualTo("User deleted successfully."));
            Assert.That(apiResponse.Data, Is.True);

            // Verify MediatR was called
            await _sender.Received(1).Send(Arg.Is<DeleteUserCommand>(c => c.UserId == userIdToDelete), _token);
        }

        [Test]
        public async Task DeleteUser_ShouldReturn200Ok_WithFalseData_WhenDeletionFails()
        {
            // Arrange
            var userIdToDelete = Guid.NewGuid();

            // Setup the sender to return false (deletion failed, e.g., user not found in the handler)
            // Assuming the MediatR handler returns 'false' for an unsuccessful operation before a specific exception is thrown.
            _sender.Send(Arg.Is<DeleteUserCommand>(c => c.UserId == userIdToDelete), _token)
                .Returns(false);

            // Act
            var result = await _controller.DeleteUser(userIdToDelete, _token);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);
            var apiResponse = okResult!.Value as ApiResponse<bool>;
            // The controller currently assumes a successful response structure (200 OK) even if the data is 'false'.
            // In a real scenario, a handler might throw a NotFoundException, which would be handled by middleware
            // but for this simple controller logic, we check for a 200 OK with data: false
            Assert.That(apiResponse!.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(apiResponse.Message, Is.EqualTo("User deleted successfully.")); // Message might be misleading, but matches controller logic
            Assert.That(apiResponse.Data, Is.False);
        }

        [Test]
        public async Task UpdateUser_ShouldReturn401_WhenUserIdClaimIsMissing()
        {
            // Arrange
            var targetUserId = Guid.NewGuid();
            var dto = new UpdateUserRequestDto { FullName = "Test" };

            // Setup claims without NameIdentifier
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Role, "User")
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            // Act
            var result = await _controller.UpdateUser(targetUserId, dto, _token);

            // Assert
            var unauthorizedResult = result as UnauthorizedObjectResult;
            Assert.That(unauthorizedResult, Is.Not.Null);
        }

        [Test]
        public async Task GetUserById_ShouldReturn401_WhenUserIdClaimIsMissing()
        {
            // Arrange
            var targetUserId = Guid.NewGuid();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Role, "User")
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var principal = new ClaimsPrincipal(identity);

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = principal }
            };

            // Act
            var result = await _controller.GetUserById(targetUserId, _token);

            // Assert
            var unauthorizedResult = result as UnauthorizedObjectResult;
            Assert.That(unauthorizedResult, Is.Not.Null);
        }

        //#region GetUsers (GetAllUser) Tests

        //[Test]
        //public async Task GetUsers_ShouldReturn200Ok_WithListOfUsers()
        //{
        //    // Arrange
        //    var query = new GetUsersQuery(null, null, null, 1, 10);
        //    var expectedList = new List<UserDto>
        //    {
        //        new UserDto
        //    {
        //        UserId = createdUser.UserId,
        //        Email = createdUser.Email,
        //        FullName = createdUser.FullName,
        //        PhoneNumber = command.PhoneNumber,
        //        Gender = command.Gender,
        //        IdentityNumber = command.IdentityNumber,
        //        Address = command.Address,
        //        Role = new RoleDto
        //        {
        //            RoleId = 1,
        //            RoleName = "Default Role",
        //            RoleCode = "DEFAULT",
        //            Description = "Default role description",
        //            Privileges = new List<PrivilegeDto>()
        //        },
        //        DateOfBirth = DateOnly.ParseExact(query.DateOfBirth, "MM/dd/yyyy", null),
        //        Age = 33,
        //        IsPatient = false,
        //        NeedsVerification = true
        //    }
        //        new UserDto { UserId = Guid.NewGuid(), FullName = "User 2" }
        //    };

        //    // Setup the sender to return the list
        //    _sender.Send(query, Arg.Any<CancellationToken>())
        //        .Returns(expectedList);

        //    // Act
        //    var result = await _controller.GetUsers(query);

        //    // Assert
        //    var okResult = result as OkObjectResult;
        //    Assert.That(okResult, Is.Not.Null);
        //    Assert.That(okResult!.Value, Is.EqualTo(expectedList));
        //    Assert.That(okResult.StatusCode, Is.EqualTo(StatusCodes.Status200OK));

        //    // Verify MediatR was called
        //    await _sender.Received(1).Send(query, Arg.Any<CancellationToken>());
        //}

        //#endregion
    }
}
