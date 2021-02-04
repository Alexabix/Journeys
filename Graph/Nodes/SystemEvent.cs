using Graph.Enum;
using Newtonsoft.Json;

namespace Graph.Nodes
{
    public class SystemEvent : NodeBase
    {
        public SystemEvent() { Type = NodeType.SystemEvent; }
        public SystemEvent(string description)
        {
            Description = description;
            Type = NodeType.SystemEvent;
        }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }
    }
}
