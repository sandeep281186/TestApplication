using DatabaseAccess;
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
        private DataTable selectedUserData;

        public MainForm()
        {
            InitializeComponent();
            _aspectLocs = new Dictionary<string, List<float>>();
            _aspectDims = new Dictionary<string, List<float>>();
            _dbManager = DBAccessManager.GetInstance();          
            AddMenus();
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

            turretUserData = _dbManager.LoadTurretData();
            LoadCombobox();
        }

        private void LoadCombobox()
        {
            comboBoxUserList.DataSource = _dbManager.GetUsers();
            comboBoxUserList.DisplayMember = "USER";
            comboBoxUserList.BindingContext = this.BindingContext;
            comboBoxUserList.SelectedIndex = -1;
            comboBoxUserList.Update();
        }

        private void LoadPageData()
        {

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
                        b.Location = new Point(Convert.ToInt32(Math.Round(this.Width * _aspectLocs[b.Name][0])), Convert.ToInt32(Math.Round(this.Height * _aspectLocs[b.Name][1])));
                    }
                    else
                    {
                        Label l = c as Label;
                        if(l != null)
                        {
                            l.Width = Convert.ToInt32(Math.Round(_aspectDims[l.Name][0] * this.Width));
                            l.Height = Convert.ToInt32(Math.Round(_aspectDims[l.Name][1] * this.Height));
                            l.Location = new Point(Convert.ToInt32(Math.Round(this.Width * _aspectLocs[l.Name][0])), Convert.ToInt32(Math.Round(this.Height * _aspectLocs[l.Name][1])));
                        }
                    }
                }
            }
        }

        private void buttonLoadExcel_Click(object sender, EventArgs e)
        {
            using (FileDialog fd = new OpenFileDialog())
            {
                fd.FileName = "Excel File(*.xls,*.xlsx) | *.xls;*.xlsx";

                if (fd.ShowDialog() == DialogResult.OK)
                {
                    using (ExcelReader reader = new ExcelReader(fd.FileName))
                    {
                        List<TurretData> recs = reader.LoadData();
                        {
                            if (_dbManager.ExecuteSql(recs))
                            {
                                LoadCombobox();
                                MessageBox.Show("Import Success.");
                            }
                            else
                                MessageBox.Show("Import data failed.");
                        }
                    }
                }
            }
        }

        private void buttonUp_Click(object sender, EventArgs e)
        {

        }

        private void buttonDown_Click(object sender, EventArgs e)
        {

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
                }
            }
        }

        private void LoadIndexesForMenus(int index)
        {
            //foreach(MainMenu m in menus)
            {
                Clear();
                MainMenu m = menus[index];
                m.PageIndexes = new Dictionary<int, List<Record>>();

                foreach (int page in m.Pages)
                {
                    m.PageIndexes.Add(page, GetIndexes(selectedUserData, page));

                    if(m.PageIndexes[page].Count > 0)
                    {
                        foreach (Record rec in m.PageIndexes[page])
                        {
                            Button button = this.Controls.Find("buttonIndex" + rec.PageIndex, false)[0] as Button;
                            button.Text = rec.KeyLabel;
                        }
                    }
                }
            }
        }

        private void comboBoxUserList_SelectedIndexChanged(object sender, EventArgs e)
        {
            DataRowView drv = comboBoxUserList.SelectedValue as DataRowView;

            if (drv != null)
            {
                string usr = drv[0].ToString();
                selectedUserData = GetUserData(usr);
                LoadIndexesForMenus(0);
            }              
        }

        private DataTable GetUserData(string usr)
        {
            DataView dv = new DataView(turretUserData);
            dv.RowFilter = string.Format("[{0}] = '{1}'", "USER", usr);
            return dv.ToTable();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            LoadIndexesForMenus(0);
        }

        private void buttonDialtone_Click(object sender, EventArgs e)
        {
            LoadIndexesForMenus(1);
        }

        private void buttonTeamSpeedDials_Click(object sender, EventArgs e)
        {
            LoadIndexesForMenus(2);
        }

        private void buttonPersonalSpeedDials_Click(object sender, EventArgs e)
        {
            LoadIndexesForMenus(3);
        }

        private void buttonPrivateWires_Click(object sender, EventArgs e)
        {
            LoadIndexesForMenus(4);
        }

        private void buttonIntercomSpeedDials_Click(object sender, EventArgs e)
        {
            LoadIndexesForMenus(5);
        }

        private void buttonActivityPage_Click(object sender, EventArgs e)
        {
            LoadIndexesForMenus(6);
        }

        private void buttonCallRegisterShortcut_Click(object sender, EventArgs e)
        {
            LoadIndexesForMenus(7);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(_dbManager != null)
            {
                _dbManager.Dispose();
            }
        }

        private void buttonIndex8_Click(object sender, EventArgs e)
        {

        }

        private void buttonIndex24_Click(object sender, EventArgs e)
        {

        }
    }

    public class MainMenu
    {
        public string Name { get; set; }
        public List<int> Pages { get; set; }
        public Dictionary<int, List<Record>> PageIndexes { get; set; }
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