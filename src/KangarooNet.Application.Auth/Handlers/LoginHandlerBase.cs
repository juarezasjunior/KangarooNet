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

    public abstract class LoginHandlerBase<TApplicationUser, TLoginRequest, TLoginResponse>
        : IRequestHandler<TLoginRequest, TLoginResponse>
        where TApplicationUser : IdentityUser<Guid>, IApplicationUser, new()
        where TLoginRequest : ILoginRequest, IRequest<TLoginResponse>
        where TLoginResponse : ILoginResponse, new()
    {
        private readonly UserManager<TApplicationUser> userManager;
        private readonly IMediator mediator;

        public LoginHandlerBase(
            UserManager<TApplicationUser> userManager,
            IMediator mediator)
        {
            this.userManager = userManager;
            this.mediator = mediator;
        }

        public async Task<TLoginResponse> Handle(TLoginRequest request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var applicationUser = (await this.mediator.Send(new LoginValidatorRequest<TApplicationUser, TLoginRequest>() { LoginRequest = request })).ApplicationUser;

            var result = await this.mediator.Send(new GenerateTokenRequest<TApplicationUser>()
            {
                ApplicationUser = applicationUser,
            });

            return new TLoginResponse()
            {
                Token = result.Token,
                RefreshToken = result.RefreshToken,
            };
        }
    }
}
