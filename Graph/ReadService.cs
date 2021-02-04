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
            // The reason I am casting here is so when GetNextNodeAsync<CurrentT> uses `typeof(CurrentT).Name`
            // it uses the concrete types name instead of "INode"
            // This could potentially be written better.
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
            // Temporary variable is a work around for the Cypher function not doing deep traversal when parsing expression queries
            // I've reported it to the developers to be fixed
            var nodeId = node.Id;

            var results = await graphClient.Cypher
                        .Match($"(n:{typeof(CurrentT).Name})-[r]->(b)")
                        // Note that "n" alias etc defined in Match query must match your variables below
                        .Where((CurrentT n) => n.Id == nodeId)
                        .Return((r, b)
                        => new
                        {
                            // Using "AnswerIs" here regardless of what the proper concrete type is because all the other relationship types currently don't have properties I care about for this simple app
                            Relates = r.As<AnswerIs>(),
                            // Casting this as a string isn't ideal but I can't find a different way to return an unknown type (as this could be UserPath or SystemPath etc.) while keeping it's properties
                            Next = b.As<string>()
                        })
                        .ResultsAsync;

            var findNextResults = new FindNextNodeResult(node);

            foreach (var item in results)
            {
                // Parse the JSON string to a JObject
                // Worth noting dynamic keyword can give you a runtime type error, so typically should be always avoided.
                // Shouldn't ever with this usage though and this is an Interop function, so falls into acceptable use imo.
                dynamic answerNode = JObject.Parse(item.Next);
                NodeType nextNodeType = answerNode.type.ToObject<NodeType>();

                var nextNode = new NextNode(answerNode, nextNodeType, item.Relates.AcceptableAnswers);
                findNextResults.NextNodes.Add(nextNode);
            }

            return findNextResults;
        }
    }
}
