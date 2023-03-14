// Copyright Contributors to the KangarooNet project.
// This file is licensed to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICE files in the project root for full license information.

namespace KangarooNet.Application.Auth.Handlers
{
    using System.Threading;
    using System.Threading.Tasks;
    using KangarooNet.Domain.CacheKeys;
    using KangarooNet.Domain.Entities;
    using KangarooNet.Domain.Exceptions;
    using MediatR;
    using Microsoft.Extensions.Caching.Distributed;

    public class CurrentUserValidatorHandler : IRequestHandler<CurrentUserValidatorRequest>
    {
        private readonly IDistributedCache distributedCache;

        public CurrentUserValidatorHandler(IDistributedCache distributedCache)
        {
            this.distributedCache = distributedCache;
        }

        public async Task<Unit> Handle(CurrentUserValidatorRequest request, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrEmpty(request.Email))
            {
                throw new KangarooNetSecurityException();
            }

            var token = await this.distributedCache.GetStringAsync(string.Format(JwtCacheKeys.UserHasLogoutKey, request.Email));

            if (!string.IsNullOrEmpty(token))
            {
                throw new KangarooNetSecurityException();
            }

            return Unit.Value;
        }
    }
}
