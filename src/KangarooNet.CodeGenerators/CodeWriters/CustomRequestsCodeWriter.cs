// Copyright Contributors to the KangarooNet project.
// This file is licensed to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICE files in the project root for full license information.

namespace KangarooNet.CodeGenerators.CodeWriters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using KangarooNet.CodeGenerators.Extensions;
    using KangarooNet.CodeGenerators.Structure;
    using KangarooNet.CodeGenerators.Writers;
    using Microsoft.CodeAnalysis;

    internal static class CustomRequestsCodeWriter
    {
        public static void Generate(CodeGeneratorSettings codeGeneratorSettings, List<CodeGenerator> codeGenerators, SourceProductionContext sourceProductionContext)
        {
            foreach (var codeGenerator in codeGenerators)
            {
                foreach (var customRequest in codeGenerator.CustomRequest)
                {
                    if (codeGeneratorSettings.BackendCustomRequestsSettings != null
                        && (customRequest.Location == Structure.Location.Both || customRequest.Location == Structure.Location.Backend))
                    {
                        WriteRequest(codeGeneratorSettings, sourceProductionContext, customRequest, true);
                    }

                    if (codeGeneratorSettings.FrontendCustomRequestsSettings != null
                        && (customRequest.Location == Structure.Location.Both || customRequest.Location == Structure.Location.Frontend))
                    {
                        WriteRequest(codeGeneratorSettings, sourceProductionContext, customRequest, false);
                    }
                }
            }
        }

        private static void WriteRequest(CodeGeneratorSettings codeGeneratorSettings, SourceProductionContext sourceProductionContext, CustomRequest customRequest, bool isBackend)
        {
            var className = $"{customRequest.Name}Request";
            var currentLocation = isBackend ? Structure.Location.Backend : Structure.Location.Frontend;
            var keyField = customRequest.RequestFields?.KeyField;
            var keyType = keyField?.KeyType;
            var inheritance = "IEndpointRequest";
            var classNamespace = isBackend ? codeGeneratorSettings.BackendCustomRequestsSettings?.CustomRequestsNamespace : codeGeneratorSettings.FrontendCustomRequestsSettings?.CustomRequestsNamespace;
            var validatorNamespace = isBackend ? codeGeneratorSettings.BackendCustomRequestsSettings?.ValidatorsNamespace : codeGeneratorSettings.FrontendCustomRequestsSettings?.ValidatorsNamespace;
            var shouldGenerateNotifyPropertyChanges = isBackend ? false : codeGeneratorSettings.FrontendCustomRequestsSettings?.GenerateNotifyPropertyChanges ?? false;
            var useObservableCollection = isBackend ? false : codeGeneratorSettings.FrontendCustomRequestsSettings?.UseObservableCollection ?? false;

            if (keyType.HasValue)
            {
                switch (keyType)
                {
                    case KeyType.Int:
                        inheritance += ", IHasIntegerKey";
                        break;
                    case KeyType.Guid:
                        inheritance += ", IHasGuidKey";
                        break;
                    default:
                        break;
                }
            }

            var useMediator = isBackend && customRequest.IncludeCommandInterface;

            if (useMediator)
            {
                inheritance += $", IRequest<{customRequest.Name}Response>";
            }

            var fileWriter = new CSFileWriter(
                    CSFileWriterType.Class,
                    classNamespace,
                    className,
                    isPartial: true,
                    inheritance: inheritance);

            fileWriter.WriteUsing("System");
            fileWriter.WriteUsing("KangarooNet.Domain");
            fileWriter.WriteUsing("KangarooNet.Domain.Entities");

            if (useMediator)
            {
                fileWriter.WriteUsing("MediatR");
            }

            var validatorClassName = $"{className}Validator";
            var validatorFileWriter = new CSFileWriter(
                    CSFileWriterType.Class,
                    validatorNamespace,
                    validatorClassName,
                    isPartial: true,
                    inheritance: $"AbstractValidator<{className}>");

            validatorFileWriter.WriteUsing("System");
            validatorFileWriter.WriteUsing("FluentValidation");
            validatorFileWriter.WriteUsing("KangarooNet.Domain.Entities");
            validatorFileWriter.WriteUsing(classNamespace);

            validatorFileWriter.WriteMethod("SetCustomRules", isPartial: true);

            if (customRequest.AdditionalUsings?.Using != null)
            {
                foreach (var customUsing in customRequest.AdditionalUsings.Using)
                {
                    fileWriter.WriteUsing(customUsing.Content);
                }
            }

            if (customRequest.CustomAttributes?.CustomAttribute != null)
            {
                foreach (var classAttribute in customRequest.CustomAttributes.CustomAttribute)
                {
                    fileWriter.WriteClassAttribute(classAttribute.Attribute);
                }
            }

            customRequest.RequestFields?.HandleFields(EntityFieldCodeWriter.WriteField(fileWriter, validatorFileWriter, shouldGenerateNotifyPropertyChanges, useObservableCollection, currentLocation));

            EntityFieldCodeWriter.WriteKeyField(customRequest.RequestFields?.KeyField, fileWriter, currentLocation);

            validatorFileWriter.WriteConstructorAdditionalBodyLine($"this.SetCustomRules();");

            sourceProductionContext.WriteNewCSFile(className, fileWriter);
            sourceProductionContext.WriteNewCSFile(validatorClassName, validatorFileWriter);
        }
    }
}
