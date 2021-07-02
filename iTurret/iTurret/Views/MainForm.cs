﻿using DatabaseAccess;
using iTurret.Views;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace iTurret
{
    public partial class MainForm : Form
    {
        private float aspectWidth;
        private float aspectHeight;
        private float aspectX;
        private float aspectY;
        private Dictionary<string, List<float>> _aspectLocs;
        private Dictionary<string, List<float>> _aspectDims;
        private DBAccessManager _dbManager;
        private List<MainMenu> menus;
        private DataTable turretUserData;
        private DataTable turretUserDataUpdates;
        private bool isDirty;
        private DataTable selectedUserData;
        private int activeMenuIndex = 0;

        public MainForm()
        {
            InitializeComponent();
            _aspectLocs = new Dictionary<string, List<float>>();
            _aspectDims = new Dictionary<string, List<float>>();
            _dbManager = DBAccessManager.GetInstance();          
            AddMenus();
            isDirty = false;
        }

        private MainMenu GetMenu(string name, int start, int end)
        {
            MainMenu mm = new MainMenu { Name = name };

            mm.Pages = new List<int>();

            for (int page = start; page <= end; ++page)
                mm.Pages.Add(page);

            return mm;
        }

        private void AddMenus()
        {
            menus = new List<MainMenu>();
            menus.Add( GetMenu(MenuName.Personal, 1, 9) );
            menus.Add(GetMenu(MenuName.Dialtone, 10, 29));
            menus.Add(GetMenu(MenuName.TeamSpeedDials, 30, 39));
            menus.Add(GetMenu(MenuName.PersonalSpeedDials, 40, 49));
            menus.Add(GetMenu(MenuName.PrivateWires, 50, 89));
            menus.Add(GetMenu(MenuName.IntercomSpeedDials, 90, 99));
            menus.Add(GetMenu(MenuName.ActivityPage, 100, 100));
            menus.Add(GetMenu(MenuName.CallRegisterShortcut, 100, 100));
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            foreach (Control c in this.Controls)
            {
                aspectX = c.Location.X / (this.Width * 1.0f);
                aspectY = c.Location.Y / (this.Height * 1.0f);

                this._aspectLocs.Add(c.Name, new List<float>() { aspectX, aspectY });

                aspectWidth = c.Width / (this.Width * 1.0f);
                aspectHeight = c.Height / (this.Height * 1.0f);
                this._aspectDims.Add(c.Name, new List<float>() { aspectWidth, aspectHeight });
            }

            RefreshData();

            //Load all data to local ds
            for (int index = 0; index < menus.Count; ++index)
            {
                LoadIndexesForMenus(index);
            }

            //Load Default Personal Menu
            LoadIndexesForMenus(activeMenuIndex);
        }

        private void LoadCombobox()
        {
            comboBoxUserList.DataSource = _dbManager.GetUsers();
            comboBoxUserList.DisplayMember = "USER";
            comboBoxUserList.BindingContext = this.BindingContext;
            comboBoxUserList.Update();
        }

        private void RefreshData()
        {
            turretUserData = _dbManager.LoadTurretData();
            turretUserData.AcceptChanges();
            turretUserDataUpdates = turretUserData.Copy();
            turretUserDataUpdates.AcceptChanges();
            LoadCombobox();
        }

        private void MainForm_SizeChanged(object sender, EventArgs e)
        {
            if (aspectWidth != 0 && aspectHeight != 0 && aspectX != 0 && aspectY != 0)
            {
                foreach (Control c in this.Controls)
                {
                    Button b = c as Button;

                    if (b != null)
                    {
                        b.Width = Convert.ToInt32(Math.Round(_aspectDims[b.Name][0] * this.Width));
                        b.Height = Convert.ToInt32(Math.Round(_aspectDims[b.Name][1] * this.Height));
                        b.Location = new Point(Convert.ToInt32(Math.Round(this.Width * _aspectLocs[b.Name][0])), 
                                               Convert.ToInt32(Math.Round(this.Height * _aspectLocs[b.Name][1])));
                    }
                    else
                    {
                        Label l = c as Label;
                        if(l != null)
                        {
                            l.Width = Convert.ToInt32(Math.Round(_aspectDims[l.Name][0] * this.Width));
                            l.Height = Convert.ToInt32(Math.Round(_aspectDims[l.Name][1] * this.Height));
                            l.Location = new Point(Convert.ToInt32(Math.Round(this.Width * _aspectLocs[l.Name][0])), 
                                                   Convert.ToInt32(Math.Round(this.Height * _aspectLocs[l.Name][1])));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Import Excel File and input data to Database
        /// If Database is not empty before importing excel, delete all data from Database and continue with import
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonLoadExcel_Click(object sender, EventArgs e)
        {
            if(_dbManager.CheckForRecords())
            {
                if(MessageBox.Show("Database is not empty, this data import will erase all existing data, Do you want to continue?", "iTurret", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk) == DialogResult.No)
                {
                    return;
                }
                else
                {
                    _dbManager.TruncateDB();
                }
            }

            DataImportWizardP1 frmP1 = new DataImportWizardP1();

            if(frmP1.ShowDialog() == DialogResult.OK)
            {
                DataImportWizardP2 frmP2 = new DataImportWizardP2(frmP1.GetSelectedFile());

                if(frmP2.ShowDialog() == DialogResult.OK)
                {
                    RefreshData();
                }
            }
        }

        private List<Record> GetIndexes(DataTable dt, int pageNum)
        {
            List<Record> keyIndexesAndLabels = new List<Record>();
            DataView dView = new DataView(dt);
            dView.RowFilter = string.Format("[{0}] = {1}", "Page Number", pageNum);

            DataTable dtmp = dView.ToTable();

            foreach(DataRow dr in dtmp.Rows)
            {
                keyIndexesAndLabels.Add(new Record { PageIndex = Convert.ToInt32(dr["Key Index"].ToString()), KeyLabel= dr["Key Label"].ToString() } );
            }

            return keyIndexesAndLabels;
        }

        private void Clear()
        {
            foreach (Control c in this.Controls)
            {
                Button button = c as Button;

                if(button != null &&  button.Name.Where(ch=>Char.IsDigit(ch)).ToList().Count > 0 )
                {
                    button.Text = string.Empty;

                    int btnIndex = Int32.Parse(new string(button.Name.Where(ch => Char.IsDigit(ch)).ToArray()));

                    if (btnIndex >= 25 && btnIndex % 2 == 1)
                    {
                        button.Text = "Speaker Channel";
                    }
                }             
            }
        }

        /// <summary>
        /// Populate buttons with page-wise data
        /// </summary>
        /// <param name="index"></param>
        private void LoadIndexesForMenus(int index)
        {
            if(selectedUserData != null)
            {
                Clear();
                MainMenu m = menus[index];
                m.PageIndexes = new Dictionary<int, List<Record>>();
                bool load = true;

                foreach (int page in m.Pages)
                {
                    List<Record> rcds = GetIndexes(selectedUserData, page);

                    if (rcds.Count > 0)
                    {
                        m.PageIndexes.Add(page, rcds);

                        if (load)
                        {                           
                            load = false;
                            labelPSD1.Text = labelPSD2.Text = labelPSD3.Text = m.Name + "-" + page;

                            foreach (Record rec in m.PageIndexes[page])
                            {
                                Button button = this.Controls.Find("buttonIndex" + rec.PageIndex, false)[0] as Button;
                                button.Text = rec.KeyLabel;
                            }
                        }
                    }
                }
            }
        }

        private void comboBoxUserList_SelectedIndexChanged(object sender, EventArgs e)
        {
            DataRowView drv = comboBoxUserList.SelectedItem as DataRowView;

            if (drv != null)
            {
                string usr = drv[0].ToString();
                selectedUserData = GetUserData(usr);
                selectedUserData.AcceptChanges();
                LoadIndexesForMenus(activeMenuIndex);
            }              
        }

        /// <summary>
        /// User-wise filtering of data
        /// </summary>
        /// <param name="usr"></param>
        /// <returns></returns>
        private DataTable GetUserData(string usr)
        {
            DataView dv = new DataView(turretUserDataUpdates);
            dv.RowFilter = string.Format("[{0}] = '{1}'", "USER", usr);
            return dv.ToTable();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            LoadIndexesForMenus(activeMenuIndex=0);
        }

        private void buttonDialtone_Click(object sender, EventArgs e)
        {
            LoadIndexesForMenus(activeMenuIndex = 1);
        }

        private void buttonTeamSpeedDials_Click(object sender, EventArgs e)
        {
            LoadIndexesForMenus(activeMenuIndex = 2);
        }

        private void buttonPersonalSpeedDials_Click(object sender, EventArgs e)
        {
            LoadIndexesForMenus(activeMenuIndex = 3);
        }

        private void buttonPrivateWires_Click(object sender, EventArgs e)
        {
            LoadIndexesForMenus(activeMenuIndex = 4);
        }

        private void buttonIntercomSpeedDials_Click(object sender, EventArgs e)
        {
            LoadIndexesForMenus(activeMenuIndex = 5);
        }

        private void buttonActivityPage_Click(object sender, EventArgs e)
        {
            LoadIndexesForMenus(activeMenuIndex = 6);
        }

        private void buttonCallRegisterShortcut_Click(object sender, EventArgs e)
        {
            LoadIndexesForMenus(activeMenuIndex = 7);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(_dbManager != null)
            {
                if(this.isDirty)
                {
                    _dbManager.UpdateDB(this.turretUserData, this.turretUserDataUpdates);
                }

                _dbManager.Dispose();
            }
        }

        /// <summary>
        /// Goto previous page within the menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonPREV_Click(object sender, EventArgs e)
        {
            MainMenu mm = menus[activeMenuIndex];

            if (mm.PageIndexes != null && mm.PageIndexes.Count > 0)
            {
                Clear();
                int prevPage = mm.PageIndexes.ElementAt((int)Math.Max(mm.activePage - 1, 0)).Key;
                mm.activePage = (int)Math.Max(mm.activePage - 1, 0);
                labelPSD1.Text = labelPSD2.Text = labelPSD3.Text = mm.Name + "-" + prevPage;

                foreach (Record rec in mm.PageIndexes[prevPage])
                {
                    Button button = this.Controls.Find("buttonIndex" + rec.PageIndex, false)[0] as Button;
                    button.Text = rec.KeyLabel;
                }
            }
        }

        /// <summary>
        /// Goto next page within the menu
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonNext_Click(object sender, EventArgs e)
        {
            MainMenu mm = menus[activeMenuIndex];

            if (mm.PageIndexes != null && mm.PageIndexes.Count > 0)
            {
                Clear();
                int nextPage = mm.PageIndexes.ElementAt((int)Math.Min(mm.activePage + 1, mm.PageIndexes.Count - 1)).Key;
                mm.activePage = (int)Math.Min(mm.activePage + 1, mm.PageIndexes.Count - 1);
                labelPSD1.Text = labelPSD2.Text = labelPSD3.Text = mm.Name + "-" + nextPage;

                foreach (Record rec in mm.PageIndexes[nextPage])
                {
                    Button button = this.Controls.Find("buttonIndex" + rec.PageIndex, false)[0] as Button;
                    button.Text = rec.KeyLabel;
                }
            }
        }

        /// <summary>
        /// Update/Delete Key Label
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void buttonUpdate_Click(object sender, EventArgs e)
        {
            MainMenu mm = menus[activeMenuIndex];

            if (mm.PageIndexes != null && mm.PageIndexes.Count > 0)
            {
                KeyValuePair<int, List<Record>> pages = mm.PageIndexes.ElementAt(mm.activePage);
                DataTable dt = new DataTable();
                dt.Columns.Add("Module 1");
                dt.Columns.Add("Module 2");
                dt.Columns.Add("Module 3");

                List<Record> rc1 = pages.Value.Where(r => r.PageIndex >= 0 && r.PageIndex <= 7).ToList();
                List<Record> rc2 = pages.Value.Where(r => r.PageIndex >= 8 && r.PageIndex <= 15).ToList();
                List<Record> rc3 = pages.Value.Where(r => r.PageIndex >= 16 && r.PageIndex <= 23).ToList();

                for(int row=0; row < 8; ++row)
                {
                    DataRow dr = dt.NewRow();
                    dt.Rows.Add(dr);
                }

                foreach (Record rec in rc1)
                {
                    dt.Rows[rec.PageIndex]["Module 1"] = rec.KeyLabel;
                }

                foreach (Record rec in rc2)
                {
                    dt.Rows[rec.PageIndex%8]["Module 2"] = rec.KeyLabel;
                }

                foreach (Record rec in rc3)
                {
                    dt.Rows[rec.PageIndex % 8]["Module 3"] = rec.KeyLabel;
                }

                UpdateForm from = new UpdateForm(dt);

                if(from.ShowDialog()== DialogResult.OK)
                {
                    //DB update
                    DataTable dtUpdated = from.GetUpdates();

                    foreach(DataRow dr in dtUpdated.Rows)
                    {
                        List<DataRow> rowToUpdate = turretUserDataUpdates.AsEnumerable().Where(x => x.Field<string>("USER").Equals(selectedUserData.Rows[0]["USER"].ToString()) && 
                                                    x.Field<int>("Page Number")==pages.Key && x.Field<int>("Key Index")==dr.Table.Rows.IndexOf(dr)).ToList();
                        
                        if (rowToUpdate != null && rowToUpdate.Count > 0)
                        {
                            rowToUpdate[0]["Key Label"] = dr["Module 1"];
                        }

                        rowToUpdate = turretUserDataUpdates.AsEnumerable().Where(x => x.Field<string>("USER").Equals(selectedUserData.Rows[0]["USER"].ToString()) && 
                                      x.Field<int>("Page Number") == pages.Key && x.Field<int>("Key Index") == dr.Table.Rows.IndexOf(dr) + 8).ToList();

                        if (rowToUpdate != null && rowToUpdate.Count > 0)
                        {
                            rowToUpdate[0]["Key Label"] = dr["Module 2"];
                        }

                        rowToUpdate = turretUserDataUpdates.AsEnumerable().Where(x => x.Field<string>("USER").Equals(selectedUserData.Rows[0]["USER"].ToString()) && 
                                      x.Field<int>("Page Number") == pages.Key && x.Field<int>("Key Index") == dr.Table.Rows.IndexOf(dr) + 16).ToList();

                        if (rowToUpdate != null && rowToUpdate.Count > 0)
                        {
                            rowToUpdate[0]["Key Label"] = dr["Module 3"];
                        }
                    }

 
                    DataRowView drv = comboBoxUserList.SelectedItem as DataRowView;
                    
                    if (drv != null)
                    {
                        string usr = drv[0].ToString();
                        selectedUserData = GetUserData(usr);
                        selectedUserData.AcceptChanges();
                        LoadIndexesForMenus(activeMenuIndex);
                        labelPSD1.Text = labelPSD2.Text = labelPSD3.Text = mm.Name + "-" + pages.Key;
                        mm = menus[activeMenuIndex];
                        Clear();

                        foreach (Record rec in mm.PageIndexes[pages.Key])
                        {
                            Button button = this.Controls.Find("buttonIndex" + rec.PageIndex, false)[0] as Button;
                            button.Text = rec.KeyLabel;
                        }
                        isDirty = true;
                    }
                }
            }
        }

        private void buttonExit_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void buttonViewOriginal_Click(object sender, EventArgs e)
        {
            using (ViewOriginalRecords from = new ViewOriginalRecords(menus, selectedUserData, activeMenuIndex))
            {
                from.ShowDialog();
            }
        }
    }

    public class MainMenu
    {
        public string Name { get; set; }
        public List<int> Pages { get; set; }
        public Dictionary<int, List<Record>> PageIndexes { get; set; }
        public int activePage { get; set; }

        public MainMenu()
        {
            activePage = 0;
        }
    }

    public class Record
    {
        public int PageIndex {get;set;}
        public string KeyLabel { get; set; }
    }

    internal class MenuName
    {
        public const string Personal = "Personal";
        public const string Dialtone = "Dialtone";
        public const string TeamSpeedDials = "Team Speed Dials";
        public const string PersonalSpeedDials = "Personal Speed Dials";
        public const string PrivateWires = "Private Wires";
        public const string IntercomSpeedDials = "Intercom Speed Dials";
        public const string ActivityPage = "Activity Page";
        public const string CallRegisterShortcut = "Call Register Shortcut";
    }
}