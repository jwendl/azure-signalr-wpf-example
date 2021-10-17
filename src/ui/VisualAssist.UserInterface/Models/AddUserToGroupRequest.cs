using System.Text.Json.Serialization;

namespace VisualAssist.UserInterface.Models
{
    public class AddUserToGroupRequest
    {
        [JsonPropertyName("userId")]
        public string UserId { get; set; }

        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("groupName")]
        public string GroupName { get; set; }
    }
}
