﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace DataCore
{
    public interface ITranslator
    {
        void Top<T>(Query<T> query, int count);

        string GetFormatFor(ExpressionType type);

        string GetBooleanValue(object constantExpressionValue);
        string GetStringValue(object value);
        string GetDateTimeValue(DateTime date);

        string GetTableName(string tableName);
        string GetCreateTableIfNotExistsQuery(string tableName, IEnumerable<FieldDefinition> fields);
    }
}