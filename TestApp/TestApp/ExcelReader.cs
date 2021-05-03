using DatabaseAccess;
using Microsoft.Office.Interop.Excel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Application = Microsoft.Office.Interop.Excel.Application;

namespace TestApp
{
    class ExcelReader : IDisposable
    {
        private string _excelFile;
        private Application _excelApp;
        private Workbook _excelWorkbook;
        private List<string> _sheets;
         
        public ExcelReader(string excelFilePath)
        {
            _excelFile = excelFilePath;
        }

        public void Dispose()
        {
            Close();
        }

        private bool Read()
        {
            try
            {
                _excelApp = new Application();
                _excelWorkbook = _excelApp.Workbooks.Open(_excelFile);
                _sheets = new List<string>();

                foreach (Worksheet sheet in _excelWorkbook.Sheets)
                {
                    _sheets.Add(sheet.Name);
                }
            }
            catch (Exception ex)
            { 
                string error = ex.Message;
            }
            return _excelWorkbook != null;
        }

        private void Close()
        {
            if (_excelWorkbook != null)
            {
                this._excelWorkbook.Close(false, _excelFile, null);
                Marshal.FinalReleaseComObject(_excelWorkbook);
                this._excelWorkbook = null;
            }

            if (_excelApp != null)
            {
                this._excelApp.Quit();
                Marshal.FinalReleaseComObject(_excelApp);
                this._excelApp = null;
            }
        }

        public List<TurretData> LoadData()
        {
            Worksheet worksheet = null;
            List<TurretData> records = new List<TurretData>();

            try
            {
                if (Read())
                {
                    worksheet = _excelWorkbook.Worksheets["iTurretData"] as Worksheet;

                    Range range = worksheet.UsedRange;
                    int rowCount = range.Rows.Count;

                    for (int id = 2; id <= rowCount; ++id)
                    {
                        TurretData tData = new TurretData();

                        Range r = range.Rows[id];
                        Array arr = (Array)r.Cells.Value;

                        for (int i = 1; i <= range.Columns.Count; i++)
                        {
                            string cellValue = arr.GetValue(1,i) == null? " " : arr.GetValue(1,i).ToString();
                            tData.AddItem(i, cellValue);
                        }

                        records.Add(tData);
                    }

                    //MessageBox.Show("File di configurazione caricato", "Configurazione caricata");
                }
                else
                {
                    MessageBox.Show("File read error !", "Error");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                if (worksheet != null)
                {
                    Marshal.FinalReleaseComObject(worksheet);
                    worksheet = null;
                }
            }

            return records;
        }
    }
}