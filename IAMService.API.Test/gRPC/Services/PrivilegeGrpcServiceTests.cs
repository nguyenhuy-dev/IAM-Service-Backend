using Grpc.Core;
using IAMService.API.gRPC.Protos;
using IAMService.API.gRPC.Services;
using IAMService.Application.Features.PrivilegeMediatR.Queries.GetAllPrivileges;
using IAMService.Domain.Entities;
using MediatR;
using Moq;
using Privilege=IAMService.Domain.Entities.Privilege;

namespace IAMService.API.Test.gRPC.Services
{
    [TestFixture]
    public class PrivilegeGrpcServiceTests
    {

        [SetUp]
        public void Setup()
        {
            _mockSender = new Mock<ISender>();
            _service = new PrivilegeGrpcService(_mockSender.Object);
        }
        private Mock<ISender> _mockSender;
        private PrivilegeGrpcService _service;

        [Test]
        public async Task GetAllPrivileges_ShouldReturnMappedPrivileges_WhenDataExists()
        {
            // Arrange
            var privileges = new List<Privilege>
            {
                new Privilege("ManageUsers")
                {
                    PrivilegeId = 1,
                    Roles = new List<Role>
                    {
                        new Role(1, "Admin", "ADMIN", "Admin Role"),
                        new Role(2, "Supervisor", "SUPV", "Supervisor Role")
                    }
                },
                new Privilege("ViewReports")
                {
                    PrivilegeId = 2,
                    Roles = new List<Role>
                    {
                        new Role(3, "User", "USER", "User Role")
                    }
                }
            };

            _mockSender.Setup(s => s.Send(It.IsAny<GetPrivilegesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(privileges);

            var context = CreateFakeServerCallContext();

            // Act
            var result = await _service.GetAllPrivileges(new Empty(), context);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Privileges.Count, Is.EqualTo(2));

            var first = result.Privileges[0];
            Assert.Multiple(() =>
            {
                Assert.That(first.PrivilegeId, Is.EqualTo(1));
                Assert.That(first.PrivilegeName, Is.EqualTo("ManageUsers"));
                Assert.That(first.Roles, Has.Count.EqualTo(2));
            });
            Assert.Multiple(() =>
            {
                Assert.That(first.Roles[0].RoleId, Is.EqualTo(1));
                Assert.That(first.Roles[0].RoleCode, Is.EqualTo("ADMIN"));
                Assert.That(first.Roles[1].RoleCode, Is.EqualTo("SUPV"));
            });

            var second = result.Privileges[^1];
            Assert.Multiple(() =>
            {
                Assert.That(second.PrivilegeId, Is.EqualTo(2));
                Assert.That(second.PrivilegeName, Is.EqualTo("ViewReports"));
                Assert.That(second.Roles.Single().RoleCode, Is.EqualTo("USER"));
            });

            _mockSender.Verify(s => s.Send(It.IsAny<GetPrivilegesQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Test]
        public async Task GetAllPrivileges_ShouldReturnEmptyList_WhenNoPrivileges()
        {
            // Arrange
            _mockSender.Setup(s => s.Send(It.IsAny<GetPrivilegesQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Privilege>());

            var context = CreateFakeServerCallContext();

            // Act
            var result = await _service.GetAllPrivileges(new Empty(), context);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Privileges, Is.Empty);

            _mockSender.Verify(s => s.Send(It.IsAny<GetPrivilegesQuery>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        // --- Helper để giả lập gRPC Context ---
        private static ServerCallContext CreateFakeServerCallContext()
        {
            return TestServerCallContext.Create(
                "GetAllPrivileges",
                default!,
                DateTime.UtcNow.AddMinutes(1),
                new Metadata(),
                CancellationToken.None,
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
    internal class TestServerCallContext : ServerCallContext
    {
        private readonly Func<Metadata, Task> _writeHeadersFunc;

        private TestServerCallContext(
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
            return new TestServerCallContext(method, host, deadline, requestHeaders, cancellationToken, peer,
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
