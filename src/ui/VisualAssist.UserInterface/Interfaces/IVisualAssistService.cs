using Refit;
using System.Threading.Tasks;
using VisualAssist.UserInterface.Models;

namespace VisualAssist.UserInterface.Interfaces
{
    public interface IVisualAssistService
    {
        [Headers("Authorization: Bearer")]
        [Post("/api/sendMessageToGroup")]
        Task SendMessageToGroupAsync(ScreenAssistMessageRequest screenAssistMessageRequest);

        [Headers("Authorization: Bearer")]
        [Post("/api/addUserToGroup")]
        Task AddUserToGroupAsync(AddUserToGroupRequest addUserToGroupRequest);
    }
}
