using Graph.Enum;
using Newtonsoft.Json;

namespace Graph.Nodes
{
    public class User : NodeBase
    {
        public User() { Type = NodeType.User; }

        public User(string fullname)
        {
            FullName = fullname;
            Type = NodeType.User;
        }

        [JsonProperty(PropertyName = "fullname")]
        public string FullName { get; set; }
    }
}
