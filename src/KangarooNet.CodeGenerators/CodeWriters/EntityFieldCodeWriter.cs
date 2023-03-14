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

    internal static class EntityFieldCodeWriter
    {
        public static void WriteKeyField(KeyField keyField, CSFileWriter fileWriter, Location location)
        {
            if (keyField != null)
            {
                if (keyField.Location != Location.Both && keyField.Location != location)
                {
                    return;
                }

                List<string> getKeyMethodBodyLines = new List<string>();
                List<string> setKeyMethodBodyLines = new List<string>();

                getKeyMethodBodyLines.Add($"return this.{keyField.Name};");
                setKeyMethodBodyLines.Add($"this.{keyField.Name} = key;");

                switch (keyField.KeyType)
                {
                    case KeyType.Int:
                        fileWriter.WriteMethod("GetKey", "int", bodyLines: getKeyMethodBodyLines);
                        fileWriter.WriteMethod("SetKey", parameters: "int key", bodyLines: setKeyMethodBodyLines);
                        break;
                    case KeyType.Guid:
                        fileWriter.WriteMethod("GetKey", "Guid", bodyLines: getKeyMethodBodyLines);
                        fileWriter.WriteMethod("SetKey", parameters: "Guid key", bodyLines: setKeyMethodBodyLines);
                        break;
                    default:
                        break;
                }
            }
        }

        public static Action<object> WriteField(CSFileWriter fileWriter, CSFileWriter validatorFileWriter, bool hasNotifyPropertyChanged, bool useObservableCollection, Location location)
        {
            return x =>
            {
                if (x is IField field)
                {
                    if (field.Location != Location.Both && field.Location != location)
                    {
                        return;
                    }

                    var fieldType = field.GetFieldType();
                    var isList = field is EntityCollectionField || field is CollectionField;
                    var fieldValue = string.Empty;
                    var isObservableCollection = false;

                    if (isList)
                    {
                        if (useObservableCollection)
                        {
                            fieldType = $"ObservableCollection<{fieldType}>";
                            fieldValue = $"new ObservableCollection<{fieldType}>()";
                            isObservableCollection = true;
                        }
                        else
                        {
                            fieldType = $"IList<{fieldType}>";
                            fieldValue = $"new List<{fieldType}>()";
                        }
                    }

                    var isRequired = false;

                    if (field is ICanBeRequired requiredField)
                    {
                        isRequired = requiredField.IsRequired;
                    }

                    var maxLength = 0;

                    if (field is StringField stringField)
                    {
                        maxLength = stringField.MaxLength;
                    }

                    if (isRequired)
                    {
                        validatorFileWriter.WriteConstructorAdditionalBodyLine($"this.RuleFor(x => x.{field.Name}).NotNull().NotEmpty();");
                    }

                    if (maxLength > 0)
                    {
                        validatorFileWriter.WriteConstructorAdditionalBodyLine($"this.RuleFor(x => x.{field.Name}).MaximumLength({maxLength});");
                    }

                    if (field is EntityField entityField)
                    {
                        validatorFileWriter.WriteConstructorAdditionalBodyLine($"this.RuleFor(x => x.{field.Name}).SetValidator(x => new {entityField.Type}Validator());");
                    }

                    var attributes = new List<string>();

                    if (field.CustomAttributes?.CustomAttribute != null)
                    {
                        foreach (var attribute in field.CustomAttributes.CustomAttribute)
                        {
                            attributes.Add(attribute.Attribute);
                        }
                    }

                    fileWriter.WriteProperty(fieldType, field.Name, value: fieldValue, hasNotifyPropertyChanged: hasNotifyPropertyChanged, isObservableCollection: isObservableCollection, attributes: attributes);
                }
            };
        }
    }
}
