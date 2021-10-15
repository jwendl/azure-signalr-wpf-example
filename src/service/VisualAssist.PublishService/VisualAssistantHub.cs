using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;
using System.Security.Claims;
using System.Threading.Tasks;

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

        [FunctionName(nameof(AddToGroupAsync))]
        public async Task AddToGroupAsync([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "add/{groupName}")] HttpRequest req, string groupName, ClaimsPrincipal claimsPrincipal, [SignalR(HubName = "visualAssist")] IAsyncCollector<SignalRGroupAction> signalRGroupActions)
        {
            var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier);
            await signalRGroupActions.AddAsync(
                new SignalRGroupAction
                {
                    UserId = userIdClaim.Value,
                    GroupName = groupName,
                    Action = GroupAction.Add
                });
        }

        [FunctionName(nameof(RemoveFromGroupAsync))]
        public async Task RemoveFromGroupAsync([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "remove/{groupName}")] HttpRequest req, string groupName, ClaimsPrincipal claimsPrincipal, [SignalR(HubName = "visualAssist")] IAsyncCollector<SignalRGroupAction> signalRGroupActions)
        {
            var userIdClaim = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier);
            await signalRGroupActions.AddAsync(
                new SignalRGroupAction
                {
                    UserId = userIdClaim.Value,
                    GroupName = groupName,
                    Action = GroupAction.Remove
                });
        }

        [FunctionName(nameof(LogMessageAsync))]
        public async Task LogMessageAsync([SignalRTrigger] InvocationContext invocationContext, string message, ILogger logger)
        {
            logger.LogInformation($"Receive {message} from {invocationContext.ConnectionId}.");
        }
    }
}
