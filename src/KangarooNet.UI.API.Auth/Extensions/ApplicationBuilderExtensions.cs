// Copyright Contributors to the KangarooNet project.
// This file is licensed to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICE files in the project root for full license information.

namespace KangarooNet.UI.API.Auth.Extensions
{
    using KangarooNet.Domain.OptionsSettings;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.Extensions.DependencyInjection;

    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// It will configure some JWT options settings based on your settings file.
        /// </summary>
        /// <param name="builder">Application builder.</param>
        /// <returns>The same application builder received.</returns>
        public static WebApplicationBuilder ConfigureKangarooNetJWTOptions(this WebApplicationBuilder builder)
        {
            builder.Services.Configure<JwtOptions>(
                builder.Configuration.GetSection(JwtOptions.Jwt));

            return builder;
        }
    }
}
