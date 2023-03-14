// Copyright Contributors to the KangarooNet project.
// This file is licensed to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICE files in the project root for full license information.

namespace KangarooNet.CodeGenerators.CodeWriters
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using KangarooNet.CodeGenerators.Extensions;
    using KangarooNet.CodeGenerators.Structure;
    using KangarooNet.CodeGenerators.Writers;
    using Microsoft.CodeAnalysis;

    internal static class ApplicationCodeWriter
    {
        public static void Generate(CodeGeneratorSettings codeGeneratorSettings, List<CodeGenerator> codeGenerators, SourceProductionContext sourceProductionContext)
        {
            foreach (var codeGenerator in codeGenerators)
            {
                foreach (var entity in codeGenerator.Entity)
                {
                    if (codeGeneratorSettings.ApplicationSettings != null
                        && entity.GenerateEntityHandlerRequest?.GenerateEntityHandler != null)
                    {
                        WriteEntityHandler(codeGeneratorSettings, sourceProductionContext, entity);
                    }
                }
            }

            if (!string.IsNullOrEmpty(codeGeneratorSettings.ApplicationSettings?.GenerateAuthHandlersBasedOnCustomUserClass))
            {
                GenerateAuthHandlers(codeGeneratorSettings, sourceProductionContext);
            }
        }

        private static void GenerateAuthHandlers(CodeGeneratorSettings codeGeneratorSettings, SourceProductionContext sourceProductionContext)
        {
            WriteApplicationUserInsertHandler(codeGeneratorSettings, sourceProductionContext);
            WriteGenerateApplicationUserToInsertHandler(codeGeneratorSettings, sourceProductionContext);
            WriteChangePasswordHandler(codeGeneratorSettings, sourceProductionContext);
            WriteGenerateTokenHandler(codeGeneratorSettings, sourceProductionContext);
            WriteLoginHandler(codeGeneratorSettings, sourceProductionContext);
            WriteLoginValidatorHandler(codeGeneratorSettings, sourceProductionContext);
            WriteLogoutHandler(codeGeneratorSettings, sourceProductionContext);
            WriteRefreshTokenHandler(codeGeneratorSettings, sourceProductionContext);
        }

        private static void WriteApplicationUserInsertHandler(CodeGeneratorSettings codeGeneratorSettings, SourceProductionContext sourceProductionContext)
        {
            var customUserClassName = codeGeneratorSettings.ApplicationSettings?.GenerateAuthHandlersBasedOnCustomUserClass;
            var serviceName = $"ApplicationUserInsertHandler";
            var serviceFileWriter = new CSFileWriter(
                CSFileWriterType.Class,
                codeGeneratorSettings.ApplicationSettings?.ApplicationNamespace,
                serviceName,
                isPartial: true,
                inheritance: $"ApplicationUserInsertHandlerBase<{customUserClassName}, ApplicationUserInsertRequest, ApplicationUserInsertResponse>");
            serviceFileWriter.WriteUsing("System");
            serviceFileWriter.WriteUsing("KangarooNet.Application.Auth.Handlers");
            serviceFileWriter.WriteUsing("MediatR");
            serviceFileWriter.WriteUsing("Microsoft.AspNetCore.Identity");
            serviceFileWriter.WriteUsing(codeGeneratorSettings.ApplicationSettings?.DbContextNamespace);
            serviceFileWriter.WriteUsing(codeGeneratorSettings.ApplicationSettings?.DatabaseRepositoriesNamespace);
            serviceFileWriter.WriteUsing(codeGeneratorSettings.ApplicationSettings?.DatabaseEntitiesNamespace);
            serviceFileWriter.WriteUsing(codeGeneratorSettings.ApplicationSettings?.EntitiesNamespace);

            serviceFileWriter.WriteDependencyInjection($"UserManager<{customUserClassName}>", "userManager", shouldSendToConstructorBase: true);
            serviceFileWriter.WriteDependencyInjection("IMediator", "mediator", shouldSendToConstructorBase: true);
            sourceProductionContext.WriteNewCSFile(serviceName, serviceFileWriter);
        }

        private static void WriteGenerateApplicationUserToInsertHandler(CodeGeneratorSettings codeGeneratorSettings, SourceProductionContext sourceProductionContext)
        {
            var customUserClassName = codeGeneratorSettings.ApplicationSettings?.GenerateAuthHandlersBasedOnCustomUserClass;
            var serviceName = $"GenerateApplicationUserToInsertHandler";
            var serviceFileWriter = new CSFileWriter(
                CSFileWriterType.Class,
                codeGeneratorSettings.ApplicationSettings?.ApplicationNamespace,
                serviceName,
                isPartial: true,
                inheritance: $"GenerateApplicationUserToInsertHandlerBase<{customUserClassName}, ApplicationUserInsertRequest>");
            serviceFileWriter.WriteUsing("System");
            serviceFileWriter.WriteUsing("KangarooNet.Application.Auth.Handlers");
            serviceFileWriter.WriteUsing("Microsoft.AspNetCore.Identity");
            serviceFileWriter.WriteUsing(codeGeneratorSettings.ApplicationSettings?.DbContextNamespace);
            serviceFileWriter.WriteUsing(codeGeneratorSettings.ApplicationSettings?.DatabaseRepositoriesNamespace);
            serviceFileWriter.WriteUsing(codeGeneratorSettings.ApplicationSettings?.DatabaseEntitiesNamespace);
            serviceFileWriter.WriteUsing(codeGeneratorSettings.ApplicationSettings?.EntitiesNamespace);

            sourceProductionContext.WriteNewCSFile(serviceName, serviceFileWriter);
        }

        private static void WriteChangePasswordHandler(CodeGeneratorSettings codeGeneratorSettings, SourceProductionContext sourceProductionContext)
        {
            var customUserClassName = codeGeneratorSettings.ApplicationSettings?.GenerateAuthHandlersBasedOnCustomUserClass;
            var serviceName = $"ChangePasswordHandler";
            var serviceFileWriter = new CSFileWriter(
                CSFileWriterType.Class,
                codeGeneratorSettings.ApplicationSettings?.ApplicationNamespace,
                serviceName,
                isPartial: true,
                inheritance: $"ChangePasswordHandlerBase<{customUserClassName}, ChangePasswordRequest, ChangePasswordResponse>");
            serviceFileWriter.WriteUsing("System");
            serviceFileWriter.WriteUsing("KangarooNet.Application.Auth.Handlers");
            serviceFileWriter.WriteUsing("KangarooNet.Application.Auth.Services");
            serviceFileWriter.WriteUsing("KangarooNet.Application.Services");
            serviceFileWriter.WriteUsing("Microsoft.AspNetCore.Identity");
            serviceFileWriter.WriteUsing(codeGeneratorSettings.ApplicationSettings?.DatabaseEntitiesNamespace);
            serviceFileWriter.WriteUsing(codeGeneratorSettings.ApplicationSettings?.EntitiesNamespace);

            serviceFileWriter.WriteDependencyInjection($"UserManager<{customUserClassName}>", "userManager", shouldSendToConstructorBase: true);
            serviceFileWriter.WriteDependencyInjection("ICurrentUserService", "currentUserService", shouldSendToConstructorBase: true);
            sourceProductionContext.WriteNewCSFile(serviceName, serviceFileWriter);
        }

        private static void WriteGenerateTokenHandler(CodeGeneratorSettings codeGeneratorSettings, SourceProductionContext sourceProductionContext)
        {
            var customUserClassName = codeGeneratorSettings.ApplicationSettings?.GenerateAuthHandlersBasedOnCustomUserClass;
            var serviceName = $"GenerateTokenHandler";
            var serviceFileWriter = new CSFileWriter(
                CSFileWriterType.Class,
                codeGeneratorSettings.ApplicationSettings?.ApplicationNamespace,
                serviceName,
                isPartial: true,
                inheritance: $"GenerateTokenHandlerBase<{customUserClassName}>");
            serviceFileWriter.WriteUsing("System");
            serviceFileWriter.WriteUsing("KangarooNet.Domain.OptionsSettings");
            serviceFileWriter.WriteUsing("KangarooNet.Application.Auth.Handlers");
            serviceFileWriter.WriteUsing("Microsoft.AspNetCore.Identity");
            serviceFileWriter.WriteUsing("Microsoft.Extensions.Caching.Distributed");
            serviceFileWriter.WriteUsing("Microsoft.Extensions.Options");
            serviceFileWriter.WriteUsing(codeGeneratorSettings.ApplicationSettings?.DatabaseEntitiesNamespace);
            serviceFileWriter.WriteUsing(codeGeneratorSettings.ApplicationSettings?.EntitiesNamespace);

            serviceFileWriter.WriteDependencyInjection($"UserManager<{customUserClassName}>", "userManager", shouldSendToConstructorBase: true);
            serviceFileWriter.WriteDependencyInjection("IDistributedCache", "distributedCache", shouldSendToConstructorBase: true);
            serviceFileWriter.WriteDependencyInjection("IOptions<JwtOptions>", "jwtOptions", shouldSendToConstructorBase: true);
            sourceProductionContext.WriteNewCSFile(serviceName, serviceFileWriter);
        }

        private static void WriteLoginHandler(CodeGeneratorSettings codeGeneratorSettings, SourceProductionContext sourceProductionContext)
        {
            var customUserClassName = codeGeneratorSettings.ApplicationSettings?.GenerateAuthHandlersBasedOnCustomUserClass;
            var serviceName = $"LoginHandler";
            var serviceFileWriter = new CSFileWriter(
                CSFileWriterType.Class,
                codeGeneratorSettings.ApplicationSettings?.ApplicationNamespace,
                serviceName,
                isPartial: true,
                inheritance: $"LoginHandlerBase<{customUserClassName}, LoginRequest, LoginResponse>");
            serviceFileWriter.WriteUsing("System");
            serviceFileWriter.WriteUsing("KangarooNet.Application.Auth.Handlers");
            serviceFileWriter.WriteUsing("MediatR");
            serviceFileWriter.WriteUsing("Microsoft.AspNetCore.Identity");
            serviceFileWriter.WriteUsing(codeGeneratorSettings.ApplicationSettings?.DatabaseEntitiesNamespace);
            serviceFileWriter.WriteUsing(codeGeneratorSettings.ApplicationSettings?.EntitiesNamespace);

            serviceFileWriter.WriteDependencyInjection($"UserManager<{customUserClassName}>", "userManager", shouldSendToConstructorBase: true);
            serviceFileWriter.WriteDependencyInjection("IMediator", "mediator", shouldSendToConstructorBase: true);
            sourceProductionContext.WriteNewCSFile(serviceName, serviceFileWriter);
        }

        private static void WriteLoginValidatorHandler(CodeGeneratorSettings codeGeneratorSettings, SourceProductionContext sourceProductionContext)
        {
            var customUserClassName = codeGeneratorSettings.ApplicationSettings?.GenerateAuthHandlersBasedOnCustomUserClass;
            var serviceName = $"LoginValidatorHandler";
            var serviceFileWriter = new CSFileWriter(
                CSFileWriterType.Class,
                codeGeneratorSettings.ApplicationSettings?.ApplicationNamespace,
                serviceName,
                isPartial: true,
                inheritance: $"LoginValidatorHandlerBase<{customUserClassName}, LoginRequest>");
            serviceFileWriter.WriteUsing("System");
            serviceFileWriter.WriteUsing("KangarooNet.Application.Auth.Handlers");
            serviceFileWriter.WriteUsing("MediatR");
            serviceFileWriter.WriteUsing("Microsoft.AspNetCore.Identity");
            serviceFileWriter.WriteUsing(codeGeneratorSettings.ApplicationSettings?.DatabaseEntitiesNamespace);
            serviceFileWriter.WriteUsing(codeGeneratorSettings.ApplicationSettings?.EntitiesNamespace);

            serviceFileWriter.WriteDependencyInjection($"UserManager<{customUserClassName}>", "userManager", shouldSendToConstructorBase: true);
            sourceProductionContext.WriteNewCSFile(serviceName, serviceFileWriter);
        }

        private static void WriteLogoutHandler(CodeGeneratorSettings codeGeneratorSettings, SourceProductionContext sourceProductionContext)
        {
            var serviceName = $"LogoutHandler";
            var serviceFileWriter = new CSFileWriter(
                CSFileWriterType.Class,
                codeGeneratorSettings.ApplicationSettings?.ApplicationNamespace,
                serviceName,
                isPartial: true,
                inheritance: "LogoutHandlerBase<LogoutRequest, LogoutResponse>");
            serviceFileWriter.WriteUsing("System");
            serviceFileWriter.WriteUsing("KangarooNet.Application.Auth.Handlers");
            serviceFileWriter.WriteUsing("KangarooNet.Application.Auth.Services");
            serviceFileWriter.WriteUsing("KangarooNet.Application.Services");
            serviceFileWriter.WriteUsing("MediatR");
            serviceFileWriter.WriteUsing(codeGeneratorSettings.ApplicationSettings?.DatabaseEntitiesNamespace);
            serviceFileWriter.WriteUsing(codeGeneratorSettings.ApplicationSettings?.EntitiesNamespace);

            serviceFileWriter.WriteDependencyInjection("IMediator", "mediator", shouldSendToConstructorBase: true);
            serviceFileWriter.WriteDependencyInjection("ICurrentUserService", "currentUserService", shouldSendToConstructorBase: true);
            sourceProductionContext.WriteNewCSFile(serviceName, serviceFileWriter);
        }

        private static void WriteRefreshTokenHandler(CodeGeneratorSettings codeGeneratorSettings, SourceProductionContext sourceProductionContext)
        {
            var customUserClassName = codeGeneratorSettings.ApplicationSettings?.GenerateAuthHandlersBasedOnCustomUserClass;
            var serviceName = $"RefreshTokenHandler";
            var serviceFileWriter = new CSFileWriter(
                CSFileWriterType.Class,
                codeGeneratorSettings.ApplicationSettings?.ApplicationNamespace,
                serviceName,
                isPartial: true,
                inheritance: $"RefreshTokenHandlerBase<{customUserClassName}, RefreshTokenRequest, RefreshTokenResponse>");
            serviceFileWriter.WriteUsing("System");
            serviceFileWriter.WriteUsing("KangarooNet.Domain.OptionsSettings");
            serviceFileWriter.WriteUsing("KangarooNet.Application.Auth.Handlers");
            serviceFileWriter.WriteUsing("KangarooNet.Application.Auth.Services");
            serviceFileWriter.WriteUsing("KangarooNet.Application.Services");
            serviceFileWriter.WriteUsing("MediatR");
            serviceFileWriter.WriteUsing("Microsoft.AspNetCore.Identity");
            serviceFileWriter.WriteUsing("Microsoft.Extensions.Caching.Distributed");
            serviceFileWriter.WriteUsing("Microsoft.Extensions.Options");
            serviceFileWriter.WriteUsing(codeGeneratorSettings.ApplicationSettings?.DatabaseEntitiesNamespace);
            serviceFileWriter.WriteUsing(codeGeneratorSettings.ApplicationSettings?.EntitiesNamespace);

            serviceFileWriter.WriteDependencyInjection($"UserManager<{customUserClassName}>", "userManager", shouldSendToConstructorBase: true);
            serviceFileWriter.WriteDependencyInjection("IMediator", "mediator", shouldSendToConstructorBase: true);
            serviceFileWriter.WriteDependencyInjection("ICurrentUserService", "currentUserService", shouldSendToConstructorBase: true);
            serviceFileWriter.WriteDependencyInjection("IDistributedCache", "distributedCache", shouldSendToConstructorBase: true);
            serviceFileWriter.WriteDependencyInjection("IOptions<JwtOptions>", "jwtOptions", shouldSendToConstructorBase: true);
            sourceProductionContext.WriteNewCSFile(serviceName, serviceFileWriter);
        }

        private static void WriteEntityHandler(
            CodeGeneratorSettings codeGeneratorSettings,
            SourceProductionContext sourceProductionContext,
            Entity entity)
        {
            var databaseEntityName = string.IsNullOrEmpty(entity.GenerateEntityHandlerRequest?.GenerateEntityHandler?.DatabaseEntityName)
                ? GetDatabaseEntityNameWithPrefix(codeGeneratorSettings, entity.Name)
                : entity.GenerateEntityHandlerRequest.GenerateEntityHandler.DatabaseEntityName;
            var entityName = entity.Name;
            var handlerRequestName = $"{entity.Name}HandlerRequest";
            var handlerResponseName = $"{entity.Name}HandlerResponse";

            var handlerName = $"{entityName}Handler";
            var handlerFileWriter = new CSFileWriter(
                CSFileWriterType.Class,
                codeGeneratorSettings.ApplicationSettings?.ApplicationNamespace,
                handlerName,
                isPartial: true,
                inheritance: $"DatabaseEntityHandlerBase<ApplicationDbContext, {databaseEntityName}, {entityName}, {handlerRequestName}, {handlerResponseName}>");

            handlerFileWriter.WriteUsing("System");
            handlerFileWriter.WriteUsing("KangarooNet.Application.Handlers");
            handlerFileWriter.WriteUsing("KangarooNet.Application.Services");
            handlerFileWriter.WriteUsing("AutoMapper");
            handlerFileWriter.WriteUsing(codeGeneratorSettings.ApplicationSettings?.DbContextNamespace);
            handlerFileWriter.WriteUsing(codeGeneratorSettings.ApplicationSettings?.DatabaseRepositoriesNamespace);
            handlerFileWriter.WriteUsing(codeGeneratorSettings.ApplicationSettings?.DatabaseEntitiesNamespace);
            handlerFileWriter.WriteUsing(codeGeneratorSettings.ApplicationSettings?.EntitiesNamespace);

            handlerFileWriter.WriteDependencyInjection("IApplicationDatabaseRepository", "databaseRepository", shouldSendToConstructorBase: true);
            handlerFileWriter.WriteDependencyInjection("IMapper", "mapper", shouldSendToConstructorBase: true);
            handlerFileWriter.WriteDependencyInjection("ICurrentUserService", "currentUserService", shouldSendToConstructorBase: true);

            sourceProductionContext.WriteNewCSFile(handlerName, handlerFileWriter);
        }

        private static string GetDatabaseEntityNameWithPrefix(CodeGeneratorSettings codeGeneratorSettings, string databaseEntityName) => codeGeneratorSettings?.ApplicationSettings?.DatabaseEntityPrefix + databaseEntityName;
    }
}
