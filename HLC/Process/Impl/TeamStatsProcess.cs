using HLC.DriverComponents;
using HLC.DriverComponents.Entities;
using HLC.Util;
using System.Collections.Generic;
using System.Linq;

namespace HLC.Process.Impl
{
    internal class TeamStatsProcess : HLCProcess
    {
        public override string ProcessName => DefineProcessName(typeof(MatchesDailyProcess));
        private List<string[]> Data;
        private List<string[]> DataGrid;
        private Dictionary<string, string[]> GridMapStat;
        

        public TeamStatsProcess(ComponentNode componentNode, SeleniumBase selenium) : base(componentNode, selenium)
        {
            URL = componentNode.Data["URL_TEAM"].ToString();
            Data = new List<string[]>();
            DataGrid = new List<string[]>();
            GridMapStat = new Dictionary<string, string[]>();
        }

        public override void ExtractData()
        {
            bool statsDisplayed = (bool) Selenium.ExecuteJS("return document.querySelector('.two-grid .col .map-pool') == null");

            if (!statsDisplayed)
            {
                string[] mapNames = Selenium.SearchByCssSelectorAll(".two-grid .col .map-pool").Select(el => el.Text).ToArray();
                string[] labelStat = Selenium.SearchByCssSelectorAll(".stats-row :first-child", Selenium.SearchByCssSelector(".two-grid .col .stats-rows.standard-box"))
                                                                                                        .Select(el => el.Text)
                                                                                                        .ToArray();
                DataGrid = Selenium.SearchByCssSelectorAll(".two-grid .col .stats-rows.standard-box")
                                                    .Select(el => Selenium.SearchByCssSelectorAll(".stats-row :nth-child(2)", el)
                                                    .Select(elm => FormatData.FormatTd(elm.Text)).ToArray()).ToList();
                Data.Add(mapNames);
                Data.Add(labelStat);
                GetMapsStats();
            }
            else
            {
                CurrentNode.Data.Add("TEAM_STATS_NOT_DISPLAYED", null);
            }
        }

        private void GetMapsStats()
        {
            List<string[]> urlMaps = Selenium.SearchByCssSelectorAll(".stats-top-menu-item.stats-top-menu-item-link",
                                     Selenium.SearchByCssSelectorAll(".tabs.standard-box")[1]).Select(el => {
                                         string[] mapBundle = { el.Text, el.GetAttribute("href")};
                                         return mapBundle;
                                     }).ToList();
            string[] label = null;

            foreach (string[] mapBundle in urlMaps) {
                Selenium.Restart();
                Selenium.TryNavigate(mapBundle[1]);
                bool captcha = (bool)Selenium.ExecuteJS("return document.querySelector('#challenge-running') === null;");
                if (!captcha)
                {
                    Selenium.AddCookieCloudflare();
                }
                Selenium.WaitFullLoad();

                if (label == null)
                {
                    label = Selenium.SearchByCssSelectorAll(".stats-rows.standard-box .stats-row :first-child")
                                      .Select(el => el.Text)
                                      .ToArray();
                }

                string[] fields = Selenium.SearchByCssSelectorAll(".stats-rows.standard-box .stats-row :nth-child(2)")
                                          .Select(el => FormatData.FormatTd(el.Text)).ToArray();

                GridMapStat[mapBundle[0]] = fields;
            }
            GridMapStat.Add("LABEL_MAP_STAT", label);
        }

        public override void SaveData()
        {
            string wbPath = (string) LastNode.Data["WORKBOOK_PATH"];

            if (!CurrentNode.Data.ContainsKey("TEAM_STATS_NOT_DISPLAYED"))
            {
                string tabName = $"TEAM - {(string)LastNode.Data["CURRENT_TEAM"]}";
                WorkbookService wb = new WorkbookService(wbPath);
                wb.CreateWorksheet(tabName);

                string[] labelColumns = Data[0];
                string[] labelRows = Data[1].Concat(GridMapStat["LABEL_MAP_STAT"]).ToArray();

                wb.Record(1, 1, "STATS");
                wb.HorizontalRecord(0, 1, labelColumns);
                wb.VerticalRecord(1, 0, labelRows);

                int column = 1;
                DataGrid.ToList().ForEach(tableStat =>
                {
                    wb.VerticalRecord(1, column, tableStat);
                    column++;
                });

                int row = Data[1].Length + 1;
                column = 1;

                labelColumns.ToList().ForEach(tableStat =>
                {
                    wb.VerticalRecord(row, column, GridMapStat[tableStat]);
                    column++;
                });
                wb.Autofit();
                CurrentNode.SetNode(ProcessName, true, "TAB_NAME_TEAM_STATS", tabName);
                wb.Dispose();
            }
        }

        public override void EndProcess()
        {
            Selenium.TearDown();
        }
    }
}
