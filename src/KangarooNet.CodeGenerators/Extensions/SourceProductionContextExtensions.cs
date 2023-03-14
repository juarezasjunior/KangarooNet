// Copyright Contributors to the KangarooNet project.
// This file is licensed to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICE files in the project root for full license information.

namespace KangarooNet.CodeGenerators.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;
    using KangarooNet.CodeGenerators.Writers;
    using Microsoft.CodeAnalysis;

    internal static class SourceProductionContextExtensions
    {
        public static void WriteNewCSFile(this SourceProductionContext sourceProductionContext, string fileNameWithoutExtension, CSFileWriter fileWriter)
        {
            var fileName = $"{fileNameWithoutExtension}.g.cs";
            var fileContent = fileWriter.GetFileContent();

            sourceProductionContext.AddSource(fileName, fileContent);
            Debug.WriteLine($"The {fileName} was auto generated with this content: " + fileContent);
        }

        public static void WriteNewCSFile(this SourceProductionContext sourceProductionContext, string fileNameWithoutExtension, string fileContent)
        {
            var fileName = $"{fileNameWithoutExtension}.g.cs";

            sourceProductionContext.AddSource(fileName, fileContent);
            Debug.WriteLine($"The {fileName} was auto generated with this content: " + fileContent);
        }
    }
}
