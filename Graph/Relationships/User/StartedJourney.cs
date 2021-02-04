using Newtonsoft.Json;
using System;

namespace Graph.Relationships.User
{
    public class StartedJourney : RelationshipBase
    {
        public StartedJourney()
        {
            Name = $"STARTED_JOURNEY";
            StartDate = DateTime.Now;
        }

        [JsonProperty(PropertyName = "startDate")]
        public DateTime StartDate { get; set; }
    }
}
