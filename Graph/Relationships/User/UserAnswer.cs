using Newtonsoft.Json;
using System;

namespace Graph.Relationships.User
{
    public abstract class UserAnswer : RelationshipBase
    {
        public UserAnswer()
        {
            AnswerDate = DateTime.Now;
        }

        [JsonProperty(PropertyName = "answerDate")]
        public DateTime AnswerDate { get; set; }
    }
}
