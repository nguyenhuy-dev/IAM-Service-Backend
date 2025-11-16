using Grpc.Core;
using IAMService.API.gRPC.Protos.UserProto;
using IAMService.API.gRPC.Services;
using IAMService.Application.Features.User.Queries.GetAllUsers;
using IAMService.Application.Interfaces;
using IAMService.Domain.Entities;
using MediatR;
using Moq;
namespace IAMService.API.Test.gRPC.Services
{
    [TestFixture]
    public class UserGrpcServiceTests
    {

        [SetUp]
        public void Setup()
        {
            _mockSender = new Mock<ISender>();
            _mockEncryptionService = new Mock<IStringEncryptionService>();
            _service = new UserGrpcService(_mockSender.Object, _mockEncryptionService.Object);
        }
        private Mock<ISender> _mockSender;
        private Mock<IStringEncryptionService> _mockEncryptionService;
        private UserGrpcService _service;

        [Test]
        public async Task GetAllUsers_ShouldReturnMappedUsers_WhenDataExists()
        {
            // Arrange - Service only uses: UserId, FullName (encrypted), Email (encrypted), IsActive
            var users = new List<User>
            {
                new User(
                    "encrypted_John_Doe",
                    "0123456789",
                    "encrypted_john@test.com",
                    "hash",
                    true,
                    "123456789012",
                    DateOnly.FromDateTime(DateTime.Now),
                    "addr",
                    1
                )
                {
                    UserId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                    IsActive = true
                },
                new User(
                    "encrypted_Jane_Smith",
                    "0987654321",
                    "encrypted_jane@test.com",
                    "hash",
                    false,
                    "987654321098",
                    DateOnly.FromDateTime(DateTime.Now),
                    "addr",
                    2,
                    true
                )
                {
                    UserId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                    IsActive = false
                }
            };

            _mockSender.Setup(s => s.Send(It.IsAny<GetAllUsersQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(users);

            _mockEncryptionService.Setup(e => e.DecryptString("encrypted_John_Doe"))
                .Returns("John Doe");
            _mockEncryptionService.Setup(e => e.DecryptString("encrypted_john@test.com"))
                .Returns("john@test.com");
            _mockEncryptionService.Setup(e => e.DecryptString("encrypted_Jane_Smith"))
                .Returns("Jane Smith");
            _mockEncryptionService.Setup(e => e.DecryptString("encrypted_jane@test.com"))
                .Returns("jane@test.com");

            var context = CreateFakeServerCallContext();

            // Act
            var result = await _service.GetAllUsers(new GetAllUsersRequest(), context);

            // Assert - Verify only 4 fields: UserId, FullName, Email, IsActive
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Users.Count, Is.EqualTo(2));

            var first = result.Users[0];
            Assert.Multiple(() =>
            {
                Assert.That(first.UserId, Is.EqualTo("11111111-1111-1111-1111-111111111111"));
                Assert.That(first.FullName, Is.EqualTo("John Doe"));
                Assert.That(first.Email, Is.EqualTo("john@test.com"));
                Assert.That(first.IsActive, Is.True);
            });

            var second = result.Users[1];
            Assert.Multiple(() =>
            {
                Assert.That(second.UserId, Is.EqualTo("22222222-2222-2222-2222-222222222222"));
                Assert.That(second.FullName, Is.EqualTo("Jane Smith"));
                Assert.That(second.Email, Is.EqualTo("jane@test.com"));
                Assert.That(second.IsActive, Is.False);
            });

            _mockSender.Verify(s => s.Send(It.IsAny<GetAllUsersQuery>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockEncryptionService.Verify(e => e.DecryptString(It.IsAny<string>()), Times.Exactly(4));
        }

        [Test]
        public async Task GetAllUsers_ShouldReturnEmptyList_WhenNoUsers()
        {
            // Arrange
            _mockSender.Setup(s => s.Send(It.IsAny<GetAllUsersQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<User>());

            var context = CreateFakeServerCallContext();

            // Act
            var result = await _service.GetAllUsers(new GetAllUsersRequest(), context);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Users, Is.Empty);

            _mockSender.Verify(s => s.Send(It.IsAny<GetAllUsersQuery>(), It.IsAny<CancellationToken>()), Times.Once);
            _mockEncryptionService.Verify(e => e.DecryptString(It.IsAny<string>()), Times.Never);
        }

        [Test]
        public async Task GetAllUsers_ShouldDecryptAllFields_WhenUsersHaveEncryptedData()
        {
            // Arrange - Test decrypt for FullName and Email only
            var user = new User(
                "enc_name",
                "0111111111",
                "enc@test.com",
                "hash",
                true,
                "111111111111",
                DateOnly.FromDateTime(DateTime.Now),
                "addr",
                1
            )
            {
                UserId = Guid.NewGuid(),
                IsActive = true
            };

            _mockSender.Setup(s => s.Send(It.IsAny<GetAllUsersQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<User> { user });

            _mockEncryptionService.Setup(e => e.DecryptString("enc_name"))
                .Returns("Decrypted Name");
            _mockEncryptionService.Setup(e => e.DecryptString("enc@test.com"))
                .Returns("decrypted@email.com");

            var context = CreateFakeServerCallContext();

            // Act
            var result = await _service.GetAllUsers(new GetAllUsersRequest(), context);

            // Assert - Verify FullName and Email are decrypted
            Assert.That(result.Users.Count, Is.EqualTo(1));
            Assert.That(result.Users[0].FullName, Is.EqualTo("Decrypted Name"));
            Assert.That(result.Users[0].Email, Is.EqualTo("decrypted@email.com"));

            _mockEncryptionService.Verify(e => e.DecryptString("enc_name"), Times.Once);
            _mockEncryptionService.Verify(e => e.DecryptString("enc@test.com"), Times.Once);
        }

        [Test]
        public async Task GetAllUsers_ShouldHandleAnyEncryptedValues_WhenUsersHaveEncryptedData()
        {
            // Arrange
            var user = new User(
                "AnyEncName",
                "0111111111",
                "any@enc.com",
                "hash",
                true,
                "111111111111",
                DateOnly.FromDateTime(DateTime.Now),
                "addr",
                1
            )
            {
                UserId = Guid.NewGuid(),
                IsActive = true
            };

            _mockSender.Setup(s => s.Send(It.IsAny<GetAllUsersQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<User> { user });

            _mockEncryptionService.Setup(e => e.DecryptString("AnyEncName"))
                .Returns("Decrypted Name");
            _mockEncryptionService.Setup(e => e.DecryptString("any@enc.com"))
                .Returns("decrypted@email.com");

            var context = CreateFakeServerCallContext();

            // Act
            var result = await _service.GetAllUsers(new GetAllUsersRequest(), context);

            // Assert
            Assert.That(result.Users.Count, Is.EqualTo(1));
            Assert.That(result.Users[0].FullName, Is.EqualTo("Decrypted Name"));
            Assert.That(result.Users[0].Email, Is.EqualTo("decrypted@email.com"));

            _mockEncryptionService.Verify(e => e.DecryptString("AnyEncName"), Times.Once);
            _mockEncryptionService.Verify(e => e.DecryptString("any@enc.com"), Times.Once);
        }

        [Test]
        public async Task GetAllUsers_ShouldUseNullCoalescing_WhenServiceHandlesValues()
        {
            // Arrange - Verify ?? string.Empty logic in service
            var userId = Guid.NewGuid();
            var user = new User(
                "Name",
                "0111111111",
                "test@test.com",
                "hash",
                true,
                "111111111111",
                DateOnly.FromDateTime(DateTime.Now),
                "addr",
                1
            )
            {
                UserId = userId,
                IsActive = true
            };

            _mockSender.Setup(s => s.Send(It.IsAny<GetAllUsersQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<User> { user });

            _mockEncryptionService.Setup(e => e.DecryptString("Name"))
                .Returns("Decrypted Name");
            _mockEncryptionService.Setup(e => e.DecryptString("test@test.com"))
                .Returns("decrypted@test.com");

            var context = CreateFakeServerCallContext();

            // Act
            var result = await _service.GetAllUsers(new GetAllUsersRequest(), context);

            // Assert - Verify 4 mapped fields
            Assert.That(result.Users.Count, Is.EqualTo(1));
            Assert.That(result.Users[0].UserId, Is.EqualTo(userId.ToString()));
            Assert.That(result.Users[0].FullName, Is.EqualTo("Decrypted Name"));
            Assert.That(result.Users[0].Email, Is.EqualTo("decrypted@test.com"));

            _mockEncryptionService.Verify(e => e.DecryptString("Name"), Times.Once);
            _mockEncryptionService.Verify(e => e.DecryptString("test@test.com"), Times.Once);
            _mockEncryptionService.Verify(e => e.DecryptString(string.Empty), Times.Never);
        }
        [Test]
        public async Task GetAllUsers_ShouldRespectCancellationToken()
        {
            // Arrange
            var cts = new CancellationTokenSource();
            cts.Cancel();

            _mockSender.Setup(s => s.Send(It.IsAny<GetAllUsersQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new OperationCanceledException());

            var context = CreateFakeServerCallContext(cts.Token);

            // Act & Assert
            Assert.ThrowsAsync<OperationCanceledException>(async () => await _service.GetAllUsers(new GetAllUsersRequest(), context));

            _mockSender.Verify(s => s.Send(It.IsAny<GetAllUsersQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        // --- Helper để giả lập gRPC Context ---
        private static ServerCallContext CreateFakeServerCallContext(CancellationToken cancellationToken = default)
        {
            return TestServerCallContext.Create(
                "GetAllUsers",
                default!,
                DateTime.UtcNow.AddMinutes(1),
                new Metadata(),
                cancellationToken,
                "localhost",
                default!,
                default!,
                _ => Task.CompletedTask,
                _ => Task.CompletedTask,
                default!
            );
        }
    }

    // --- Fake gRPC context ---
    internal class TestServerCallContextUser : ServerCallContext
    {
        private readonly Func<Metadata, Task> _writeHeadersFunc;

        private TestServerCallContextUser(
            string method,
            string host,
            DateTime deadline,
            Metadata requestHeaders,
            CancellationToken cancellationToken,
            string peer,
            AuthContext authContext,
            ContextPropagationToken contextPropagationToken,
            Func<Metadata, Task> writeHeadersFunc,
            Func<Metadata, Task> writeTrailersFunc,
            WriteOptions writeOptions)
        {
            MethodCore = method;
            PeerCore = peer;
            DeadlineCore = deadline;
            RequestHeadersCore = requestHeaders;
            CancellationTokenCore = cancellationToken;
            AuthContextCore = authContext;
            _writeHeadersFunc = writeHeadersFunc;
            WriteOptionsCore = writeOptions;
        }

        protected override string MethodCore { get; }
        protected override string HostCore
        {
            get => "localhost";
        }
        protected override string PeerCore { get; }
        protected override DateTime DeadlineCore { get; }
        protected override Metadata RequestHeadersCore { get; }
        protected override CancellationToken CancellationTokenCore { get; }
        protected override Metadata ResponseTrailersCore { get; } = new Metadata();
        protected override Status StatusCore { get; set; }
        protected override WriteOptions? WriteOptionsCore { get; set; }
        protected override AuthContext AuthContextCore { get; }

        public static ServerCallContext Create(
            string method,
            string host,
            DateTime deadline,
            Metadata requestHeaders,
            CancellationToken cancellationToken,
            string peer,
            AuthContext authContext,
            ContextPropagationToken contextPropagationToken,
            Func<Metadata, Task> writeHeadersFunc,
            Func<Metadata, Task> writeTrailersFunc,
            WriteOptions writeOptions)
        {
            return new TestServerCallContextUser(method, host, deadline, requestHeaders, cancellationToken, peer,
                authContext, contextPropagationToken, writeHeadersFunc, writeTrailersFunc, writeOptions);
        }
        protected override ContextPropagationToken CreatePropagationTokenCore(ContextPropagationOptions? options)
        {
            return default!;
        }
        protected override Task WriteResponseHeadersAsyncCore(Metadata responseHeaders)
        {
            return _writeHeadersFunc(responseHeaders);
        }
    }
}
