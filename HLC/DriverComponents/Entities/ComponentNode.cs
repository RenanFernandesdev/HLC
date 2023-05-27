using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms.Layout;

namespace HLC.DriverComponents.Entities
{
    class ComponentNode
    {
        public string NodeName { get; set; }
        public bool Success { get; set; }
        public Dictionary<string, object> Data { get; set; }

        public ComponentNode() 
        {
            Data = new Dictionary<string, object>();
        }

        public void SetNode(string nodeName, bool success, Dictionary<string, object> data)
        {
            NodeName = nodeName;
            Success = success;
            Data = data;
        }
        public void SetNode(string nodeName, bool success, string kData, object vData)
        {
            NodeName = nodeName;
            Success = success;
            Data.Add(kData, vData);
        }

        public static ComponentNode GetLastNode(List<ComponentNode> nodes)
        {
            return nodes[nodes.Count - 1];
        }

        public static ComponentNode MergeDataNodes(string processNameA, string processNameB, List<ComponentNode> nodes)
        {
            ComponentNode componentNode = new ComponentNode();
            List<ComponentNode> components = nodes.Where(node => node.NodeName == processNameA || node.NodeName == processNameB).ToList();
            bool exists = components.Count > 1;

            if (exists)
            {
                Dictionary<string, object> valuesMerged = components
                                                            .Aggregate(new Dictionary<string, object>(),
                                                                      (result, data) => result.Concat(data.Data)
                                                                                              .ToDictionary(x => x.Key, y => y.Value));
                componentNode.SetNode($"[MERGED] - {processNameA} => {processNameB}", true, valuesMerged);
            }
            return componentNode;
        }

        public static ComponentNode MergeDataNodes(ComponentNode processA, ComponentNode processB)
        {
            ComponentNode componentNode = new ComponentNode();

            Dictionary<string, object> valuesMerged = processA.Data.Concat(processB.Data).ToDictionary(x => x.Key, y => y.Value);
            
            componentNode.SetNode($"[MERGED]", true, valuesMerged);
            
            return componentNode;
        }
    }
}
