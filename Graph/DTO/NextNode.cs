using Graph.Enum;
using Graph.Interfaces;
using Graph.Nodes;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Graph.DTO
{
    public class NextNode
    {
        public NextNode(JObject node, NodeType type, IEnumerable<int> acceptableAnswers)
        {
            SetNode(node, type);

            if (acceptableAnswers != null)
            {
                AcceptableAnswers = acceptableAnswers;
            }
        }

        public INode Node { get; private set; }
        public IEnumerable<int> AcceptableAnswers { get; set; } = new List<int>();

        /// <summary>
        /// Sets Node property to concrete type to correct name is used for writes/reads
        /// </summary>
        /// <param name="node"></param>
        /// <param name="type"></param>
        private void SetNode(JObject node, NodeType type)
        {
            switch (type)
            {
                case NodeType.Journey:
                    Node = node.ToObject<Journey>();
                    break;
                case NodeType.UserPath:
                    Node = node.ToObject<UserPath>();
                    break;
                case NodeType.SystemPath:
                    Node = node.ToObject<SystemPath>();
                    break;
                case NodeType.SystemEvent:
                    Node = node.ToObject<SystemEvent>();
                    break;
                default:
                    break;
            }
        }
    }
}
