// Copyright Contributors to the KangarooNet project.
// This file is licensed to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICE files in the project root for full license information.

namespace KangarooNet.Application.Auth.Handlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using KangarooNet.Application.Services;
    using KangarooNet.Domain.Entities;
    using KangarooNet.Domain.Entities.Auth;
    using MediatR;

    public abstract class LogoutHandlerBase<TLogoutRequest, TLogoutResponse>
        : IRequestHandler<TLogoutRequest, TLogoutResponse>
        where TLogoutRequest : ILogoutRequest, IRequest<TLogoutResponse>
        where TLogoutResponse : ILogoutResponse, new()
    {
        private readonly IMediator mediator;
        private readonly ICurrentUserService currentUserService;

        public LogoutHandlerBase(
            IMediator mediator,
            ICurrentUserService currentUserService)
        {
            this.mediator = mediator;
            this.currentUserService = currentUserService;
        }

        public async Task<TLogoutResponse> Handle(TLogoutRequest request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await this.mediator.Send(new LogoutEmailRequest() { Email = this.currentUserService.CurrentUserEmail });

            return new TLogoutResponse();
        }
    }
}
