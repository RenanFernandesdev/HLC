using HLC.DriverComponents;
using HLC.DriverComponents.Entities;
using HLC.Services.Impl;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace HLC.Services
{
    internal class HLCService
    {
        private SeleniumBase SeleniumBase = new SeleniumBase();
        private List<ComponentNode> Nodes = new List<ComponentNode>();
        public void Requester()
        {
            AssignNode(new MatchesDailyProcess(SeleniumBase).EstablishRoutine());

            ComponentNode aggregateNode = Nodes.FirstOrDefault();
            string[] urlMatches = (string[]) aggregateNode.Data["MATCH_LIST"];

            for(int i = 0; i < urlMatches.Length; i++) {
                aggregateNode.Data["URL_MATCH"] = urlMatches[i];
                AssignNode(new TeamStatsIntersectProcess(aggregateNode, SeleniumBase).EstablishRoutine());

                ComponentNode lastNode = Nodes.LastOrDefault();

                if (!lastNode.Data.ContainsKey("TEAMS_NOT_DISPLAYED"))
                {
                    aggregateNode = ComponentNode.MergeDataNodes(aggregateNode, lastNode);

                    List<string[]> teamPack = (List<string[]>)aggregateNode.Data["TEAMS_LIST"];
                    string[] teamNames = teamPack.Select(team => team[0]).ToArray();
                    string[] urlTeams = teamPack.Select(url => url[1]).ToArray();

                    List<string[]> hrefsPlayers = (List<string[]>)aggregateNode.Data["PLAYERS_LIST"];

                    for (int j = 0; j < urlTeams.Length; j++)
                    {
                        aggregateNode.Data["URL_TEAM"] = urlTeams[j];
                        aggregateNode.Data["CURRENT_TEAM"] = teamNames[j];

                        AssignNode(new TeamStatsProcess(aggregateNode, SeleniumBase).EstablishRoutine());

                        lastNode = Nodes.LastOrDefault();
                        aggregateNode = ComponentNode.MergeDataNodes(aggregateNode, lastNode);
                        string[] currentHrefPlayer = hrefsPlayers[j];

                        for(int k = 0;  k < currentHrefPlayer.Length; k++) 
                        {
                            aggregateNode.Data["CURRENT_URL_PLAYER"] = currentHrefPlayer[k];
                            AssignNode(new PlayerStatsProcess(aggregateNode, SeleniumBase).EstablishRoutine());
                        }
                        aggregateNode.Data.Remove("TAB_NAME_TEAM_STATS");
                    }
                    aggregateNode.Data.Remove("TEAMS_LIST");
                    aggregateNode.Data.Remove("PLAYERS_LIST");
                    aggregateNode.Data.Remove("CURRENT_URL_PLAYER");
                }
            }
        }

        private void AssignNode(ComponentNode external)
        {
            Nodes.Add(external);
        }
    }
}
