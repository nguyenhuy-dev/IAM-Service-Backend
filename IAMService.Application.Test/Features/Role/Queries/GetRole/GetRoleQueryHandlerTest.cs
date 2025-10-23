using AutoMapper;
using FluentAssertions;
using IAMService.Application.DTOs;
using IAMService.Application.Features.Role.Queries.GetRole;
using IAMService.Application.Interfaces;
using IAMService.Application.Mappings;
using IAMService.Domain.Entities;
using MockQueryable;
using NSubstitute;

namespace IAMService.Application.Test.Features.Role.Queries.GetRole
{
    /// <summary>
    /// Unit tests for <see cref="GetRoleQueryHandler"/>
    /// </summary>
    [TestFixture]
    public class GetRoleQueryHandlerTests
    {
        /// <summary>
        /// The role repository
        /// </summary>
        private IRoleRepository _roleRepository;
        /// <summary>
        /// The mapper
        /// </summary>
        private IMapper _mapper;
        /// <summary>
        /// The handler
        /// </summary>
        private GetRoleQueryHandler _handler;
        /// <summary>
        /// The test roles
        /// </summary>
        private List<Domain.Entities.Role> _testRoles;
        /// <summary>
        /// Fixtures the setup.
        /// </summary>
        [OneTimeSetUp] // Configure AutoMapper once for all tests in this fixture
        public void FixtureSetup()
        {
            var config = new MapperConfiguration(cfg => {
                cfg.AddProfile<MappingProfile>();
            });
            // Check for configuration errors
            config.AssertConfigurationIsValid();
            _mapper = config.CreateMapper();
        }

        /// <summary>
        /// Setups this instance.
        /// </summary>
        [SetUp]
        public void Setup()
        {
            _roleRepository = Substitute.For<IRoleRepository>();
            _handler = new GetRoleQueryHandler(_roleRepository, _mapper);

            // Setup test data
            var privilegeReadOnly = new Privilege { PrivilegeId = 1, PrivilegeName = "ReadOnly" };
            var privilegeWrite = new Privilege { PrivilegeId = 2, PrivilegeName = "WriteData" };
            var privilegeDelete = new Privilege { PrivilegeId = 3, PrivilegeName = "DeleteData" };

            _testRoles = new List<Domain.Entities.Role>
            {
                new Domain.Entities.Role
                {
                    RoleId = 1,
                    RoleName = "Admin",
                    RoleCode = "ADMIN",
                    Description = "Administrator role with full access",
                    IsDefault = true,
                    Privileges = new List<Privilege>() { privilegeReadOnly, privilegeWrite, privilegeDelete }
                },
                new Domain.Entities.Role
                {
                    RoleId = 2,
                    RoleName = "User",
                    RoleCode = "USER",
                    Description = "Standard user role",
                    IsDefault = false,
                    Privileges = new List<Privilege>() { privilegeReadOnly }
                },
                new Domain.Entities.Role
                {
                    RoleId = 3,
                    RoleName = "Manager",
                    RoleCode = "MANAGER",
                    Description = "Manager role with elevated permissions",
                    IsDefault = false,
                    Privileges = new List<Privilege>() { privilegeReadOnly, privilegeWrite, privilegeDelete }
                },
                new Domain.Entities.Role
                {
                    RoleId = 4,
                    RoleName = "Guest",
                    RoleCode = "GUEST",
                    Description = "",
                    IsDefault = false,
                    Privileges = new List<Privilege>()
                }
            };
        }

        /// <summary>
        /// Handles the no filters returns all roles paginated.
        /// </summary>
        [Test]
        public async Task Handle_NoFilters_ReturnsAllRolesPaginated()
        {
            // Arrange
            var query = new GetRoleQuery(null, null, null, 1, 10);

            var mockQueryable = _testRoles.BuildMock();
            _roleRepository.GetRoleWithPrivileges().Returns(mockQueryable);

            var expectedDtos = new List<GetRoleRequest>
            {
                new GetRoleRequest { RoleId = 1, RoleName = "Admin", RoleCode = "ADMIN", Description = "Administrator role with full access", IsDefault = true, Privileges = new List<PrivilegeDto> { new PrivilegeDto { PrivilegeId = 1, PrivilegeName = "ReadOnly"}, new PrivilegeDto { PrivilegeId = 2, PrivilegeName = "WriteData"}, new PrivilegeDto { PrivilegeId = 3, PrivilegeName = "DeleteData"} } },
                new GetRoleRequest { RoleId = 2, RoleName = "User", RoleCode = "USER", Description = "Standard user role", IsDefault = false, Privileges = new List<PrivilegeDto> { new PrivilegeDto { PrivilegeId = 1, PrivilegeName = "ReadOnly" } } },
                new GetRoleRequest { RoleId = 3, RoleName = "Manager", RoleCode = "MANAGER", Description = "Manager role with elevated permissions", IsDefault = false, Privileges = new List<PrivilegeDto> { new PrivilegeDto { PrivilegeId = 1, PrivilegeName = "ReadOnly" }, new PrivilegeDto { PrivilegeId = 2, PrivilegeName = "WriteData"}, new PrivilegeDto { PrivilegeId = 3, PrivilegeName = "DeleteData"} } },
                new GetRoleRequest { RoleId = 4, RoleName = "Guest", RoleCode = "GUEST", Description = "", IsDefault = false, Privileges = new List<PrivilegeDto>() },
            };
            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(4);
            result.PageNumber.Should().Be(1);
            result.TotalPages.Should().Be(1);
            result.TotalCount.Should().Be(4);
            result.Items.Should().BeEquivalentTo(expectedDtos, options => options.WithStrictOrdering());
            _roleRepository.Received(1).GetRoleWithPrivileges();
        }

        /// <summary>
        /// Handles the name of the with search term filters roles by.
        /// </summary>
        [Test]
        public async Task Handle_WithSearchTerm_FiltersRolesByName()
        {
            // Arrange
            var query = new GetRoleQuery("Admin", null, null, 1, 10);

            var mockQueryable = _testRoles.BuildMock();
            _roleRepository.GetRoleWithPrivileges().Returns(mockQueryable);

            var expectedDtos = new List<GetRoleRequest>
            {
                new GetRoleRequest { RoleId = 1, RoleName = "Admin", RoleCode = "ADMIN", Description = "Administrator role with full access", IsDefault = true, Privileges = new List<PrivilegeDto> { new PrivilegeDto { PrivilegeId = 1, PrivilegeName = "ReadOnly"}, new PrivilegeDto { PrivilegeId = 2, PrivilegeName = "WriteData"}, new PrivilegeDto { PrivilegeId = 3, PrivilegeName = "DeleteData"} } }
            };
            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Items.Should().HaveCount(1);
            result.PageNumber.Should().Be(1);
            result.TotalPages.Should().Be(1);
            result.TotalCount.Should().Be(1);
            result.Items[0].RoleName.Should().Be("Admin");
            result.Items.Should().BeEquivalentTo(expectedDtos, options => options.WithStrictOrdering());
            _roleRepository.Received(1).GetRoleWithPrivileges();
        }

        /// <summary>
        /// Handles the with search term filters roles by code.
        /// </summary>
        [Test]
        public async Task Handle_WithSearchTerm_FiltersRolesByCode()
        {
            // Arrange
            var query = new GetRoleQuery("USER", null, null, 1, 10);

            var mockQueryable = _testRoles.BuildMock();
            _roleRepository.GetRoleWithPrivileges().Returns(mockQueryable);

            var expectedDtos = new List<GetRoleRequest>
            {
                new GetRoleRequest { RoleId = 2, RoleName = "User", RoleCode = "USER", Description = "Standard user role", IsDefault = false, Privileges = new List<PrivilegeDto> { new PrivilegeDto { PrivilegeId = 1, PrivilegeName = "ReadOnly" } } }
            };
            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Items.Should().HaveCount(1);
            result.PageNumber.Should().Be(1);
            result.TotalPages.Should().Be(1);
            result.TotalCount.Should().Be(1);
            result.Items[0].RoleCode.Should().Be("USER");
            result.Items.Should().BeEquivalentTo(expectedDtos, options => options.WithStrictOrdering());
            _roleRepository.Received(1).GetRoleWithPrivileges();
        }

        /// <summary>
        /// Handles the with search term filters roles by description.
        /// </summary>
        [Test]
        public async Task Handle_WithSearchTerm_FiltersRolesByDescription()
        {
            // Arrange
            var query = new GetRoleQuery("elevated", null, null, 1, 10);

            var mockQueryable = _testRoles.BuildMock();
            _roleRepository.GetRoleWithPrivileges().Returns(mockQueryable);

            var expectedDtos = new List<GetRoleRequest>
            {
                new GetRoleRequest { RoleId = 3, RoleName = "Manager", RoleCode = "MANAGER", Description = "Manager role with elevated permissions", IsDefault = false, Privileges = new List<PrivilegeDto> { new PrivilegeDto { PrivilegeId = 1, PrivilegeName = "ReadOnly" }, new PrivilegeDto { PrivilegeId = 2, PrivilegeName = "WriteData"}, new PrivilegeDto { PrivilegeId = 3, PrivilegeName = "DeleteData"} } }
            };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Items.Should().HaveCount(1);
            result.Items[0].RoleName.Should().Be("Manager");
            result.Items.Should().BeEquivalentTo(expectedDtos, options => options.WithStrictOrdering());
            _roleRepository.Received(1).GetRoleWithPrivileges();
        }

        /// <summary>
        /// Handles the empty search term returns all roles.
        /// </summary>
        [Test]
        public async Task Handle_EmptySearchTerm_ReturnsAllRoles()
        {
            // Arrange
            var query = new GetRoleQuery("   ", null, null, 1, 10);

            var mockQueryable = _testRoles.BuildMock();
            _roleRepository.GetRoleWithPrivileges().Returns(mockQueryable);

            var expectedDtos = new List<GetRoleRequest>
            {
                new GetRoleRequest { RoleId = 1, RoleName = "Admin", RoleCode = "ADMIN", Description = "Administrator role with full access", IsDefault = true, Privileges = new List<PrivilegeDto> { new PrivilegeDto { PrivilegeId = 1, PrivilegeName = "ReadOnly"}, new PrivilegeDto { PrivilegeId = 2, PrivilegeName = "WriteData"}, new PrivilegeDto { PrivilegeId = 3, PrivilegeName = "DeleteData"} } },
                new GetRoleRequest { RoleId = 2, RoleName = "User", RoleCode = "USER", Description = "Standard user role", IsDefault = false, Privileges = new List<PrivilegeDto> { new PrivilegeDto { PrivilegeId = 1, PrivilegeName = "ReadOnly" } } },
                new GetRoleRequest { RoleId = 3, RoleName = "Manager", RoleCode = "MANAGER", Description = "Manager role with elevated permissions", IsDefault = false, Privileges = new List<PrivilegeDto> { new PrivilegeDto { PrivilegeId = 1, PrivilegeName = "ReadOnly" }, new PrivilegeDto { PrivilegeId = 2, PrivilegeName = "WriteData"}, new PrivilegeDto { PrivilegeId = 3, PrivilegeName = "DeleteData"} } },
                new GetRoleRequest { RoleId = 4, RoleName = "Guest", RoleCode = "GUEST", Description = "", IsDefault = false, Privileges = new List<PrivilegeDto>() },
            };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Items.Should().HaveCount(4);
            result.Items.Should().BeEquivalentTo(expectedDtos, options => options.WithStrictOrdering());
            _roleRepository.Received(1).GetRoleWithPrivileges();
        }

        /// <summary>
        /// Handles the sort by role name ascending returns sorted roles.
        /// </summary>
        [Test]
        public async Task Handle_SortByRoleName_Ascending_ReturnsSortedRoles()
        {
            // Arrange
            var query = new GetRoleQuery(null, "RoleName", "asc", 1, 10);

            var mockQueryable = _testRoles.BuildMock();
            _roleRepository.GetRoleWithPrivileges().Returns(mockQueryable);

            var expectedDtos = new List<GetRoleRequest>
            {
                new GetRoleRequest { RoleId = 1, RoleName = "Admin", RoleCode = "ADMIN", Description = "Administrator role with full access", IsDefault = true, Privileges = new List<PrivilegeDto> { new PrivilegeDto { PrivilegeId = 1, PrivilegeName = "ReadOnly"}, new PrivilegeDto { PrivilegeId = 2, PrivilegeName = "WriteData"}, new PrivilegeDto { PrivilegeId = 3, PrivilegeName = "DeleteData"} } },
                new GetRoleRequest { RoleId = 4, RoleName = "Guest", RoleCode = "GUEST", Description = "", IsDefault = false, Privileges = new List<PrivilegeDto>() },
                new GetRoleRequest { RoleId = 3, RoleName = "Manager", RoleCode = "MANAGER", Description = "Manager role with elevated permissions", IsDefault = false, Privileges = new List<PrivilegeDto> { new PrivilegeDto { PrivilegeId = 1, PrivilegeName = "ReadOnly" }, new PrivilegeDto { PrivilegeId = 2, PrivilegeName = "WriteData"}, new PrivilegeDto { PrivilegeId = 3, PrivilegeName = "DeleteData"} } }, // Corrected privileges based on error
                new GetRoleRequest { RoleId = 2, RoleName = "User", RoleCode = "USER", Description = "Standard user role", IsDefault = false, Privileges = new List<PrivilegeDto> { new PrivilegeDto { PrivilegeId = 1, PrivilegeName = "ReadOnly" } } }
            };


            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Items.Should().HaveCount(4);
            result.Items[0].RoleName.Should().Be("Admin");
            result.Items.Should().BeEquivalentTo(expectedDtos, options => options.WithStrictOrdering());
            _roleRepository.Received(1).GetRoleWithPrivileges();
        }
        /// <summary>
        /// Handles the sort by role name descending returns sorted roles.
        /// </summary>
        [Test]
        public async Task Handle_SortByRoleName_Descending_ReturnsSortedRoles()
        {
            // Arrange
            var query = new GetRoleQuery(null, "RoleName", "desc", 1, 10);

            var mockQueryable = _testRoles.BuildMock();
            _roleRepository.GetRoleWithPrivileges().Returns(mockQueryable);

            var expectedDtos = new List<GetRoleRequest>
            {
                new GetRoleRequest { RoleId = 2, RoleName = "User", RoleCode = "USER", Description = "Standard user role", IsDefault = false, Privileges = new List<PrivilegeDto> { new PrivilegeDto { PrivilegeId = 1, PrivilegeName = "ReadOnly" } } },
                new GetRoleRequest { RoleId = 4, RoleName = "Guest", RoleCode = "GUEST", Description = "", IsDefault = false, Privileges = new List<PrivilegeDto>() },    
                new GetRoleRequest { RoleId = 3, RoleName = "Manager", RoleCode = "MANAGER", Description = "Manager role with elevated permissions", IsDefault = false, Privileges = new List<PrivilegeDto> { new PrivilegeDto { PrivilegeId = 1, PrivilegeName = "ReadOnly" }, new PrivilegeDto { PrivilegeId = 2, PrivilegeName = "WriteData"}, new PrivilegeDto { PrivilegeId = 3, PrivilegeName = "DeleteData"} } }, // Corrected privileges based on error
                new GetRoleRequest { RoleId = 1, RoleName = "Admin", RoleCode = "ADMIN", Description = "Administrator role with full access", IsDefault = true, Privileges = new List<PrivilegeDto> { new PrivilegeDto { PrivilegeId = 1, PrivilegeName = "ReadOnly"}, new PrivilegeDto { PrivilegeId = 2, PrivilegeName = "WriteData"}, new PrivilegeDto { PrivilegeId = 3, PrivilegeName = "DeleteData"} } } 
            };
            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Items.Should().HaveCount(4);
            result.Items[0].RoleName.Should().Be("User");
            result.Items.Should().BeEquivalentTo(expectedDtos);
            _roleRepository.Received(1).GetRoleWithPrivileges();
        }

        /// <summary>
        /// Handles the sort by role code ascending returns sorted roles.
        /// </summary>
        [Test]
        public async Task Handle_SortByRoleCode_Ascending_ReturnsSortedRoles()
        {
            // Arrange
            var query = new GetRoleQuery(null, "RoleCode", "asc", 1, 10);

            var mockQueryable = _testRoles.BuildMock();
            _roleRepository.GetRoleWithPrivileges().Returns(mockQueryable);

            var expectedDtos = new List<GetRoleRequest>
            {
                new GetRoleRequest { RoleId = 1, RoleName = "Admin", RoleCode = "ADMIN", Description = "Administrator role with full access", IsDefault = true, Privileges = new List<PrivilegeDto> { new PrivilegeDto { PrivilegeId = 1, PrivilegeName = "ReadOnly"}, new PrivilegeDto { PrivilegeId = 2, PrivilegeName = "WriteData"}, new PrivilegeDto { PrivilegeId = 3, PrivilegeName = "DeleteData"} } },
                new GetRoleRequest { RoleId = 4, RoleName = "Guest", RoleCode = "GUEST", Description = "", IsDefault = false, Privileges = new List<PrivilegeDto>() },
                new GetRoleRequest { RoleId = 3, RoleName = "Manager", RoleCode = "MANAGER", Description = "Manager role with elevated permissions", IsDefault = false, Privileges = new List<PrivilegeDto> { new PrivilegeDto { PrivilegeId = 1, PrivilegeName = "ReadOnly" }, new PrivilegeDto { PrivilegeId = 2, PrivilegeName = "WriteData"}, new PrivilegeDto { PrivilegeId = 3, PrivilegeName = "DeleteData"} } }, // Corrected privileges based on error
                new GetRoleRequest { RoleId = 2, RoleName = "User", RoleCode = "USER", Description = "Standard user role", IsDefault = false, Privileges = new List<PrivilegeDto> { new PrivilegeDto { PrivilegeId = 1, PrivilegeName = "ReadOnly" } } }
            };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Items.Should().HaveCount(4);
            result.Items[0].RoleCode.Should().Be("ADMIN");
            result.Items.Should().BeEquivalentTo(expectedDtos, options => options.WithStrictOrdering());
            _roleRepository.Received(1).GetRoleWithPrivileges();
        }
        /// <summary>
        /// Handles the name of the invalid sort by defaults to role.
        /// </summary>
        [Test]
        public async Task Handle_InvalidSortBy_DefaultsToRoleName()
        {
            // Arrange
            var query = new GetRoleQuery(null, "InvalidField", "asc", 1, 10);

            var mockQueryable = _testRoles.BuildMock();
            _roleRepository.GetRoleWithPrivileges().Returns(mockQueryable);

            var expectedDtos = new List<GetRoleRequest>
            {
                new GetRoleRequest { RoleId = 1, RoleName = "Admin", RoleCode = "ADMIN", Description = "Administrator role with full access", IsDefault = true, Privileges = new List<PrivilegeDto> { new PrivilegeDto { PrivilegeId = 1, PrivilegeName = "ReadOnly"}, new PrivilegeDto { PrivilegeId = 2, PrivilegeName = "WriteData"}, new PrivilegeDto { PrivilegeId = 3, PrivilegeName = "DeleteData"} } },
                new GetRoleRequest { RoleId = 2, RoleName = "User", RoleCode = "USER", Description = "Standard user role", IsDefault = false, Privileges = new List<PrivilegeDto> { new PrivilegeDto { PrivilegeId = 1, PrivilegeName = "ReadOnly" } } },
                new GetRoleRequest { RoleId = 3, RoleName = "Manager", RoleCode = "MANAGER", Description = "Manager role with elevated permissions", IsDefault = false, Privileges = new List<PrivilegeDto> { new PrivilegeDto { PrivilegeId = 1, PrivilegeName = "ReadOnly" }, new PrivilegeDto { PrivilegeId = 2, PrivilegeName = "WriteData"}, new PrivilegeDto { PrivilegeId = 3, PrivilegeName = "DeleteData"} } },
                new GetRoleRequest { RoleId = 4, RoleName = "Guest", RoleCode = "GUEST", Description = "", IsDefault = false, Privileges = new List<PrivilegeDto>() },
            };
            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Items.Should().HaveCount(4);
            result.Items[0].RoleName.Should().Be("Admin");
            result.Items.Should().BeEquivalentTo(expectedDtos, options => options.WithStrictOrdering());
            _roleRepository.Received(1).GetRoleWithPrivileges();
        }
        /// <summary>
        /// Handles the no matching roles returns empty list.
        /// </summary>
        [Test]
        public async Task Handle_NoMatchingRoles_ReturnsEmptyList()
        {
            // Arrange
            var query = new GetRoleQuery("NonExistentRole", null, null, 1, 10);

            var mockQueryable = _testRoles.BuildMock();
            _roleRepository.GetRoleWithPrivileges().Returns(mockQueryable);

            var expectedDtos = new List<GetRoleRequest>();

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Items.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
            result.Items.Should().BeEquivalentTo(expectedDtos, options => options.WithStrictOrdering());
            _roleRepository.Received(1).GetRoleWithPrivileges();
        }
        /// <summary>
        /// Handles the case sensitive search filters correctly.
        /// </summary>
        [Test]
        public async Task Handle_CaseSensitiveSearch_FiltersCorrectly()
        {
            // Arrange
            var query = new GetRoleQuery("admin", null, null, 1, 10);

            var mockQueryable = _testRoles.BuildMock();
            _roleRepository.GetRoleWithPrivileges().Returns(mockQueryable);

            var expectedDtos = new List<GetRoleRequest>
            {
                new GetRoleRequest { RoleId = 1, RoleName = "Admin", RoleCode = "ADMIN", Description = "Administrator role with full access", IsDefault = true, Privileges = new List<PrivilegeDto> { new PrivilegeDto { PrivilegeId = 1, PrivilegeName = "ReadOnly"}, new PrivilegeDto { PrivilegeId = 2, PrivilegeName = "WriteData"}, new PrivilegeDto { PrivilegeId = 3, PrivilegeName = "DeleteData"} } }
            };

            // Act
            var result = await _handler.Handle(query, CancellationToken.None);

            // Assert
            result.Items.Should().HaveCount(1);
            result.Items[0].RoleName.Should().Be("Admin");
            result.Items.Should().BeEquivalentTo(expectedDtos, options => options.WithStrictOrdering());
            _roleRepository.Received(1).GetRoleWithPrivileges();
        }

        /// <summary>
        /// Handles the with cancellation token completes successfully.
        /// </summary>
        [Test]
        public async Task Handle_WithCancellationToken_CompletesSuccessfully()
        {
            // Arrange
            var query = new GetRoleQuery(null, null, null, 1, 10);
            var cancellationToken = new CancellationToken();

            var mockQueryable = _testRoles.BuildMock();
            _roleRepository.GetRoleWithPrivileges().Returns(mockQueryable);

            var expectedDtos = new List<GetRoleRequest>
            {
                new GetRoleRequest { RoleId = 1, RoleName = "Admin", RoleCode = "ADMIN", Description = "Administrator role with full access", IsDefault = true, Privileges = new List<PrivilegeDto> { new PrivilegeDto { PrivilegeId = 1, PrivilegeName = "ReadOnly"}, new PrivilegeDto { PrivilegeId = 2, PrivilegeName = "WriteData"}, new PrivilegeDto { PrivilegeId = 3, PrivilegeName = "DeleteData"} } },
                new GetRoleRequest { RoleId = 2, RoleName = "User", RoleCode = "USER", Description = "Standard user role", IsDefault = false, Privileges = new List<PrivilegeDto> { new PrivilegeDto { PrivilegeId = 1, PrivilegeName = "ReadOnly" } } },
                new GetRoleRequest { RoleId = 3, RoleName = "Manager", RoleCode = "MANAGER", Description = "Manager role with elevated permissions", IsDefault = false, Privileges = new List<PrivilegeDto> { new PrivilegeDto { PrivilegeId = 1, PrivilegeName = "ReadOnly" }, new PrivilegeDto { PrivilegeId = 2, PrivilegeName = "WriteData"}, new PrivilegeDto { PrivilegeId = 3, PrivilegeName = "DeleteData"} } },
                new GetRoleRequest { RoleId = 4, RoleName = "Guest", RoleCode = "GUEST", Description = "", IsDefault = false, Privileges = new List<PrivilegeDto>() },
            };

            // Act
            var result = await _handler.Handle(query, cancellationToken);

            // Assert
            result.Should().NotBeNull();
            result.Items.Should().BeEquivalentTo(expectedDtos, options => options.WithStrictOrdering());
            _roleRepository.Received(1).GetRoleWithPrivileges();
        }
    }
}