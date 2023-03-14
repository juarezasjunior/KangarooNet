// Copyright Contributors to the KangarooNet project.
// This file is licensed to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICE files in the project root for full license information.

namespace KangarooNet.UI.API.Auth.Extensions
{
    using System.Collections.Generic;
    using System.Reflection;
    using System.Text;
    using KangarooNet.Application.Auth.Services;
    using KangarooNet.Application.Services;
    using KangarooNet.Domain.OptionsSettings;
    using KangarooNet.UI.API.Extensions;
    using MediatR;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.IdentityModel.Tokens;

    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Add the handlers, services and pipelines related with Auth in your DI.
        /// </summary>
        /// <param name="services">Service collection.</param>
        /// <param name="assemblies">Assemblies having application files.</param>
        /// <returns>The same service collection received.</returns>
        public static IServiceCollection AddKangarooNetApplicationAuth(this IServiceCollection services, params Assembly[] assemblies)
        {
            var assembliesToScan = new List<Assembly>();
            assembliesToScan.AddRange(assemblies);
            assembliesToScan.Add(typeof(CurrentUserService).Assembly);

            services.AddKangarooNetApplication(assembliesToScan.ToArray());

            services.AddScoped<ICurrentUserService, CurrentUserService>();

            return services;
        }

        /// <summary>
        /// Add JWT authentication based in your settings file.
        /// </summary>
        /// <param name="services">Service collection.</param>
        /// <param name="configuration">Configuration.</param>
        /// <returns>The same service collection received.</returns>
        public static IServiceCollection AddKangarooNetAuthenticationJwt(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtOptions = new JwtOptions();
            configuration.GetSection(JwtOptions.Jwt).Bind(jwtOptions);

            var validIssuer = jwtOptions.JwtIssuer;
            var validAudience = jwtOptions.JwtAudience;
            var secretKey = jwtOptions.JwtSecurityKey;

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = validIssuer,
                        ValidAudience = validAudience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
                    };
                });

            return services;
        }
    }
}
