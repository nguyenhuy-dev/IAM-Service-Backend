using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediatR;

namespace IAMService.Application.Features.User.Commands.DeleteUser
{
    public record DeleteUserCommand(Guid UserId) : IRequest<bool>;
}