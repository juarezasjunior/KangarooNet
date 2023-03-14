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

    internal static class CustomResponsesCodeWriter
    {
        public static void Generate(CodeGeneratorSettings codeGeneratorSettings, List<CodeGenerator> codeGenerators, SourceProductionContext sourceProductionContext)
        {
            foreach (var codeGenerator in codeGenerators)
            {
                foreach (var customResponse in codeGenerator.CustomResponse)
                {
                    if (codeGeneratorSettings.BackendCustomResponsesSettings != null
                        && (customResponse.Location == Structure.Location.Both || customResponse.Location == Structure.Location.Backend))
                    {
                        WriteResponse(codeGeneratorSettings, sourceProductionContext, customResponse, true);
                    }

                    if (codeGeneratorSettings.FrontendCustomResponsesSettings != null
                        && (customResponse.Location == Structure.Location.Both || customResponse.Location == Structure.Location.Frontend))
                    {
                        WriteResponse(codeGeneratorSettings, sourceProductionContext, customResponse, false);
                    }
                }
            }
        }

        private static void WriteResponse(CodeGeneratorSettings codeGeneratorSettings, SourceProductionContext sourceProductionContext, CustomResponse customResponse, bool isBackend)
        {
            var className = $"{customResponse.Name}Response";
            var currentLocation = isBackend ? Structure.Location.Backend : Structure.Location.Frontend;
            var keyField = customResponse.ResponseFields?.KeyField;
            var keyType = keyField?.KeyType;
            var inheritance = "IEndpointResponse";
            var classNamespace = isBackend ? codeGeneratorSettings.BackendCustomResponsesSettings?.CustomResponsesNamespace : codeGeneratorSettings.FrontendCustomResponsesSettings?.CustomResponsesNamespace;
            var validatorNamespace = isBackend ? codeGeneratorSettings.BackendCustomResponsesSettings?.ValidatorsNamespace : codeGeneratorSettings.FrontendCustomResponsesSettings?.ValidatorsNamespace;
            var shouldGenerateNotifyPropertyChanges = isBackend ? false : codeGeneratorSettings.FrontendCustomResponsesSettings?.GenerateNotifyPropertyChanges ?? false;
            var useObservableCollection = isBackend ? false : codeGeneratorSettings.FrontendCustomResponsesSettings?.UseObservableCollection ?? false;

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

            var fileWriter = new CSFileWriter(
                    CSFileWriterType.Class,
                    classNamespace,
                    className,
                    isPartial: true,
                    inheritance: inheritance);

            fileWriter.WriteUsing("System");
            fileWriter.WriteUsing("KangarooNet.Domain");
            fileWriter.WriteUsing("KangarooNet.Domain.Entities");

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

            if (customResponse.AdditionalUsings?.Using != null)
            {
                foreach (var customUsing in customResponse.AdditionalUsings.Using)
                {
                    fileWriter.WriteUsing(customUsing.Content);
                }
            }

            if (customResponse.CustomAttributes?.CustomAttribute != null)
            {
                foreach (var classAttribute in customResponse.CustomAttributes.CustomAttribute)
                {
                    fileWriter.WriteClassAttribute(classAttribute.Attribute);
                }
            }

            customResponse.ResponseFields?.HandleFields(EntityFieldCodeWriter.WriteField(fileWriter, validatorFileWriter, shouldGenerateNotifyPropertyChanges, useObservableCollection, currentLocation));

            EntityFieldCodeWriter.WriteKeyField(customResponse.ResponseFields?.KeyField, fileWriter, currentLocation);

            validatorFileWriter.WriteConstructorAdditionalBodyLine($"this.SetCustomRules();");

            sourceProductionContext.WriteNewCSFile(className, fileWriter);
            sourceProductionContext.WriteNewCSFile(validatorClassName, validatorFileWriter);
        }
    }
}
