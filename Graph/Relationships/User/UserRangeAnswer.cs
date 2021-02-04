using Newtonsoft.Json;

namespace Graph.Relationships.User
{
    public class UserRangeAnswer : UserAnswer
    {
        public UserRangeAnswer() { Name = "USER_RANGE_ANSWER"; }
        public UserRangeAnswer(int answer)
        {
            Name = "USER_RANGE_ANSWER";
            Answer = answer;
        }

        [JsonProperty(PropertyName = "answer")]
        public int Answer { get; set; }
    }
}
