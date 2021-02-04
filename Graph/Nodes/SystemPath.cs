using Graph.Enum;
using Graph.Interfaces;
using Newtonsoft.Json;

namespace Graph.Nodes
{
    public class SystemPath : NodeBase, IChoice
    {
        public SystemPath() { Type = NodeType.SystemPath; ChoiceType = ChoiceType.Binary; }
        public SystemPath(string description, ChoiceType choiceType)
        {
            Description = description;
            Type = NodeType.SystemPath;
            ChoiceType = choiceType;
        }

        [JsonProperty(PropertyName = "description")]
        public string Description { get; set; }

        [JsonProperty(PropertyName = "choiceType")]
        public ChoiceType ChoiceType { get; set; }
    }
}
