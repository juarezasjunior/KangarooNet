// Copyright Contributors to the KangarooNet project.
// This file is licensed to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICE files in the project root for full license information.

namespace KangarooNet.Application.Auth.Handlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using KangarooNet.Application.Services;
    using KangarooNet.Domain;
    using KangarooNet.Domain.Entities;
    using KangarooNet.Domain.Entities.Auth;
    using KangarooNet.Domain.Exceptions;
    using MediatR;
    using Microsoft.AspNetCore.Identity;

    public abstract class ChangePasswordHandlerBase<TApplicationUser, TChangePasswordRequest, TChangePasswordResponse>
        : IRequestHandler<TChangePasswordRequest, TChangePasswordResponse>
        where TApplicationUser : IdentityUser<Guid>, IApplicationUser, new()
        where TChangePasswordRequest : IChangePasswordRequest, IRequest<TChangePasswordResponse>
        where TChangePasswordResponse : IChangePasswordResponse, new()
    {
        private readonly UserManager<TApplicationUser> userManager;
        private readonly ICurrentUserService currentUserService;

        public ChangePasswordHandlerBase(
            UserManager<TApplicationUser> userManager,
            ICurrentUserService currentUserService)
        {
            this.userManager = userManager;
            this.currentUserService = currentUserService;
        }

        public async Task<TChangePasswordResponse> Handle(TChangePasswordRequest request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var currentUserId = this.currentUserService.CurrentUserId;
            var applicationUser = await this.userManager.FindByIdAsync(currentUserId.ToString());

            if (applicationUser == null)
            {
                throw new KangarooNetSecurityException();
            }

            var result = await this.userManager.ChangePasswordAsync(
                applicationUser,
                request.CurrentPassword,
                request.NewPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(x => x.Description));
                throw new KangarooNetException(internalErrorCode: KangarooNetErrorCode.InvalidPassword, additionalInfo: errors);
            }

            return new TChangePasswordResponse();
        }
    }
}
