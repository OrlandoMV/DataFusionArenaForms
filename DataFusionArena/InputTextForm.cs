using System;
using System.Windows.Forms;

namespace DataFusionArena
{
    public class InputTextForm : Form
    {
        private TextBox txt;
        private Button ok;
        private Button cancel;

        public string InputText => txt.Text;

        public InputTextForm(string initial)
        {
            txt = new TextBox() { Left = 10, Top = 10, Width = 360, Text = initial };
            ok = new Button() { Left = 200, Top = 40, Text = "OK", DialogResult = DialogResult.OK };
            cancel = new Button() { Left = 280, Top = 40, Text = "Cancel", DialogResult = DialogResult.Cancel };
            this.Controls.Add(txt);
            this.Controls.Add(ok);
            this.Controls.Add(cancel);
            this.AcceptButton = ok;
            this.CancelButton = cancel;
            this.ClientSize = new System.Drawing.Size(380, 80);
            this.Text = "Nombre de tabla";
        }
    }
}
