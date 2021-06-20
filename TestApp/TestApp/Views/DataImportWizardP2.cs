using DatabaseAccess;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows.Forms;

namespace TestApp.Views
{
    public partial class DataImportWizardP2 : Form
    {
        private string _excelFile = string.Empty;
        private DBAccessManager _dbManager;
        public delegate void DataDelegate(string txtMsg);
        public DataDelegate _dlgLbl;
        public delegate void ProgressDelegate();
        public ProgressDelegate _dlgPrg;

        public DataImportWizardP2(string file)
        {
            _excelFile = file;
            _dbManager = DBAccessManager.GetInstance();
            InitializeComponent();
            _dlgLbl = new DataDelegate(Update);
            _dlgPrg = new ProgressDelegate(UpdateProgress);
            new Thread(() => LoadData()).Start();
        }

        private void Update(string txtMsg)
        {
            this.label1.Text = txtMsg;
        }

        private void UpdateProgress()
        {
            progressBar1.Style = ProgressBarStyle.Continuous;
            progressBar1.Value = progressBar1.Maximum;
            buttonFinish.Enabled = true;
        }

        private void LoadData()
        {
            using (ExcelReader reader = new ExcelReader(_excelFile))
            {
                List<TurretData> recs = reader.LoadData();
                {
                    label1.Invoke(_dlgLbl, "Inserting Excel data into DB...");
                    _dbManager.ExecuteSql(recs);
                }
            }

            label1.Invoke(_dlgLbl, "Finished Adding data to DB...");
            progressBar1.Invoke(_dlgPrg);
        }

        private void buttonFinish_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
        }
    }
}
