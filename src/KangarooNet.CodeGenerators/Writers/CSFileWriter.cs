// Copyright Contributors to the KangarooNet project.
// This file is licensed to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICE files in the project root for full license information.

namespace KangarooNet.CodeGenerators.Writers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using KangarooNet.CodeGenerators.Extensions;

    internal class CSFileWriter
    {
        private readonly CSFileWriterType fileWriterType;
        private readonly string fileNamespace;
        private readonly string name;
        private readonly bool isPartial;
        private readonly bool isAbstract;
        private readonly List<string> usingNamespaces = new List<string>();
        private readonly List<string> classAttributes = new List<string>();
        private readonly List<string> constructorAdditionalBodyLines = new List<string>();
        private readonly List<DependencyInjection> dependencyInjections = new List<DependencyInjection>();
        private readonly List<Field> fields = new List<Field>();
        private readonly List<Property> properties = new List<Property>();
        private readonly List<EnumField> enumFields = new List<EnumField>();
        private readonly List<Method> methods = new List<Method>();
        private bool wasNotifyPropertyChangedStructureAdded;
        private bool wasAddObservableCollectionStructureAdded;
        private string inheritance;

        public CSFileWriter(CSFileWriterType fileWriterType, string fileNamespace, string name, bool isPartial = false, bool isAbstract = false, string inheritance = "")
        {
            this.fileWriterType = fileWriterType;
            this.fileNamespace = fileNamespace;
            this.name = name;
            this.isPartial = isPartial;
            this.isAbstract = isAbstract;
            this.inheritance = inheritance;
        }

        public static string GetWhiteSpace(int times = 1)
        {
            var space = string.Empty;

            for (int i = 0; i < times; i++)
            {
                space += "    ";
            }

            return space;
        }

        public void WriteUsing(string usingNamespace)
        {
            if (string.IsNullOrEmpty(usingNamespace))
            {
                return;
            }

            var usingStatement = $"{GetWhiteSpace()}using {usingNamespace};";

            if (!this.usingNamespaces.Any(x => x == usingStatement))
            {
                this.usingNamespaces.Add(usingStatement);
            }
        }

        public void WriteClassAttribute(string attribute)
        {
            if (string.IsNullOrEmpty(attribute))
            {
                return;
            }

            this.classAttributes.Add($"{GetWhiteSpace()}[{attribute}]");
        }

        public void WriteConstructorAdditionalBodyLine(string bodyLine)
        {
            if (string.IsNullOrEmpty(bodyLine))
            {
                return;
            }

            this.constructorAdditionalBodyLines.Add($"{GetWhiteSpace(3)}{bodyLine}");
        }

        public void WriteDependencyInjection(string type, string name, CSFileWriterAccessModifierType accessModifierType = CSFileWriterAccessModifierType.Private, bool shouldSendToConstructorBase = false)
        {
            this.dependencyInjections.Add(new DependencyInjection()
            {
                AccessModifierType = accessModifierType,
                Type = type,
                Name = name,
                ShouldSendToConstructorBase = shouldSendToConstructorBase,
            });
        }

        public void WriteField(string type, string name, CSFileWriterAccessModifierType accessModifierType = CSFileWriterAccessModifierType.Private, string value = "", bool isReadOnly = false)
        {
            this.fields.Add(new Field()
            {
                AccessModifierType = accessModifierType,
                IsReadOnly = isReadOnly,
                Type = type,
                Name = name,
                Value = value,
            });
        }

        public void WriteProperty(string type, string name, string value = "", bool isFullProperty = true, bool hasNotifyPropertyChanged = false, bool isObservableCollection = false, bool isVirtual = false, List<string> attributes = null)
        {
            if (attributes == null)
            {
                attributes = new List<string>();
            }

            this.properties.Add(new Property()
            {
                IsFullProperty = isFullProperty,
                IsVirtual = isVirtual,
                Type = type,
                Name = name,
                Value = value,
                HasNotifyPropertyChanged = hasNotifyPropertyChanged,
                Attributes = attributes,
            });

            if (isFullProperty && !string.IsNullOrEmpty(name))
            {
                this.WriteField(type, name.FirstCharToLowerCase(), value: value, isReadOnly: false);
            }

            if (hasNotifyPropertyChanged && !isObservableCollection)
            {
                this.AddNotifyPropertyChangedStructure();
            }

            if (isObservableCollection)
            {
                this.AddObservableCollectionStructure();
            }
        }

        public void WriteEnumField(string name, string value = "")
        {
            this.enumFields.Add(new EnumField()
            {
                Name = name,
                Value = value,
            });
        }

        public void WriteMethod(string name, string returnType = "", string parameters = "", List<string> attributes = null, List<string> bodyLines = null, CSFileWriterAccessModifierType accessModifierType = CSFileWriterAccessModifierType.Public, bool isPartial = false, bool isVirtual = false, bool isOverride = false)
        {
            if (bodyLines == null)
            {
                bodyLines = new List<string>();
            }

            this.methods.Add(new Method()
            {
                AccessModifierType = accessModifierType,
                IsPartial = isPartial,
                IsVirtual = isVirtual,
                IsOverride = isOverride,
                ReturnType = returnType,
                Name = name,
                Parameters = parameters,
                Attributes = attributes ?? new List<string>(),
                BodyLines = bodyLines ?? new List<string>(),
            });
        }

        public string GetFileContent()
        {
            var fileStringBuilder = new StringBuilder();

            var shouldHaveHeaderNamespace = !string.IsNullOrEmpty(fileNamespace);

            if (shouldHaveHeaderNamespace)
            {
                this.WriteFileHeader(fileStringBuilder);
                this.OpenFileNamespace(fileStringBuilder);
            }

            this.WriteUsings(fileStringBuilder);
            this.WriteClassAttributes(fileStringBuilder);
            this.OpenFileBody(fileStringBuilder);

            this.WriteDependencyInjectionFields(fileStringBuilder);
            this.WriteFields(fileStringBuilder);
            this.WriteConstructor(fileStringBuilder);
            this.WriteProperties(fileStringBuilder);
            this.WriteEnumFields(fileStringBuilder);
            this.WriteMethods(fileStringBuilder);

            this.CloseFileBody(fileStringBuilder);

            if (shouldHaveHeaderNamespace)
            {
                this.CloseFileNamespace(fileStringBuilder);
            }

            return fileStringBuilder.ToString();
        }

        private void AddNotifyPropertyChangedStructure()
        {
            if (!this.wasNotifyPropertyChangedStructureAdded)
            {
                this.wasNotifyPropertyChangedStructureAdded = true;
                this.WriteUsing("System.ComponentModel");
                this.WriteUsing("System.Runtime.CompilerServices");

                if (string.IsNullOrEmpty(this.inheritance))
                {
                    this.inheritance = "INotifyPropertyChanged";
                }
                else
                {
                    this.inheritance += ", INotifyPropertyChanged";
                }

                this.WriteField("event PropertyChangedEventHandler?", "PropertyChanged", CSFileWriterAccessModifierType.Public);

                this.WriteMethod("OnPropertyChanged", parameters: "[CallerMemberName] string propertyName = null", isPartial: true);
                this.WriteMethod("OnPropertyChanging", parameters: "object oldValue, object newValue, [CallerMemberName] string propertyName = null", isPartial: true);

                var notifyPropertyChangedBodyLines = new List<string>();

                notifyPropertyChangedBodyLines.Add("this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));");
                notifyPropertyChangedBodyLines.Add("this.OnPropertyChanged(propertyName);");

                this.WriteMethod("NotifyPropertyChanged", parameters: "[CallerMemberName] string propertyName = null", accessModifierType: CSFileWriterAccessModifierType.Private, bodyLines: notifyPropertyChangedBodyLines);
            }
        }

        private void AddObservableCollectionStructure()
        {
            if (!this.wasAddObservableCollectionStructureAdded)
            {
                this.wasAddObservableCollectionStructureAdded = true;
                this.WriteUsing("System.Collections.ObjectModel");
            }
        }

        private void WriteFileHeader(StringBuilder stringBuilder)
        {
            stringBuilder.AppendLine(@"// <auto-generated>");
            stringBuilder.AppendLine();
        }

        private void OpenFileNamespace(StringBuilder stringBuilder)
        {
            stringBuilder.AppendLine($"namespace {this.fileNamespace}");
            stringBuilder.AppendLine(@"{");
        }

        private void CloseFileNamespace(StringBuilder stringBuilder)
        {
            stringBuilder.AppendLine(@"}");
        }

        private void WriteUsings(StringBuilder stringBuilder)
        {
            foreach (var usingNamespace in this.usingNamespaces.OrderBy(x => x))
            {
                stringBuilder.AppendLine(usingNamespace);
            }

            if (this.usingNamespaces.Any())
            {
                stringBuilder.AppendLine();
            }
        }

        private void WriteClassAttributes(StringBuilder stringBuilder)
        {
            foreach (var classAttribute in this.classAttributes)
            {
                stringBuilder.AppendLine(classAttribute);
            }

            if (this.classAttributes.Any())
            {
                stringBuilder.AppendLine();
            }
        }

        private void OpenFileBody(StringBuilder stringBuilder)
        {
            var bodyStatement = string.Empty;

            var partial = this.isPartial ? " partial" : string.Empty;

            var abstractStatement = this.isAbstract ? " abstract" : string.Empty;

            switch (this.fileWriterType)
            {
                case CSFileWriterType.Class:
                    bodyStatement = $"{GetWhiteSpace()}public{partial}{abstractStatement} class {this.name}";
                    break;
                case CSFileWriterType.Interface:
                    bodyStatement = $"{GetWhiteSpace()}public{partial} interface {this.name}";
                    break;
                case CSFileWriterType.Enum:
                    bodyStatement = $"{GetWhiteSpace()}public enum {this.name}";
                    break;
                default:
                    break;
            }

            if (!string.IsNullOrEmpty(this.inheritance))
            {
                bodyStatement += $" : {this.inheritance}";
            }

            stringBuilder.AppendLine(bodyStatement);
            stringBuilder.AppendLine(GetWhiteSpace() + @"{");
        }

        private void CloseFileBody(StringBuilder stringBuilder)
        {
            stringBuilder.AppendLine(GetWhiteSpace() + @"}");
        }

        private void WriteDependencyInjectionFields(StringBuilder stringBuilder)
        {
            foreach (var dependencyInjection in this.dependencyInjections.Where(x => !x.ShouldSendToConstructorBase))
            {
                stringBuilder.AppendLine($"{GetWhiteSpace(2)}{this.GetAccessModifierType(dependencyInjection.AccessModifierType)}readonly {dependencyInjection.Type} {dependencyInjection.Name};");
            }

            if (this.dependencyInjections.Any())
            {
                stringBuilder.AppendLine();
            }
        }

        private void WriteFields(StringBuilder stringBuilder)
        {
            foreach (var field in this.fields.OrderByDescending(x => x.IsReadOnly).ThenByDescending(x => (int)x.AccessModifierType))
            {
                var readOnlyStatement = field.IsReadOnly ? "readonly " : string.Empty;
                var fieldStatement = $"{GetWhiteSpace(2)}{this.GetAccessModifierType(field.AccessModifierType)}{readOnlyStatement}{field.Type} {field.Name}";

                if (string.IsNullOrEmpty(field.Value))
                {
                    fieldStatement += ";";
                }
                else
                {
                    fieldStatement += $" = {field.Value};";
                }

                stringBuilder.AppendLine(fieldStatement);
            }

            if (this.fields.Any())
            {
                stringBuilder.AppendLine();
            }
        }

        private void WriteConstructor(StringBuilder stringBuilder)
        {
            if (this.dependencyInjections.Any())
            {
                stringBuilder.AppendLine($"{GetWhiteSpace(2)}public {this.name}(");

                var count = 0;

                foreach (var dependencyInjection in this.dependencyInjections)
                {
                    count++;

                    var dependencyInjectionStatement = $"{GetWhiteSpace(3)}{dependencyInjection.Type} {dependencyInjection.Name}";

                    if (count == this.dependencyInjections.Count())
                    {
                        dependencyInjectionStatement += ")";
                    }
                    else
                    {
                        dependencyInjectionStatement += ",";
                    }

                    stringBuilder.AppendLine(dependencyInjectionStatement);
                }

                if (this.dependencyInjections.Any(x => x.ShouldSendToConstructorBase))
                {
                    count = 0;
                    var dependencyInjectionBaseStatement = string.Empty;
                    var constructorBaseDependencyInjections = this.dependencyInjections.Where(x => x.ShouldSendToConstructorBase);

                    foreach (var dependencyInjection in constructorBaseDependencyInjections)
                    {
                        count++;

                        dependencyInjectionBaseStatement += dependencyInjection.Name;

                        if (count < constructorBaseDependencyInjections.Count())
                        {
                            dependencyInjectionBaseStatement += ", ";
                        }
                    }

                    stringBuilder.AppendLine($"{GetWhiteSpace(3)}: base({dependencyInjectionBaseStatement})");
                }

                stringBuilder.AppendLine(GetWhiteSpace(2) + @"{");

                foreach (var dependencyInjection in this.dependencyInjections.Where(x => !x.ShouldSendToConstructorBase))
                {
                    stringBuilder.AppendLine($"{GetWhiteSpace(3)}this.{dependencyInjection.Name} = {dependencyInjection.Name};");
                }

                foreach (var constructorAdditionalBodyLine in this.constructorAdditionalBodyLines)
                {
                    stringBuilder.AppendLine(constructorAdditionalBodyLine);
                }

                stringBuilder.AppendLine(GetWhiteSpace(2) + @"}");

                stringBuilder.AppendLine();
            }
            else if (this.constructorAdditionalBodyLines.Any())
            {
                stringBuilder.AppendLine($"{GetWhiteSpace(2)}public {this.name}()");

                stringBuilder.AppendLine(GetWhiteSpace(2) + @"{");

                foreach (var constructorAdditionalBodyLine in this.constructorAdditionalBodyLines)
                {
                    stringBuilder.AppendLine(constructorAdditionalBodyLine);
                }

                stringBuilder.AppendLine(GetWhiteSpace(2) + @"}");

                stringBuilder.AppendLine();
            }
        }

        private void WriteProperties(StringBuilder stringBuilder)
        {
            var count = 0;

            foreach (var property in this.properties.OrderBy(x => (int)x.AccessModifierType))
            {
                count++;

                foreach (var attribute in property.Attributes)
                {
                    stringBuilder.AppendLine($"{GetWhiteSpace(2)}[{attribute}]");
                }

                var virtualStatement = property.IsVirtual ? "virtual " : string.Empty;

                if (property.IsFullProperty)
                {
                    stringBuilder.AppendLine($"{GetWhiteSpace(2)}{this.GetAccessModifierType(property.AccessModifierType)}{virtualStatement}{property.Type} {property.Name}");
                    stringBuilder.AppendLine(GetWhiteSpace(2) + "{");
                    stringBuilder.AppendLine($"{GetWhiteSpace(3)}get => this.{property.Name.FirstCharToLowerCase()};");

                    if (property.HasNotifyPropertyChanged)
                    {
                        stringBuilder.AppendLine($"{GetWhiteSpace(3)}set");
                        stringBuilder.AppendLine(GetWhiteSpace(3) + @"{");

                        stringBuilder.AppendLine(GetWhiteSpace(4) + $"if (this.{property.Name.FirstCharToLowerCase()} != value)");
                        stringBuilder.AppendLine(GetWhiteSpace(4) + @"{");

                        stringBuilder.AppendLine(GetWhiteSpace(5) + $"this.OnPropertyChanging(this.{property.Name.FirstCharToLowerCase()}, value);");
                        stringBuilder.AppendLine(GetWhiteSpace(5) + $"this.{property.Name.FirstCharToLowerCase()} = value;");
                        stringBuilder.AppendLine(GetWhiteSpace(5) + $"this.NotifyPropertyChanged();");

                        stringBuilder.AppendLine(GetWhiteSpace(4) + @"}");

                        stringBuilder.AppendLine(GetWhiteSpace(3) + @"}");
                    }
                    else
                    {
                        stringBuilder.AppendLine($"{GetWhiteSpace(3)}set => this.{property.Name.FirstCharToLowerCase()} = value;");
                    }

                    stringBuilder.AppendLine(GetWhiteSpace(2) + "}");
                }
                else
                {
                    var propertyStatement = $"{GetWhiteSpace(2)}public {virtualStatement}{property.Type} {property.Name}" + @" { get; set; }";

                    if (!string.IsNullOrEmpty(property.Value))
                    {
                        propertyStatement += $" = {property.Value};";
                    }

                    stringBuilder.AppendLine(propertyStatement);
                }

                if (count < this.properties.Count() || this.methods.Any())
                {
                    stringBuilder.AppendLine();
                }
            }
        }

        private void WriteEnumFields(StringBuilder stringBuilder)
        {
            var count = 0;

            foreach (var enumField in this.enumFields.OrderBy(x => x.IntValue).ThenBy(x => x.Name))
            {
                count++;

                var enumFieldStatement = $"{GetWhiteSpace(2)}{enumField.Name}";

                if (!string.IsNullOrEmpty(enumField.Value))
                {
                    enumFieldStatement += $" = {enumField.Value}";
                }

                if (count < this.enumFields.Count())
                {
                    enumFieldStatement += ",";
                }

                stringBuilder.AppendLine(enumFieldStatement);
            }
        }

        private void WriteMethods(StringBuilder stringBuilder)
        {
            var count = 0;

            foreach (var method in this.methods.OrderBy(x => (int)x.AccessModifierType))
            {
                count++;

                var methodStatement = GetWhiteSpace(2);

                if (!method.IsPartial)
                {
                    methodStatement += this.GetAccessModifierType(method.AccessModifierType);
                }

                if (method.IsOverride)
                {
                    methodStatement += "override ";
                }

                methodStatement += method.IsPartial ? "partial " : string.Empty;
                methodStatement += method.IsVirtual ? "virtual " : string.Empty;
                methodStatement += string.IsNullOrEmpty(method.ReturnType) ? "void " : $"{method.ReturnType} ";
                methodStatement += $"{method.Name}(";
                methodStatement += string.IsNullOrEmpty(method.Parameters) ? string.Empty : method.Parameters;
                methodStatement += @")";

                if (method.IsPartial || this.fileWriterType == CSFileWriterType.Interface)
                {
                    methodStatement += @";";
                }

                foreach (var methodAttribute in method.Attributes)
                {
                    stringBuilder.AppendLine($"{GetWhiteSpace(2)}[{methodAttribute}]");
                }

                stringBuilder.AppendLine(methodStatement);

                if (!method.IsPartial && this.fileWriterType != CSFileWriterType.Interface)
                {
                    stringBuilder.AppendLine(GetWhiteSpace(2) + @"{");

                    foreach (var bodyLine in method.BodyLines)
                    {
                        stringBuilder.AppendLine(GetWhiteSpace(3) + bodyLine);
                    }

                    stringBuilder.AppendLine(GetWhiteSpace(2) + @"}");
                }

                if (count < this.methods.Count())
                {
                    stringBuilder.AppendLine();
                }
            }
        }

        private string GetAccessModifierType(CSFileWriterAccessModifierType accessModifierType)
        {
            switch (accessModifierType)
            {
                case CSFileWriterAccessModifierType.Public:
                    return "public ";
                case CSFileWriterAccessModifierType.Protected:
                    return "protected ";
                case CSFileWriterAccessModifierType.Private:
                    return "private ";
                default:
                    throw new NotImplementedException();
            }
        }

        private class DependencyInjection
        {
            public CSFileWriterAccessModifierType AccessModifierType { get; set; }

            public string Type { get; set; }

            public string Name { get; set; }

            public bool ShouldSendToConstructorBase { get; set; }
        }

        private class Field
        {
            public CSFileWriterAccessModifierType AccessModifierType { get; set; }

            public bool IsReadOnly { get; set; }

            public string Type { get; set; }

            public string Name { get; set; }

            public string Value { get; set; }
        }

        private class Property
        {
            public CSFileWriterAccessModifierType AccessModifierType { get; set; }

            public bool IsFullProperty { get; set; }

            public bool IsVirtual { get; set; }

            public string Type { get; set; }

            public string Name { get; set; }

            public string Value { get; set; }

            public bool HasNotifyPropertyChanged { get; set; }

            public List<string> Attributes { get; set; } = new List<string>();
        }

        private class EnumField
        {
            public string Name { get; set; }

            public string Value { get; set; }

            public int IntValue
            {
                get
                {
                    if (int.TryParse(this.Value, out var intValue))
                    {
                        return intValue;
                    }

                    return default;
                }
            }
        }

        private class Method
        {
            public CSFileWriterAccessModifierType AccessModifierType { get; set; }

            public bool IsPartial { get; set; }

            public bool IsVirtual { get; set; }

            public bool IsOverride { get; set; }

            public string ReturnType { get; set; }

            public string Name { get; set; }

            public string Parameters { get; set; }

            public List<string> Attributes { get; set; } = new List<string>();

            public List<string> BodyLines { get; set; } = new List<string>();
        }
    }
}
