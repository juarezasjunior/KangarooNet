// Copyright Contributors to the KangarooNet project.
// This file is licensed to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICE files in the project root for full license information.

namespace KangarooNet.UI.API.Extensions
{
    using System.Collections.Generic;
    using System.Reflection;
    using KangarooNet.Application.Pipelines;
    using KangarooNet.Application.Services;
    using KangarooNet.Infrastructure.DatabaseRepositories;
    using MediatR;
    using Microsoft.Extensions.DependencyInjection;

    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Add the handlers, services and pipelines in your DI.
        /// </summary>
        /// <param name="services">Service collection.</param>
        /// <param name="assemblies">Assemblies having application files.</param>
        /// <returns>The same service collection received.</returns>
        public static IServiceCollection AddKangarooNetApplication(this IServiceCollection services, params Assembly[] assemblies)
        {
            var assembliesToScan = new List<Assembly>();
            assembliesToScan.AddRange(assemblies);
            assembliesToScan.Add(typeof(ICurrentUserService).Assembly);

            services.AddMediatR(assembliesToScan.ToArray());

            services.Scan(x =>
                x.FromAssemblies(assembliesToScan)
                .AddClasses(y =>
                    y.AssignableTo<IPrimaryPipeline>())
                .AsImplementedInterfaces()
                .WithTransientLifetime());

            services.Scan(x =>
                x.FromAssemblies(assembliesToScan)
                .AddClasses(y =>
                    y.AssignableTo<ISecondaryPipeline>())
                .AsImplementedInterfaces()
                .WithScopedLifetime());

            return services;
        }

        /// <summary>
        /// Add the database repository in your DI.
        /// </summary>
        /// <param name="services">Service collection.</param>
        /// <param name="assemblies">Assemblies where your database repositories are.</param>
        /// <returns>The same service collection received.</returns>
        public static IServiceCollection AddKangarooNetDatabaseRepositories(this IServiceCollection services, params Assembly[] assemblies)
        {
            return services.Scan(x =>
                x.FromAssemblies(assemblies)
                .AddClasses(y =>
                    y.AssignableTo(typeof(IDatabaseRepository<>)))
                .AsImplementedInterfaces()
                .WithTransientLifetime());
        }
    }
}
