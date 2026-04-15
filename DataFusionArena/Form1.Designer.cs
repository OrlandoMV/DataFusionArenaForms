using System.Windows.Forms;
using System.Drawing;

namespace DataFusionArena
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Button btnCargarArchivo;
        private System.Windows.Forms.Button btnExportarBD;
        private System.Windows.Forms.DataGridView dgvDatos;
        private System.Windows.Forms.TextBox txtCategoria;
        private System.Windows.Forms.ComboBox cmbCampoFiltro;
        private System.Windows.Forms.Button btnFiltrar;
        private System.Windows.Forms.PictureBox chartCategorias;
        private System.Windows.Forms.Button btnOrdenarPrecio;
        private System.Windows.Forms.CheckBox chkAscendente;
        private System.Windows.Forms.ComboBox cmbCampoOrden;

        private System.Windows.Forms.Button btnLimpiar;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            btnCargarArchivo = new Button();
            btnExportarBD = new Button();
            dgvDatos = new DataGridView();
            txtCategoria = new TextBox();
            cmbCampoFiltro = new ComboBox();
            btnFiltrar = new Button();
            chartCategorias = new PictureBox();
            btnOrdenarPrecio = new Button();
            chkAscendente = new CheckBox();
            cmbCampoOrden = new ComboBox();
            btnImportar = new Button();
            btnLimpiar = new Button();
            groupBox1 = new GroupBox();
            groupBox2 = new GroupBox();
            groupBox3 = new GroupBox();
            ((System.ComponentModel.ISupportInitialize)dgvDatos).BeginInit();
            ((System.ComponentModel.ISupportInitialize)chartCategorias).BeginInit();
            groupBox1.SuspendLayout();
            groupBox2.SuspendLayout();
            groupBox3.SuspendLayout();
            SuspendLayout();
            // 
            // btnCargarArchivo
            // 
            btnCargarArchivo.Location = new Point(5, 15);
            btnCargarArchivo.Margin = new Padding(2);
            btnCargarArchivo.Name = "btnCargarArchivo";
            btnCargarArchivo.Size = new Size(119, 34);
            btnCargarArchivo.TabIndex = 0;
            btnCargarArchivo.Text = "Cargar Archivo";
            btnCargarArchivo.UseVisualStyleBackColor = true;
            btnCargarArchivo.Click += btnCargarArchivo_Click;
            // 
            // btnExportarBD
            // 
            btnExportarBD.Location = new Point(128, 15);
            btnExportarBD.Margin = new Padding(2);
            btnExportarBD.Name = "btnExportarBD";
            btnExportarBD.Size = new Size(116, 34);
            btnExportarBD.TabIndex = 1;
            btnExportarBD.Text = "Exportar a BD";
            btnExportarBD.UseVisualStyleBackColor = true;
            btnExportarBD.Click += btnExportarBD_Click;
            // 
            // dgvDatos
            // 
            dgvDatos.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            dgvDatos.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dgvDatos.Location = new Point(10, 79);
            dgvDatos.Margin = new Padding(2);
            dgvDatos.Name = "dgvDatos";
            dgvDatos.RowHeadersWidth = 62;
            dgvDatos.Size = new Size(1241, 367);
            dgvDatos.TabIndex = 2;
            // 
            // txtCategoria
            // 
            txtCategoria.Location = new Point(128, 15);
            txtCategoria.Margin = new Padding(2);
            txtCategoria.Name = "txtCategoria";
            txtCategoria.Size = new Size(110, 27);
            txtCategoria.TabIndex = 3;
            // 
            // cmbCampoFiltro
            // 
            cmbCampoFiltro.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbCampoFiltro.Location = new Point(5, 15);
            cmbCampoFiltro.Margin = new Padding(2);
            cmbCampoFiltro.Name = "cmbCampoFiltro";
            cmbCampoFiltro.Size = new Size(110, 28);
            cmbCampoFiltro.TabIndex = 3;
            // 
            // btnFiltrar
            // 
            btnFiltrar.Location = new Point(248, 14);
            btnFiltrar.Margin = new Padding(2);
            btnFiltrar.Name = "btnFiltrar";
            btnFiltrar.Size = new Size(110, 28);
            btnFiltrar.TabIndex = 4;
            btnFiltrar.Text = "Filtrar";
            btnFiltrar.UseVisualStyleBackColor = true;
            btnFiltrar.Click += btnFiltrar_Click;
            // 
            // chartCategorias
            // 
            chartCategorias.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            chartCategorias.Location = new Point(10, 500);
            chartCategorias.Margin = new Padding(2);
            chartCategorias.Name = "chartCategorias";
            chartCategorias.Size = new Size(1237, 394);
            chartCategorias.TabIndex = 5;
            chartCategorias.TabStop = false;
            // 
            // btnOrdenarPrecio
            // 
            btnOrdenarPrecio.Location = new Point(5, 13);
            btnOrdenarPrecio.Margin = new Padding(2);
            btnOrdenarPrecio.Name = "btnOrdenarPrecio";
            btnOrdenarPrecio.Size = new Size(110, 32);
            btnOrdenarPrecio.TabIndex = 6;
            btnOrdenarPrecio.Text = "Ordenar";
            btnOrdenarPrecio.UseVisualStyleBackColor = true;
            btnOrdenarPrecio.Click += btnOrdenarPrecio_Click;
            // 
            // chkAscendente
            // 
            chkAscendente.Location = new Point(235, 12);
            chkAscendente.Margin = new Padding(2);
            chkAscendente.Name = "chkAscendente";
            chkAscendente.Size = new Size(110, 32);
            chkAscendente.TabIndex = 7;
            chkAscendente.Text = "Ascendente";
            chkAscendente.UseVisualStyleBackColor = true;
            // 
            // cmbCampoOrden
            // 
            cmbCampoOrden.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbCampoOrden.Location = new Point(118, 14);
            cmbCampoOrden.Margin = new Padding(2);
            cmbCampoOrden.Name = "cmbCampoOrden";
            cmbCampoOrden.Size = new Size(110, 28);
            cmbCampoOrden.TabIndex = 8;
            // 
            // btnImportar
            // 
            btnImportar.Location = new Point(248, 15);
            btnImportar.Margin = new Padding(2);
            btnImportar.Name = "btnImportar";
            btnImportar.Size = new Size(127, 34);
            btnImportar.TabIndex = 11;
            btnImportar.Text = "Importar de BD";
            btnImportar.UseVisualStyleBackColor = true;
            btnImportar.Click += btnImportar_Click;
            // 
            // btnLimpiar
            // 
            btnLimpiar.Location = new Point(379, 14);
            btnLimpiar.Margin = new Padding(2);
            btnLimpiar.Name = "btnLimpiar";
            btnLimpiar.Size = new Size(85, 34);
            btnLimpiar.TabIndex = 12;
            btnLimpiar.Text = "Limpiar";
            btnLimpiar.UseVisualStyleBackColor = true;
            btnLimpiar.Click += btnLimpiar_Click;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(btnImportar);
            groupBox1.Controls.Add(btnLimpiar);
            groupBox1.Controls.Add(btnCargarArchivo);
            groupBox1.Controls.Add(btnExportarBD);
            groupBox1.Location = new Point(12, 7);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(473, 62);
            groupBox1.TabIndex = 13;
            groupBox1.TabStop = false;
            // 
            // groupBox2
            // 
            groupBox2.Controls.Add(cmbCampoFiltro);
            groupBox2.Controls.Add(txtCategoria);
            groupBox2.Controls.Add(btnFiltrar);
            groupBox2.Location = new Point(504, 8);
            groupBox2.Name = "groupBox2";
            groupBox2.Size = new Size(366, 61);
            groupBox2.TabIndex = 14;
            groupBox2.TabStop = false;
            // 
            // groupBox3
            // 
            groupBox3.Controls.Add(chkAscendente);
            groupBox3.Controls.Add(btnOrdenarPrecio);
            groupBox3.Controls.Add(cmbCampoOrden);
            groupBox3.Location = new Point(887, 7);
            groupBox3.Name = "groupBox3";
            groupBox3.Size = new Size(360, 62);
            groupBox3.TabIndex = 15;
            groupBox3.TabStop = false;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(8F, 20F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1262, 928);
            Controls.Add(groupBox3);
            Controls.Add(groupBox2);
            Controls.Add(groupBox1);
            Controls.Add(chartCategorias);
            Controls.Add(dgvDatos);
            Margin = new Padding(2);
            Name = "Form1";
            StartPosition = FormStartPosition.CenterParent;
            Text = "Data Fusion Arena";
            ((System.ComponentModel.ISupportInitialize)dgvDatos).EndInit();
            ((System.ComponentModel.ISupportInitialize)chartCategorias).EndInit();
            groupBox1.ResumeLayout(false);
            groupBox2.ResumeLayout(false);
            groupBox2.PerformLayout();
            groupBox3.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Button btnImportar;
        private GroupBox groupBox1;
        private GroupBox groupBox2;
        private GroupBox groupBox3;
    }
}
