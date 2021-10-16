using System.Text.Json.Serialization;

namespace VisualAssist.UserInterface.Models
{
    public class AddUserToGroupRequest
    {
        [JsonPropertyName("username")]
        public string Username { get; set; }

        [JsonPropertyName("groupName")]
        public string GroupName { get; set; }
    }
}
