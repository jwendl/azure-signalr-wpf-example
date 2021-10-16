using System;
using System.Text.Json.Serialization;

namespace VisualAssist.UserInterface.Models
{
    public class ScreenAssistMessageResponse
    {
        [JsonPropertyName("sentBy")]
        public string SentBy { get; set; }

        [JsonPropertyName("messageDate")]
        public DateTime MessageDate { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; }
    }
}
