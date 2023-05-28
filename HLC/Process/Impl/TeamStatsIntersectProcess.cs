using HLC.DriverComponents.Entities;
using System.Collections.Generic;
using System.Linq;
using System;
using HLC.DriverComponents;

namespace HLC.Process.Impl
{
    internal class TeamStatsIntersectProcess : HLCProcess
    {
        public override string ProcessName => DefineProcessName(typeof(MatchesDailyProcess));
        private readonly Dictionary<string, object> Stats;

        public TeamStatsIntersectProcess(ComponentNode componentNode, SeleniumBase selenium) : base(componentNode, selenium)
        {
            URL = componentNode.Data["URL_MATCH"].ToString();
            Stats = new Dictionary<string, object>();
        }

        public override void ExtractData()
        {
            try
            {
                bool teamsDisplayed = (bool) Selenium.ExecuteJS("return document.querySelector('.map-stats-infobox .team') === null;") ||
                                      (bool) Selenium.ExecuteJS("return document.querySelector('.lineups-compare-container') === null;");

                if (!teamsDisplayed)
                {
                    List<string[]> teams = Selenium.SearchByCssSelectorAll(".map-stats-infobox .team")
                                                    .Select(team => {
                                                        string[] elemets = { team.Text, Selenium.SearchByCssSelector("a", team).GetAttribute("href") };
                                                        return elemets;
                                                    }).ToList();

                    List<string[]> hrefsPlayers = DeserializeLocally();

                    Stats.Add("TEAMS_LIST", teams);
                    Stats.Add("PLAYERS_LIST", hrefsPlayers);
                }
                else
                {
                    Stats.Add("TEAMS_NOT_DISPLAYED", null);
                }
            }
            catch(Exception ex)
            {
                throw new ApplicationException($"Data extraction error: {ex.Message} => {ex.TargetSite.Name} IN  {ProcessName}");
            }
        }

        public override void SaveData()
        {
            
        }

        public override void EndProcess()
        {
            Selenium.TearDown();
            CurrentNode.SetNode(ProcessName, true, Stats);
        }

        private List<string[]> DeserializeLocally()
        {
            string command = @"return Object.entries(
                                      JSON.parse(document.querySelector('.lineups-compare-container').getAttribute('data-compare-links'))).map((items, index) => { 
                                      var item = Object.values(items[1]);  return item[index] })";            
            object resultJs = Selenium.ExecuteJS(command);
            if( resultJs == null)
            {
                return new List<string[]> { new string[] { "LIST_PLAYER_UNDEFINED" }};
            }

            try
            {
                string threeMonthAgo = DateTime.Now.AddMonths(-3).ToString("yyyy-MM-dd");
                string today = DateTime.Now.ToString("yyyy-MM-dd");

                IEnumerable<object> aur = (IEnumerable<object>)resultJs;
                List<string[]> resultsX = aur.Select(obj =>
                {
                    string[] splitted = obj.ToString().Substring(1).Split('/');
                    string[] allow = { 
                        $"https://www.hltv.org/{splitted[0]}/{splitted[1]}/{splitted[splitted.Length - 4]}/{splitted[splitted.Length - 3]}?startDate={threeMonthAgo}&endDate={today}",
                        $"https://www.hltv.org/{splitted[0]}/{splitted[1]}/{splitted[splitted.Length - 2]}/{splitted[splitted.Length - 1]}?startDate={threeMonthAgo}&endDate={today}"
                    };
                    return allow;
                }).ToList();

                string[] team1 = resultsX.Select(aux => aux[0]).ToArray();
                string[] team2 = resultsX.Select(aux => aux[1]).ToArray();

                resultsX.Clear();
                resultsX.Add(team1);
                resultsX.Add(team2);

                return resultsX;
            }
            catch (Exception ex)
            {
                return new List<string[]> { new string[] { "LIST_PLAYER_UNDEFINED" } };
            }
        }
    }
}
