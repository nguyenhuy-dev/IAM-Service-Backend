using IAMService.Domain.Entities;
namespace IAMService.Application.Interfaces
{
    /// <summary>
    /// The role clone service interface.
    /// </summary>
    public interface IRoleCloneService
    {
        /// <summary>
        /// Clones an existing user's role and assigns a customized set of privileges.
        /// </summary>
        /// <param name="user">The user whose role will be cloned.</param>
        /// <param name="privilegeIds">The identifiers of privileges to associate with the cloned role.</param>
        /// <param name="cancellationToken">The cancellation token to observe during the operation.</param>
        /// <returns>
        /// The newly created role that contains the customized set of privileges.
        /// </returns>
        Task<Role> CloneRoleWithPrivilegesAsync(User user, List<int> privilegeIds, CancellationToken cancellationToken);
    }
}
