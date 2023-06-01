using HLC.DriverComponents.Entities;
using HLC.DriverComponents;
using System.Collections.Generic;
using System.Linq;
using HLC.Process.Impl;
using HLC.Util;

namespace HLC.Services.Impl
{
    internal class MatchStandaloneService : HLCService
    {
        private SeleniumBase SeleniumBase = new SeleniumBase();
        private List<ComponentNode> Nodes = new List<ComponentNode>();

        private string UrlMatch = string.Empty;

        public MatchStandaloneService(string urlMatch)
        {
            UrlMatch = urlMatch;
        }

        public void Requester()
        {
            ComponentNode aggregateNode = new ComponentNode();
            aggregateNode.Data["URL_MATCH"] = UrlMatch;

            AssignNode(new TeamStatsIntersectProcess(aggregateNode, SeleniumBase).EstablishRoutine());
            ComponentNode lastNode = Nodes.LastOrDefault();

            if (!lastNode.Data.ContainsKey("TEAMS_NOT_DISPLAYED"))
            {
                aggregateNode = ComponentNode.MergeDataNodes(aggregateNode, lastNode);

                List<string[]> teamPack = (List<string[]>)aggregateNode.Data["TEAMS_LIST"];
                string[] teamNames = teamPack.Select(team => team[0]).ToArray();
                string[] urlTeams = teamPack.Select(url => url[1]).ToArray();

                string workbookPath = GetWorkbookPath($"{teamNames[0]} vs {teamNames[1]}");
                aggregateNode.Data.Add("WORKBOOK_PATH", workbookPath);

                List<string[]> hrefsPlayers = (List<string[]>)aggregateNode.Data["PLAYERS_LIST"];

                for (int j = 0; j < urlTeams.Length; j++)
                {
                    aggregateNode.Data["URL_TEAM"] = urlTeams[j];
                    aggregateNode.Data["CURRENT_TEAM"] = teamNames[j];

                    AssignNode(new TeamStatsProcess(aggregateNode, SeleniumBase).EstablishRoutine());

                    lastNode = Nodes.LastOrDefault();
                    if (!lastNode.Data.ContainsKey("TEAM_STATS_NOT_DISPLAYED")) {
                        aggregateNode = ComponentNode.MergeDataNodes(aggregateNode, lastNode);
                        string[] currentHrefPlayer = hrefsPlayers[j];

                        for (int k = 0; k < currentHrefPlayer.Length; k++)
                        {
                            aggregateNode.Data["CURRENT_URL_PLAYER"] = currentHrefPlayer[k];
                            AssignNode(new PlayerStatsProcess(aggregateNode, SeleniumBase).EstablishRoutine());
                        }
                        aggregateNode.Data.Remove("TAB_NAME_TEAM_STATS");
                    }
                }
                aggregateNode.Data.Remove("TEAMS_LIST");
                aggregateNode.Data.Remove("PLAYERS_LIST");
                aggregateNode.Data.Remove("CURRENT_URL_PLAYER");
            }
        }

        private void AssignNode(ComponentNode external)
        {
            Nodes.Add(external);
        }

        private string GetWorkbookPath(string workbookName)
        {
            WorkbookService wb = new WorkbookService();
            wb.SetWorkbookName(workbookName);
            return wb.CreateWorkbook("STANDALONE_MATCH");
        }
    }
}
