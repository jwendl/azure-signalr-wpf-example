using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using VisualAssist.PublishService.Models;

namespace VisualAssist.PublishService
{
    public class VisualAssistantHub
        : ServerlessHub
    {
        [FunctionName(nameof(Negotiate))]
        public SignalRConnectionInfo Negotiate([HttpTrigger(AuthorizationLevel.Anonymous)] HttpRequest req, [SignalRConnectionInfo(HubName = "visualAssist", UserId = "{headers.x-ms-client-principal-id}")] SignalRConnectionInfo signalRConnectionInfo)
        {
            return signalRConnectionInfo;
        }

        [FunctionName(nameof(SendToGroup))]
        public async Task SendToGroup([SignalRTrigger] InvocationContext invocationContext, string groupName, string message)
        {
            await Clients.Group(groupName).SendCoreAsync("sendMessage", new[] {
                new ScreenAssistMessage()
                {
                    SentBy = invocationContext.UserId,
                    MessageDateTime = DateTime.UtcNow,
                    Message = message,
                }
            });
        }

        [FunctionName(nameof(JoinUserToGroup))]
        public async Task JoinUserToGroup([SignalRTrigger] InvocationContext invocationContext, string userName, string groupName)
        {
            await UserGroups.AddToGroupAsync(userName, groupName);
        }

        [FunctionName(nameof(LogMessageAsync))]
        public async Task LogMessageAsync([SignalRTrigger] InvocationContext invocationContext, string message, ILogger logger)
        {
            logger.LogInformation($"Receive {message} from {invocationContext.ConnectionId}.");
        }
    }
}
