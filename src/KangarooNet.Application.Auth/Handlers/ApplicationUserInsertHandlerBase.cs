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

    public abstract class ApplicationUserInsertHandlerBase<TApplicationUser, TApplicationUserInsertRequest, TApplicationUserInsertResponse>
        : IRequestHandler<TApplicationUserInsertRequest, TApplicationUserInsertResponse>
        where TApplicationUser : IdentityUser<Guid>, IApplicationUser, new()
        where TApplicationUserInsertRequest : IApplicationUserInsertRequest, IRequest<TApplicationUserInsertResponse>
        where TApplicationUserInsertResponse : IApplicationUserInsertResponse, new()
    {
        private readonly UserManager<TApplicationUser> userManager;
        private readonly IMediator mediator;

        public ApplicationUserInsertHandlerBase(
            UserManager<TApplicationUser> userManager,
            IMediator mediator)
        {
            this.userManager = userManager;
            this.mediator = mediator;
        }

        public async Task<TApplicationUserInsertResponse> Handle(TApplicationUserInsertRequest request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var applicationUser = (await this.mediator.Send(new GenerateApplicationUserToInsertRequest<TApplicationUser, TApplicationUserInsertRequest>() { ApplicationUserInsertRequest = request })).ApplicationUser;

            var result = await this.userManager.CreateAsync(applicationUser, request.Password);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(x => x.Description));
                throw new KangarooNetException(internalErrorCode: KangarooNetErrorCode.InvalidPassword, additionalInfo: errors);
            }

            return new TApplicationUserInsertResponse();
        }
    }
}
