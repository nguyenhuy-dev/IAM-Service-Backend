using IAMService.API.Common;
using IAMService.API.Controllers;
using IAMService.API.Middleware;
using IAMService.Application.DTOs;
using IAMService.Application.Features.Role.Commands.CreateRole;
using IAMService.Application.Features.Role.Commands.DeleteRole;
using IAMService.Application.Features.Role.Commands.UpdateRole;
using IAMService.Application.Features.Role.Queries.GetRole;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace IAMService.API.Test
{
    [TestFixture]
    public class RolesControllerTests
    {
        private ISender _sender;
        private RolesController _controller;
        private CancellationToken _token;

        [SetUp]
        public void SetUp()
        {
            _sender = Substitute.For<ISender>();
            _controller = new RolesController(_sender);
            _token = CancellationToken.None;

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            };
        }

        #region CreateRole Tests

        [Test]
        public async Task CreateRole_ShouldReturn201Created_WhenRoleIsCreatedSuccessfully()
        {
            // Arrange
            var command = new CreateRoleCommand(
                "Admin",
                "ADMIN",
                "Administrator role",
                new List<int> { 10, 20 }
            );

            var expectedRoleDto = new RoleDto
            {
                RoleId = 1,
                RoleName = "Admin",
                RoleCode = "ADMIN",
                Description = "Administrator role",
                Privileges = new List<PrivilegeDto>()
            };


            _sender.Send(command, _token).Returns(expectedRoleDto);

            // Act
            var result = await _controller.CreateRole(command, _token);

            // Assert
            var statusCodeResult = result as ObjectResult;
            Assert.That(statusCodeResult, Is.Not.Null);
            Assert.That(statusCodeResult.StatusCode, Is.EqualTo(StatusCodes.Status201Created));

            var apiResponse = statusCodeResult!.Value as ApiResponse<RoleDto>;
            Assert.That(apiResponse, Is.Not.Null);
            Assert.That(apiResponse!.StatusCode, Is.EqualTo(StatusCodes.Status201Created));
            Assert.That(apiResponse.Message, Is.EqualTo("Role created successfully."));
            Assert.That(apiResponse.Data, Is.EqualTo(expectedRoleDto));
            Assert.That(apiResponse.Data.RoleId, Is.EqualTo(1));
            Assert.That(apiResponse.Data.RoleName, Is.EqualTo(command.RoleName));

            await _sender.Received(1).Send(command, _token);
        }

        [Test]
        public async Task CreateRole_WithMinimalData_ShouldReturn201Created()
        {
            // Arrange
            var command = new CreateRoleCommand(
                 "Basic Role",
                 "BASIC",
                 null,
                 null
             );

            var expectedRoleDto = new RoleDto
            {
                RoleId = 2,
                RoleName = "Basic Role",
                RoleCode = "BASIC",
                Description = null,
                Privileges = new List<PrivilegeDto>()
            };

            _sender.Send(command, _token).Returns(expectedRoleDto);

            // Act
            var result = await _controller.CreateRole(command, _token);

            // Assert
            var statusCodeResult = result as ObjectResult;
            Assert.That(statusCodeResult, Is.Not.Null);
            Assert.That(statusCodeResult.StatusCode, Is.EqualTo(StatusCodes.Status201Created));

            var apiResponse = statusCodeResult!.Value as ApiResponse<RoleDto>;
            Assert.That(apiResponse!.Data.Privileges, Is.Empty);

            await _sender.Received(1).Send(command, _token);
        }

        #endregion

        #region UpdateRole Tests

        [Test]
        public async Task UpdateRole_ShouldReturn200Ok_WhenRoleIsUpdatedSuccessfully()
        {
            // Arrange
            var roleId = 1;
            var request = new UpdateRoleRequest("Updated Role", "UPDATED_ROLE", "Updated description", new[] { 1 });


            var expectedDto = new RoleDto
            {
                RoleId = roleId,
                RoleName = request.RoleName,
                RoleCode = request.RoleCode,
                Description = request.Description,
                Privileges = new List<PrivilegeDto>
                {
                    new PrivilegeDto { PrivilegeId = 1, PrivilegeName = "Read" },
                    new PrivilegeDto { PrivilegeId = 2, PrivilegeName = "Write" }
                }
            };

            _sender.Send(Arg.Is<UpdateRoleCommand>(c =>
                c.RoleId == roleId &&
                c.RoleName == request.RoleName &&
                c.RoleCode == request.RoleCode &&
                c.Description == request.Description &&
                c.PrivilegeIds == request.PrivilegeIds), _token)
                .Returns(expectedDto);

            // Act
            var result = await _controller.UpdateRole(roleId, request, _token);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);

            var apiResponse = okResult!.Value as ApiResponse<RoleDto>;
            Assert.That(apiResponse, Is.Not.Null);
            Assert.That(apiResponse!.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(apiResponse.Message, Is.EqualTo("Role updated successfully."));
            Assert.That(apiResponse.Data, Is.EqualTo(expectedDto));
            Assert.That(apiResponse.Data.RoleId, Is.EqualTo(roleId));

            await _sender.Received(1).Send(Arg.Is<UpdateRoleCommand>(c =>
                c.RoleId == roleId), _token);
        }

        [Test]
        public async Task UpdateRole_WithNullDescription_ShouldReturn200Ok()
        {
            // Arrange
            var roleId = 1;

            var request = new UpdateRoleRequest("Role Without Description", "NO_DESC", null, new[] { 1 });

            var expectedDto = new RoleDto
            {
                RoleId = roleId,
                RoleName = request.RoleName,
                RoleCode = request.RoleCode,
                Description = null,
                Privileges = new List<PrivilegeDto>
                {
                    new PrivilegeDto { PrivilegeId = 1, PrivilegeName = "Read" }
                }
            };

            _sender.Send(Arg.Any<UpdateRoleCommand>(), _token).Returns(expectedDto);

            // Act
            var result = await _controller.UpdateRole(roleId, request, _token);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);

            var apiResponse = okResult!.Value as ApiResponse<RoleDto>;
            Assert.That(apiResponse!.Data.Description, Is.Null);
        }

        [Test]
        public async Task UpdateRole_WithEmptyPrivileges_ShouldReturn200Ok()
        {
            // Arrange
            var roleId = 1;
            
            var request = new UpdateRoleRequest("Role Without Privileges", "NO_PRIV", "Role with no privileges", new int[] { });

            var expectedDto = new RoleDto
            {
                RoleId = roleId,
                RoleName = request.RoleName,
                RoleCode = request.RoleCode,
                Description = request.Description,
                Privileges = new List<PrivilegeDto>()
            };

            _sender.Send(Arg.Any<UpdateRoleCommand>(), _token).Returns(expectedDto);

            // Act
            var result = await _controller.UpdateRole(roleId, request, _token);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);

            var apiResponse = okResult!.Value as ApiResponse<RoleDto>;
            Assert.That(apiResponse!.Data.Privileges, Is.Empty);
        }

        #endregion

        #region GetRoles Tests

        [Test]
        public async Task GetRoles_ShouldReturn200Ok_WithRoles_WhenRolesExist()
        {
            // Arrange
            var query = new GetRoleQuery(null, null, null, 1, 10);

            var roles = new List<GetRoleRequest>
            {
                new GetRoleRequest
                {
                    RoleId = 1,
                    RoleName = "Admin",
                    RoleCode = "ADMIN",
                    Description = "Administrator role"
                },
                new GetRoleRequest
                {
                    RoleId = 2,
                    RoleName = "User",
                    RoleCode = "USER",
                    Description = "Regular user role"
                }
            };

            var paginatedResult = new PaginatedList<GetRoleRequest>(
                roles,
                roles.Count,
                query.PageNumber,
                query.PageSize);

            _sender.Send(query, default).Returns(paginatedResult);

            // Act
            var result = await _controller.GetRoles(query);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);

            var apiResponse = okResult!.Value as ApiResponse<PaginatedList<GetRoleRequest>>;
            Assert.That(apiResponse, Is.Not.Null);
            Assert.That(apiResponse!.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(apiResponse.Message, Is.EqualTo("Get roles successfully."));
            Assert.That(apiResponse.Data.Items, Has.Count.EqualTo(2));
            Assert.That(apiResponse.Data.TotalCount, Is.EqualTo(2));

            await _sender.Received(1).Send(query, default);
        }

        [Test]
        public async Task GetRoles_ShouldReturn200Ok_WithEmptyList_WhenNoRolesExist()
        {
            // Arrange
            var query = new GetRoleQuery(null, null, null, 1, 10);

            var emptyList = new List<GetRoleRequest>();
            var paginatedResult = new PaginatedList<GetRoleRequest>(
                emptyList,
                0,
                query.PageNumber,
                query.PageSize);

            _sender.Send(query, default).Returns(paginatedResult);

            // Act
            var result = await _controller.GetRoles(query);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);

            var apiResponse = okResult!.Value as ApiResponse<PaginatedList<GetRoleRequest>>;
            Assert.That(apiResponse, Is.Not.Null);
            Assert.That(apiResponse!.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(apiResponse.Message, Is.EqualTo("No Roles found."));
            Assert.That(apiResponse.Data.Items, Is.Empty);
            Assert.That(apiResponse.Data.TotalCount, Is.EqualTo(0));
        }

        [Test]
        public async Task GetRoles_WithSearchTerm_ShouldReturn200Ok_WithFilteredResults()
        {
            // Arrange
            var query = new GetRoleQuery("Admin", null, null, 1, 10);

            var roles = new List<GetRoleRequest>
            {
                new GetRoleRequest
                {
                    RoleId = 1,
                    RoleName = "Admin",
                    RoleCode = "ADMIN",
                    Description = "Administrator role"
                }
            };

            var paginatedResult = new PaginatedList<GetRoleRequest>(
                roles,
                1,
                query.PageNumber,
                query.PageSize);

            _sender.Send(query, default).Returns(paginatedResult);

            // Act
            var result = await _controller.GetRoles(query);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);

            var apiResponse = okResult!.Value as ApiResponse<PaginatedList<GetRoleRequest>>;
            Assert.That(apiResponse!.Data.Items, Has.Count.EqualTo(1));
            Assert.That(apiResponse.Data.Items[0].RoleName, Does.Contain("Admin"));
        }

        [Test]
        public async Task GetRoles_WithPagination_ShouldReturn200Ok()
        {
            // Arrange
            var query = new GetRoleQuery(null, null, null, 2, 5);

            var roles = new List<GetRoleRequest>
            {
                new GetRoleRequest { RoleId = 6, RoleName = "Role 6", RoleCode = "R6" },
                new GetRoleRequest { RoleId = 7, RoleName = "Role 7", RoleCode = "R7" }
            };

            var paginatedResult = new PaginatedList<GetRoleRequest>(
                roles,
                12,
                query.PageNumber,
                query.PageSize);

            _sender.Send(query, default).Returns(paginatedResult);

            // Act
            var result = await _controller.GetRoles(query);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);

            var apiResponse = okResult!.Value as ApiResponse<PaginatedList<GetRoleRequest>>;
            Assert.That(apiResponse!.Data.PageNumber, Is.EqualTo(2));
            Assert.That(apiResponse.Data.TotalCount, Is.EqualTo(12));
        }

        #endregion

        #region DeleteRole Tests

        [Test]
        public async Task DeleteRole_ShouldReturn200Ok_WhenDeletionIsSuccessful()
        {
            // Arrange
            var roleId = 1;

            await _sender.Send(Arg.Is<DeleteRoleCommand>(c => c.RoleId == roleId), _token);

            // Act
            var result = await _controller.DeleteRole(roleId, _token);

            // Assert
            var okResult = result as OkObjectResult;
            Assert.That(okResult, Is.Not.Null);

            var apiResponse = okResult!.Value as ApiResponse<bool>;
            Assert.That(apiResponse, Is.Not.Null);
            Assert.That(apiResponse!.StatusCode, Is.EqualTo(StatusCodes.Status200OK));
            Assert.That(apiResponse.Message, Is.EqualTo("Role deleted successfully."));
            Assert.That(apiResponse.Data, Is.True);

            await _sender.Received(1).Send(Arg.Is<DeleteRoleCommand>(c => c.RoleId == roleId), _token);
        }

        //[Test]
        //public async Task DeleteRole_WithDifferentRoleId_ShouldCallMediatorWithCorrectId()
        //{
        //    // Arrange
        //    var roleId = 99;

        //    _sender.Send(Arg.Is<DeleteRoleCommand>(c => c.RoleId == roleId), _token)
        //        .Returns(Task.CompletedTask);

        //    // Act
        //    var result = await _controller.DeleteRole(roleId, _token);

        //    // Assert
        //    var okResult = result as OkObjectResult;
        //    Assert.That(okResult, Is.Not.Null);

        //    await _sender.Received(1).Send(
        //        Arg.Is<DeleteRoleCommand>(c => c.RoleId == 99),
        //        _token);
        //}

        //[Test]
        //public async Task DeleteRole_ShouldReturnTrue_InResponseData()
        //{
        //    // Arrange
        //    var roleId = 5;

        //    _sender.Send(Arg.Any<DeleteRoleCommand>(), _token)
        //        .Returns(Task.CompletedTask);

        //    // Act
        //    var result = await _controller.DeleteRole(roleId, _token);

        //    // Assert
        //    var okResult = result as OkObjectResult;
        //    var apiResponse = okResult!.Value as ApiResponse<bool>;
        //    Assert.That(apiResponse!.Data, Is.True);
        //}

        #endregion

        #region Edge Cases and Cancellation Token Tests

        [Test]
        public async Task CreateRole_ShouldPassCancellationToken_ToMediator()
        {
            // Arrange
            var cts = new CancellationTokenSource();
            var token = cts.Token;
            var command = new CreateRoleCommand(
                "Admin",
                "ADMIN",
                "Administrator role",
                new List<int> { 10, 20 }
            );

            var expectedRoleDto = new RoleDto
            {
                RoleId = 1,
                RoleName = "Admin",
                RoleCode = "ADMIN",
                Description = "Administrator role",
                Privileges = new List<PrivilegeDto>()
            };

            _sender.Send(command, token).Returns(expectedRoleDto);

            // Act
            await _controller.CreateRole(command, token);

            // Assert
            await _sender.Received(1).Send(command, token);
        }

        [Test]
        public async Task UpdateRole_ShouldPassCancellationToken_ToMediator()
        {
            // Arrange
            var cts = new CancellationTokenSource();
            var token = cts.Token;
            var roleId = 1;
            var request = new UpdateRoleRequest("Test", "TEST", null, new int[] { });

            var expectedDto = new RoleDto
            {
                RoleId = roleId,
                RoleName = "Test",
                RoleCode = "TEST",
                Description = null,
                Privileges = new List<PrivilegeDto>()
            };

            _sender.Send(Arg.Any<UpdateRoleCommand>(), token).Returns(expectedDto);

            // Act
            await _controller.UpdateRole(roleId, request, token);

            // Assert
            await _sender.Received(1).Send(Arg.Any<UpdateRoleCommand>(), token);
        }

        //[Test]
        //public async Task DeleteRole_ShouldPassCancellationToken_ToMediator()
        //{
        //    // Arrange
        //    var cts = new CancellationTokenSource();
        //    var token = cts.Token;
        //    var roleId = 1;

        //    _sender.Send(Arg.Any<DeleteRoleCommand>(), token)
        //        .Returns(Task.CompletedTask);

        //    // Act
        //    await _controller.DeleteRole(roleId, token);

        //    // Assert
        //    await _sender.Received(1).Send(Arg.Any<DeleteRoleCommand>(), token);
        //}

        #endregion
    }
}