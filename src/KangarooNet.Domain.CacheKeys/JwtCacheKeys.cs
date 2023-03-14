// Copyright Contributors to the KangarooNet project.
// This file is licensed to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICE files in the project root for full license information.

namespace KangarooNet.Domain.CacheKeys
{
    using System;

    public static class JwtCacheKeys
    {
        public const string UserHasLogoutKey = "UserHasLogout_{0}";

        public const string RefreshTokenKey = "JWTRefreshToken_{0}";

        public const string RefreshTokenExpirationTimeKey = "JWTRefreshTokenExp_{0}";
    }
}
