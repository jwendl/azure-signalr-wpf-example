using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace VisualAssist.PublishService
{
    public class VisualAssistantHub
        : ServerlessHub
    {
        [FunctionName("negotiate")]
        public SignalRConnectionInfo Negotiate([HttpTrigger(AuthorizationLevel.Anonymous)] HttpRequest req, [SignalRConnectionInfo(HubName = "visualAssist", UserId = "{headers.x-ms-client-principal-id}")] SignalRConnectionInfo signalRConnectionInfo)
        {
            return signalRConnectionInfo;
        }

        [FunctionName("publishMessage")]
        public async Task PublishMessage([SignalRTrigger] InvocationContext invocationContext, string message, ILogger logger)
        {
            logger.LogInformation($"Receive {message} from {invocationContext.ConnectionId}.");
        }
    }
}
