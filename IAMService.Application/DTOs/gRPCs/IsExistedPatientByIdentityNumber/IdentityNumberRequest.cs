namespace IAMService.Application.DTOs.gRPCs.IsExistedPatientByIdentityNumber
{
    /// <summary>
    ///     Identity Number Request supports for data tranfer object with gRPC server from Patient Service.
    /// </summary>
    /// <seealso
    ///     cref="System.IEquatable&lt;IAMService.Application.DTOs.gRPCs.IsExistedPatientByIdentityNumber.IdentityNumberRequest&gt;" />
    public sealed record IdentityNumberRequest(string IdentityNumber);
}
