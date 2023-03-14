// Copyright Contributors to the KangarooNet project.
// This file is licensed to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICE files in the project root for full license information.

namespace KangarooNet.Application.Auth.Services
{
    using System.Security.Claims;
    using KangarooNet.Application.Services;
    using KangarooNet.Domain.Claims;
    using KangarooNet.Domain.Exceptions;

    public class CurrentUserService : ICurrentUserService
    {
        public Guid CurrentUserId { get; private set; }

        public string CurrentUserFullName { get; private set; } = string.Empty;

        public string CurrentUserEmail { get; private set; } = string.Empty;

        public string GetCurrentUserNameToAudit()
        {
            return this.CurrentUserFullName;
        }

        public void SetCurrentUser(ClaimsPrincipal principal)
        {
            if (principal == null)
            {
                throw new KangarooNetSecurityException();
            }

            this.CurrentUserEmail = principal.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
            Guid.TryParse(principal.Claims.FirstOrDefault(x => x.Type == KangarooNetClaims.UserId)?.Value, out var userId);
            this.CurrentUserId = userId;
            this.CurrentUserFullName = principal.Claims.FirstOrDefault(x => x.Type == KangarooNetClaims.FullName)?.Value;

            if (string.IsNullOrEmpty(this.CurrentUserEmail)
                || this.CurrentUserId == default
                || string.IsNullOrEmpty(this.CurrentUserFullName))
            {
                throw new KangarooNetSecurityException();
            }
        }
    }
}
