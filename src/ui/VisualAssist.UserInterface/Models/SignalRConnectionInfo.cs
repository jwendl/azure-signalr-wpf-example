using System;
using System.Text.Json.Serialization;

namespace VisualAssist.UserInterface.Models
{
    public class SignalRConnectionInfo
    {
        [JsonPropertyName("url")]
        public Uri Url { get; set; }

        [JsonPropertyName("accessToken")]
        public string AccessToken { get; set; }
    }
}
