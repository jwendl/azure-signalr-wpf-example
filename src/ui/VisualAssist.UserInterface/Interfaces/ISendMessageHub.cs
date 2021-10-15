using System.Threading.Tasks;
using VisualAssist.UserInterface.Models;

namespace VisualAssist.UserInterface.Interfaces
{
    public interface ISendMessageHub
    {
        Task SendMessageEvent(string user, ScreenAssistMessage screenAssistMessage);
    }
}
