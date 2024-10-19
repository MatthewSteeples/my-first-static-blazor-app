using BlazorApp.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.DurableTask;
using Microsoft.DurableTask.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WebPush;

namespace Api
{
    public class NotificationScheduler
    {
        private readonly ILogger<NotificationScheduler> _logger;
        private readonly IConfiguration _configuration;

        public NotificationScheduler(ILogger<NotificationScheduler> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        [Function("Notification")]
        public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Anonymous, "post")] NotificationSubscription notificationSubscription, [DurableClient] DurableTaskClient client)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            await client.ScheduleNewOrchestrationInstanceAsync(nameof(NotificationWorkflow), notificationSubscription);

            return new OkObjectResult("Welcome to Azure Functions!");
        }

        [Function(nameof(NotificationWorkflow))]
        public async Task NotificationWorkflow(
            [OrchestrationTrigger] TaskOrchestrationContext context,
            NotificationSubscription input)
        {
            await context.CreateTimer(input.TargetTime, CancellationToken.None);

            var section = _configuration.GetSection("VapidKeys");
            var vapidPublicKey = section["PublicKey"];
            var vapidPrivateKey = section["PrivateKey"];

            var pushSubscription = new PushSubscription(
                input.Url,
                input.P256dh,
                input.Auth);

            var vapidDetails = new VapidDetails("mailto:example@tracker.p4nda.co.uk", vapidPublicKey, vapidPrivateKey);

            using (var webPushClient = new WebPushClient())
                await webPushClient.SendNotificationAsync(pushSubscription, input.Message, vapidDetails);
        }
    }
}
