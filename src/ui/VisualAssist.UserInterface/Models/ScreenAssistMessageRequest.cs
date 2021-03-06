using System.Text.Json.Serialization;

namespace VisualAssist.UserInterface.Models
{
    public class ScreenAssistMessageRequest
    {
        [JsonPropertyName("groupName")]
        public string GroupName { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }
    }
}
