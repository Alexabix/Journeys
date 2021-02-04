using Newtonsoft.Json;

namespace Graph.Relationships.User
{
    public class UserBinaryAnswer : UserAnswer
    {
        public UserBinaryAnswer() { Name = "USER_BINARY_ANSWER"; }
        public UserBinaryAnswer(bool answer)
        {
            Name = "USER_BINARY_ANSWER";
            Answer = answer;
        }

        [JsonProperty(PropertyName = "answer")]
        public bool Answer { get; set; }
    }
}
