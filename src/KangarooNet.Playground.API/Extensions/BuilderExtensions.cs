// Copyright Contributors to the KangarooNet project.
// This file is licensed to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICE files in the project root for full license information.

namespace KangarooNet.Playground.API.Extensions
{
    using System;
    using System.Threading.Tasks;
    using FluentValidation;
    using FluentValidation.AspNetCore;
    using KangarooNet.Playground.Infrastructure.DatabaseEntities;
    using KangarooNet.Playground.Infrastructure.DatabaseRepositories.DBContexts;
    using KangarooNet.Playground.Infrastructure.DatabaseRepositories.Mapper;
    using KangarooNet.UI.API.Auth.ActionFilters;
    using KangarooNet.UI.API.Auth.Extensions;
    using KangarooNet.UI.API.Extensions;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public static class BuilderExtensions
    {
        public static void AddServiceCollection(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddKangarooNetApplicationAuth(typeof(BuilderExtensions).Assembly);
            services.AddKangarooNetDatabaseRepositories(typeof(BuilderExtensions).Assembly);
            services.AddKangarooNetAuthenticationJwt(configuration);
            services.AddDistributedMemoryCache();

            services.AddIdentityCore<ApplicationUser>()
                .AddRoles<IdentityRole<Guid>>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddDbContext<ApplicationDbContext>(x => x.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Database=MyDbTest;Trusted_Connection=True;",
                    y => y.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery))
                    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));

            services.AddLogging(x => x.AddDebug())
                .AddAutoMapper(typeof(ApplicationAutoMapperProfile))
                .AddValidatorsFromAssembly(typeof(BuilderExtensions).Assembly);

            services.AddFluentValidationAutoValidation();

            services.AddMvc(x =>
            {
                x.Filters.Add(typeof(KangarooNetAuthorizationActionFilter));
                x.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
            });
        }

        public static async Task ConfigureDatabase(this IApplicationBuilder applicationBuilder)
        {
            using (var serviceScope = applicationBuilder.ApplicationServices.CreateScope())
            {
                var myDbContext = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();

                await myDbContext.Database.EnsureDeletedAsync();
                await myDbContext.Database.EnsureCreatedAsync();

                myDbContext.Countries.Add(new TbCountry { Name = "Country 1", CreatedByUserName = "Admin", CreatedAt = DateTimeOffset.Now });
                myDbContext.Countries.Add(new TbCountry { Name = "Country 2", CreatedByUserName = "Admin", CreatedAt = DateTimeOffset.Now });
                myDbContext.Countries.Add(new TbCountry { Name = "Country 3", CreatedByUserName = "Admin", CreatedAt = DateTimeOffset.Now });
                await myDbContext.SaveChangesAsync();
                myDbContext.ChangeTracker.Clear();
            }
        }
    }
}
