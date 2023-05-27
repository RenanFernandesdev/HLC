using HLC.Util;
using System;
using ClosedXML.Excel;

namespace HLC.Services.DataModel
{
    internal class WorkbookService : IDisposable
    {
        private XLWorkbook CurrentWorkbook;
        private IXLWorksheet CurrentWorksheet;
        private string WorkbookName = $"HL STATISCS - {DateTime.Now.ToString("ddMMyyyyHHmmss")}.xlsx";
        private string DirectoryOut;

        public string[] ColumnLabels;
        public string TabName;
        public string PathSaved;

        public WorkbookService()
        {
            DirectoryOut = DirectoryUtilities.GetInstance.CreateDirectory();
        }

        public WorkbookService(string pathSaved)
        {
            CurrentWorkbook = new XLWorkbook(pathSaved);
        }

        public string CreateWorkbook(string firstTab)
        {
            CurrentWorkbook = new XLWorkbook();
            CreateWorksheet(firstTab);
            string path = $@"{DirectoryOut}\{WorkbookName}";

            CurrentWorkbook.SaveAs(path);
            return path;
        }

        public void CreateWorksheet(string tabName)
        {
            CurrentWorksheet = CurrentWorkbook.AddWorksheet(tabName);
        }

        public void GetWorksheet(string tabName)
        {
            CurrentWorksheet = CurrentWorkbook.Worksheet(tabName);
        }

        public void HorizontalRecord(int row, int column, string[] columnsContent)
        {
            for (int i = 1; i <= columnsContent.Length; i++) CurrentWorksheet.Cell(1 + row, i + column).Value = columnsContent[i - 1];
            CurrentWorkbook.Save();
        }

        public void VerticalRecord(int row, int column, string[] rowsContent)
        {
            for (int i = 1; i <= rowsContent.Length; i++) CurrentWorksheet.Cell(i + row, 1 + column).Value = rowsContent[i - 1];
            CurrentWorkbook.Save();
        }

        public void Record(int row, int column, string rowsContent)
        {
            CurrentWorksheet.Cell(row,column).Value = rowsContent;
            CurrentWorkbook.Save();
        }

        public int LastRowUsed(int column)
        {
            return CurrentWorksheet.Column(column).LastCellUsed().WorksheetRow().RowNumber();
        }

        public bool IsCellUsed(int row, int column)
        {
            return CurrentWorksheet.Cell(row, column).IsEmpty();
        }

        public void Autofit()
        {
            CurrentWorksheet.Columns().AdjustToContents();
            CurrentWorkbook.Save();
        }

        public void SaveData(string[] fields)
        {
            int rows = CurrentWorksheet.LastRowUsed().RowNumber();
            for (int i = 1; i <= fields.Length; i++ )
            {
                CurrentWorksheet.Cell( rows + 1 , i ).Value = fields[i -1];
            }
            CurrentWorksheet.Columns().AdjustToContents();
            CurrentWorkbook.Save();
        }

        public void Dispose()
        {
            CurrentWorkbook.Dispose();
        }
    }
}