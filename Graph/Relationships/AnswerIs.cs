using Graph.Enum;
using Graph.Interfaces;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace Graph.Relationships
{
    public class AnswerIs : RelationshipBase, IChoice
    {
        public AnswerIs() { }

        public AnswerIs(string conditionDescription, ChoiceType choiceType, IEnumerable<int> acceptableAnswers)
        {
            Name = $"ANSWER_IS_{conditionDescription}";
            ChoiceType = choiceType;
            AcceptableAnswers = acceptableAnswers;
        }

        [JsonProperty(PropertyName = "choiceType")]
        public ChoiceType ChoiceType { get; set; }

        /// <summary>
        /// Defines the answers that have to be provided to traverse this route
        /// </summary>
        [JsonProperty(PropertyName = "acceptableAnswers")]
        public IEnumerable<int> AcceptableAnswers { get; set; }
    }
}
