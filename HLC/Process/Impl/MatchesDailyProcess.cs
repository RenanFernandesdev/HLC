using HLC.DriverComponents.Entities;
using System.Collections.Generic;
using OpenQA.Selenium;
using System.Linq;
using System;
using HLC.DriverComponents;
using HLC.Util;

namespace HLC.Process.Impl
{
    internal class MatchesDailyProcess : HLCProcess
    {
        public override string ProcessName => DefineProcessName(typeof(MatchesDailyProcess));

        private List<string[]> Data = new List<string[]>();
        private readonly string[] ColumnLabel = new string[] { "DATE", "HOUR", "TEAM_A", "TEAM_B", "HREF" };

        public MatchesDailyProcess(SeleniumBase selenium) : base(selenium)
        {
            URL = "https://www.hltv.org/matches";
        }

        public MatchesDailyProcess(ComponentNode componentNode, SeleniumBase selenium) : base(componentNode, selenium)
        {
        }

        public override void ExtractData()
        {
            try
            {
                string matchDailyHeadline = Selenium.GetTextFromDOM(".upcomingMatchesSection:first-child > .matchDayHeadline");
                List<IWebElement> matchList = Selenium.SearchByCssSelectorAll(".upcomingMatchesSection:first-child > .upcomingMatch");
                int count = 0;
                matchList.ForEach(matchItem =>
                {
                    string command = $"return document.querySelectorAll('.upcomingMatchesSection:first-child > .upcomingMatch')[{count}].querySelector('.matchInfoEmpty') === null;";
                    bool containerNotEmpty = (bool) Selenium.ExecuteJS(command);
                    if (containerNotEmpty)
                    {
                        string time = Selenium.SearchByCssSelector(".matchInfo > .matchTime", matchItem).Text;
                        string teamA = Selenium.SearchByCssSelector(".matchTeams > .team1", matchItem).Text;
                        string teamB = Selenium.SearchByCssSelector(".matchTeams > .team2", matchItem).Text;
                        string href = Selenium.SearchByCssSelector("a", matchItem).GetAttribute("href");

                        Data.Add(new string[] { matchDailyHeadline, time, teamA, teamB, href });
                    }
                    count++;
                });
            }catch(Exception ex)
            {
                throw new ApplicationException($"Data extraction error: {ex.Message} => {ex.TargetSite.Name} IN {ProcessName}");
            }
        }

        public override void SaveData()
        {
            WorkbookService wb = new WorkbookService();
            string wbPath = wb.CreateWorkbook(ProcessName);
            wb.HorizontalRecord(0, 0, ColumnLabel);
            Data.ForEach(item =>  wb.SaveData(item));
            wb.Autofit();
            wb.Dispose();
            CurrentNode.Data.Add("WORKBOOK_PATH", wbPath);
        }

        public override void EndProcess()
        {
            Selenium.TearDown();
            string[] hrefMatches = Data.Select(fields => fields[fields.Length -1]).ToArray();
            CurrentNode.SetNode(ProcessName, true, "MATCH_LIST", hrefMatches);
        }
    }
}
