using Graph.Enum;
using Newtonsoft.Json;
using System;

namespace Graph.Interfaces
{
    public interface INode
    {
        [JsonProperty("id")]
        public Guid Id { get; }

        [JsonProperty("type")]
        public NodeType Type { get; set; }
    }
}
