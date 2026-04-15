using Microsoft.VisualBasic.ApplicationServices;
using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;

namespace DataFusionArena
{
    public partial class Form1 : Form
    {
        private readonly DataProcessor processor = new DataProcessor();
        private System.Data.DataTable currentTable = null;
        // mapping between DataTable column names and DataItem properties
        private string mappedIdColumn = null;
        private string mappedNameColumn = null;
        private string mappedCategoryColumn = null;
        private string mappedPriceColumn = null;

        public Form1()
        {
            try
            {
                InitializeComponent();
                PopulateComboBoxes();
                dgvDatos.AutoGenerateColumns = true;
                RefreshGrid();
            }
            catch (Exception ex)
            {
                // Show error and rethrow so Program.Main can also display it
                try { MessageBox.Show("Error iniciando Form1: " + ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); } catch { }
                throw;
            }
        }

        // Map columns heuristically and convert rows into DataItem list
        private List<DataItem> ConvertDataTableToDataItems(DataTable dt)
        {
            var list = new List<DataItem>();
            if (dt == null) return list;
            MapColumnsToDataItemProperties(dt);
            foreach (DataRow row in dt.Rows)
            {
                try
                {
                    var it = new DataItem();
                    if (!string.IsNullOrEmpty(mappedIdColumn) && dt.Columns.Contains(mappedIdColumn))
                    {
                        var v = row[mappedIdColumn]?.ToString();
                        if (int.TryParse(v, out int id)) it.Id = id; else it.Id = 0;
                    }
                    if (!string.IsNullOrEmpty(mappedNameColumn) && dt.Columns.Contains(mappedNameColumn)) it.Nombre = row[mappedNameColumn]?.ToString() ?? string.Empty;
                    if (!string.IsNullOrEmpty(mappedCategoryColumn) && dt.Columns.Contains(mappedCategoryColumn)) it.Categoria = row[mappedCategoryColumn]?.ToString() ?? string.Empty;
                    if (!string.IsNullOrEmpty(mappedPriceColumn) && dt.Columns.Contains(mappedPriceColumn))
                    {
                        var v = row[mappedPriceColumn]?.ToString();
                        if (decimal.TryParse(v, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal p)) it.Precio = p; else it.Precio = 0m;
                    }
                    list.Add(it);
                }
                catch { continue; }
            }
            return list;
        }

        private void MapColumnsToDataItemProperties(DataTable dt)
        {
            mappedIdColumn = null; mappedNameColumn = null; mappedCategoryColumn = null; mappedPriceColumn = null;
            if (dt == null) return;
            foreach (DataColumn col in dt.Columns)
            {
                var name = col.ColumnName.Trim();
                var low = name.ToLowerInvariant();
                if (mappedIdColumn == null && (low == "id" || low.Contains("id") || low.EndsWith("_id"))) mappedIdColumn = col.ColumnName;
                else if (mappedPriceColumn == null && (low == "precio" || low == "price" || low.Contains("precio") || low.Contains("price") || low.Contains("cost") || low.Contains("amount"))) mappedPriceColumn = col.ColumnName;
                else if (mappedCategoryColumn == null && (low == "categoria" || low == "category" || low.Contains("cat"))) mappedCategoryColumn = col.ColumnName;
                else if (mappedNameColumn == null && (low == "nombre" || low == "name" || low.Contains("title") || low.Contains("descripcion") || low.Contains("description"))) mappedNameColumn = col.ColumnName;
            }
            // Fallbacks: if some not found, try common positions
            if (mappedNameColumn == null && dt.Columns.Count >= 2) mappedNameColumn = dt.Columns[1].ColumnName;
            if (mappedCategoryColumn == null)
            {
                // try to find a text column other than id/name/price
                foreach (DataColumn col in dt.Columns)
                {
                    if (col.ColumnName == mappedIdColumn || col.ColumnName == mappedNameColumn || col.ColumnName == mappedPriceColumn) continue;
                    if (col.DataType == typeof(string)) { mappedCategoryColumn = col.ColumnName; break; }
                }
            }
        }

        // Helper to run an action while showing a busy cursor and disabling the form
        private void RunWithWaitCursor(Action action, Cursor busyCursor = null)
        {
            Cursor chosen = busyCursor ?? Cursors.WaitCursor;
            try
            {
                this.UseWaitCursor = true;
                Cursor.Current = chosen;
                this.Enabled = false;
                action?.Invoke();
            }
            finally
            {
                this.Enabled = true;
                this.UseWaitCursor = false;
                Cursor.Current = Cursors.Default;
            }
        }

        private void btnCargarArchivo_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Filter = "All supported|*.json;*.csv;*.xml;*.txt|JSON files|*.json|CSV files|*.csv|XML files|*.xml|Text files|*.txt";
                if (ofd.ShowDialog() != DialogResult.OK) return;
                string path = ofd.FileName;
                try
                {
                    RunWithWaitCursor(() =>
                    {
                        List<DataItem> nuevos = new List<DataItem>();
                        string ext = System.IO.Path.GetExtension(path).ToLowerInvariant();
                        if (ext == ".json") currentTable = DataImporter.LeerJsonDataTable(path);
                        else if (ext == ".csv") currentTable = DataImporter.LeerCsvDataTable(path);
                        else if (ext == ".xml") currentTable = DataImporter.LeerXmlDataTable(path);
                        else if (ext == ".txt") currentTable = DataImporter.LeerTxtDataTable(path);
                        else throw new Exception("Extensión no soportada: " + ext);

                        // Mostrar DataTable en DataGridView
                        dgvDatos.DataSource = null;
                        dgvDatos.DataSource = currentTable;

                        // Actualizar combos según columnas
                        UpdateComboBoxesFromDataTable(currentTable);

                        // Convertir filas a DataItem y agregar al processor (y almacenar mapeos)
                        nuevos = ConvertDataTableToDataItems(currentTable);
                        processor.AgregarDatos(nuevos);
                        // seleccionar por defecto la columna mapeada como categoria en el combo
                        if (!string.IsNullOrEmpty(mappedCategoryColumn))
                        {
                            try { cmbCampoFiltro.SelectedItem = mappedCategoryColumn; } catch { }
                        }
                        RefreshChart();
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error cargando archivo: " + ex.ToString());
                }
            }
        }

        private void btnConectarSql_Click(object sender, EventArgs e)
        {
            try
            {
                // Default connection to local SQLEXPRESS using Windows Authentication
                string conn = "Server=192.168.1.171\\SQLEXPRESS;Database=DataFusionArena;User = sa;Password=123;TrustServerCertificate=True;";
                // Ask user for connection string optionally
                using (var input = new InputConnectionStringForm(conn))
                {
                    if (input.ShowDialog() == DialogResult.OK)
                    {
                        string cs = input.ConnectionString;
                        RunWithWaitCursor(() =>
                        {
                            var datos = DataImporter.LeerDesdeSqlServer(cs);
                            processor.AgregarDatos(datos);
                            RefreshGrid();
                            RefreshChart();
                        }, Cursors.AppStarting);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error conectando a SQL Server: " + ex.ToString());
            }
        }

        private void btnLimpiar_Click(object sender, EventArgs e)
        {
            // Clear current table and processor items
            RunWithWaitCursor(() =>
            {
                currentTable = null;
                mappedIdColumn = mappedNameColumn = mappedCategoryColumn = mappedPriceColumn = null;
                // clear processor internal lists by creating a new instance
                // (processor is readonly, so we clear via reflection of its private list if necessary)
                try
                {
                    var itemsField = typeof(DataProcessor).GetField("items", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var idLookupField = typeof(DataProcessor).GetField("idLookup", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    var catLookupField = typeof(DataProcessor).GetField("categoryLookup", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (itemsField != null) itemsField.SetValue(processor, new List<DataItem>());
                    if (idLookupField != null) idLookupField.SetValue(processor, new Dictionary<int, DataItem>());
                    if (catLookupField != null) catLookupField.SetValue(processor, new Dictionary<string, List<DataItem>>(StringComparer.OrdinalIgnoreCase));
                }
                catch { }
                dgvDatos.DataSource = null;
                UpdateComboBoxesFromDataTable(null);
                RefreshChart();
            });
        }

        private void btnFiltrar_Click(object sender, EventArgs e)
        {
            RunWithWaitCursor(() =>
            {
                string valor = txtCategoria.Text.Trim();
                string campo = cmbCampoFiltro?.SelectedItem?.ToString();
                if (string.IsNullOrEmpty(valor))
                {
                    RefreshGrid();
                    return;
                }

                // If currentTable has the column, filter the DataTable directly
                if (currentTable != null && !string.IsNullOrEmpty(campo) && currentTable.Columns.Contains(campo))
                {
                    var dtf = currentTable.Clone();
                    foreach (DataRow r in currentTable.Rows)
                    {
                        var s = (r[campo] ?? string.Empty).ToString();
                        if (s.IndexOf(valor, StringComparison.OrdinalIgnoreCase) >= 0) dtf.Rows.Add(r.ItemArray);
                    }
                    currentTable = dtf;
                    UpdateComboBoxesFromDataTable(currentTable);
                    dgvDatos.DataSource = null;
                    dgvDatos.DataSource = currentTable;
                    return;
                }

                // fallback to processor filtering (mapped column names)
                string useCampo = campo ?? mappedCategoryColumn ?? "Categoria";
                var filtrados = processor.FiltrarPorCampo(useCampo, valor);
                dgvDatos.DataSource = null;
                dgvDatos.DataSource = filtrados;
            });
        }

        private void btnExportarBD_Click(object sender, EventArgs e)
        {
            try
            {
                // Pedir cadena de conexión
                string defaultConn = "Server=192.168.1.171\\SQLEXPRESS;Database=DataFusionArena;User=sa;Password=123;TrustServerCertificate=True;";
                using (var inputConn = new InputConnectionStringForm(defaultConn))
                {
                    if (inputConn.ShowDialog() != DialogResult.OK) return;
                    string cs = inputConn.ConnectionString;

                    // Pedir nombre de tabla
                    using (var inputName = new InputTextForm("Ventas"))
                    {
                        if (inputName.ShowDialog() != DialogResult.OK) return;
                        string tableName = inputName.InputText;

                        System.Data.DataTable tableToExport = currentTable;
                        if (tableToExport == null)
                        {
                            // convertir lista unificada a DataTable
                            tableToExport = DataImporter.DataItemsToDataTable(processor.GetAllItems());
                        }

                        int count = 0;
                        RunWithWaitCursor(() =>
                        {
                            count = DataExporter.ExportDataTableToSqlServer(tableToExport, cs, tableName);
                        }, Cursors.AppStarting);
                        MessageBox.Show($"Exportación completada. Filas insertadas: {count}");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error exportando a BD: " + ex.ToString());
            }
        }

        private void btnOrdenarPrecio_Click(object sender, EventArgs e)
        {
            RunWithWaitCursor(() =>
            {
                bool asc = chkAscendente?.Checked ?? true;
                string campo = cmbCampoOrden?.SelectedItem?.ToString();
                // If currentTable has the column, sort DataTable and show
                if (currentTable != null && !string.IsNullOrEmpty(campo) && currentTable.Columns.Contains(campo))
                {
                    var dv = currentTable.DefaultView;
                    dv.Sort = campo + (asc ? " ASC" : " DESC");
                    var sorted = dv.ToTable();
                    currentTable = sorted;
                    UpdateComboBoxesFromDataTable(currentTable);
                    dgvDatos.DataSource = null;
                    dgvDatos.DataSource = currentTable;
                    return;
                }

                string useCampo = campo ?? mappedPriceColumn ?? "Precio";
                if (string.Equals(useCampo, "Precio", StringComparison.OrdinalIgnoreCase) || useCampo == mappedPriceColumn)
                {
                    processor.OrdenarPorPrecioManual(asc);
                }
                else
                {
                    processor.OrdenarPorCampoManual(useCampo, asc);
                }
                RefreshGrid();
            });
        }

        // btnAgrupar removed

        // btnDetectarDuplicados removed

        private void btnImportar_Click(object sender, EventArgs e)
        {
            try
            {
                string defaultConn = "Server=192.168.1.171\\SQLEXPRESS;Database=DataFusionArena;User=sa;Password=123;TrustServerCertificate=True;";
                using (var inputConn = new InputConnectionStringForm(defaultConn))
                {
                    if (inputConn.ShowDialog() != DialogResult.OK) return;
                    string cs = inputConn.ConnectionString;

                    // Ask user for table name (allow any table)
                    using (var inputTable = new InputTextForm("Ventas"))
                    {
                        if (inputTable.ShowDialog() != DialogResult.OK) return;
                        string tableName = inputTable.InputText?.Trim();
                        if (string.IsNullOrEmpty(tableName)) return;

                        RunWithWaitCursor(() =>
                        {
                            // Read selected table into DataTable using reflection-based SqlClient similar to DataImporter
                            var dt = new System.Data.DataTable();
                            Type connType = Type.GetType("System.Data.SqlClient.SqlConnection, System.Data.SqlClient")
                                            ?? Type.GetType("Microsoft.Data.SqlClient.SqlConnection, Microsoft.Data.SqlClient");
                            if (connType == null) throw new Exception("No se encontró un proveedor de SqlClient en tiempo de ejecución.");

                            using (var connObj = Activator.CreateInstance(connType, new object[] { cs }) as System.Data.Common.DbConnection)
                            {
                                if (connObj == null) throw new Exception("No se pudo crear la conexión SQL.");
                                connObj.Open();
                                using (var cmd = connObj.CreateCommand())
                                {
                                    cmd.CommandText = $"SELECT TOP (10000) * FROM dbo.[{tableName}]";
                                    try
                                    {
                                        using (var rdr = cmd.ExecuteReader())
                                        {
                                            dt.Load(rdr);
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        // Try without schema
                                        cmd.CommandText = $"SELECT TOP (10000) * FROM [{tableName}]";
                                        using (var rdr = cmd.ExecuteReader()) dt.Load(rdr);
                                    }
                                }
                            }

                            // Show DataTable in grid
                            currentTable = dt;
                            dgvDatos.Invoke((Action)(() =>
                            {
                                dgvDatos.DataSource = null;
                                dgvDatos.DataSource = currentTable;
                            }));

                            // Update combos to reflect columns
                            UpdateComboBoxesFromDataTable(dt);

                            // Convert to DataItems and add to processor (store mapping)
                            var datos = ConvertDataTableToDataItems(dt);
                            processor.AgregarDatos(datos);
                            RefreshChart();
                        }, Cursors.AppStarting);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error importando desde BD: " + ex.ToString());
            }
        }

        private void PopulateComboBoxes()
        {
            try
            {
                var fields = new[] { "Id", "Nombre", "Categoria", "Precio" };
                cmbCampoFiltro.Items.Clear();
                cmbCampoOrden.Items.Clear();
                foreach (var f in fields)
                {
                    cmbCampoFiltro.Items.Add(f);
                    cmbCampoOrden.Items.Add(f);
                }
                cmbCampoFiltro.SelectedIndex = 2; // Categoria
                cmbCampoOrden.SelectedIndex = 3; // Precio
            }
            catch { }
        }

        private void RefreshGrid()
        {
            dgvDatos.DataSource = null;
            if (currentTable != null)
            {
                dgvDatos.DataSource = currentTable;
            }
            else
            {
                dgvDatos.DataSource = processor.GetAllItems();
            }
        }

        private void UpdateComboBoxesFromDataTable(DataTable dt)
        {
            try
            {
                cmbCampoFiltro.Items.Clear();
                cmbCampoOrden.Items.Clear();
                if (dt != null)
                {
                    foreach (DataColumn col in dt.Columns)
                    {
                        cmbCampoFiltro.Items.Add(col.ColumnName);
                        cmbCampoOrden.Items.Add(col.ColumnName);
                    }
                    // prefer mapped category and price if available
                    if (!string.IsNullOrEmpty(mappedCategoryColumn) && cmbCampoFiltro.Items.Contains(mappedCategoryColumn)) cmbCampoFiltro.SelectedItem = mappedCategoryColumn;
                    else if (cmbCampoFiltro.Items.Count > 0) cmbCampoFiltro.SelectedIndex = 0;
                    if (!string.IsNullOrEmpty(mappedPriceColumn) && cmbCampoOrden.Items.Contains(mappedPriceColumn)) cmbCampoOrden.SelectedItem = mappedPriceColumn;
                    else if (cmbCampoOrden.Items.Count > 0) cmbCampoOrden.SelectedIndex = 0;
                }
                else
                {
                    // fallback
                    var fields = new[] { "Id", "Nombre", "Categoria", "Precio" };
                    foreach (var f in fields)
                    {
                        cmbCampoFiltro.Items.Add(f);
                        cmbCampoOrden.Items.Add(f);
                    }
                    cmbCampoFiltro.SelectedIndex = 2;
                    cmbCampoOrden.SelectedIndex = 3;
                }
            }
            catch { }
        }

        private void RefreshChart()
        {
            var grouping = processor.GetCategoryDictionary();
            int width = Math.Max(200, chartCategorias.Width);
            int height = Math.Max(120, chartCategorias.Height);
            var bmp = new System.Drawing.Bitmap(width, height);
            bool success = false;
            try
            {
                using (var g = System.Drawing.Graphics.FromImage(bmp))
                {
                    g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    g.Clear(System.Drawing.Color.White);
                    int margin = 30;
                    int x = margin;
                    int maxCount = 0;
                    foreach (var kvp in grouping)
                    {
                        int c = kvp.Value?.Count ?? 0;
                        if (c > maxCount) maxCount = c;
                    }
                    if (maxCount == 0) maxCount = 1;
                    int bars = Math.Max(1, grouping.Count);
                    int barWidth = Math.Max(20, (width - margin * 2) / bars);

                    // Draw Y axis
                    g.DrawLine(System.Drawing.Pens.Black, margin - 10, margin - 5, margin - 10, height - margin + 5);
                    g.DrawLine(System.Drawing.Pens.Black, margin - 12, height - margin + 5, width - margin + 5, height - margin + 5);

                    // draw bars
                    int iBar = 0;
                    foreach (var kvp in grouping)
                    {
                        int c = kvp.Value?.Count ?? 0;
                        int barHeight = (int)((height - margin * 2) * ((double)c / maxCount));
                        int bx = margin + iBar * barWidth;
                        var rect = new System.Drawing.Rectangle(bx, height - margin - barHeight, Math.Max(0, barWidth - 6), Math.Max(0, barHeight));
                        if (rect.Width > 0 && rect.Height > 0)
                        {
                            try
                            {
                                using (var brush = new System.Drawing.Drawing2D.LinearGradientBrush(rect, System.Drawing.Color.SteelBlue, System.Drawing.Color.LightSkyBlue, 90f))
                                {
                                    g.FillRectangle(brush, rect);
                                }
                            }
                            catch
                            {
                                g.FillRectangle(System.Drawing.Brushes.SteelBlue, rect);
                            }
                        }
                        else
                        {
                            // fallback solid fill when size is zero
                            if (rect.Width > 0 && rect.Height > 0) g.FillRectangle(System.Drawing.Brushes.SteelBlue, rect);
                        }
                        g.DrawRectangle(System.Drawing.Pens.DarkBlue, rect);

                        // category label (wrap if needed)
                        var label = (kvp.Key ?? string.Empty).ToString();
                        var sf = new System.Drawing.StringFormat() { Alignment = System.Drawing.StringAlignment.Center };
                        // truncate very long labels to avoid GDI+ layout issues
                        var displayLabel = label.Length > 80 ? label.Substring(0, 80) + "..." : label;
                        try
                        {
                            // center by using x position and stringformat
                            float lx = bx + (barWidth - 6) / 2f;
                            float ly = height - margin + 2f;
                            g.DrawString(displayLabel, this.Font, System.Drawing.Brushes.Black, lx, ly, sf);
                        }
                        catch { }
                        // value
                        try
                        {
                            float vx = bx + (barWidth - 6) / 2f;
                            float vy = height - margin - barHeight - 14f;
                            if (!float.IsNaN(vx) && !float.IsNaN(vy) && !float.IsInfinity(vx) && !float.IsInfinity(vy))
                                g.DrawString(c.ToString(), this.Font, System.Drawing.Brushes.Black, vx, vy, sf);
                        }
                        catch { }
                        iBar++;
                    }

                    // Legend / title
                    try
                    {
                        using (var titleFont = new System.Drawing.Font(this.Font.FontFamily, Math.Max(8f, this.Font.Size + 1), System.Drawing.FontStyle.Bold))
                        {
                            g.DrawString("Distribución por categoría", titleFont, System.Drawing.Brushes.Black, margin, 4);
                        }
                    }
                    catch { }
                }
                success = true;
            }
            catch (Exception ex)
            {
                try { var p = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "DataFusionArena_error.log"); System.IO.File.AppendAllText(p, DateTime.Now.ToString("s") + " - Chart error: " + ex + Environment.NewLine); } catch { }
                success = false;
            }
            if (success)
            {
                try
                {
                    chartCategorias.Image?.Dispose();
                    chartCategorias.Image = bmp;
                }
                catch
                {
                    try { bmp.Dispose(); } catch { }
                    chartCategorias.Image = null;
                }
            }
            else
            {
                try { bmp.Dispose(); } catch { }
                chartCategorias.Image = null;
            }
        }
    }
}
