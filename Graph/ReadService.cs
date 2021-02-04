using Graph.DTO;
using Graph.Enum;
using Graph.Interfaces;
using Graph.Nodes;
using Graph.Relationships;
using Neo4jClient;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace Graph
{
    public class ReadService
    {
        private readonly IGraphClient graphClient;

        public ReadService()//IGraphClient client)
        {
            var username = "neo4j";
            var password = "GxUG-LGN+!dr3KT4";

            // Singleton in production
            graphClient = new GraphClient(new Uri("http://localhost:7474/"), username, password);
            graphClient.ConnectAsync().Wait();
        }

        public async Task<UserPath> GetFirstNode(bool isManager)
        {
            const string managerStart = "IS_MANAGER_START";
            const string employeeStart = "IS_EMPLOYEE_START";

            var relationship = isManager ? managerStart : employeeStart;

            // MATCH (journey:Journey)-[:IS_MANAGER_START]-(userPath:UserPath)
            // RETURN journey

            IEnumerable<UserPath> results = await graphClient.Cypher
                .Match($"(journey:Journey)-[:{relationship}]-(userPath:UserPath)")
                // var names here must match aliases in string above
                .Return((userPath)
                    => userPath.As<UserPath>())
                .ResultsAsync;

            var current = results.First();

            return current;
        }

        public async Task<FindNextNodeResult> GetNextNodeResultAsync(INode node)
        {
            if (node is UserPath userPath)
            {
                return await GetNextNodeAsync(userPath);
            }
            else if (node is SystemEvent systemEvent)
            {
                return await GetNextNodeAsync(systemEvent);
            }
            else if (node is SystemPath systemPath)
            {
                return await GetNextNodeAsync(systemPath);
            }
            else if (node is Journey journey)
            {
                return await GetNextNodeAsync(journey);
            }

            throw new IndexOutOfRangeException();
        }

        private async Task<FindNextNodeResult> GetNextNodeAsync<CurrentT>(CurrentT node) where CurrentT : INode
        {
            var nodeId = node.Id;

            var results = await graphClient.Cypher
                        .Match($"(n:{typeof(CurrentT).Name})-[r]->(b)")
                        .Where((CurrentT n) => n.Id == nodeId)
                        .Return((r, b)
                        => new
                        {
                                // Using this because all the other types are just labels
                                Relates = r.As<AnswerIs>(),
                            Next = b.As<string>()
                        })
                        .ResultsAsync;

            var findNextResults = new FindNextNodeResult(node);

            foreach (var item in results)
            {
                dynamic answerNode = JObject.Parse(item.Next);
                NodeType nextNodeType = answerNode.type.ToObject<NodeType>();

                var nextNode = new NextNode(answerNode, nextNodeType, item.Relates.AcceptableAnswers);
                findNextResults.NextNodes.Add(nextNode);
            }

            return findNextResults;
        }
    }
}
