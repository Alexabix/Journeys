"# Journeys" 

Utilises https://neo4j.com/ you will need to download Neo4J Desktop to create a local database server and use the local DB browser tool for queries/visualations

- https://towardsdatascience.com/getting-started-with-neo4j-in-10-minutes-94788d99cc2b
- Cypher Cheatsheet for Neo4j for https://neo4j.com/docs/cypher-refcard/current/

Couple useful Chypers

    //Delete all
    MATCH (n)
    DETACH DELETE n
    
    //Find all
    Match (n)-[r]-(b)
    Return n, r, b
    
Notice the Match command is directional. so ()-[]-() is all directions ()-[]->() is towards target etc.

https://github.com/DotNet4Neo4j/Neo4jClient is used as C# wrapper for Neo4j. The wiki is slightly out of date. I have found that when writing a Cypher through the wrapper you can do `.Query.DebugQueryText` and it will output a string of exactly what the Cypher would look like (including resolved parameters) so you can check that against the official neo4j cypher documenation to see where you went wrong.
- https://github.com/DotNet4Neo4j/Neo4jClient/wiki
- 

*Considerations for designing the data model*
Designing a good graph datamodel requires thought on exactly what the useful nodes and relations should be, it's not going to be as simple as how you might write a flow chart to explain something to a person.

- https://neo4j.com/blog/7-ways-data-is-graph/ This womans talk was very informative, she's been working with data models since pre-SQL days and clearly knows her shit.
- https://neo4j.com/developer/modeling-designs/
