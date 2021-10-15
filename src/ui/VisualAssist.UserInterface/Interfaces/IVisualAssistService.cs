using Refit;
using System.Threading.Tasks;
using VisualAssist.UserInterface.Models;

namespace VisualAssist.UserInterface.Interfaces
{
    public interface IVisualAssistService
    {
        [Headers("Authorization: Bearer")]
        [Post("/api/add/{groupName}")]
        Task<SignalRConnectionInfo> AddToGroupAsync(string groupName);

        [Headers("Authorization: Bearer")]
        [Post("/api/remove/{groupName}")]
        Task<SignalRConnectionInfo> RemoveFromGroupAsync(string groupName);
    }
}
