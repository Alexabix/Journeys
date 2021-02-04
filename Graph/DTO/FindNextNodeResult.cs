using Graph.Interfaces;
using System.Collections.Generic;

namespace Graph.DTO
{
    public class FindNextNodeResult
    {
        public FindNextNodeResult(INode currentNode)
        {
            CurrentNode = currentNode;
        }

        public INode CurrentNode { get; set; }
        public List<NextNode> NextNodes { get; set; } = new List<NextNode>();
    }
}
