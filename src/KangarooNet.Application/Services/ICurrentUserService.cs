// Copyright Contributors to the KangarooNet project.
// This file is licensed to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICE files in the project root for full license information.

namespace KangarooNet.Application.Services
{
    using System.Security.Claims;

    public interface ICurrentUserService
    {
        public Guid CurrentUserId { get; }

        public string CurrentUserFullName { get; }

        public string CurrentUserEmail { get; }

        /// <summary>
        /// Get the current user name to be included in the audit field.
        /// </summary>
        /// <returns>The user name.</returns>
        public string GetCurrentUserNameToAudit();

        /// <summary>
        /// Set the current user based on claims principal.
        /// </summary>
        /// <param name="principal">The logged in information.</param>
        public void SetCurrentUser(ClaimsPrincipal principal);
    }
}