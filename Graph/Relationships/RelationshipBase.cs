using Graph.Interfaces;
using Neo4jClient;

namespace Graph.Relationships
{
    public abstract class RelationshipBase : IRelationship
    {
        [Neo4jIgnore]
        public string Name { get; set; }
    }
}
