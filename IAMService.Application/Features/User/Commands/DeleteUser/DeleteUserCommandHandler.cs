using IAMService.Application.Exceptions;
using IAMService.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace IAMService.Application.Features.User.Commands.DeleteUser
{
    public class DeleteUserCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ILogger<DeleteUserCommandHandler> logger
    ) : IRequestHandler<DeleteUserCommand, bool>
    {
        public async Task<bool> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            
            var user = await userRepository.GetByIdAsync(request.UserId);

            if (user == null)
                throw new NotFoundException("UserId", request.UserId);

            
            //if (user.Role != null && user.Role.RoleName == "Patient")
            //{
            //    throw new ForbiddenAccessException("Cannot delete patient account.");
            //}

            
            userRepository.Delete(user);
            await unitOfWork.SaveChangesAsync(cancellationToken);

          
            logger.LogInformation("User {UserId} was permanently deleted by manager at {Time}",
                request.UserId, DateTime.UtcNow);

           
            return true;
        }
    }
}
