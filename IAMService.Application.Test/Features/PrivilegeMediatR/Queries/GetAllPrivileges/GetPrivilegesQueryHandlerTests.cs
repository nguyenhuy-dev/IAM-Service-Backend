using IAMService.Application.Features.PrivilegeMediatR.Queries.GetAllPrivileges;
using IAMService.Application.Interfaces;
using IAMService.Domain.Entities;
using Moq;
namespace IAMService.Application.Test.Features.PrivilegeMediatR.Queries.GetAllPrivileges
{
    [TestFixture]
    public class GetPrivilegesQueryHandlerTests
    {

        [SetUp]
        public void SetUp()
        {
            _privilegeRepositoryMock = new Mock<IPrivilegeRepository>();
            _handler = new GetPrivilegesQueryHandler(_privilegeRepositoryMock.Object);
            _cancellationToken = CancellationToken.None;
        }
        private Mock<IPrivilegeRepository> _privilegeRepositoryMock;
        private GetPrivilegesQueryHandler _handler;
        private CancellationToken _cancellationToken;

        [Test]
        public async Task Handle_ShouldReturnPrivilegesList_WhenRepositoryReturnsData()
        {
            // Arrange
            var privileges = new List<Privilege>
            {
                new Privilege
                {
                    PrivilegeId = 1,
                    PrivilegeName = "View Users",
                    Roles = new List<Domain.Entities.Role>
                    {
                        new Domain.Entities.Role(1, "Admin", "ADM", "Administrator role")
                    }
                },
                new Privilege
                {
                    PrivilegeId = 2,
                    PrivilegeName = "Edit Roles",
                    Roles = new List<Domain.Entities.Role>
                    {
                        new Domain.Entities.Role(2, "Manager", "MNG", "Manager role")
                    }
                }
            };

            _privilegeRepositoryMock
                .Setup(repo => repo.GetPrivilegesIncludeRolesAsync(_cancellationToken))
                .ReturnsAsync(privileges);

            var query = new GetPrivilegesQuery();

            // Act
            var result = await _handler.Handle(query, _cancellationToken);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result.First().PrivilegeName, Is.EqualTo("View Users"));
            _privilegeRepositoryMock.Verify(repo => repo.GetPrivilegesIncludeRolesAsync(_cancellationToken), Times.Once);
        }

        [Test]
        public async Task Handle_ShouldReturnEmptyList_WhenRepositoryReturnsEmpty()
        {
            // Arrange
            var privileges = new List<Privilege>();
            _privilegeRepositoryMock
                .Setup(repo => repo.GetPrivilegesIncludeRolesAsync(_cancellationToken))
                .ReturnsAsync(privileges);

            var query = new GetPrivilegesQuery();

            // Act
            var result = await _handler.Handle(query, _cancellationToken);

            // Assert
            Assert.That(result, Is.Empty);
            _privilegeRepositoryMock.Verify(repo => repo.GetPrivilegesIncludeRolesAsync(_cancellationToken), Times.Once);
        }

        [Test]
        public async Task Handle_ShouldCallRepositoryWithCorrectCancellationToken()
        {
            // Arrange
            var privileges = new List<Privilege>
            {
                new Privilege { PrivilegeId = 1, PrivilegeName = "Test Privilege" }
            };

            var token = new CancellationTokenSource().Token;

            _privilegeRepositoryMock
                .Setup(repo => repo.GetPrivilegesIncludeRolesAsync(token))
                .ReturnsAsync(privileges);

            var query = new GetPrivilegesQuery();

            // Act
            var result = await _handler.Handle(query, token);

            // Assert
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].PrivilegeName, Is.EqualTo("Test Privilege"));
            _privilegeRepositoryMock.Verify(repo => repo.GetPrivilegesIncludeRolesAsync(token), Times.Once);
        }
    }
}
