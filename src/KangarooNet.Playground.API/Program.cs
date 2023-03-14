// Copyright Contributors to the KangarooNet project.
// This file is licensed to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICE files in the project root for full license information.

using KangarooNet.UI.API.Extensions;
using KangarooNet.Playground.API;
using KangarooNet.Playground.API.Extensions;
using Microsoft.OpenApi.Models;
using KangarooNet.UI.API.Auth.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

// Was changed
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

// Was Added
builder.Services.AddServiceCollection(builder.Configuration);

builder.ConfigureKangarooNetJWTOptions();

var app = builder.Build();

// Was Added
await app.ConfigureDatabase();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Was added
app.UseKangarooNetException();

app.UseHttpsRedirection();

// Was added
app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
