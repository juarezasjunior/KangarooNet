// Copyright Contributors to the KangarooNet project.
// This file is licensed to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICE files in the project root for full license information.

namespace KangarooNet.Application.Auth.Handlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using KangarooNet.Domain.CacheKeys;
    using KangarooNet.Domain.Entities;
    using MediatR;
    using Microsoft.Extensions.Caching.Distributed;

    public class LogoutEmailHandler : IRequestHandler<LogoutEmailRequest>
    {
        private readonly IDistributedCache distributedCache;

        public LogoutEmailHandler(IDistributedCache distributedCache)
        {
            this.distributedCache = distributedCache;
        }

        public async Task<Unit> Handle(LogoutEmailRequest request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            await this.distributedCache.SetStringAsync(string.Format(JwtCacheKeys.UserHasLogoutKey, request.Email), "true");
            await this.distributedCache.RemoveAsync(string.Format(JwtCacheKeys.RefreshTokenKey, request.Email));
            await this.distributedCache.RemoveAsync(string.Format(JwtCacheKeys.RefreshTokenExpirationTimeKey, request.Email));

            return Unit.Value;
        }
    }
}
