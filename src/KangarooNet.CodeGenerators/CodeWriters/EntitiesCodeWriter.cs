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

    internal static class EntitiesCodeWriter
    {
        public static void Generate(CodeGeneratorSettings codeGeneratorSettings, List<CodeGenerator> codeGenerators, SourceProductionContext sourceProductionContext)
        {
            foreach (var codeGenerator in codeGenerators)
            {
                foreach (var entity in codeGenerator.Entity)
                {
                    if (codeGeneratorSettings.BackendEntititesSettings != null
                        && (entity.Location == Structure.Location.Both || entity.Location == Structure.Location.Backend))
                    {
                        GenerateEntities(codeGeneratorSettings, sourceProductionContext, entity, true);
                    }

                    if (codeGeneratorSettings.FrontendEntititesSettings != null
                        && (entity.Location == Structure.Location.Both || entity.Location == Structure.Location.Frontend))
                    {
                        GenerateEntities(codeGeneratorSettings, sourceProductionContext, entity, false);
                    }
                }

                foreach (var summary in codeGenerator.Summary)
                {
                    if (codeGeneratorSettings.BackendEntititesSettings != null
                        && (summary.Location == Structure.Location.Both || summary.Location == Structure.Location.Backend))
                    {
                        GenerateSummaries(codeGeneratorSettings, sourceProductionContext, summary, true);
                    }

                    if (codeGeneratorSettings.FrontendEntititesSettings != null
                        && (summary.Location == Structure.Location.Both || summary.Location == Structure.Location.Frontend))
                    {
                        GenerateSummaries(codeGeneratorSettings, sourceProductionContext, summary, false);
                    }
                }
            }

            if (codeGeneratorSettings.BackendEntititesSettings?.GenerateAuthEntities == true)
            {
                GenerateAuthEntities(codeGeneratorSettings, sourceProductionContext, true);
            }

            if (codeGeneratorSettings.FrontendEntititesSettings?.GenerateAuthEntities == true)
            {
                GenerateAuthEntities(codeGeneratorSettings, sourceProductionContext, false);
            }
        }

        private static void GenerateEntities(CodeGeneratorSettings codeGeneratorSettings, SourceProductionContext sourceProductionContext, Entity entity, bool isBackend)
        {
            WriteEntity(
                codeGeneratorSettings,
                sourceProductionContext,
                entityName: entity.Name,
                defaultInheritance: "IEntity",
                additionalUsings: entity.AdditionalUsings,
                customAttributes: entity.CustomAttributes,
                fields: entity.EntityFields,
                includeDataState: entity.IncludeDataState,
                includeRowVersionControl: entity.IncludeRowVersionControl,
                includeAuditLog: entity.IncludeAuditLog,
                isBackend: isBackend);

            if (entity.GenerateEntityHandlerRequest != null)
            {
                WriteRequestResponse(
                    codeGeneratorSettings,
                    sourceProductionContext,
                    isRequest: true,
                    classPrefix: $"{entity.Name}Handler",
                    inheritance: $"IEntityHandlerRequest<{entity.Name}>",
                    additionalUsings: entity.GenerateEntityHandlerRequest.AdditionalUsings,
                    customAttributes: entity.GenerateEntityHandlerRequest.CustomAttributes,
                    fields: entity.GenerateEntityHandlerRequest.AdditionalFields,
                    isBackend: isBackend,
                    entityPropertyType: entity.Name,
                    entityPropertyName: "Entity",
                    entityPropertyHasValidator: true);

                WriteRequestResponse(
                    codeGeneratorSettings,
                    sourceProductionContext,
                    isRequest: false,
                    classPrefix: $"{entity.Name}Handler",
                    inheritance: $"IEntityHandlerResponse<{entity.Name}>",
                    additionalUsings: entity.GenerateEntityHandlerRequest.AdditionalUsings,
                    customAttributes: entity.GenerateEntityHandlerRequest.CustomAttributes,
                    fields: null,
                    isBackend: isBackend,
                    entityPropertyType: entity.Name,
                    entityPropertyName: "Entity");
            }

            if (entity.GenerateEntityQueryRequest != null)
            {
                var entityQueryRequestInheritance = "IEntityQueryRequest";
                var entityQueryRequestFields = entity.GenerateEntityQueryRequest.AdditionalFields;

                if (entity.EntityFields?.KeyField?.KeyType != null)
                {
                    entityQueryRequestFields = entityQueryRequestFields ?? new Fields();

                    entityQueryRequestFields.KeyField = entity.EntityFields?.KeyField;

                    switch (entity.EntityFields?.KeyField?.KeyType)
                    {
                        case KeyType.Int:
                            entityQueryRequestInheritance += ", IHasIntegerKey";
                            break;
                        case KeyType.Guid:
                            entityQueryRequestInheritance += ", IHasGuidKey";
                            break;
                        default:
                            break;
                    }
                }

                WriteRequestResponse(
                    codeGeneratorSettings,
                    sourceProductionContext,
                    isRequest: true,
                    classPrefix: $"{entity.Name}Query",
                    inheritance: entityQueryRequestInheritance,
                    additionalUsings: entity.GenerateEntityQueryRequest.AdditionalUsings,
                    customAttributes: entity.GenerateEntityQueryRequest.CustomAttributes,
                    fields: entityQueryRequestFields,
                    isBackend: isBackend);

                WriteRequestResponse(
                    codeGeneratorSettings,
                    sourceProductionContext,
                    isRequest: false,
                    classPrefix: $"{entity.Name}Query",
                    inheritance: $"IEntityQueryResponse<{entity.Name}>",
                    additionalUsings: entity.GenerateEntityQueryRequest.AdditionalUsings,
                    customAttributes: entity.GenerateEntityQueryRequest.CustomAttributes,
                    fields: null,
                    isBackend: isBackend,
                    entityPropertyType: entity.Name,
                    entityPropertyName: "Entity");
            }

            if (entity.GenerateEntitiesQueryRequest != null)
            {
                WriteRequestResponse(
                    codeGeneratorSettings,
                    sourceProductionContext,
                    isRequest: true,
                    classPrefix: $"{entity.PluralName}Query",
                    inheritance: "IEntitiesQueryRequest",
                    additionalUsings: entity.GenerateEntityQueryRequest.AdditionalUsings,
                    customAttributes: entity.GenerateEntityQueryRequest.CustomAttributes,
                    fields: entity.GenerateEntityQueryRequest.AdditionalFields,
                    isBackend: isBackend);

                var useObservableCollection = isBackend ? false : codeGeneratorSettings.FrontendEntititesSettings?.UseObservableCollection ?? false;
                var collectionType = useObservableCollection ? "ObservableCollection" : "IList";
                var collectionTypeInstantiateCommand = useObservableCollection ? "new ObservableCollection" : "new List";

                WriteRequestResponse(
                    codeGeneratorSettings,
                    sourceProductionContext,
                    isRequest: false,
                    classPrefix: $"{entity.PluralName}Query",
                    inheritance: $"IEntitiesQueryResponse<{entity.Name}, {collectionType}<{entity.Name}>>",
                    additionalUsings: entity.GenerateEntityQueryRequest.AdditionalUsings,
                    customAttributes: entity.GenerateEntityQueryRequest.CustomAttributes,
                    fields: null,
                    isBackend: isBackend,
                    entityPropertyType: $"{collectionType}<{entity.Name}>",
                    entityPropertyName: "Entities",
                    entityPropertyValue: $"{collectionTypeInstantiateCommand}<{entity.Name}>()",
                    entityPropertyHasValidator: false,
                    entityPropertyIsObservableCollection: useObservableCollection);
            }
        }

        private static void GenerateSummaries(CodeGeneratorSettings codeGeneratorSettings, SourceProductionContext sourceProductionContext, Summary summary, bool isBackend)
        {
            WriteEntity(
                codeGeneratorSettings,
                sourceProductionContext,
                entityName: summary.Name,
                defaultInheritance: "ISummary",
                additionalUsings: summary.AdditionalUsings,
                customAttributes: summary.CustomAttributes,
                fields: summary.SummaryFields,
                includeDataState: false,
                includeRowVersionControl: summary.IncludeRowVersionControl,
                includeAuditLog: summary.IncludeAuditLog,
                isBackend: isBackend);

            if (summary.GenerateSummaryQueryRequest != null)
            {
                var summaryQueryRequestInheritance = "ISummaryQueryRequest";
                var summaryQueryRequestFields = summary.GenerateSummaryQueryRequest.AdditionalFields;

                if (summary.SummaryFields?.KeyField?.KeyType != null)
                {
                    summaryQueryRequestFields = summaryQueryRequestFields ?? new Fields();

                    summaryQueryRequestFields.KeyField = summary.SummaryFields?.KeyField;

                    switch (summary.SummaryFields?.KeyField?.KeyType)
                    {
                        case KeyType.Int:
                            summaryQueryRequestInheritance += ", IHasIntegerKey";
                            break;
                        case KeyType.Guid:
                            summaryQueryRequestInheritance += ", IHasGuidKey";
                            break;
                        default:
                            break;
                    }
                }

                WriteRequestResponse(
                    codeGeneratorSettings,
                    sourceProductionContext,
                    isRequest: true,
                    classPrefix: $"{summary.Name}Query",
                    inheritance: summaryQueryRequestInheritance,
                    additionalUsings: summary.GenerateSummaryQueryRequest.AdditionalUsings,
                    customAttributes: summary.GenerateSummaryQueryRequest.CustomAttributes,
                    fields: summaryQueryRequestFields,
                    isBackend: isBackend);

                WriteRequestResponse(
                    codeGeneratorSettings,
                    sourceProductionContext,
                    isRequest: false,
                    classPrefix: $"{summary.Name}Query",
                    inheritance: $"ISummaryQueryResponse<{summary.Name}>",
                    additionalUsings: summary.GenerateSummaryQueryRequest.AdditionalUsings,
                    customAttributes: summary.GenerateSummaryQueryRequest.CustomAttributes,
                    fields: null,
                    isBackend: isBackend,
                    entityPropertyType: summary.Name,
                    entityPropertyName: "Summary");
            }

            if (summary.GenerateSummariesQueryRequest != null)
            {
                WriteRequestResponse(
                    codeGeneratorSettings,
                    sourceProductionContext,
                    isRequest: true,
                    classPrefix: $"{summary.PluralName}Query",
                    inheritance: "ISummariesQueryRequest",
                    additionalUsings: summary.GenerateSummariesQueryRequest.AdditionalUsings,
                    customAttributes: summary.GenerateSummariesQueryRequest.CustomAttributes,
                    fields: summary.GenerateSummariesQueryRequest.RequestFields,
                    isBackend: isBackend);

                var useObservableCollection = isBackend ? false : codeGeneratorSettings.FrontendEntititesSettings?.UseObservableCollection ?? false;
                var collectionType = useObservableCollection ? "ObservableCollection" : "IList";
                var collectionTypeInstantiateCommand = useObservableCollection ? "new ObservableCollection" : "new List";

                WriteRequestResponse(
                    codeGeneratorSettings,
                    sourceProductionContext,
                    isRequest: false,
                    classPrefix: $"{summary.PluralName}Query",
                    inheritance: $"ISummariesQueryResponse<{summary.Name}, {collectionType}<{summary.Name}>>",
                    additionalUsings: summary.GenerateSummariesQueryRequest.AdditionalUsings,
                    customAttributes: summary.GenerateSummariesQueryRequest.CustomAttributes,
                    fields: null,
                    isBackend: isBackend,
                    entityPropertyType: $"{collectionType}<{summary.Name}>",
                    entityPropertyName: "Summaries",
                    entityPropertyValue: $"{collectionTypeInstantiateCommand}<{summary.Name}>()",
                    entityPropertyHasValidator: false,
                    entityPropertyIsObservableCollection: useObservableCollection);
            }
        }

        private static void GenerateAuthEntities(CodeGeneratorSettings codeGeneratorSettings, SourceProductionContext sourceProductionContext, bool isBackend)
        {
            var authAdditionalUsings = new AdditionalUsings() { Using = new List<Using>() { new Using() { Content = "KangarooNet.Domain.Entities.Auth" } } };

            WriteRequestResponse(
                codeGeneratorSettings,
                sourceProductionContext,
                isRequest: true,
                classPrefix: "ApplicationUserInsert",
                inheritance: $"IApplicationUserInsertRequest",
                entityPropertyType: string.Empty,
                entityPropertyName: string.Empty,
                entityPropertyValue: string.Empty,
                entityPropertyHasValidator: false,
                additionalUsings: authAdditionalUsings,
                customAttributes: null,
                fields: new Fields()
                {
                    StringField = new List<StringField>()
                    {
                        new StringField()
                        {
                            Name = "FullName",
                            IsRequired = true,
                            MaxLength = 255,
                        },
                        new StringField()
                        {
                            Name = "Email",
                            IsRequired = true,
                            MaxLength = 255,
                        },
                        new StringField()
                        {
                            Name = "Password",
                            IsRequired = true,
                            MaxLength = 255,
                        },
                    },
                },
                isBackend: isBackend);

            WriteRequestResponse(
                codeGeneratorSettings,
                sourceProductionContext,
                isRequest: false,
                classPrefix: "ApplicationUserInsert",
                inheritance: $"IApplicationUserInsertResponse",
                entityPropertyType: string.Empty,
                entityPropertyName: string.Empty,
                entityPropertyValue: string.Empty,
                entityPropertyHasValidator: false,
                additionalUsings: authAdditionalUsings,
                customAttributes: null,
                fields: null,
                isBackend: isBackend);

            WriteRequestResponse(
                codeGeneratorSettings,
                sourceProductionContext,
                isRequest: true,
                classPrefix: "Login",
                inheritance: $"ILoginRequest",
                entityPropertyType: string.Empty,
                entityPropertyName: string.Empty,
                entityPropertyValue: string.Empty,
                entityPropertyHasValidator: false,
                additionalUsings: authAdditionalUsings,
                customAttributes: null,
                fields: new Fields()
                {
                    StringField = new List<StringField>()
                    {
                        new StringField()
                        {
                            Name = "Email",
                            IsRequired = true,
                            MaxLength = 255,
                        },
                        new StringField()
                        {
                            Name = "Password",
                            IsRequired = true,
                            MaxLength = 255,
                        },
                    },
                },
                isBackend: isBackend);

            WriteRequestResponse(
                codeGeneratorSettings,
                sourceProductionContext,
                isRequest: false,
                classPrefix: "Login",
                inheritance: $"ILoginResponse",
                entityPropertyType: string.Empty,
                entityPropertyName: string.Empty,
                entityPropertyValue: string.Empty,
                entityPropertyHasValidator: false,
                additionalUsings: authAdditionalUsings,
                customAttributes: null,
                fields: new Fields()
                {
                    StringField = new List<StringField>()
                    {
                        new StringField()
                        {
                            Name = "Token",
                            IsRequired = true,
                        },
                        new StringField()
                        {
                            Name = "RefreshToken",
                            IsRequired = true,
                        },
                    },
                },
                isBackend: isBackend);

            WriteRequestResponse(
                codeGeneratorSettings,
                sourceProductionContext,
                isRequest: true,
                classPrefix: "RefreshToken",
                inheritance: $"IRefreshTokenRequest",
                entityPropertyType: string.Empty,
                entityPropertyName: string.Empty,
                entityPropertyValue: string.Empty,
                entityPropertyHasValidator: false,
                additionalUsings: authAdditionalUsings,
                customAttributes: null,
                fields: new Fields()
                {
                    StringField = new List<StringField>()
                    {
                        new StringField()
                        {
                            Name = "Token",
                            IsRequired = true,
                        },
                        new StringField()
                        {
                            Name = "RefreshToken",
                            IsRequired = true,
                        },
                    },
                },
                isBackend: isBackend);

            WriteRequestResponse(
                codeGeneratorSettings,
                sourceProductionContext,
                isRequest: false,
                classPrefix: "RefreshToken",
                inheritance: $"IRefreshTokenResponse",
                entityPropertyType: string.Empty,
                entityPropertyName: string.Empty,
                entityPropertyValue: string.Empty,
                entityPropertyHasValidator: false,
                additionalUsings: authAdditionalUsings,
                customAttributes: null,
                fields: new Fields()
                {
                    StringField = new List<StringField>()
                    {
                        new StringField()
                        {
                            Name = "Token",
                            IsRequired = true,
                        },
                        new StringField()
                        {
                            Name = "RefreshToken",
                            IsRequired = true,
                        },
                    },
                },
                isBackend: isBackend);

            WriteRequestResponse(
                codeGeneratorSettings,
                sourceProductionContext,
                isRequest: true,
                classPrefix: "Logout",
                inheritance: $"ILogoutRequest",
                entityPropertyType: string.Empty,
                entityPropertyName: string.Empty,
                entityPropertyValue: string.Empty,
                entityPropertyHasValidator: false,
                additionalUsings: authAdditionalUsings,
                customAttributes: null,
                fields: null,
                isBackend: isBackend);

            WriteRequestResponse(
                codeGeneratorSettings,
                sourceProductionContext,
                isRequest: false,
                classPrefix: "Logout",
                inheritance: $"ILogoutResponse",
                entityPropertyType: string.Empty,
                entityPropertyName: string.Empty,
                entityPropertyValue: string.Empty,
                entityPropertyHasValidator: false,
                additionalUsings: authAdditionalUsings,
                customAttributes: null,
                fields: null,
                isBackend: isBackend);

            WriteRequestResponse(
                codeGeneratorSettings,
                sourceProductionContext,
                isRequest: true,
                classPrefix: "ChangePassword",
                inheritance: $"IChangePasswordRequest",
                entityPropertyType: string.Empty,
                entityPropertyName: string.Empty,
                entityPropertyValue: string.Empty,
                entityPropertyHasValidator: false,
                additionalUsings: authAdditionalUsings,
                customAttributes: null,
                fields: new Fields()
                {
                    StringField = new List<StringField>()
                    {
                        new StringField()
                        {
                            Name = "CurrentPassword",
                            IsRequired = true,
                            MaxLength = 255,
                        },
                        new StringField()
                        {
                            Name = "NewPassword",
                            IsRequired = true,
                            MaxLength = 255,
                        },
                    },
                },
                isBackend: isBackend);

            WriteRequestResponse(
                codeGeneratorSettings,
                sourceProductionContext,
                isRequest: false,
                classPrefix: "ChangePassword",
                inheritance: $"IChangePasswordResponse",
                entityPropertyType: string.Empty,
                entityPropertyName: string.Empty,
                entityPropertyValue: string.Empty,
                entityPropertyHasValidator: false,
                additionalUsings: authAdditionalUsings,
                customAttributes: null,
                fields: null,
                isBackend: isBackend);
        }

        private static void WriteEntity(
            CodeGeneratorSettings codeGeneratorSettings,
            SourceProductionContext sourceProductionContext,
            string entityName,
            string defaultInheritance,
            AdditionalUsings additionalUsings,
            CustomAttributes customAttributes,
            Fields fields,
            bool includeDataState,
            bool includeRowVersionControl,
            bool includeAuditLog,
            bool isBackend)
        {
            var currentLocation = isBackend ? Structure.Location.Backend : Structure.Location.Frontend;
            var keyField = fields?.KeyField;
            var keyType = keyField?.KeyType;
            var entityNamespace = isBackend ? codeGeneratorSettings.BackendEntititesSettings?.EntitiesNamespace : codeGeneratorSettings.FrontendEntititesSettings?.EntitiesNamespace;
            var entityValidatorNamespace = isBackend ? codeGeneratorSettings.BackendEntititesSettings?.ValidatorsNamespace : codeGeneratorSettings.FrontendEntititesSettings?.ValidatorsNamespace;
            var shouldGenerateNotifyPropertyChanges = isBackend ? false : codeGeneratorSettings.FrontendEntititesSettings?.GenerateNotifyPropertyChanges ?? false;
            var useObservableCollection = isBackend ? false : codeGeneratorSettings.FrontendEntititesSettings?.UseObservableCollection ?? false;

            var inheritance = defaultInheritance;

            if (includeDataState)
            {
                inheritance += ", IHasDataState";
            }

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

            if (includeRowVersionControl)
            {
                inheritance += ", IHasRowVersionControl";
            }

            if (includeAuditLog)
            {
                inheritance += ", IHasAuditLog";
            }

            var entityFileWriter = new CSFileWriter(
                    CSFileWriterType.Class,
                    entityNamespace,
                    entityName,
                    isPartial: true,
                    inheritance: inheritance);

            entityFileWriter.WriteUsing("System");
            entityFileWriter.WriteUsing("KangarooNet.Domain");
            entityFileWriter.WriteUsing("KangarooNet.Domain.Entities");

            var validatorClassName = $"{entityName}Validator";
            var entityValidatorFileWriter = new CSFileWriter(
                    CSFileWriterType.Class,
                    entityValidatorNamespace,
                    validatorClassName,
                    isPartial: true,
                    inheritance: $"AbstractValidator<{entityName}>");

            entityValidatorFileWriter.WriteUsing("System");
            entityValidatorFileWriter.WriteUsing("FluentValidation");
            entityValidatorFileWriter.WriteUsing("KangarooNet.Domain.Entities");
            entityValidatorFileWriter.WriteUsing(entityNamespace);

            entityValidatorFileWriter.WriteMethod("SetCustomRules", isPartial: true);

            if (additionalUsings?.Using != null)
            {
                foreach (var customUsing in additionalUsings.Using)
                {
                    entityFileWriter.WriteUsing(customUsing.Content);
                }
            }

            if (customAttributes?.CustomAttribute != null)
            {
                foreach (var classAttribute in customAttributes.CustomAttribute)
                {
                    entityFileWriter.WriteClassAttribute(classAttribute.Attribute);
                }
            }

            fields?.HandleFields(EntityFieldCodeWriter.WriteField(entityFileWriter, entityValidatorFileWriter, shouldGenerateNotifyPropertyChanges, useObservableCollection, currentLocation));

            if (includeRowVersionControl)
            {
                entityFileWriter.WriteProperty("byte[]", "RowVersion", hasNotifyPropertyChanged: shouldGenerateNotifyPropertyChanges, isVirtual: false);
            }

            if (includeAuditLog)
            {
                entityFileWriter.WriteProperty("string", "CreatedByUserName", hasNotifyPropertyChanged: shouldGenerateNotifyPropertyChanges, isVirtual: false);
                entityFileWriter.WriteProperty("DateTimeOffset", "CreatedAt", hasNotifyPropertyChanged: shouldGenerateNotifyPropertyChanges, isVirtual: false);

                entityFileWriter.WriteProperty("string", "UpdatedByUserName", hasNotifyPropertyChanged: shouldGenerateNotifyPropertyChanges, isVirtual: false);
                entityFileWriter.WriteProperty("DateTimeOffset?", "UpdatedAt", hasNotifyPropertyChanged: shouldGenerateNotifyPropertyChanges, isVirtual: false);
            }

            if (includeDataState)
            {
                entityFileWriter.WriteProperty("DataState", "DataState", hasNotifyPropertyChanged: shouldGenerateNotifyPropertyChanges, isVirtual: false);
            }

            EntityFieldCodeWriter.WriteKeyField(keyField, entityFileWriter, currentLocation);

            entityValidatorFileWriter.WriteConstructorAdditionalBodyLine($"this.SetCustomRules();");

            sourceProductionContext.WriteNewCSFile(entityName, entityFileWriter);
            sourceProductionContext.WriteNewCSFile(validatorClassName, entityValidatorFileWriter);
        }

        private static void WriteRequestResponse(
            CodeGeneratorSettings codeGeneratorSettings,
            SourceProductionContext sourceProductionContext,
            bool isRequest,
            string classPrefix,
            string inheritance,
            AdditionalUsings additionalUsings,
            CustomAttributes customAttributes,
            Fields fields,
            bool isBackend,
            string entityPropertyType = null,
            string entityPropertyName = null,
            string entityPropertyValue = null,
            bool entityPropertyHasValidator = false,
            bool entityPropertyIsObservableCollection = false)
        {
            var currentLocation = isBackend ? Structure.Location.Backend : Structure.Location.Frontend;
            var classNamespace = isBackend ? codeGeneratorSettings.BackendEntititesSettings?.EntitiesNamespace : codeGeneratorSettings.FrontendEntititesSettings?.EntitiesNamespace;
            var validatorNamespace = isBackend ? codeGeneratorSettings.BackendEntititesSettings?.ValidatorsNamespace : codeGeneratorSettings.FrontendEntititesSettings?.ValidatorsNamespace;
            var shouldGenerateNotifyPropertyChanges = isBackend ? false : codeGeneratorSettings.FrontendEntititesSettings?.GenerateNotifyPropertyChanges ?? false;
            var useObservableCollection = isBackend ? false : codeGeneratorSettings.FrontendEntititesSettings?.UseObservableCollection ?? false;

            var useMediator = isBackend && isRequest;

            if (useMediator)
            {
                inheritance += $", IRequest<{classPrefix}Response>";
            }

            var className = classPrefix + (isRequest ? "Request" : "Response");

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

            if (entityPropertyIsObservableCollection)
            {
                fileWriter.WriteUsing("System.Collections.ObjectModel");
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

            if (additionalUsings?.Using != null)
            {
                foreach (var customUsing in additionalUsings.Using)
                {
                    fileWriter.WriteUsing(customUsing.Content);
                }
            }

            if (customAttributes?.CustomAttribute != null)
            {
                foreach (var classAttribute in customAttributes.CustomAttribute)
                {
                    fileWriter.WriteClassAttribute(classAttribute.Attribute);
                }
            }

            if (!string.IsNullOrEmpty(entityPropertyName))
            {
                fileWriter.WriteProperty(
                    type: entityPropertyType,
                    name: entityPropertyName,
                    value: entityPropertyValue,
                    hasNotifyPropertyChanged: entityPropertyIsObservableCollection ? false : shouldGenerateNotifyPropertyChanges,
                    isObservableCollection: entityPropertyIsObservableCollection);

                if (entityPropertyHasValidator)
                {
                    validatorFileWriter.WriteConstructorAdditionalBodyLine($"this.RuleFor(x => x.{entityPropertyName}).NotNull().NotEmpty();");
                    validatorFileWriter.WriteConstructorAdditionalBodyLine($"this.RuleFor(x => x.{entityPropertyName}).SetValidator(x => new {entityPropertyType}Validator());");
                }
            }

            fields?.HandleFields(EntityFieldCodeWriter.WriteField(fileWriter, validatorFileWriter, shouldGenerateNotifyPropertyChanges, useObservableCollection, currentLocation));

            EntityFieldCodeWriter.WriteKeyField(fields?.KeyField, fileWriter, currentLocation);

            validatorFileWriter.WriteConstructorAdditionalBodyLine($"this.SetCustomRules();");

            sourceProductionContext.WriteNewCSFile(className, fileWriter);
            sourceProductionContext.WriteNewCSFile(validatorClassName, validatorFileWriter);
        }
    }
}
