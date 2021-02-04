namespace Graph.Relationships
{
    public class IsChoice : RelationshipBase
    {
        public IsChoice() { }

        /// <summary>
        /// Who's choice is it?
        /// </summary>
        /// <param name="decider">"MANAGER", "EMPLOYEE" or "SYSTEM"</param>
        public IsChoice(string decider)
        {
            Name = $"IS_{decider}_CHOICE";
        }
    }
}
