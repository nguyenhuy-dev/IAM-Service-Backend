using FluentValidation;
using IAMService.Application.Interfaces;
namespace IAMService.Application.Features.Role.Validators
{
    /// <summary>
    ///     Provides reusable FluentValidation rules for role-related validations.
    ///     These extension methods can be used across different validators (Create, Update, etc.)
    ///     to ensure consistent validation logic without code duplication.
    /// </summary>
    public static class RoleValidationRules
    {
        /// <summary>
        ///     Validates that a role code is unique in the system.
        ///     For update operations, it allows the same code if it belongs to the role being updated.
        /// </summary>
        /// <typeparam name="T">The type of the command or model being validated</typeparam>
        /// <param name="ruleBuilder">The FluentValidation rule builder</param>
        /// <param name="roleRepository">Repository for accessing role data</param>
        /// <param name="getRoleId">Optional function to extract the role ID from the context (used for updates to skip self-check)</param>
        /// <returns>Rule builder options for further chaining</returns>
        /// <example>
        ///     Usage in Create: <code>RuleFor(x => x.RoleCode).MustBeUniqueRoleCode(roleRepository)</code>
        ///     Usage in Update: <code>RuleFor(x => x.RoleCode).MustBeUniqueRoleCode(roleRepository, x => x.RoleId)</code>
        /// </example>
        public static IRuleBuilderOptions<T, string> MustBeUniqueRoleCode<T>(
            this IRuleBuilder<T, string> ruleBuilder,
            IRoleRepository roleRepository,
            Func<T, int?>? getRoleId = null)
        {
            return ruleBuilder.MustAsync(async (context, roleCode, _) =>
                {
                    // For create operations (no roleId), simply check if code exists
                    var currentRoleId = getRoleId?.Invoke(context);
                    if (!currentRoleId.HasValue)
                        return !await roleRepository.ExistsByCodeAsync(roleCode);

                    // For update operations, allow the same code if it belongs to the current role
                    var existingRole = await roleRepository.GetByIdAsync(currentRoleId.Value);
                    if (existingRole?.RoleCode == roleCode)
                        return true; // Same code, no conflict

                    // Check if another role has this code
                    return !await roleRepository.ExistsByCodeAsync(roleCode);
                })
                .WithMessage("Role with code '{PropertyValue}' already exists.");
        }

        /// <summary>
        ///     Validates that a role name is unique in the system.
        ///     For update operations, it allows the same name if it belongs to the role being updated.
        /// </summary>
        /// <typeparam name="T">The type of the command or model being validated</typeparam>
        /// <param name="ruleBuilder">The FluentValidation rule builder</param>
        /// <param name="roleRepository">Repository for accessing role data</param>
        /// <param name="getRoleId">Optional function to extract the role ID from the context (used for updates to skip self-check)</param>
        /// <returns>Rule builder options for further chaining</returns>
        /// <example>
        ///     Usage in Create: <code>RuleFor(x => x.RoleName).MustBeUniqueRoleName(roleRepository)</code>
        ///     Usage in Update: <code>RuleFor(x => x.RoleName).MustBeUniqueRoleName(roleRepository, x => x.RoleId)</code>
        /// </example>
        public static IRuleBuilderOptions<T, string> MustBeUniqueRoleName<T>(
            this IRuleBuilder<T, string> ruleBuilder,
            IRoleRepository roleRepository,
            Func<T, int?>? getRoleId = null)
        {
            return ruleBuilder.MustAsync(async (context, roleName, _) =>
                {
                    // For create operations (no roleId), simply check if name exists
                    var currentRoleId = getRoleId?.Invoke(context);
                    if (!currentRoleId.HasValue)
                        return !await roleRepository.ExistsByNameAsync(roleName);

                    // For update operations, allow the same name if it belongs to the current role
                    var existingRole = await roleRepository.GetByIdAsync(currentRoleId.Value);
                    if (existingRole?.RoleName == roleName)
                        return true; // Same name, no conflict

                    // Check if another role has this name
                    return !await roleRepository.ExistsByNameAsync(roleName);
                })
                .WithMessage("Role with name '{PropertyValue}' already exists.");
        }

        /// <summary>
        ///     Validates that all provided privilege IDs exist in the system.
        ///     Allows null or empty collections (validation passes).
        /// </summary>
        /// <typeparam name="T">The type of the command or model being validated</typeparam>
        /// <param name="ruleBuilder">The FluentValidation rule builder</param>
        /// <param name="privilegeRepository">Repository for accessing privilege data</param>
        /// <returns>Rule builder options for further chaining</returns>
        /// <remarks>
        ///     This validation only checks existence, not whether privileges are appropriate for the role.
        ///     Use with .When() clause to skip validation when privilege list is empty.
        /// </remarks>
        /// <example>
        ///     <code>RuleFor(x => x.PrivilegeIds)
        ///     .MustHaveValidPrivileges(privilegeRepository)
        ///     .When(x => x.PrivilegeIds?.Any() == true);</code>
        /// </example>
        public static IRuleBuilderOptions<T, IEnumerable<int>?> MustHaveValidPrivileges<T>(
            this IRuleBuilder<T, IEnumerable<int>?> ruleBuilder,
            IPrivilegeRepository privilegeRepository)
        {
            return ruleBuilder.MustAsync(async (privilegeIds, _) =>
                {
                    // Allow null or empty collections
                    if (privilegeIds == null || !privilegeIds.Any())
                        return true;

                    // Verify all provided privilege IDs exist
                    var ids = privilegeIds.ToList();
                    return await privilegeRepository.AllExistAsync(ids);
                })
                .WithMessage("One or more privilege IDs are invalid.");
        }
    }
}
