using System;
using System.Windows.Forms;

namespace iTurret.Views
{
    public partial class DataImportWizardP1 : Form
    {
        private string _selectedFile = string.Empty;

        public DataImportWizardP1()
        {
            InitializeComponent();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            using (FileDialog fd = new OpenFileDialog())
            {
                fd.Filter = "Excel Files(*.xls,*.xlsx)|*.xls;*.xlsx";

                if(fd.ShowDialog() == DialogResult.OK)
                {
                    linkLabel1.Text = "Selected File: " + fd.FileName;
                    _selectedFile = fd.FileName;
                }
            }
        }

        private void buttonNxt_Click(object sender, EventArgs e)
        {
            if(!string.IsNullOrEmpty(_selectedFile))
                this.DialogResult = DialogResult.OK;
        }

        public string GetSelectedFile()
        {
            return _selectedFile;
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }
    }
}
