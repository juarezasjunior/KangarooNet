# KangarooNet
This is a framework to create .NET APIs and interact with them.

KangarooNet uses some other frameworks like MediatR, AutoMapper, EntityFramework, Refit and others to create a complete API and a way to call its endpoints.

## Introduction
Imagine that you are creating an API that must be able to Create, Read, Update and Delete resources in a database. 

To do this, you need to create endpoints that receive and send DTOs, create classes that handle these requests, and classes that persist the data in the database. You also need to worry about API authentication and how to access it over a client connection.

With KangarooNet, you can create all the resources you need using XML files and writing just a few lines of code.

## How to use
See ahead how to use some resources from KangarooNet. It will create an API having JWT authentication and comunication with MS Sql Server.

### Create a new ASP.NET Core Web API
Open Visual Studio, and create a new ASP.NET Core Web API project using .NET 6 or higher (this sample is using .NET 6 template). We'll call our project as **MyFirstSample**.

### Install NuGet Dependencies
Install the following dependencies:

```
Install-Package KangarooNet.UI.API.Auth
Install-Package KangarooNet.CodeGenerators
Install-Package Microsoft.EntityFrameworkCore.SqlServer
```

### Create a class to represent your user in the database
In order to use our authentication mechanism, you need to create a partial class inheriting from Microsoft.AspNetCore.Identity.IdentityUser and KangarooNet.Domain.DatabaseEntities.IApplicationUser.

```
using KangarooNet.Domain;
using Microsoft.AspNetCore.Identity;

public partial class ApplicationUser : IdentityUser<Guid>, IApplicationUser
{
    public string FullName { get; set; }
}
```

### Create a KCG file that represents your entity
To generate EF entities, DTOs, handlers and controllers automatically, you must having .kcg.xml files that represent the entities.

You can write all entities in one .kcg.xml file or you can have many .kcg.xml files. Splitting them in many files is a good practice!

Inside this file, you can write your entity and its configuration to our CodeGenerators generates all necessary and desirable classes.

In our example, we will create an entity that represents the country registry. With that, we will generate a table to store the countries and some handlers and API controllers to handle this data.

So, just add a new file in your project with the name `Country.kcg.xml`. Don't forget that the extension of those kind of files must be `.kcg.xml`.

This is the content of this file:

```
<?xml version="1.0" encoding="utf-8" ?>
<CodeGenerator xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
		 xsi:noNamespaceSchemaLocation="../../KangarooNet.CodeGenerators/Structure/CodeGeneratorSchema.xsd">
	<Entity Name="Country" PluralName="Countries">
		<EntityFields>
			<KeyField KeyType="Guid" Name="CountryId"/>
			<StringField Name="Name" IsRequired="true"/>
		</EntityFields>

		<GenerateDatabaseEntity>
			<GenerateDatabaseEntityConfiguration />
		</GenerateDatabaseEntity>

		<GenerateAutoMapper AutoMapperSourceType="Entity" />
		<GenerateAutoMapper AutoMapperSourceType="DatabaseEntity" />

		<GenerateEntityHandlerRequest>
			<GenerateController>
				<Permissions>
					<Permission Guid="fa31fdf0-62e7-494a-a287-2a0d31da7728"
								Name="General_Country_Handler"/>
				</Permissions>
			</GenerateController>
			<GenerateEntityHandler />
		</GenerateEntityHandlerRequest>

		<GenerateEntityQueryRequest>
			<GenerateController>
				<Permissions>
					<Permission Guid="329ad8b9-8a3d-4f97-b4c4-6d64d21fc305"
								Name="General_Country_Get"/>
				</Permissions>
			</GenerateController>
		</GenerateEntityQueryRequest>

		<GenerateEntitiesQueryRequest>
			<GenerateController>
				<Permissions>
					<Permission Guid="924faed8-4eaf-4aae-b03d-bb242674cd6d"
								Name="General_Countries_Get"/>
				</Permissions>
			</GenerateController>
		</GenerateEntitiesQueryRequest>
	</Entity>
</CodeGenerator>
```

As noticed, inside the tag Entity, you can add `EntityFields` of many type. Each field will be created in your DTO and in the class that represents your database table. 

Also, you have in this example, other tags like:

- **GenerateDatabaseEntity**: Generates the database entity class;
- **GenerateDatabaseEntityConfiguration**: Generates an entity configuration class that allows you to seed data, create database indexes or other settings;
- **GenerateAutoMapper**: Generates an AutoMapper profiler including the conversion from your entity DTO to your database entity and vice-versa;
- **GenerateEntityHandlerRequest**: Can generate the following resources to handle with your entity:
- - **GenerateController**: API Controller to insert, update and delete an entity. Will be generated only one endpoint receiving a DTO that represents your entity. Inside this DTO, you have a DataState option to indicate if you want to insert, update or delete a record;
- - **GenerateEntityHandler**: Handlers classes that do the bridge between your controller and your database repository using Mediator pattern.

Also, inside the `GenerateController` tag, you can include one or more permissions. Those permissions will be used to authorize the endpoint.

**Important**: After saving your new `.kcg.xml` file, right click on it in Solution Explorer. In the context menu opened, click on `Properties` option. Inside the Properties tab, change the value of `Build Action` to `C# analyzer additional file` and save it. 

### Create your KangarooNet Project Settings file
In order to know what classes should be generated in each project, we must have a `.kcgs.xml` file.

This class has the information about what kind of classes will be generated in this project. It is usefull to differentiate backend from frontend projects. Also it's usefull to split your solution in other projects (for example, you can have a project to have database repository files, other to have application or handler files, etc.).

So, add a new file called `GeneratorSettings.kcgs.xml` inside your project. Don't forget! This file extension must be `.kcgs.xml`.

This is the content of this file:

```
<?xml version="1.0" encoding="utf-8" ?>
<CodeGeneratorSettings xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
					   xsi:noNamespaceSchemaLocation="../../KangarooNet.CodeGenerators/Structure/CodeGeneratorSettingsSchema.xsd">

	<DatabaseRepositoriesSettings
		EntitiesNamespace="MyFirstSample.Entities"
		DatabaseEntitiesNamespace="MyFirstSample.Infrastructure.DatabaseEntities"
		DatabaseEntityTypeConfigurationNamespace="MyFirstSample.Infrastructure.DatabaseRepositories.EntityTypeConfiguration"
		DbContextNamespace="MyFirstSample.Infrastructure.DatabaseRepositories.DBContexts"
		DatabaseRepositoriesNamespace="MyFirstSample.Infrastructure.DatabaseRepositories"
		DatabaseEntityMapperNamespace="MyFirstSample.Infrastructure.DatabaseRepositories.Mapper"
		IdentityDbContextCustomUserClass="ApplicationUser"/>

	<BackendEntititesSettings
		EntitiesNamespace="MyFirstSample.Entities"
		ValidatorsNamespace="MyFirstSample.Entities.Validators"
		GenerateAuthEntities="true"/>

	<ApplicationSettings
		ApplicationNamespace="MyFirstSample.Application"
		DbContextNamespace="MyFirstSample.Infrastructure.DatabaseRepositories.DBContexts"
		DatabaseRepositoriesNamespace="MyFirstSample.Infrastructure.DatabaseRepositories"
		DatabaseEntitiesNamespace="MyFirstSample.Infrastructure.DatabaseEntities"
		EntitiesNamespace="MyFirstSample.Entities"
		GenerateAuthHandlersBasedOnCustomUserClass="ApplicationUser"/>

	<APISettings
		ControllersNamespace="MyFirstSample.API.Controllers"
		ApplicationNamespace="MyFirstSample.Application"
		EntitiesNamespace="MyFirstSample.Entities"
		GenerateAuthController="true"/>

</CodeGeneratorSettings>
```

As noticed, you can set in this file some namespaces and options that will be used to generate all of the classes files.

**Important**: After saving your new `.kcgs.xml` file, right click on it in Solution Explorer. In the context menu opened, click on `Properties` option. Inside the Properties tab, change the value of `Build Action` to `C# analyzer additional file` and save it. 

### Create QueryHandler classes
We have just created an EntityHandler class, that is used to insert/update/delete data in your database. To retrieve data, you should create your own handler class.

Since that we are using MediatR and all get requests are implementing `IRequest<TResponse>`, you can create a QueryHandler class for each request generated.

These are the query requests generated:
- **EntityQueryRequest**: it is used to retrieve a single record based in the Id received inside the request;
- **EntitiesQueryRequest**: you can retrieve more than one record based in the request parameters;
- **SummaryQueryRequest**: you can retrieve a single summary record based in your request. A Summary is used to you don't bring all of the fields from a table. Using a summary, you can specify which fields you want to send back;
- **SummariesQueryRequest**: you can retrieve more than one record based in the request parameters.

As mentioned before, we are creating only the DTOs to get data. You need to implement your handler and the logic to retrieve them. See in the next steps how we are creating two query handler classes: `CountryQueryHandler` and `CountriesQueryHandler`.

#### Create the CountryQueryHandler class
Create the CountryQueryHandler class with the following content:

```
namespace MyFirstSample
{
    using System.Threading.Tasks;
    using KangarooNet.Application;
    using MediatR;
    using MyFirstSample.Entities;
    using MyFirstSample.Infrastructure.DatabaseEntities;
    using MyFirstSample.Infrastructure.DatabaseRepositories;

    public partial class CountryQueryHandler : IRequestHandler<CountryQueryRequest, CountryQueryResponse>
    {
        private readonly IApplicationDatabaseRepository applicationDatabaseRepository;

        public CountryQueryHandler(IApplicationDatabaseRepository applicationDatabaseRepository)
        {
            this.applicationDatabaseRepository = applicationDatabaseRepository;
        }

        public async Task<CountryQueryResponse> Handle(CountryQueryRequest request, CancellationToken cancellationToken)
        {
            var result = (await this.applicationDatabaseRepository.GetByConditionAsync<TbCountry, Country>(
                x => x.Where(y => y.CountryId == request.CountryId), cancellationToken))?.FirstOrDefault();

            return new CountryQueryResponse() { Entity = result };
        }
    }
}
```

In this file, we are using the ApplicationDatabaseRepository that was generated to execute database queries.

#### Create the CountriesQueryHandler class
Now, let's create the CountriesQueryHandler class with this content:

```
namespace MyFirstSample
{
    using System.Threading.Tasks;
    using KangarooNet.Application;
    using MediatR;
    using MyFirstSample.Entities;
    using MyFirstSample.Infrastructure.DatabaseEntities;
    using MyFirstSample.Infrastructure.DatabaseRepositories;

    public partial class CountriesQueryHandler : IRequestHandler<CountriesQueryRequest, CountriesQueryResponse>
    {
        private readonly IApplicationDatabaseRepository applicationDatabaseRepository;

        public CountriesQueryHandler(IApplicationDatabaseRepository applicationDatabaseRepository)
        {
            this.applicationDatabaseRepository = applicationDatabaseRepository;
        }

        public async Task<CountriesQueryResponse> Handle(CountriesQueryRequest request, CancellationToken cancellationToken)
        {
            var result = await this.applicationDatabaseRepository.GetAllAsync<TbCountry, Country>(cancellationToken);

            return new CountriesQueryResponse() { Entities = result };
        }
    }
}

```

### Build your solution
After the changes above, just build your solution to generate all files. You can see all of the generated files in Solution Explorer by expanding your `Project Folder > Dependencies > Analyzers > KangarooNet.CodeGenerators > KangarooNet.CodeGenerators.MainIncrementalGenerator`.

### Changes in your Program.cs file
In Program.cs file, call the following methods to add some dependencies injection:

```
builder.Services.AddKangarooNetApplicationAuth(typeof(Program).Assembly);
builder.Services.AddKangarooNetDatabaseRepositories(typeof(Program).Assembly);
builder.Services.AddKangarooNetAuthenticationJwt(builder.Configuration);
builder.Services.AddDistributedMemoryCache();
```

We are using a memory distributed cache, but you can use any other cache storage that you want to.

Now, add the Identity Core services based in your user class that you created above.

```
builder.Services.AddIdentityCore<ApplicationUser>()
  .AddRoles<IdentityRole<Guid>>()
  .AddEntityFrameworkStores<ApplicationDbContext>();
```

KangarooNet will generate a DbContext for you. So you need just to configure the database, that is MS Sql Server in this case. You can use other database. Don't forget that MS Sql Server has its own license and costs.

```
builder.Services.AddDbContext<ApplicationDbContext>(x => x.UseSqlServer("Data Source=(localdb)\\MSSQLLocalDB;Database=MyDbTest;Trusted_Connection=True;",
  y => y.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery))
  .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));
```

Now, let's add some external dependencies, like logging, AutoMapper and FluentValidation.

```
builder.Services.AddLogging(x => x.AddDebug())
  .AddAutoMapper(typeof(ApplicationAutoMapperProfile))
  .AddValidatorsFromAssembly(typeof(Program).Assembly);
```

Lastly, we will add FluentValidations and MVC controllers.

```
builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddMvc(x =>
{
    x.Filters.Add(typeof(KangarooNetAuthorizationActionFilter));
    x.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
});
```

If you are using Swagger in your API, you can change a bit the part `builder.Services.AddSwaggerGen();` to this:

```
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
```

You should configure KangarooNet Options as well by adding this code:

```
builder.ConfigureKangarooNetJWTOptions();
```

Now, it's time to configure our **middlewares** in the same Program.cs.

First, before `app.UseHttpsRedirection();`, paste the code `app.UseKangarooNetException();`. Then, before `app.UseAuthorization();` you can paste `app.UseAuthentication();`

Finally, we will just add a code to create our database everytime we run the application. But, is should not be done in production! It's just to avoid executing the EntityFramework migration now. So, in Program.cs before `app.Run();`, just add the following code:

```
if (app is IApplicationBuilder applicationBuilder)
{
    using (var serviceScope = applicationBuilder.ApplicationServices.CreateScope())
    {
        var myDbContext = serviceScope.ServiceProvider.GetService<ApplicationDbContext>();

        await myDbContext.Database.EnsureDeletedAsync();
        await myDbContext.Database.EnsureCreatedAsync();
    }
}
```

Your Program.cs file will be like this:

```
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
```

### Add JWT section in your appsettings.json
Open your appsettings.json file and include the following section:

```
"Jwt": {
    "JwtSecurityKey": "Paste here a securable key to generate JWT tokens",
    "JwtIssuer": "https://localhost",
    "JwtAudience": "https://localhost",
    "JwtExpiryInMinutes": 30,
    "JwtRefreshTokenExpiryInMinutes": 120
  }
```
In production, you must change the values above for what is more securable to you. Don't use the same JwtSecurityKey that we mentioned above. You should create your own key and save it in a securable place. This key is used to generate the token. Also, you need to change the JwtIssuer and JwtAudience to your web app address and inform the expiry minutes and refresh expiry minutes that is more appropriate in your scenario. 

### Run your project
Since that you did the code above, run your project.

After executing, you will see the Swagger showing all of the generated controllers.

We have some controllers that are used to authenticate your API:
- **/api/Auth/InsertApplicationUser**: this endpoint can be called to insert a new user;
- **/api/Auth/Login**: call this endpoint to login in your API;
- **/api/Auth/RefreshToken**: once you are logged, you can refresh your JWT token by calling this endpoint;
- **/api/Auth/Logout**: if you want to logout, you can call this endpoint;
- **/api/Auth/ChangePassword**: this endpoint was created to change the password from the current connected user.

### About MediatR
We are using MediatR to insert, update and delete database entities. To get data, we can create our own service as mentioned before. But, if you need to intercept something, like doing a custom validation before saving database or publish a notification, you can create your own MediatR Pipeline based in the DTO request that you want to.

Since that pipelines has its own priority to be executed, we have created two interfaces that can "classify" the priority of your pipeline. They are in `KangarooNet.Application` with the name `IPrimaryPipeline` and `ISecondaryPipeline`. When creating your MediatR pipeline, you can inherit from one of those interfaces as well and your pipeline will be added in your Dependency Injection Service automatically.

### Create a client project
All of the changes above was to have the backend API. But, as mentioned earlier, you can have frontend's DTOs and Refit interfaces to call the API.

A note about Refit. Since that Refit generates classes when compiling interfaces, we cannot use Source Generator to auto generate those interfaces, because they will not be compiled by Refit. So, we only generate a commented class that you can copy its content and paste it in a local class file.

So, let's create a client project. In Visual Studio, add a new Class Library project. We'll call our new project as **MyFirstSample.Client**.

### Install NuGet Dependencies in your client project
Install the following dependencies:

```
Install-Package KangarooNet.UI.APIClient.Auth
Install-Package KangarooNet.CodeGenerators
```

### Add the .kcg.xml files in your project
You need to add the .kcg.xml files that represent your entities. But you don't need to add a new file. You can add the existing .kcg.xml file.

To do that, right click on your `MyFirstSample.Client` project and select the option `Add > Existing Item`. Browse to `MyFirstSample` folder and select the `Country.kcg.xml` file that you created before. But instead of `Add` this file, select the option `Add As Link` on the arrow next to the add button.

**Important**: After adding your existing `.kcg.xml` file, right click on it in Solution Explorer. In the context menu opened, click on `Properties` option. Inside the Properties tab, change the value of `Build Action` to `C# analyzer additional file` and save it. 

### Add a new .kcgs.xml file in your project
You need to create a new `.kcgs.xml` file in your client project, having only the frontend settings.

Create a new file called `GeneratorSettings.kcgs.xml` with this content:

```
<?xml version="1.0" encoding="utf-8" ?>
<CodeGeneratorSettings xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance"
					   xsi:noNamespaceSchemaLocation="../../KangarooNet.CodeGenerators/Structure/CodeGeneratorSettingsSchema.xsd">

	<FrontendEntititesSettings
		EntitiesNamespace="MyFirstSample.Client.Entities"
		ValidatorsNamespace="MyFirstSample.Client.Entities.Validators"
		GenerateAuthEntities="true"/>

	<APIClientSettings
		APIClientNamespace="MyFirstSample.Client.APIClient"
		EntitiesNamespace="MyFirstSample.Client.Entities"
		GenerateAuthAPIClient="true"/>

</CodeGeneratorSettings>
```

**Important**: After saving your new `.kcgs.xml` file, right click on it in Solution Explorer. In the context menu opened, click on `Properties` option. Inside the Properties tab, change the value of `Build Action` to `C# analyzer additional file` and save it.

### Build your client project
Now, just build your client project. You can see all of the generated files in Solution Explorer by expanding your `MyFirstSample.Client Folder > Dependencies > Analyzers > KangarooNet.CodeGenerators > KangarooNet.CodeGenerators.MainIncrementalGenerator`.

KangarooNet framework generated all of the DTOs having INotifyPropertyChanged and all of the interfaces used by Refit.

You can now copy the content from APIClientContent.g.cs file and paste it in a new file, like APIClientContent.cs (don't forget to uncomment the content). After that, you can configure Refit to call your API and use the interfaces that we generated. When configuring Refit, you can scan all interfaces inheriting from IAPIClient.

## Open the sample code
[See the source of this sample by clicking here](https://github.com/juarezasjunior/KangarooNet/tree/main/samples/MyFirstSample)

## Important About JWT
The JWT authentication will be generated to you. But, it is your responsibility to check all of the security aspects involved.

## License
Copyright Contributors to the KangarooNet project.

KangarooNet is licensed under Apache 2.0 license. You must read [our license terms](https://github.com/juarezasjunior/KangarooNet/blob/master/LICENSE) and [the NOTICE file](https://github.com/juarezasjunior/KangarooNet/blob/master/NOTICE) before contribute, fork, copy, distribute, use or do any other thing. You can proceed only if you agree.

KangarooNet uses some third party libraries and frameworks. Their copyright and license info can be found [in our NOTICE file](https://github.com/juarezasjunior/KangarooNet/blob/master/NOTICE).

Some resources, libraries and frameworks used in KangarooNet may have some costs to be used. So, it is your responsability to check their terms, licenses and costs.

## Contribution
See [our Contributing file](https://github.com/juarezasjunior/Komber/blob/master/CONTRIBUTING.md).
