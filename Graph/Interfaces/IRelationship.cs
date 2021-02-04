using Neo4jClient;

namespace Graph.Interfaces
{
    public interface IRelationship
    {
        [Neo4jIgnore]
        public string Name { get; set; }
    }
}
