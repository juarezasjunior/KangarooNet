﻿// Copyright Contributors to the KangarooNet project.
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
            if (field is KeyField keyField)
            {
                switch (keyField.KeyType)
                {
                    case KeyType.Int:
                        return "int";
                    case KeyType.Guid:
                        return "Guid";
                    default:
                        break;
                }
            }
            else if (field is GuidField)
            {
                return "Guid";
            }
            else if (field is StringField)
            {
                return "string";
            }
            else if (field is BoolField)
            {
                return "bool";
            }
            else if (field is DateTimeField)
            {
                return "DateTime";
            }
            else if (field is DateTimeOffsetField)
            {
                return "DateTimeOffset";
            }
            else if (field is DecimalField)
            {
                return "decimal";
            }
            else if (field is IntField)
            {
                return "int";
            }
            else if (field is ITypedField typedField)
            {
                return typedField.Type;
            }

            throw new NotImplementedException();
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
