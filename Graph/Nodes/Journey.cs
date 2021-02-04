using Graph.Enum;
using Newtonsoft.Json;

namespace Graph.Nodes
{
    public class Journey : NodeBase
    {
        public Journey() { Type = NodeType.Journey; }

        public Journey(string description)
        {
            Description = description;
            Type = NodeType.Journey;
        }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }
    }
}
