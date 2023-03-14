// Copyright Contributors to the KangarooNet project.
// This file is licensed to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICE files in the project root for full license information.

namespace KangarooNet.Application.Auth.Handlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using KangarooNet.Domain;
    using KangarooNet.Domain.Entities;
    using KangarooNet.Domain.Entities.Auth;
    using KangarooNet.Domain.Exceptions;
    using MediatR;
    using Microsoft.AspNetCore.Identity;

    public abstract class LoginValidatorHandlerBase<TApplicationUser, TLoginRequest>
        : IRequestHandler<LoginValidatorRequest<TApplicationUser, TLoginRequest>, LoginValidatorResponse<TApplicationUser>>
        where TApplicationUser : IdentityUser<Guid>, IApplicationUser, new()
        where TLoginRequest : ILoginRequest
    {
        private readonly UserManager<TApplicationUser> userManager;

        public LoginValidatorHandlerBase(UserManager<TApplicationUser> userManager)
        {
            this.userManager = userManager;
        }

        public async Task<LoginValidatorResponse<TApplicationUser>> Handle(LoginValidatorRequest<TApplicationUser, TLoginRequest> request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var applicationUser = await this.userManager.FindByEmailAsync(request.LoginRequest.Email);

            if (applicationUser == null)
            {
                throw new KangarooNetSecurityException();
            }

            var isPasswordValid = await this.userManager.CheckPasswordAsync(applicationUser, request.LoginRequest.Password);

            if (!isPasswordValid)
            {
                throw new KangarooNetSecurityException();
            }

            return new LoginValidatorResponse<TApplicationUser>()
            {
                ApplicationUser = applicationUser,
            };
        }
    }
}
