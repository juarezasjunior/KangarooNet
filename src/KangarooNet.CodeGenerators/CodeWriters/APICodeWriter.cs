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

    internal static class APICodeWriter
    {
        public static void Generate(CodeGeneratorSettings codeGeneratorSettings, List<CodeGenerator> codeGenerators, SourceProductionContext sourceProductionContext)
        {
            foreach (var codeGenerator in codeGenerators)
            {
                foreach (var entity in codeGenerator.Entity)
                {
                    if (codeGeneratorSettings.APISettings != null)
                    {
                        GenerateControllers(codeGeneratorSettings, sourceProductionContext, entity);
                    }
                }

                foreach (var summary in codeGenerator.Summary)
                {
                    if (codeGeneratorSettings.APISettings != null)
                    {
                        GenerateControllers(codeGeneratorSettings, sourceProductionContext, summary);
                    }
                }
            }

            if (codeGeneratorSettings.APISettings?.GenerateAuthController == true)
            {
                GenerateAuthController(codeGeneratorSettings, sourceProductionContext);
            }
        }

        private static void GenerateControllers(CodeGeneratorSettings codeGeneratorSettings, SourceProductionContext sourceProductionContext, Entity entity)
        {
            if (entity.GenerateEntityHandlerRequest?.GenerateController == null
                && entity.GenerateEntityQueryRequest?.GenerateController == null
                && entity.GenerateEntitiesQueryRequest?.GenerateController == null)
            {
                return;
            }

            var className = $"{entity.Name}Controller";
            var controllerFileWriter = new CSFileWriter(
                CSFileWriterType.Class,
                codeGeneratorSettings.APISettings?.ControllersNamespace,
                className,
                isPartial: true,
                inheritance: "ControllerBase");

            controllerFileWriter.WriteClassAttribute("ApiController");
            controllerFileWriter.WriteClassAttribute("Route(\"/api/[controller]/[action]\")");

            controllerFileWriter.WriteUsing("System");
            controllerFileWriter.WriteUsing("System.Threading.Tasks");
            controllerFileWriter.WriteUsing("MediatR");
            controllerFileWriter.WriteUsing("Microsoft.AspNetCore.Authorization");
            controllerFileWriter.WriteUsing("Microsoft.AspNetCore.Mvc");
            controllerFileWriter.WriteUsing(codeGeneratorSettings.APISettings?.ApplicationNamespace);
            controllerFileWriter.WriteUsing(codeGeneratorSettings.APISettings?.EntitiesNamespace);

            controllerFileWriter.WriteDependencyInjection("IMediator", "mediator");

            if (entity.GenerateEntityHandlerRequest?.GenerateController != null)
            {
                WriteEntityHandlerController(controllerFileWriter, entity);
            }

            if (entity.GenerateEntityQueryRequest?.GenerateController != null)
            {
                WriteEntityQueryController(controllerFileWriter, entity);
            }

            if (entity.GenerateEntitiesQueryRequest?.GenerateController != null)
            {
                WriteEntitiesQueryController(controllerFileWriter, entity);
            }

            sourceProductionContext.WriteNewCSFile(className, controllerFileWriter);
        }

        private static void GenerateControllers(CodeGeneratorSettings codeGeneratorSettings, SourceProductionContext sourceProductionContext, Summary summary)
        {
            var summaryName = summary.Name;
            var className = $"{summaryName}Controller";
            var controllerFileWriter = new CSFileWriter(
                CSFileWriterType.Class,
                codeGeneratorSettings.APISettings?.ControllersNamespace,
                className,
                isPartial: true,
                inheritance: "ControllerBase");

            controllerFileWriter.WriteClassAttribute("ApiController");
            controllerFileWriter.WriteClassAttribute("Route(\"/api/[controller]/[action]\")");

            controllerFileWriter.WriteUsing("System");
            controllerFileWriter.WriteUsing("System.Threading.Tasks");
            controllerFileWriter.WriteUsing("MediatR");
            controllerFileWriter.WriteUsing("Microsoft.AspNetCore.Authorization");
            controllerFileWriter.WriteUsing("Microsoft.AspNetCore.Mvc");
            controllerFileWriter.WriteUsing(codeGeneratorSettings.APISettings?.ApplicationNamespace);
            controllerFileWriter.WriteUsing(codeGeneratorSettings.APISettings?.EntitiesNamespace);

            controllerFileWriter.WriteDependencyInjection("IMediator", "mediator");

            if (summary.GenerateSummaryQueryRequest?.GenerateController != null)
            {
                WriteSummaryQueryController(controllerFileWriter, summary);
            }

            if (summary.GenerateSummariesQueryRequest?.GenerateController != null)
            {
                WriteSummariesQueryController(controllerFileWriter, summary);
            }

            sourceProductionContext.WriteNewCSFile(className, controllerFileWriter);
        }

        private static void GenerateAuthController(CodeGeneratorSettings codeGeneratorSettings, SourceProductionContext sourceProductionContext)
        {
            var className = $"AuthController";
            var controllerFileWriter = new CSFileWriter(
                CSFileWriterType.Class,
                codeGeneratorSettings.APISettings?.ControllersNamespace,
                className,
                isPartial: true,
                inheritance: "ControllerBase");

            controllerFileWriter.WriteClassAttribute("ApiController");
            controllerFileWriter.WriteClassAttribute("Route(\"/api/[controller]/[action]\")");

            controllerFileWriter.WriteUsing("System");
            controllerFileWriter.WriteUsing("System.Threading.Tasks");
            controllerFileWriter.WriteUsing("MediatR");
            controllerFileWriter.WriteUsing("Microsoft.AspNetCore.Authorization");
            controllerFileWriter.WriteUsing("Microsoft.AspNetCore.Mvc");
            controllerFileWriter.WriteUsing(codeGeneratorSettings.APISettings?.ApplicationNamespace);
            controllerFileWriter.WriteUsing(codeGeneratorSettings.APISettings?.EntitiesNamespace);

            controllerFileWriter.WriteDependencyInjection("IMediator", "mediator");

            var insertApplicationUserMethodLines = new List<string>();
            insertApplicationUserMethodLines.Add("return Ok(await this.mediator.Send(request, cancellationToken));");
            controllerFileWriter.WriteMethod(
                "InsertApplicationUserAsync",
                returnType: "async Task<IActionResult>",
                parameters: "[FromBody] ApplicationUserInsertRequest request, CancellationToken cancellationToken = default",
                attributes: new List<string>() { "HttpPost", "AllowAnonymous" },
                bodyLines: insertApplicationUserMethodLines);

            var loginMethodLines = new List<string>();
            loginMethodLines.Add("return Ok(await this.mediator.Send(request, cancellationToken));");
            controllerFileWriter.WriteMethod(
                "LoginAsync",
                returnType: "async Task<IActionResult>",
                parameters: "[FromBody] LoginRequest request, CancellationToken cancellationToken = default",
                attributes: new List<string>() { "HttpPost", "AllowAnonymous" },
                bodyLines: loginMethodLines);

            var refreshTokenMethodLines = new List<string>();
            refreshTokenMethodLines.Add("return Ok(await this.mediator.Send(request, cancellationToken));");
            controllerFileWriter.WriteMethod(
                "RefreshTokenAsync",
                returnType: "async Task<IActionResult>",
                parameters: "[FromBody] RefreshTokenRequest request, CancellationToken cancellationToken = default",
                attributes: new List<string>() { "HttpPost", "Authorize()" },
                bodyLines: refreshTokenMethodLines);

            var logoutMethodLines = new List<string>();
            logoutMethodLines.Add("return Ok(await this.mediator.Send(request, cancellationToken));");
            controllerFileWriter.WriteMethod(
                "LogoutAsync",
                returnType: "async Task<IActionResult>",
                parameters: "[FromBody] LogoutRequest request, CancellationToken cancellationToken = default",
                attributes: new List<string>() { "HttpPost", "Authorize()" },
                bodyLines: logoutMethodLines);

            var changePasswordMethodLines = new List<string>();
            changePasswordMethodLines.Add("return Ok(await this.mediator.Send(request, cancellationToken));");
            controllerFileWriter.WriteMethod(
                "ChangePasswordAsync",
                returnType: "async Task<IActionResult>",
                parameters: "[FromBody] ChangePasswordRequest request, CancellationToken cancellationToken = default",
                attributes: new List<string>() { "HttpPost", "Authorize()" },
                bodyLines: changePasswordMethodLines);

            sourceProductionContext.WriteNewCSFile(className, controllerFileWriter);
        }

        private static void WriteEntityHandlerController(
            CSFileWriter controllerFileWriter,
            Entity entity)
        {
            if (entity.GenerateEntityHandlerRequest.GenerateController.AdditionalUsings?.Using != null)
            {
                foreach (var customUsing in entity.GenerateEntityHandlerRequest.GenerateController.AdditionalUsings.Using)
                {
                    controllerFileWriter.WriteUsing(customUsing.Content);
                }
            }

            if (entity.GenerateEntityHandlerRequest.GenerateController.CustomAttributes?.CustomAttribute != null)
            {
                foreach (var classAttribute in entity.GenerateEntityHandlerRequest.GenerateController.CustomAttributes.CustomAttribute)
                {
                    controllerFileWriter.WriteClassAttribute(classAttribute.Attribute);
                }
            }

            var bodyLines = new List<string>();
            bodyLines.Add("return Ok(await this.mediator.Send(request, cancellationToken));");

            var attributes = new List<string>();
            attributes.Add("HttpPost");

            if (entity.GenerateEntityHandlerRequest.GenerateController.IsAuthenticationRequired)
            {
                if (entity.GenerateEntityHandlerRequest.GenerateController.Permissions != null)
                {
                    foreach (var permission in entity.GenerateEntityHandlerRequest.GenerateController.Permissions.Permission)
                    {
                        attributes.Add($"Authorize(Roles = \"{permission.Name}\")");
                    }
                }
                else
                {
                    attributes.Add($"Authorize()");
                }
            }

            controllerFileWriter.WriteMethod(
                "PostAsync",
                returnType: "async Task<IActionResult>",
                parameters: $"[FromBody] {entity.Name}HandlerRequest request, CancellationToken cancellationToken = default",
                attributes: attributes,
                bodyLines: bodyLines);
        }

        private static void WriteEntityQueryController(
            CSFileWriter controllerFileWriter,
            Entity entity)
        {
            if (entity.GenerateEntityQueryRequest.GenerateController.AdditionalUsings?.Using != null)
            {
                foreach (var customUsing in entity.GenerateEntityQueryRequest.GenerateController.AdditionalUsings.Using)
                {
                    controllerFileWriter.WriteUsing(customUsing.Content);
                }
            }

            if (entity.GenerateEntityQueryRequest.GenerateController.CustomAttributes?.CustomAttribute != null)
            {
                foreach (var classAttribute in entity.GenerateEntityQueryRequest.GenerateController.CustomAttributes.CustomAttribute)
                {
                    controllerFileWriter.WriteClassAttribute(classAttribute.Attribute);
                }
            }

            var bodyLines = new List<string>();
            bodyLines.Add("return Ok(await this.mediator.Send(request, cancellationToken));");

            var attributes = new List<string>();
            attributes.Add("HttpGet");

            if (entity.GenerateEntityQueryRequest.GenerateController.IsAuthenticationRequired)
            {
                if (entity.GenerateEntityQueryRequest.GenerateController.Permissions != null)
                {
                    foreach (var permission in entity.GenerateEntityQueryRequest.GenerateController.Permissions.Permission)
                    {
                        attributes.Add($"Authorize(Roles = \"{permission.Name}\")");
                    }
                }
                else
                {
                    attributes.Add($"Authorize()");
                }
            }

            controllerFileWriter.WriteMethod(
                "GetEntityAsync",
                returnType: "async Task<IActionResult>",
                parameters: $"[FromQuery] {entity.Name}QueryRequest request, CancellationToken cancellationToken = default",
                attributes: attributes,
                bodyLines: bodyLines);
        }

        private static void WriteEntitiesQueryController(
            CSFileWriter controllerFileWriter,
            Entity entity)
        {
            if (entity.GenerateEntitiesQueryRequest.GenerateController.AdditionalUsings?.Using != null)
            {
                foreach (var customUsing in entity.GenerateEntitiesQueryRequest.GenerateController.AdditionalUsings.Using)
                {
                    controllerFileWriter.WriteUsing(customUsing.Content);
                }
            }

            if (entity.GenerateEntitiesQueryRequest.GenerateController.CustomAttributes?.CustomAttribute != null)
            {
                foreach (var classAttribute in entity.GenerateEntitiesQueryRequest.GenerateController.CustomAttributes.CustomAttribute)
                {
                    controllerFileWriter.WriteClassAttribute(classAttribute.Attribute);
                }
            }

            var bodyLines = new List<string>();
            bodyLines.Add("return Ok(await this.mediator.Send(request, cancellationToken));");

            var attributes = new List<string>();
            attributes.Add("HttpGet");

            if (entity.GenerateEntitiesQueryRequest.GenerateController.IsAuthenticationRequired)
            {
                if (entity.GenerateEntitiesQueryRequest.GenerateController.Permissions != null)
                {
                    foreach (var permission in entity.GenerateEntitiesQueryRequest.GenerateController.Permissions.Permission)
                    {
                        attributes.Add($"Authorize(Roles = \"{permission.Name}\")");
                    }
                }
                else
                {
                    attributes.Add($"Authorize()");
                }
            }

            controllerFileWriter.WriteMethod(
                "GetEntitiesAsync",
                returnType: "async Task<IActionResult>",
                parameters: $"[FromQuery] {entity.PluralName}QueryRequest request, CancellationToken cancellationToken = default",
                attributes: attributes,
                bodyLines: bodyLines);
        }

        private static void WriteSummaryQueryController(
            CSFileWriter controllerFileWriter,
            Summary summary)
        {
            if (summary.GenerateSummaryQueryRequest.GenerateController.AdditionalUsings?.Using != null)
            {
                foreach (var customUsing in summary.GenerateSummaryQueryRequest.GenerateController.AdditionalUsings.Using)
                {
                    controllerFileWriter.WriteUsing(customUsing.Content);
                }
            }

            if (summary.GenerateSummaryQueryRequest.GenerateController.CustomAttributes?.CustomAttribute != null)
            {
                foreach (var classAttribute in summary.GenerateSummaryQueryRequest.GenerateController.CustomAttributes.CustomAttribute)
                {
                    controllerFileWriter.WriteClassAttribute(classAttribute.Attribute);
                }
            }

            var bodyLines = new List<string>();
            bodyLines.Add("return Ok(await this.mediator.Send(request, cancellationToken));");

            var attributes = new List<string>();
            attributes.Add("HttpGet");

            if (summary.GenerateSummaryQueryRequest.GenerateController.IsAuthenticationRequired)
            {
                if (summary.GenerateSummaryQueryRequest.GenerateController.Permissions != null)
                {
                    foreach (var permission in summary.GenerateSummaryQueryRequest.GenerateController.Permissions.Permission)
                    {
                        attributes.Add($"Authorize(Roles = \"{permission.Name}\")");
                    }
                }
                else
                {
                    attributes.Add($"Authorize()");
                }
            }

            controllerFileWriter.WriteMethod(
                "GetSummaryAsync",
                returnType: "async Task<IActionResult>",
                parameters: $"[FromQuery] {summary.Name}QueryRequest request, CancellationToken cancellationToken = default",
                attributes: attributes,
                bodyLines: bodyLines);
        }

        private static void WriteSummariesQueryController(
            CSFileWriter controllerFileWriter,
            Summary summary)
        {
            if (summary.GenerateSummariesQueryRequest.GenerateController.AdditionalUsings?.Using != null)
            {
                foreach (var customUsing in summary.GenerateSummariesQueryRequest.GenerateController.AdditionalUsings.Using)
                {
                    controllerFileWriter.WriteUsing(customUsing.Content);
                }
            }

            if (summary.GenerateSummariesQueryRequest.GenerateController.CustomAttributes?.CustomAttribute != null)
            {
                foreach (var classAttribute in summary.GenerateSummariesQueryRequest.GenerateController.CustomAttributes.CustomAttribute)
                {
                    controllerFileWriter.WriteClassAttribute(classAttribute.Attribute);
                }
            }

            var bodyLines = new List<string>();
            bodyLines.Add("return Ok(await this.mediator.Send(request, cancellationToken));");

            var attributes = new List<string>();
            attributes.Add("HttpGet");

            if (summary.GenerateSummariesQueryRequest.GenerateController.IsAuthenticationRequired)
            {
                if (summary.GenerateSummariesQueryRequest.GenerateController.Permissions != null)
                {
                    foreach (var permission in summary.GenerateSummariesQueryRequest.GenerateController.Permissions.Permission)
                    {
                        attributes.Add($"Authorize(Roles = \"{permission.Name}\")");
                    }
                }
                else
                {
                    attributes.Add($"Authorize()");
                }
            }

            controllerFileWriter.WriteMethod(
                "GetSummariesAsync",
                returnType: "async Task<IActionResult>",
                parameters: $"[FromQuery] {summary.PluralName}QueryRequest request, CancellationToken cancellationToken = default",
                attributes: attributes,
                bodyLines: bodyLines);
        }
    }
}
