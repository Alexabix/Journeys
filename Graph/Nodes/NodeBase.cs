using Graph.Enum;
using Graph.Interfaces;
using Newtonsoft.Json;
using System;

namespace Graph.Nodes
{
    public abstract class NodeBase : INode
    {
        public NodeBase()
        {
            Id = Guid.NewGuid();
        }

        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("type")]
        public NodeType Type { get; set; }
    }
}
