using Graph.Enum;
using Graph.Interfaces;
using Newtonsoft.Json;

namespace Graph.Nodes
{
    public class UserPath : NodeBase, IChoice
    {
        public UserPath() { Type = NodeType.UserPath; }

        public UserPath(string text, ChoiceType choiceType)
        {
            Text = text;
            ChoiceType = choiceType;
            Type = NodeType.UserPath;
        }

        [JsonProperty(PropertyName = "text")]
        public string Text { get; set; }

        [JsonProperty(PropertyName = "choiceType")]
        public ChoiceType ChoiceType { get; set; }
    }
}
