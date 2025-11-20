using IAMService.Domain.Enums;
namespace IAMService.Domain.Services
{
    public static class PrivilegeEnforcer
    {
        // The Key is the PARENT (Required). 
        // The Value is the list of CHILDREN that trigger the requirement.
        private static readonly Dictionary<PrivilegeEnum, PrivilegeEnum[]> _dependencies = new Dictionary<PrivilegeEnum, PrivilegeEnum[]>
        {
            {
                PrivilegeEnum.ViewConfiguration,
                [PrivilegeEnum.CreateConfiguration, PrivilegeEnum.ModifyConfiguration, PrivilegeEnum.DeleteConfiguration]
            },
            {
                PrivilegeEnum.ViewUser,
                [PrivilegeEnum.CreateUser, PrivilegeEnum.ModifyUser, PrivilegeEnum.DeleteUser, PrivilegeEnum.LockAndUnlockUser]
            },
            {
                PrivilegeEnum.ViewRole,
                [PrivilegeEnum.CreateRole, PrivilegeEnum.UpdateRole, PrivilegeEnum.DeleteRole]
            },
            {
                PrivilegeEnum.ViewInstrument,
                [PrivilegeEnum.AddInstrument, PrivilegeEnum.ActivateOrDeactivateInstrument]
            }
        };

        public static void EnsureDependencies(List<int> privilegeIds)
        {
            // Iterate through every rule defined above
            foreach (var rule in _dependencies)
            {
                var parentId = (int)rule.Key;
                var childIds = rule.Value.Select(p => (int)p).ToArray();

                // LOGIC: If the list has any of the children, but is missing the parent...
                var hasChild = childIds.Any(privilegeIds.Contains);
                var missingParent = !privilegeIds.Contains(parentId);

                if (hasChild && missingParent)
                {
                    // ... Add the parent automatically.
                    privilegeIds.Add(parentId);
                }
            }
        }
    }
}
