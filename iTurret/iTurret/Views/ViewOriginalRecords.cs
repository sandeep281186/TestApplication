using DatabaseAccess;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace iTurret.Views
{
    public partial class ViewOriginalRecords : Form
    {
        private DBAccessManager _dbManager;
        private List<MainMenu> _appMenus;
        private BindingSource _bsViewRecs;
        private DataTable _dtView;
        private DataTable _usrData;
        private DataTable _dtInitialData;
        private int _activeMenuIndex;

        public ViewOriginalRecords(List<MainMenu> ms, DataTable userData, int activeMenu)
        {
            InitializeComponent();
            _dbManager = DBAccessManager.GetInstance();
            _appMenus = ms;
            _dtView = new DataTable();
            _dtView.Columns.Add("Key Index", typeof(int));
            _dtView.Columns.Add("Key Label Initial");
            _dtView.Columns.Add("Key Label Latest");
            _bsViewRecs = new BindingSource();
            _bsViewRecs.DataSource = _dtView;
            dataGridViewOriginalRecs.DataSource = _bsViewRecs;
            _usrData = userData;
            _activeMenuIndex = activeMenu;
        }

        private void buttonClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void LoadMenu(int index)
        {
            _dtView.Clear();
            MainMenu m = _appMenus[index];

            if (m.PageIndexes.Count > 0)
            {
                KeyValuePair<int, List<Record>> pages = m.PageIndexes.ElementAt(m.activePage);

                foreach (Record r in pages.Value)
                {
                    DataRow dr = _dtView.NewRow();
                    dr["Key Index"] = r.PageIndex;
                    dr["Key Label Initial"] = _dtInitialData.AsEnumerable().Where(x => x.Field<int>("RecordID") ==
                                              _usrData.AsEnumerable().Where(y =>
                                                                            y.Field<int>("Page Number") == pages.Key &&
                                                                            y.Field<int>("Key Index") == r.PageIndex)
                                                                      .FirstOrDefault().Field<int>("RecordID"))
                                                                      .FirstOrDefault().Field<string>("Key Label");
                    dr["Key Label Latest"] = r.KeyLabel.ToLower().Equals(dr["Key Label Initial"].ToString().ToLower()) ? string.Empty : r.KeyLabel;
                    _dtView.Rows.Add(dr);
                }

                labelPageName.Text = m.Name + "-" + pages.Key;

                _bsViewRecs.DataSource = _dtView;
            }
            else
            {
                labelPageName.Text = m.Name;
            }
        }

        private void ViewOriginalRecords_Load(object sender, EventArgs e)
        {
            _dtInitialData = _dbManager.LoadInitialTurretData();

            LoadMenu(_activeMenuIndex);

            foreach (DataGridViewColumn dvc in dataGridViewOriginalRecs.Columns)
            {
                if (dvc.Name.Equals("Key Index"))
                    dvc.SortMode = DataGridViewColumnSortMode.Programmatic;
                else
                    dvc.SortMode = DataGridViewColumnSortMode.NotSortable;
            }

            dataGridViewOriginalRecs.Sort(dataGridViewOriginalRecs.Columns["Key Index"], System.ComponentModel.ListSortDirection.Ascending);
            labelUserName.Text = _usrData.Rows[0]["USER"].ToString();
        }

        private void buttonPersonal_Click(object sender, EventArgs e)
        {
            LoadMenu(_activeMenuIndex = 0);
        }

        private void buttonDialtone_Click(object sender, EventArgs e)
        {
            LoadMenu(_activeMenuIndex = 1);
        }

        private void buttonTeamSpeedDials_Click(object sender, EventArgs e)
        {
            LoadMenu(_activeMenuIndex = 2);
        }

        private void buttonPersonalSpeedDials_Click(object sender, EventArgs e)
        {
            LoadMenu(_activeMenuIndex = 3);
        }

        private void buttonPrivateWires_Click(object sender, EventArgs e)
        {
            LoadMenu(_activeMenuIndex = 4);
        }

        private void buttonIntercomSpeedDials_Click(object sender, EventArgs e)
        {
            LoadMenu(_activeMenuIndex = 5);
        }

        private void buttonActivityPage_Click(object sender, EventArgs e)
        {
            LoadMenu(_activeMenuIndex = 6);
        }

        private void buttonCallRegisterShortcut_Click(object sender, EventArgs e)
        {
            LoadMenu(_activeMenuIndex = 7);
        }

        private void buttonPrev_Click(object sender, EventArgs e)
        {
            MainMenu mm = _appMenus[_activeMenuIndex];

            if (mm.PageIndexes != null && mm.PageIndexes.Count > 0)
            {
                int prevPage = mm.PageIndexes.ElementAt((int)Math.Max(mm.activePage - 1, 0)).Key;
                mm.activePage = (int)Math.Max(mm.activePage - 1, 0);
                LoadMenu(_activeMenuIndex);
            }
        }

        private void buttonNext_Click(object sender, EventArgs e)
        {
            MainMenu mm = _appMenus[_activeMenuIndex];

            if (mm.PageIndexes != null && mm.PageIndexes.Count > 0)
            {
                int nextPage = mm.PageIndexes.ElementAt((int)Math.Min(mm.activePage + 1, mm.PageIndexes.Count - 1)).Key;
                mm.activePage = (int)Math.Min(mm.activePage + 1, mm.PageIndexes.Count - 1);
                LoadMenu(_activeMenuIndex);
            }
        }
    }
}
