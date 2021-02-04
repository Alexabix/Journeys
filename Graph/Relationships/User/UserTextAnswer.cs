using Newtonsoft.Json;

namespace Graph.Relationships.User
{
    public class UserTextAnswer : UserAnswer
    {
        public UserTextAnswer() { Name = "USER_TEXT_ANSWER"; }

        public UserTextAnswer(string answer)
        {
            Name = "USER_TEXT_ANSWER";
            Answer = answer;
        }

        [JsonProperty(PropertyName = "answer")]
        public string Answer { get; set; }
    }
}
