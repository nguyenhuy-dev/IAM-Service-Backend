using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IAMService.Application.Features.ResetPassword.Commands
{
    public class ResetPasswordCommand : IRequest<bool>
    {
        public Guid UserId { get; set; }
        public string Token { get; set; }
        public string NewPassword { get; set; }
    }
}
