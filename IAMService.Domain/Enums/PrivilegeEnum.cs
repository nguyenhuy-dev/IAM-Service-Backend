namespace IAMService.Domain.Enums
{
    public enum PrivilegeEnum
    {
        ReadOnly = 1, CreateTestOrder = 2, ModifyTestOrder = 3,
        DeleteTestOrder = 4, ReviewTestOrder = 5, AddComment = 6,
        ModifyComment = 7, DeleteComment = 8, ViewConfiguration = 9,
        CreateConfiguration = 10, ModifyConfiguration = 11, DeleteConfiguration = 12,
        ViewUser = 13, CreateUser = 14, ModifyUser = 15,
        DeleteUser = 16, LockAndUnlockUser = 17, ViewRole = 18,
        CreateRole = 19, UpdateRole = 20, DeleteRole = 21,
        ViewEventLogs = 22, AddReagents = 23, ModifyReagents = 24,
        DeleteReagents = 25, AddInstrument = 26, ViewInstrument = 27,
        ActivateOrDeactivateInstrument = 28, ExecuteBloodTesting = 29
    }
}
