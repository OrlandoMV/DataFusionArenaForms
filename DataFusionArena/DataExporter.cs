using System;
using System.Data;
using System.Data.Common;

namespace DataFusionArena
{
    public static class DataExporter
    {
        public static int ExportDataTableToSqlServer(DataTable table, string connectionString, string tableName)
        {
            if (table == null) throw new ArgumentNullException(nameof(table));
            if (string.IsNullOrWhiteSpace(tableName)) throw new ArgumentNullException(nameof(tableName));

            // Sanitize table name: allow letters, digits and underscore
            var cleaned = "";
            foreach (var ch in tableName)
            {
                if (char.IsLetterOrDigit(ch) || ch == '_') cleaned += ch;
            }
            if (string.IsNullOrEmpty(cleaned)) throw new ArgumentException("Nombre de tabla inválido.");

            string fullName = $"[dbo].[{cleaned}]";
            string schemaQualified = $"dbo.{cleaned}";

            // Create connection via reflection (support both providers)
            Type connType = Type.GetType("System.Data.SqlClient.SqlConnection, System.Data.SqlClient")
                            ?? Type.GetType("Microsoft.Data.SqlClient.SqlConnection, Microsoft.Data.SqlClient");
            if (connType == null) throw new Exception("No se encontró proveedor SqlClient.");

            using (var connObj = Activator.CreateInstance(connType, new object[] { connectionString }) as DbConnection)
            {
                if (connObj == null) throw new Exception("No se pudo crear la conexión SQL.");
                connObj.Open();
                using (var tran = connObj.BeginTransaction())
                {
                    // crear tabla si no existe
                    using (var cmd = connObj.CreateCommand())
                    {
                        cmd.Transaction = tran;
                        var createSql = $"IF OBJECT_ID('{schemaQualified}','U') IS NULL CREATE TABLE {fullName} (RowId INT IDENTITY(1,1) PRIMARY KEY)";
                        cmd.CommandText = createSql;
                        cmd.ExecuteNonQuery();
                    }

                    // agregar columnas faltantes (comprobación más robusta contra nombres especiales)
                    foreach (DataColumn col in table.Columns)
                    {
                        using (var cmd = connObj.CreateCommand())
                        {
                            cmd.Transaction = tran;
                            var colName = col.ColumnName.Replace("\"", "").Replace("'", "");
                            // Use INFORMATION_SCHEMA to verify existence (safer with strange names)
                            var alter = $"IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='{cleaned}' AND COLUMN_NAME='{colName}') ALTER TABLE {fullName} ADD [{col.ColumnName}] NVARCHAR(MAX) NULL";
                            cmd.CommandText = alter;
                            cmd.ExecuteNonQuery();
                        }
                    }

                    // Insert rows using a fresh command per row and typed parameters (NVARCHAR insertion)
                    int inserted = 0;
                    foreach (DataRow row in table.Rows)
                    {
                        using (var cmd = connObj.CreateCommand())
                        {
                            cmd.Transaction = tran;
                            var cols = new System.Text.StringBuilder();
                            var vals = new System.Text.StringBuilder();
                            int p = 0;
                            for (int i = 0; i < table.Columns.Count; i++)
                            {
                                var col = table.Columns[i];
                                if (p > 0) { cols.Append(","); vals.Append(","); }
                                cols.Append($"[{col.ColumnName}]");
                                var paramName = "@p" + p;
                                vals.Append(paramName);
                                var prm = cmd.CreateParameter();
                                prm.ParameterName = paramName;
                                // Normalize value: treat null/empty as DBNull
                                object val = row[i];
                                if (val == null || val == DBNull.Value) prm.Value = DBNull.Value;
                                else
                                {
                                    var s = val.ToString();
                                    prm.Value = string.IsNullOrEmpty(s) ? DBNull.Value : (object)s;
                                }
                                try { prm.DbType = System.Data.DbType.String; } catch { }
                                cmd.Parameters.Add(prm);
                                p++;
                            }
                            cmd.CommandText = $"INSERT INTO {fullName} ({cols}) VALUES ({vals})";
                            int affected = 0;
                            try
                            {
                                affected = cmd.ExecuteNonQuery();
                            }
                            catch (Exception ex)
                            {
                                // Log SQL failure for diagnostics and rethrow so caller can see
                                try
                                {
                                    var log = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DataFusionArena_error.log");
                                    var msg = $"Error inserting row into {fullName}: {ex}\nSQL: {cmd.CommandText}\n";
                                    foreach (DbParameter pparam in cmd.Parameters)
                                    {
                                        msg += $"Param {pparam.ParameterName} = {pparam.Value}\n";
                                    }
                                    System.IO.File.AppendAllText(log, msg);
                                }
                                catch { }
                                throw;
                            }
                            if (affected > 0) inserted += affected;
                        }
                    }
                    tran.Commit();
                    return inserted;
                }
            }
        }
    }
}
