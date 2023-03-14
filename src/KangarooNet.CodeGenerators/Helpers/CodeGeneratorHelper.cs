// Copyright Contributors to the KangarooNet project.
// This file is licensed to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICE files in the project root for full license information.

namespace KangarooNet.CodeGenerators.Helpers
{
    using System;
    using System.Collections.Generic;
    using System.Text;
    using KangarooNet.CodeGenerators.CodeWriters;
    using KangarooNet.CodeGenerators.Structure;
    using Microsoft.CodeAnalysis;

    internal static class CodeGeneratorHelper
    {
        public static void Generate(CodeGeneratorSettings codeGeneratorSettings, List<CodeGenerator> codeGenerators, SourceProductionContext sourceProductionContext)
        {
            if (codeGeneratorSettings.BackendEnumsSettings != null || codeGeneratorSettings.FrontendEnumsSettings != null)
            {
                EnumsCodeWriter.Generate(codeGeneratorSettings, codeGenerators, sourceProductionContext);
            }

            if (codeGeneratorSettings.DatabaseRepositoriesSettings != null)
            {
                DatabaseRepositoriesCodeWriter.Generate(codeGeneratorSettings, codeGenerators, sourceProductionContext);
            }

            if (codeGeneratorSettings.BackendEntititesSettings != null || codeGeneratorSettings.FrontendEntititesSettings != null)
            {
                EntitiesCodeWriter.Generate(codeGeneratorSettings, codeGenerators, sourceProductionContext);
            }

            if (codeGeneratorSettings.BackendCustomRequestsSettings != null || codeGeneratorSettings.FrontendCustomRequestsSettings != null)
            {
                CustomRequestsCodeWriter.Generate(codeGeneratorSettings, codeGenerators, sourceProductionContext);
            }

            if (codeGeneratorSettings.BackendCustomResponsesSettings != null || codeGeneratorSettings.FrontendCustomResponsesSettings != null)
            {
                CustomResponsesCodeWriter.Generate(codeGeneratorSettings, codeGenerators, sourceProductionContext);
            }

            if (codeGeneratorSettings.ApplicationSettings != null)
            {
                ApplicationCodeWriter.Generate(codeGeneratorSettings, codeGenerators, sourceProductionContext);
            }

            if (codeGeneratorSettings.APISettings != null)
            {
                APICodeWriter.Generate(codeGeneratorSettings, codeGenerators, sourceProductionContext);
            }

            if (codeGeneratorSettings.APIClientSettings != null)
            {
                APIClientCodeWriter.Generate(codeGeneratorSettings, codeGenerators, sourceProductionContext);
            }
        }
    }
}
