using System;
using System.Windows.Forms;
using System.Data.SqlClient;


namespace DataFusionArena
{
    public class InputConnectionStringForm : Form
    {
        private TextBox txtConn;
        private Button btnOk;
        private Button btnCancel;

        public string ConnectionString => txtConn.Text;

        public InputConnectionStringForm(string initial)
        {
            txtConn = new TextBox() { Left = 10, Top = 10, Width = 460, Text = initial };
            btnOk = new Button() { Left = 300, Top = 40, Text = "OK", DialogResult = DialogResult.OK };
            btnCancel = new Button() { Left = 380, Top = 40, Text = "Cancel", DialogResult = DialogResult.Cancel };
            this.Controls.Add(txtConn);
            this.Controls.Add(btnOk);
            this.Controls.Add(btnCancel);
            this.AcceptButton = btnOk;
            this.CancelButton = btnCancel;
            this.ClientSize = new System.Drawing.Size(480, 80);
            this.Text = "Connection String";
        }
    }
}
