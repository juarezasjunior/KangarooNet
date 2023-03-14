// Copyright Contributors to the KangarooNet project.
// This file is licensed to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICE files in the project root for full license information.

namespace KangarooNet.Domain.OptionsSettings
{
    public class JwtOptions
    {
        public const string Jwt = "Jwt";

        public string JwtIssuer { get; set; } = string.Empty;

        public string JwtAudience { get; set; } = string.Empty;

        public int JwtExpiryInMinutes { get; set; }

        public int JwtRefreshTokenExpiryInMinutes { get; set; }

        public string JwtSecurityKey { get; set; } = string.Empty;
    }
}
