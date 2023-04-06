// Copyright Contributors to the KangarooNet project.
// This file is licensed to you under the Apache License, Version 2.0.
// See the LICENSE and NOTICE files in the project root for full license information.

namespace KangarooNet.CodeGenerators.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using KangarooNet.CodeGenerators.Structure;

    internal static class FieldExtensions
    {
        public static string GetFieldType(this IField field)
        {
            var fieldType = string.Empty;

            if (field is KeyField keyField)
            {
                switch (keyField.KeyType)
                {
                    case KeyType.Int:
                        fieldType = "int";
                        break;
                    case KeyType.Guid:
                        fieldType = "Guid";
                        break;
                    default:
                        break;
                }
            }
            else if (field is GuidField)
            {
                fieldType = "Guid";
            }
            else if (field is StringField)
            {
                fieldType = "string";
            }
            else if (field is BoolField)
            {
                fieldType = "bool";
            }
            else if (field is DateTimeField)
            {
                fieldType = "DateTime";
            }
            else if (field is DateTimeOffsetField)
            {
                fieldType = "DateTimeOffset";
            }
            else if (field is DecimalField)
            {
                fieldType = "decimal";
            }
            else if (field is IntField)
            {
                fieldType = "int";
            }
            else if (field is ITypedField typedField)
            {
                fieldType = typedField.Type;
            }

            if (string.IsNullOrEmpty(fieldType))
            {
                throw new NotImplementedException();
            }

            if (field is ICanBeRequired canBeRequired && !canBeRequired.IsRequired)
            {
                return fieldType += "?";
            }

            return fieldType;
        }

        public static void HandleFields(this Fields fields, Action<object> action)
        {
            if (fields.KeyField != null)
            {
                action(fields.KeyField);
            }

            foreach (var field in fields.GuidField)
            {
                action(field);
            }

            foreach (var field in fields.StringField)
            {
                action(field);
            }

            foreach (var field in fields.BoolField)
            {
                action(field);
            }

            foreach (var field in fields.DateTimeField)
            {
                action(field);
            }

            foreach (var field in fields.DateTimeOffsetField)
            {
                action(field);
            }

            foreach (var field in fields.DecimalField)
            {
                action(field);
            }

            foreach (var field in fields.IntField)
            {
                action(field);
            }

            foreach (var field in fields.CollectionField)
            {
                action(field);
            }

            foreach (var field in fields.EntityCollectionField)
            {
                action(field);
            }

            foreach (var field in fields.SummaryField)
            {
                action(field);
            }

            foreach (var field in fields.EnumField)
            {
                action(field);
            }

            foreach (var field in fields.EntityField)
            {
                action(field);
            }
        }
    }
}
