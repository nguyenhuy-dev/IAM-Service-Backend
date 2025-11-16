using IAMService.Application.Interfaces.Messaging;
using IAMService.Domain.Entities;
namespace IAMService.Application.Features.PrivilegeMediatR.Queries.GetAllPrivileges
{
    /// <summary>
    ///     Get Privileges Query.
    /// </summary>
    /// <seealso
    ///     cref="IAMService.Application.Interfaces.Messaging.IQuery&lt;System.Collections.Generic.List&lt;IAMService.Domain.Entities.Privilege&gt;&gt;" />
    /// <seealso cref="MediatR.IRequest&lt;System.Collections.Generic.List&lt;IAMService.Domain.Entities.Privilege&gt;&gt;" />
    /// <seealso cref="MediatR.IBaseRequest" />
    /// <seealso
    ///     cref="System.IEquatable&lt;IAMService.Application.Features.PrivilegeMediatR.Queries.GetAllPrivileges.GetPrivilegesQuery&gt;" />
    public sealed record GetPrivilegesQuery : IQuery<List<Privilege>>;
}
