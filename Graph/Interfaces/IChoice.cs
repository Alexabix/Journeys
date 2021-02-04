using Graph.Enum;
using Newtonsoft.Json;

namespace Graph.Interfaces
{
    public interface IChoice
    {
        [JsonProperty(PropertyName = "choiceType")]
        public ChoiceType ChoiceType { get; set; }
    }
}
