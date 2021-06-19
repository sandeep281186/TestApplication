using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TestApp
{
    public partial class UpdateForm : Form
    {
        private DataTable curDt = null;
        private BindingSource bsData = null;
        private static DataTable dtUpdated = null;

        public UpdateForm(DataTable dt)
        {
            InitializeComponent();
            curDt = dt;
            bsData = new BindingSource();
            bsData.DataSource = dt;
            dataGridViewData.DataSource = bsData;
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            dtUpdated = bsData.DataSource as DataTable;
            dtUpdated.AcceptChanges();
            this.DialogResult = DialogResult.OK;
        }

        public DataTable GetUpdates()
        {
            return dtUpdated;
        }

        private void dataGridViewData_CellBeginEdit(object sender, DataGridViewCellCancelEventArgs e)
        {
            string cellValue = dataGridViewData.Rows[e.RowIndex].Cells[e.ColumnIndex].Value as string;

            if (String.IsNullOrWhiteSpace(cellValue))
                e.Cancel = true;
        }
    }
}
