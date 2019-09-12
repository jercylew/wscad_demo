using System;
using System.Windows.Forms;

namespace WSCAD_Demo
{
    public partial class ExportPDFSettings : Form
    {
        public string PdfSavePath
        {
            get
            {
                return textBoxSavePath.Text.Trim();
            }
        }
        public float PdfWidth
        {
            get
            {
                return float.Parse(textBoxWidth.Text);
            }
        }
        public float PdfHeight
        {
            get
            {
                return float.Parse(textBoxHeight.Text);
            }
        }
        public ExportPDFSettings()
        {
            InitializeComponent();
        }

        private void ButtonBrowse_Click(object sender, EventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog
            {
                Filter = @"PDF files (*.pdf)|*.pdf"
            };

            if (dialog.ShowDialog() == DialogResult.OK)
            {
                textBoxSavePath.Text = dialog.FileName;
            }

            dialog.Dispose();
        }

        private void TextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) &&
                !char.IsDigit(e.KeyChar) &&
                !(e.KeyChar == '.'))
            {
                e.Handled = true;
            }
        }

        private void ButtonOk_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBoxWidth.Text) ||
                string.IsNullOrEmpty(textBoxHeight.Text) ||
                string.IsNullOrEmpty(textBoxSavePath.Text))
            {
                MessageBox.Show(this, "Please enter the fields", "Export PDF Settings");
                return;
            }

            DialogResult = DialogResult.OK;
            this.Close();
        }

    }
}
