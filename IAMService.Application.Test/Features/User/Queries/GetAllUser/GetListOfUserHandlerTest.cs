using AutoMapper;
using FluentAssertions;
using IAMService.Application.Features.User.Queries.GetAllUser;
using IAMService.Application.Interfaces;
using IAMService.Application.Mappings;
using Microsoft.Extensions.Logging.Abstractions;
using MockQueryable;
using NSubstitute;
namespace IAMService.Application.Test.Features.User.Queries.GetAllUser
{
    [TestFixture]
    public class GetListOfUserHandlerTest
    {

        [OneTimeSetUp]
        public void FixtureSetup()
        {
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            }, new NullLoggerFactory());

            config.AssertConfigurationIsValid();
            _mapper = config.CreateMapper();
        }

        [SetUp]
        public void Setup()
        {
            _userRepository = Substitute.For<IUserRepository>();
            _stringEncryptionService = Substitute.For<IStringEncryptionService>();

            // Return input string as-is for DecryptString to avoid transformation
            _stringEncryptionService.DecryptString(Arg.Any<string>())
                .Returns(callInfo => callInfo.Arg<string>());

            _handler = new GetUsersQueryHandler(_userRepository, _mapper, _stringEncryptionService);

            var roleAdmin = new Domain.Entities.Role { RoleId = 1, RoleName = "Admin" };
            var roleUser = new Domain.Entities.Role { RoleId = 2, RoleName = "User" };

            _testUsers = new List<Domain.Entities.User>
            {
                new Domain.Entities.User
                {
                    UserId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    FullName = "Alice",
                    Email = "alice@example.com",
                    Role = roleAdmin
                },
                new Domain.Entities.User
                {
                    UserId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    FullName = "Bob",
                    Email = "bob@example.com",
                    Role = roleUser
                },
                new Domain.Entities.User
                {
                    UserId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                    FullName = "Charlie",
                    Email = "charlie@example.com",
                    Role = roleUser
                },
                new Domain.Entities.User
                {
                    UserId = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                    FullName = "David",
                    Email = "david@example.com",
                    Role = roleAdmin
                }
            };
        }
        private IUserRepository _userRepository;
        private IMapper _mapper;
        private GetUsersQueryHandler _handler;
        private List<Domain.Entities.User> _testUsers;
        private IStringEncryptionService _stringEncryptionService;

        [Test]
        public async Task Handle_NoFilters_ReturnsAllUsersPaginated()
        {
            var query = new GetUsersQuery(null, null, null);

            var mockQueryable = _testUsers.BuildMock();
            _userRepository.GetUsersQueryable().Returns(mockQueryable);

            var result = await _handler.Handle(query, CancellationToken.None);

            result.Should().NotBeNull();
            result.Items.Should().HaveCount(4);
            result.PageNumber.Should().Be(1);
            result.TotalCount.Should().Be(4);
            _userRepository.Received(1).GetUsersQueryable();
        }

        [Test]
        public async Task Handle_WithSearchTerm_FiltersByFullName()
        {
            var query = new GetUsersQuery("Alice", null, null);

            var mockQueryable = _testUsers.BuildMock();
            _userRepository.GetUsersQueryable().Returns(mockQueryable);

            var result = await _handler.Handle(query, CancellationToken.None);

            result.Items.Should().HaveCount(1);
            result.Items[0].FullName.Should().Be("Alice");
            _userRepository.Received(1).GetUsersQueryable();
        }

        [Test]
        public async Task Handle_WithSearchTerm_FiltersByEmail()
        {
            var query = new GetUsersQuery("bob@", null, null);

            var mockQueryable = _testUsers.BuildMock();
            _userRepository.GetUsersQueryable().Returns(mockQueryable);

            var result = await _handler.Handle(query, CancellationToken.None);

            result.Items.Should().HaveCount(1);
            result.Items[0].Email.Should().Be("bob@example.com");
            _userRepository.Received(1).GetUsersQueryable();
        }

        [Test]
        public async Task Handle_WithSearchTerm_FiltersByRoleName()
        {
            var query = new GetUsersQuery("Admin", null, null);

            var mockQueryable = _testUsers.BuildMock();
            _userRepository.GetUsersQueryable().Returns(mockQueryable);

            var result = await _handler.Handle(query, CancellationToken.None);

            result.Items.Should().OnlyContain(u => u.Role.RoleName == "Admin");
            _userRepository.Received(1).GetUsersQueryable();
        }

        [Test]
        public async Task Handle_SortByFullName_Ascending_ReturnsSortedUsers()
        {
            var query = new GetUsersQuery(null, "FullName", "asc");

            var mockQueryable = _testUsers.BuildMock();
            _userRepository.GetUsersQueryable().Returns(mockQueryable);

            var result = await _handler.Handle(query, CancellationToken.None);

            var sorted = _testUsers.OrderBy(u => u.FullName).ToList();

            result.Items.Select(i => i.FullName).Should().ContainInOrder(sorted.Select(u => u.FullName));
            _userRepository.Received(1).GetUsersQueryable();
        }

        [Test]
        public async Task Handle_SortByFullName_Descending_ReturnsSortedUsers()
        {
            var query = new GetUsersQuery(null, "FullName", "desc");

            var mockQueryable = _testUsers.BuildMock();
            _userRepository.GetUsersQueryable().Returns(mockQueryable);

            var result = await _handler.Handle(query, CancellationToken.None);

            var sorted = _testUsers.OrderByDescending(u => u.FullName).ToList();

            result.Items.Select(i => i.FullName).Should().ContainInOrder(sorted.Select(u => u.FullName));
            _userRepository.Received(1).GetUsersQueryable();
        }

        [Test]
        public async Task Handle_InvalidSortBy_DefaultsToFullName()
        {
            var query = new GetUsersQuery(null, "InvalidField", "asc");

            var mockQueryable = _testUsers.BuildMock();
            _userRepository.GetUsersQueryable().Returns(mockQueryable);

            var result = await _handler.Handle(query, CancellationToken.None);

            result.Items.Should().NotBeEmpty();
            _userRepository.Received(1).GetUsersQueryable();
        }

        [Test]
        public async Task Handle_NoMatchingSearch_ReturnsEmptyList()
        {
            var query = new GetUsersQuery("Zoe", null, null);

            var mockQueryable = _testUsers.BuildMock();
            _userRepository.GetUsersQueryable().Returns(mockQueryable);

            var result = await _handler.Handle(query, CancellationToken.None);

            result.Items.Should().BeEmpty();
            result.TotalCount.Should().Be(0);
            _userRepository.Received(1).GetUsersQueryable();
        }

        [Test]
        public async Task Handle_WithCancellationToken_CompletesSuccessfully()
        {
            var query = new GetUsersQuery(null, null, null);
            var token = new CancellationToken();

            var mockQueryable = _testUsers.BuildMock();
            _userRepository.GetUsersQueryable().Returns(mockQueryable);

            var result = await _handler.Handle(query, token);

            result.Should().NotBeNull();
            _userRepository.Received(1).GetUsersQueryable();
        }
    }
}
