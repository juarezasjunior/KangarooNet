using FluentValidation;
using FluentValidation.AspNetCore;
using KangarooNet.UI.API.Auth.ActionFilters;
using KangarooNet.UI.API.Auth.Extensions;
using KangarooNet.UI.API.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using MyFirstSample.Infrastructure.DatabaseRepositories.DBContexts;
using MyFirstSample.Infrastructure.DatabaseRepositories.Mapper;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddKangarooNetApplicationAuth(typeof(Program).Assembly);
builder.Services.AddKangarooNetDatabaseRepositories(typeof(Program).Assembly);
builder.Services.AddKangarooNetAuthenticationJwt(builder.Configuration);
builder.Services.AddDistributedMemoryCache();

builder.Services.AddIdentityCore<ApplicationUser>()
  .AddRoles<IdentityRole<Guid>>()
  .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddDbContext<ApplicationDbContext>(x => x.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Database=MyDbTest;Trusted_Connection=True;",
  y => y.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery))
  .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));


builder.Services.AddLogging(x => x.AddDebug())
  .AddAutoMapper(typeof(ApplicationAutoMapperProfile))
  .AddValidatorsFromAssembly(typeof(Program).Assembly);

builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddMvc(x =>
{
    x.Filters.Add(typeof(KangarooNetAuthorizationActionFilter));
    x.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(x =>
{
    x.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme.\r\n\r\n Enter 'Bearer'[space] and then your token in the text input below.\r\n\r\nExample: \"Bearer 12345abcdef\"",
    });

    x.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer",
                },
            },
            new string[]
            {
            }
        },
    });
});

builder.ConfigureKangarooNetJWTOptions();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseKangarooNetException();

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

if (app is IApplicationBuilder applicationBuilder)
{
    using (var serviceScope = applicationBuilder.ApplicationServices.CreateScope())
    {
        var myDbContext = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();

        await myDbContext.Database.EnsureDeletedAsync();
        await myDbContext.Database.EnsureCreatedAsync();
    }
}

app.Run();
