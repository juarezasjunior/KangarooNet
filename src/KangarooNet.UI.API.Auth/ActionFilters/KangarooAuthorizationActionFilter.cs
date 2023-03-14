// Copyright Contributors to the KangarooNet project.
// This file is licensed to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICE files in the project root for full license information.

namespace KangarooNet.UI.API.Auth.ActionFilters
{
    using System.Linq;
    using System.Threading.Tasks;
    using KangarooNet.Application.Services;
    using KangarooNet.Domain.Entities;
    using MediatR;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc.Filters;

    public class KangarooNetAuthorizationActionFilter : IAsyncAuthorizationFilter
    {
        private readonly ICurrentUserService currentUserService;
        private readonly IMediator mediator;

        public KangarooNetAuthorizationActionFilter(
            ICurrentUserService currentUserService,
            IMediator mediator)
        {
            this.currentUserService = currentUserService;
            this.mediator = mediator;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            if (!context.ActionDescriptor.EndpointMetadata.Any(x => x.GetType() == typeof(AllowAnonymousAttribute)))
            {
                this.currentUserService.SetCurrentUser(context.HttpContext.User);
                await this.mediator.Send(new CurrentUserValidatorRequest() { Email = this.currentUserService.CurrentUserEmail });
            }
        }
    }
}
