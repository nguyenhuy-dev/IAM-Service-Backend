using IAMService.API.Middleware.Authorization;
using IAMService.Application.Exceptions;
using IAMService.Application.Interfaces.AuthenticationServices;
using IAMService.Domain.Entities;
using IAMService.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Moq;
namespace IAMService.API.Test
{
    [TestFixture]
    public class DynamicAuthorizationPolicyProviderTests
    {

        [SetUp]
        public void SetUp()
        {
            _authOptions = new AuthorizationOptions();
            _optionsMock = new Mock<IOptions<AuthorizationOptions>>();
            _optionsMock.Setup(o => o.Value).Returns(_authOptions);

            _scopeFactoryMock = new Mock<IServiceScopeFactory>();
            _scopeMock = new Mock<IServiceScope>();
            _serviceProviderMock = new Mock<IServiceProvider>();
            _cacheServiceMock = new Mock<IAuthorizationCacheService>();

            _scopeMock.Setup(s => s.ServiceProvider).Returns(_serviceProviderMock.Object);
            _scopeFactoryMock.Setup(f => f.CreateScope()).Returns(_scopeMock.Object);

            // InMemory database
            _dbOptions = new DbContextOptionsBuilder<IAMServiceDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            _dbContext = new IAMServiceDbContext(_dbOptions);

            // Register fake services
            _serviceProviderMock
                .Setup(p => p.GetService(typeof(IAuthorizationCacheService)))
                .Returns(_cacheServiceMock.Object);
            _serviceProviderMock
                .Setup(p => p.GetService(typeof(IAMServiceDbContext)))
                .Returns(_dbContext);

            _provider = new DynamicAuthorizationPolicyProvider(_optionsMock.Object, _scopeFactoryMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _dbContext.Database.EnsureDeleted();
            _dbContext.Dispose();
        }
        private Mock<IOptions<AuthorizationOptions>> _optionsMock = null!;
        private AuthorizationOptions _authOptions = null!;
        private Mock<IServiceScopeFactory> _scopeFactoryMock = null!;
        private Mock<IServiceScope> _scopeMock = null!;
        private Mock<IServiceProvider> _serviceProviderMock = null!;
        private Mock<IAuthorizationCacheService> _cacheServiceMock = null!;
        private DbContextOptions<IAMServiceDbContext> _dbOptions = null!;
        private IAMServiceDbContext _dbContext = null!;
        private DynamicAuthorizationPolicyProvider _provider = null!;

        [Test]
        public async Task GetPolicyAsync_ReturnsExistingPolicy_FromOptions()
        {
            // Arrange
            var existingPolicy = new AuthorizationPolicyBuilder().RequireRole("Admin").Build();
            _authOptions.AddPolicy("ExistingPolicy", existingPolicy);

            // Act
            var result = await _provider.GetPolicyAsync("ExistingPolicy");

            // Assert
            Assert.That(result, Is.EqualTo(existingPolicy));
        }

        [Test]
        public async Task GetPolicyAsync_ReturnsCachedPolicy_WhenFoundInCache()
        {
            var cachedPolicy = new AuthorizationPolicyBuilder().RequireRole("User").Build();
            var outPolicy = cachedPolicy;

            _cacheServiceMock.Setup(c => c.TryGetPolicy("CachedPolicy", out outPolicy))
                .Returns(true);

            // Act
            var result = await _provider.GetPolicyAsync("CachedPolicy");

            // Assert
            Assert.That(result, Is.EqualTo(cachedPolicy));
        }

        [Test]
        public void GetPolicyAsync_Throws_WhenPolicyNameIsEmpty()
        {
            AuthorizationPolicy? dummy = null;
            _cacheServiceMock.Setup(c => c.TryGetPolicy(It.IsAny<string>(), out dummy))
                .Returns(false);

            Assert.ThrowsAsync<ForbiddenAccessException>(async () => await _provider.GetPolicyAsync(""));
        }

        [Test]
        public void GetPolicyAsync_Throws_WhenPrivilegeNotFound()
        {
            AuthorizationPolicy? dummy = null;
            _cacheServiceMock.Setup(c => c.TryGetPolicy(It.IsAny<string>(), out dummy))
                .Returns(false);

            Assert.ThrowsAsync<ForbiddenAccessException>(async () => await _provider.GetPolicyAsync("NotExists"));
        }

        [Test]
        public async Task GetPolicyAsync_CreatesPolicy_WhenPrivilegeExists()
        {
            // Arrange
            var role = new Role(1, "Admin", "ADM", "Administrator");
            var privilege = new Privilege("ManageUser") { Roles = new List<Role> { role } };
            _dbContext.Privileges.Add(privilege);
            await _dbContext.SaveChangesAsync();

            AuthorizationPolicy? dummy = null;
            _cacheServiceMock.Setup(c => c.TryGetPolicy(It.IsAny<string>(), out dummy))
                .Returns(false);

            var cacheSetCalled = false;
            _cacheServiceMock.Setup(c =>
                    c.SetPolicy(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<TimeSpan?>()))
                .Callback(() => cacheSetCalled = true);

            // Act
            var result = await _provider.GetPolicyAsync("ManageUser");

            Assert.Multiple(() =>
            {
                // Assert
                Assert.That(result, Is.Not.Null);
                Assert.That(cacheSetCalled, Is.True);
            });
        }

        [Test]
        public void GetPolicyAsync_Throws_WhenPolicyBuildFails()
        {
            // Arrange
            var role = new Role(1, "Admin", "ADM", "Administrator");
            var privilege = new Privilege("Crash") { Roles = new List<Role> { role } };
            _dbContext.Privileges.Add(privilege);
            _dbContext.SaveChanges();

            AuthorizationPolicy? dummy = null;
            _cacheServiceMock.Setup(c => c.TryGetPolicy(It.IsAny<string>(), out dummy))
                .Returns(false);

            // Dùng fake provider giả lập lỗi trong quá trình build
            var provider = new FakeFailingPolicyProvider(_optionsMock.Object, _scopeFactoryMock.Object);

            // Act & Assert
            var ex = Assert.ThrowsAsync<ForbiddenAccessException>(async () =>
                await provider.GetPolicyAsync("Crash"));

            Assert.That(ex!.Message, Is.EqualTo("Failed to create authorization policy."));
        }

        [Test]
        public async Task GetPolicyAsync_CachesPolicy_WithExpiration()
        {
            // Arrange
            var role = new Role(1, "User", "USR", "Normal user");
            var privilege = new Privilege("ReadData") { Roles = new List<Role> { role } };
            _dbContext.Privileges.Add(privilege);
            await _dbContext.SaveChangesAsync();

            AuthorizationPolicy? dummy = null;
            _cacheServiceMock.Setup(c => c.TryGetPolicy(It.IsAny<string>(), out dummy))
                .Returns(false);

            TimeSpan? duration = null;
            _cacheServiceMock.Setup(c =>
                    c.SetPolicy(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<TimeSpan?>()))
                .Callback<string, object, TimeSpan?>((_, _, span) => duration = span);

            // Act
            var policy = await _provider.GetPolicyAsync("ReadData");

            // Assert
            Assert.That(policy, Is.Not.Null);
            Assert.That(duration, Is.EqualTo(TimeSpan.FromMinutes(1)));
        }
    }

    public class FakeFailingPolicyProvider : DynamicAuthorizationPolicyProvider
    {
        public FakeFailingPolicyProvider(IOptions<AuthorizationOptions> options, IServiceScopeFactory scopeFactory)
            : base(options, scopeFactory)
        {
        }

        public override Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
        {
            throw new ForbiddenAccessException("Failed to create authorization policy.");
        }
    }
}
