﻿using System;
using System.Linq;
using System.Text;

namespace SqlGen.Generators
{
    class TableInsertProcGenerator : SqlGenerator
    {
        public override string ObjectName(Table table, TableKey key = null) => $"[{table.Schema}].[{table.TableName}_InsertTable]";

        public override string Generate(Table table, GeneratorOptions options)
        {
            var sb = new StringBuilder();
            AppendCreateOrAlterProc(table, options, sb);
            sb.AppendLine($"    @recs [{table.Schema}].[{table.TableName}_TABLE_TYPE] READONLY");
            sb.AppendLine("AS");
            sb.AppendLine();
            sb.AppendLine("-- using merge so we can capture BULK_SEQ column in output, 1 = 0 forces the insert");
            sb.AppendLine($"MERGE INTO [{table.Schema}].[{table.TableName}] USING @recs AS src ON 1 = 0");
            sb.AppendLine($"WHEN NOT MATCHED THEN INSERT");

            AddFieldNames(table, options, sb);
            AddValues(table, options, sb);
            AddOutput(table, options, sb);
            return sb.ToString();
        }

        private static void AddOutput(Table table, GeneratorOptions options, StringBuilder sb)
        {
            sb.AppendLine("OUTPUT");
            sb.AppendLine($"    src.[BULK_SEQ],");
            foreach (var c in table.Columns.Where(c => options.Audit || !c.IsAuditColumn()))
            {
                sb.AppendLine($"    INSERTED.[{c}],");
            }
            sb.Length -= 3;
            sb.AppendLine(";");
        }

        private static void AddFieldNames(Table table, GeneratorOptions options, StringBuilder sb)
        {
            sb.AppendLine("(");
            foreach (var c in table.InsertableColumns.Where(c => options.Audit || !c.IsAuditColumn()))
            {
                sb.AppendLine($"    [{c}],");
            }
            sb.Length -= 3;
            sb.AppendLine().AppendLine(")");
        }

        private static void AddValues(Table table, GeneratorOptions options, StringBuilder sb)
        {
            sb.AppendLine("VALUES");
            sb.AppendLine("(");
            foreach (var c in table.InsertableColumns.Where(c => options.Audit || !c.IsAuditColumn()))
            {
                if (c.IsSequenceNumber())
                    sb.AppendLine($"    1,");
                else
                    sb.AppendLine($"    {c.TableValue("src")},");
            }
            sb.Length -= 3;
            sb.AppendLine().AppendLine(")");
        }

        public override string ToString() => "Table Insert";
    }
}
