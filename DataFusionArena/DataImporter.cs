using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Text;
using System.Xml;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

// A tiny insertion order set to keep column order from JSON objects
internal class LinkedHashSet<T> : IEnumerable<T>
{
    private readonly List<T> list = new List<T>();
    private readonly HashSet<T> set = new HashSet<T>();
    public bool Add(T item) { if (set.Add(item)) { list.Add(item); return true; } return false; }
    public IEnumerator<T> GetEnumerator() => list.GetEnumerator();
    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => list.GetEnumerator();
}

namespace DataFusionArena
{
    public static class DataImporter
    {
        public static List<DataItem> LeerJson(string path)
        {
            try
            {
                var text = File.ReadAllText(path);
                var options = new JsonSerializerOptions {PropertyNameCaseInsensitive = true};
                // Deserialize JSON into a list of DataItem
                var items = JsonSerializer.Deserialize<List<DataItem>>(text, options);
                return items ?? new List<DataItem>();
            }
            catch (Exception ex)
            {
                throw new Exception("Error leyendo JSON: " + ex.Message, ex);
            }
        }

        public static List<DataItem> LeerCsv(string path)
        {
            var list = new List<DataItem>();
            try
            {
                using (var sr = new StreamReader(path))
                {
                    string header = sr.ReadLine();
                    // Assume header has columns but order may vary. We'll map indices.
                    string[] headers = header?.Split(',') ?? new string[0];
                    int idxId = -1, idxNombre = -1, idxCategoria = -1, idxPrecio = -1;
                    for (int i = 0; i < headers.Length; i++)
                    {
                        var h = headers[i].Trim().ToLowerInvariant();
                        if (h == "id") idxId = i;
                        else if (h == "nombre" || h == "name") idxNombre = i;
                        else if (h == "categoria" || h == "category") idxCategoria = i;
                        else if (h == "precio" || h == "price") idxPrecio = i;
                    }

                    string line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        var parts = line.Split(',');
                        try
                        {
                            var item = new DataItem();
                            if (idxId >= 0) item.Id = int.Parse(parts[idxId]);
                            else item.Id = 0;
                            if (idxNombre >= 0) item.Nombre = parts[idxNombre]; else item.Nombre = string.Empty;
                            if (idxCategoria >= 0) item.Categoria = parts[idxCategoria]; else item.Categoria = string.Empty;
                            if (idxPrecio >= 0) item.Precio = decimal.Parse(parts[idxPrecio], CultureInfo.InvariantCulture);
                            else item.Precio = 0m;
                            list.Add(item);
                        }
                        catch (Exception ex)
                        {
                            // Skip malformed line but continue
                            continue;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error leyendo CSV: " + ex.Message, ex);
            }
            return list;
        }

        // DataTable helpers for UI
        public static DataTable LeerCsvDataTable(string path)
        {
            var dt = new DataTable();
            using (var sr = new StreamReader(path))
            {
                string header = sr.ReadLine();
                if (header == null) return dt;
                char delimiter = header.Contains(";") ? ';' : ',';
                string[] cols = ParseCsvLine(header, delimiter);
                for (int i = 0; i < cols.Length; i++) dt.Columns.Add(cols[i]);

                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var parts = ParseCsvLine(line, delimiter);
                    var row = dt.NewRow();
                    for (int i = 0; i < parts.Length && i < dt.Columns.Count; i++) row[i] = parts[i];
                    dt.Rows.Add(row);
                }
            }
            return dt;
        }

        public static DataTable LeerJsonDataTable(string path)
        {
            var dt = new DataTable();
            var text = File.ReadAllText(path);
            using (var doc = JsonDocument.Parse(text))
            {
                var root = doc.RootElement;

                // helper to convert JsonElement to string
                static string JsonElemToString(JsonElement e)
                {
                    try
                    {
                        switch (e.ValueKind)
                        {
                            case JsonValueKind.String: return e.GetString() ?? string.Empty;
                            case JsonValueKind.Number: return e.GetRawText();
                            case JsonValueKind.True: return "true";
                            case JsonValueKind.False: return "false";
                            case JsonValueKind.Null: return string.Empty;
                            default: return e.ToString();
                        }
                    }
                    catch { return e.ToString(); }
                }

                if (root.ValueKind == JsonValueKind.Array)
                {
                    // collect union of property names (flatten one-level objects)
                    var cols = new LinkedHashSet<string>();
                    foreach (var elem in root.EnumerateArray())
                    {
                        if (elem.ValueKind == JsonValueKind.Object)
                        {
                            foreach (var prop in elem.EnumerateObject())
                            {
                                if (prop.Value.ValueKind == JsonValueKind.Object)
                                {
                                    foreach (var child in prop.Value.EnumerateObject()) cols.Add(prop.Name + "." + child.Name);
                                }
                                else cols.Add(prop.Name);
                            }
                        }
                        else
                        {
                            cols.Add("Value");
                        }
                    }

                    foreach (var c in cols) dt.Columns.Add(c);

                    foreach (var elem in root.EnumerateArray())
                    {
                        var row = dt.NewRow();
                        if (elem.ValueKind == JsonValueKind.Object)
                        {
                            foreach (DataColumn col in dt.Columns)
                            {
                                var parts = col.ColumnName.Split(new[] { '.' }, 2);
                                if (parts.Length == 1)
                                {
                                    if (elem.TryGetProperty(parts[0], out var p)) row[col.ColumnName] = JsonElemToString(p); else row[col.ColumnName] = string.Empty;
                                }
                                else
                                {
                                    if (elem.TryGetProperty(parts[0], out var p) && p.ValueKind == JsonValueKind.Object && p.TryGetProperty(parts[1], out var child)) row[col.ColumnName] = JsonElemToString(child);
                                    else row[col.ColumnName] = string.Empty;
                                }
                            }
                        }
                        else
                        {
                            // primitive array
                            if (dt.Columns.Contains("Value")) row["Value"] = JsonElemToString(elem);
                        }
                        dt.Rows.Add(row);
                    }
                }
                else if (root.ValueKind == JsonValueKind.Object)
                {
                    var cols = new LinkedHashSet<string>();
                    foreach (var prop in root.EnumerateObject())
                    {
                        if (prop.Value.ValueKind == JsonValueKind.Object)
                        {
                            foreach (var child in prop.Value.EnumerateObject()) cols.Add(prop.Name + "." + child.Name);
                        }
                        else cols.Add(prop.Name);
                    }
                    foreach (var c in cols) dt.Columns.Add(c);
                    var row = dt.NewRow();
                    foreach (DataColumn col in dt.Columns)
                    {
                        var parts = col.ColumnName.Split(new[] { '.' }, 2);
                        if (parts.Length == 1)
                        {
                            if (root.TryGetProperty(parts[0], out var p)) row[col.ColumnName] = JsonElemToString(p); else row[col.ColumnName] = string.Empty;
                        }
                        else
                        {
                            if (root.TryGetProperty(parts[0], out var p) && p.ValueKind == JsonValueKind.Object && p.TryGetProperty(parts[1], out var child)) row[col.ColumnName] = JsonElemToString(child);
                            else row[col.ColumnName] = string.Empty;
                        }
                    }
                    dt.Rows.Add(row);
                }
            }
            return dt;
        }

        public static DataTable LeerXmlDataTable(string path)
        {
            var dt = new DataTable();
            var doc = new XmlDocument();
            doc.Load(path);
            var nodes = doc.SelectNodes("//Item | //DataItem | //Venta | //Row");
            if (nodes == null) return dt;
            // determine columns from first node
            foreach (XmlNode node in nodes)
            {
                if (node.HasChildNodes)
                {
                    foreach (XmlNode child in node.ChildNodes)
                    {
                        if (!dt.Columns.Contains(child.Name)) dt.Columns.Add(child.Name);
                    }
                    break;
                }
            }
            foreach (XmlNode node in nodes)
            {
                var row = dt.NewRow();
                int c = 0;
                foreach (DataColumn col in dt.Columns)
                {
                    var n = node.SelectSingleNode(col.ColumnName);
                    row[c] = n?.InnerText ?? string.Empty;
                    c++;
                }
                dt.Rows.Add(row);
            }
            return dt;
        }

        public static DataTable LeerTxtDataTable(string path)
        {
            return LeerCsvDataTable(path);
        }

        private static string[] ParseCsvLine(string line, char delimiter)
        {
            var parts = new List<string>();
            bool inQuotes = false;
            var cur = new System.Text.StringBuilder();
            for (int i = 0; i < line.Length; i++)
            {
                char ch = line[i];
                if (ch == '"')
                {
                    inQuotes = !inQuotes;
                    continue;
                }
                if (ch == delimiter && !inQuotes)
                {
                    parts.Add(cur.ToString());
                    cur.Clear();
                }
                else
                {
                    cur.Append(ch);
                }
            }
            parts.Add(cur.ToString());
            return parts.ToArray();
        }

        public static List<DataItem> DataTableToDataItems(DataTable dt)
        {
            var list = new List<DataItem>();
            if (dt == null) return list;
            foreach (DataRow row in dt.Rows)
            {
                try
                {
                    var it = new DataItem();
                    if (dt.Columns.Contains("Id"))
                    {
                        var v = row["Id"]?.ToString();
                        if (int.TryParse(v, out int id)) it.Id = id; else it.Id = 0;
                    }
                    if (dt.Columns.Contains("Nombre")) it.Nombre = row["Nombre"]?.ToString() ?? string.Empty;
                    else if (dt.Columns.Contains("Name")) it.Nombre = row["Name"]?.ToString() ?? string.Empty;
                    if (dt.Columns.Contains("Categoria")) it.Categoria = row["Categoria"]?.ToString() ?? string.Empty;
                    else if (dt.Columns.Contains("Category")) it.Categoria = row["Category"]?.ToString() ?? string.Empty;
                    if (dt.Columns.Contains("Precio"))
                    {
                        var v = row["Precio"]?.ToString();
                        if (decimal.TryParse(v, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal p)) it.Precio = p; else it.Precio = 0m;
                    }
                    else if (dt.Columns.Contains("Price"))
                    {
                        var v = row["Price"]?.ToString();
                        if (decimal.TryParse(v, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal p)) it.Precio = p; else it.Precio = 0m;
                    }
                    list.Add(it);
                }
                catch
                {
                    continue;
                }
            }
            return list;
        }

        public static DataTable DataItemsToDataTable(List<DataItem> items)
        {
            var dt = new DataTable();
            dt.Columns.Add("Id");
            dt.Columns.Add("Nombre");
            dt.Columns.Add("Categoria");
            dt.Columns.Add("Precio");
            foreach (var it in items)
            {
                var row = dt.NewRow();
                row[0] = it.Id.ToString();
                row[1] = it.Nombre ?? string.Empty;
                row[2] = it.Categoria ?? string.Empty;
                row[3] = it.Precio.ToString(System.Globalization.CultureInfo.InvariantCulture);
                dt.Rows.Add(row);
            }
            return dt;
        }

        public static List<DataItem> LeerXml(string path)
        {
            var list = new List<DataItem>();
            try
            {
                var doc = new XmlDocument();
                doc.Load(path);
                var nodes = doc.SelectNodes("//Item | //DataItem | //Venta | //Row");
                if (nodes != null)
                {
                    foreach (XmlNode node in nodes)
                    {
                        try
                        {
                            var item = new DataItem();
                            foreach (XmlNode child in node.ChildNodes)
                            {
                                var name = child.Name.ToLowerInvariant();
                                if (name == "id") item.Id = int.Parse(child.InnerText);
                                else if (name == "nombre" || name == "name") item.Nombre = child.InnerText;
                                else if (name == "categoria" || name == "category") item.Categoria = child.InnerText;
                                else if (name == "precio" || name == "price") item.Precio = decimal.Parse(child.InnerText, CultureInfo.InvariantCulture);
                            }
                            list.Add(item);
                        }
                        catch
                        {
                            // skip malformed node
                            continue;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error leyendo XML: " + ex.Message, ex);
            }
            return list;
        }

        public static List<DataItem> LeerTxt(string path)
        {
            var list = new List<DataItem>();
            try
            {
                var lines = File.ReadAllLines(path);
                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    // Expect lines like: Id|Nombre|Categoria|Precio or comma separated
                    string[] parts = null;
                    if (line.Contains("|")) parts = line.Split('|');
                    else parts = line.Split(',');
                    try
                    {
                        var item = new DataItem();
                        if (parts.Length > 0) item.Id = int.Parse(parts[0]);
                        if (parts.Length > 1) item.Nombre = parts[1];
                        if (parts.Length > 2) item.Categoria = parts[2];
                        if (parts.Length > 3) item.Precio = decimal.Parse(parts[3], CultureInfo.InvariantCulture);
                        list.Add(item);
                    }
                    catch
                    {
                        // skip malformed
                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error leyendo TXT: " + ex.Message, ex);
            }
            return list;
        }

        public static List<DataItem> LeerDesdeSqlServer(string connectionString)
        {
            var list = new List<DataItem>();
            try
            {
                // Create a SqlConnection via reflection to avoid requiring a compile-time reference to SqlClient
                Type connType = Type.GetType("System.Data.SqlClient.SqlConnection, System.Data.SqlClient")
                                ?? Type.GetType("Microsoft.Data.SqlClient.SqlConnection, Microsoft.Data.SqlClient");
                if (connType == null)
                    throw new Exception("No se encontró un proveedor de SqlClient en tiempo de ejecución. Asegúrese de tener instalado System.Data.SqlClient o Microsoft.Data.SqlClient.");

                using (var connObj = Activator.CreateInstance(connType, new object[] { connectionString }) as DbConnection)
                {
                    if (connObj == null) throw new Exception("No se pudo crear la conexión SQL.");
                    connObj.Open();
                    using (DbCommand cmd = connObj.CreateCommand())
                    {
                        // Check if table exists first to avoid "Invalid object name" errors
                        cmd.CommandText = "SELECT OBJECT_ID('dbo.Ventas','U')";
                        var exists = cmd.ExecuteScalar();
                        if (exists == null || exists == DBNull.Value)
                        {
                            // Table does not exist - return empty list instead of throwing
                            return list;
                        }

                        // If the table 'Ventas' does not exist, return an empty result set instead of throwing
                        cmd.CommandText =
                            "SELECT Id, Nombre, Categoria, Precio FROM dbo.Ventas";

                        using (DbDataReader rdr = cmd.ExecuteReader())
                        {
                            while (rdr.Read())
                            {
                                try
                                {
                                    var item = new DataItem();
                                    if (!rdr.IsDBNull(rdr.GetOrdinal("Id"))) item.Id = rdr.GetInt32(rdr.GetOrdinal("Id"));
                                    else item.Id = 0;
                                    item.Nombre = rdr.IsDBNull(rdr.GetOrdinal("Nombre")) ? string.Empty : rdr.GetString(rdr.GetOrdinal("Nombre"));
                                    item.Categoria = rdr.IsDBNull(rdr.GetOrdinal("Categoria")) ? string.Empty : rdr.GetString(rdr.GetOrdinal("Categoria"));
                                    item.Precio = rdr.IsDBNull(rdr.GetOrdinal("Precio")) ? 0m : rdr.GetDecimal(rdr.GetOrdinal("Precio"));
                                    list.Add(item);
                                }
                                catch
                                {
                                    continue;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error leyendo desde SQL Server: " + ex.Message, ex);
            }
            return list;
        }
    }
}
