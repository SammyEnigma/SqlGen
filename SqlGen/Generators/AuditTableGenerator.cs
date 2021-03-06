﻿using System;
using System.Text;
namespace SqlGen
{
    public class AuditTableGenerator : SqlGenerator
    {
        public override string ObjectName(Table table, TableKey key = null) => $"[{table.Schema}].[{table.TableName}_AUDIT]";

        public override string Generate(Table table, GeneratorOptions options)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"CREATE TABLE {ObjectName(table)}");
            sb.AppendLine("(");
            foreach (var col in table.Columns)
            {
                var nullable = col.IsNullable() ? "NULL" : "NOT NULL";
                sb.AppendLine($"    [{col}] {col.TypeDeclaration()} {nullable},");
            }

            sb.AppendLine($"    [AUDIT_TYPE] char(1) NOT NULL,");
            sb.AppendLine($"    [AUDIT_END_DATE] datetime NOT NULL,");
            sb.Append($"    CONSTRAINT [PK_{table.TableName}_AUDIT] PRIMARY KEY CLUSTERED (");
            foreach (var c in table.PrimaryKey)
            {
                sb.Append($"[{c}], ");
            }
            sb.AppendLine("[SEQUENCE_NUMBER])");
            sb.AppendLine(");");
            return sb.ToString();
        }
        public override string ToString() => "Audit Table";

        public override string Grant(Table table, TableKey fk = null) => "";
    }
}