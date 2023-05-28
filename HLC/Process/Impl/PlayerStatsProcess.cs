using HLC.DriverComponents;
using HLC.DriverComponents.Entities;
using HLC.Util;
using System.Linq;

namespace HLC.Process.Impl
{
    internal class PlayerStatsProcess : HLCProcess
    {
        public override string ProcessName => DefineProcessName(typeof(MatchesDailyProcess));
        private string[] LabelStats;
        private string[] Stats;
        private string PlayerName;
        private bool IsPlayerDisplayed;

        public PlayerStatsProcess(ComponentNode componentNode, SeleniumBase selenium) : base(componentNode, selenium)
        {
            URL = componentNode.Data["CURRENT_URL_PLAYER"].ToString();
            PlayerName = string.Empty;
        }

        public override void ExtractData()
        {
            IsPlayerDisplayed = (bool)Selenium.ExecuteJS("return document.querySelector('.stats-row  :first-child') === null;");
            if (!IsPlayerDisplayed)
            {
                LabelStats = Selenium.SearchByCssSelectorAll(".stats-row  :first-child")
                                     .Select(el => el.Text)
                                     .ToArray();
                Stats = Selenium.SearchByCssSelectorAll(".stats-row :nth-child(2)")
                                .Select(el => el.Text)
                                .ToArray();
                PlayerName = Selenium.GetTextFromDOM(".summaryNickname.text-ellipsis");
            }
            else
            {
                CurrentNode.Data.Add("PLAYER_NOT_DISPLAYED", null);
            }
        }

        public override void SaveData()
        {
            if (!IsPlayerDisplayed)
            {
                string wbPath = (string)LastNode.Data["WORKBOOK_PATH"];
                string tabName = (string)LastNode.Data["TAB_NAME_TEAM_STATS"];

                WorkbookService wb = new WorkbookService(wbPath);
                wb.GetWorksheet(tabName);

                int row = 1;
                int column = 12;

                if (wb.IsCellUsed(row, column))
                {
                    wb.Record(row, column, "PLAYERS");
                    row++;
                    wb.HorizontalRecord(0, column, LabelStats);
                }
                else
                {
                    row = wb.LastRowUsed(column) + 1;
                }

                wb.Record(row, column, PlayerName);
                wb.HorizontalRecord(row - 1, column, Stats);
                wb.Autofit();
                wb.Dispose();
            }
        }

        public override void EndProcess()
        {
            Selenium.TearDown();
        }
    }
}
