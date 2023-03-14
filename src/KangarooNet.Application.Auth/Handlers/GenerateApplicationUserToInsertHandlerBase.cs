// Copyright Contributors to the KangarooNet project.
// This file is licensed to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICE files in the project root for full license information.

namespace KangarooNet.Application.Auth.Handlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using KangarooNet.Domain;
    using KangarooNet.Domain.DatabaseEntities;
    using KangarooNet.Domain.Entities;
    using KangarooNet.Domain.Entities.Auth;
    using MediatR;
    using Microsoft.AspNetCore.Identity;

    public abstract class GenerateApplicationUserToInsertHandlerBase<TApplicationUser, TApplicationUserInsertRequest>
        : IRequestHandler<GenerateApplicationUserToInsertRequest<TApplicationUser, TApplicationUserInsertRequest>, GenerateApplicationUserToInsertResponse<TApplicationUser>>
        where TApplicationUser : IdentityUser<Guid>, IApplicationUser, new()
        where TApplicationUserInsertRequest : IApplicationUserInsertRequest
    {
        public async Task<GenerateApplicationUserToInsertResponse<TApplicationUser>> Handle(GenerateApplicationUserToInsertRequest<TApplicationUser, TApplicationUserInsertRequest> request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var applicationUser = new TApplicationUser()
            {
                FullName = request.ApplicationUserInsertRequest.FullName,
                UserName = request.ApplicationUserInsertRequest.Email,
                Email = request.ApplicationUserInsertRequest.Email,
            };

            return await Task.FromResult(new GenerateApplicationUserToInsertResponse<TApplicationUser>() { ApplicationUser = applicationUser });
        }
    }
}
