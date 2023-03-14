// Copyright Contributors to the KangarooNet project.
// This file is licensed to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICE files in the project root for full license information.

namespace KangarooNet.UI.API.Extensions
{
    using KangarooNet.UI.API.Middlewares;
    using Microsoft.AspNetCore.Builder;

    public static class ApplicationBuilderExtensions
    {
        /// <summary>
        /// It configures the exception middleware.
        /// </summary>
        /// <param name="builder">Application builder.</param>
        /// <returns>The same application builder received.</returns>
        public static IApplicationBuilder UseKangarooNetException(this IApplicationBuilder builder)
        {
            builder.UseMiddleware<KangarooNetExceptionMiddleware>();

            return builder;
        }
    }
}
