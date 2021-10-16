using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using System;
using System.IO;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using VisualAssist.PublishService.Models;

namespace VisualAssist.PublishService
{
    public class VisualAssistHub
        : ServerlessHub
    {
        [FunctionName(nameof(Negotiate))]
        public SignalRConnectionInfo Negotiate([HttpTrigger(AuthorizationLevel.Anonymous)] HttpRequest req, [SignalRConnectionInfo(HubName = "visualAssist", UserId = "{headers.x-ms-client-principal-id}")] SignalRConnectionInfo signalRConnectionInfo)
        {
            return signalRConnectionInfo;
        }

        [FunctionName(nameof(SendMessageToGroup))]
        public async Task SendMessageToGroup([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "sendMessageToGroup")] HttpRequest req, [SignalR(HubName = "visualassist")] IAsyncCollector<SignalRMessage> signalRMessages, ClaimsPrincipal claimsPrincipal)
        {
            using var streamReader = new StreamReader(req.Body);
            var json = await streamReader.ReadToEndAsync();
            var screenAssistMessageRequest = JsonSerializer.Deserialize<ScreenAssistMessageRequest>(json);

            await signalRMessages.AddAsync(new SignalRMessage()
            {
                GroupName = screenAssistMessageRequest.GroupName,
                Target = "eventListener",
                Arguments = new[] {
                    new ScreenAssistMessageResponse()
                    {
                        SentBy = claimsPrincipal.Identity.Name,
                        MessageDate = DateTime.UtcNow,
                        Message = screenAssistMessageRequest.Message,
                    }
                },
            });
        }

        [FunctionName(nameof(AddUserToGroup))]
        public async Task AddUserToGroup([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "addUserToGroup")] HttpRequest req, [SignalR(HubName = "visualassist")] IAsyncCollector<SignalRMessage> signalRMessages)
        {
            using var streamReader = new StreamReader(req.Body);
            var json = await streamReader.ReadToEndAsync();
            var addUserToGroupRequest = JsonSerializer.Deserialize<AddUserToGroupRequest>(json);

            await UserGroups.AddToGroupAsync(addUserToGroupRequest.Username, addUserToGroupRequest.GroupName);
        }
    }
}
