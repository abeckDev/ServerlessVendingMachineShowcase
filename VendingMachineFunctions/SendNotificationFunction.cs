using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VendingMachineFunctions.Models;

namespace VendingMachineFunctions
{
    public class SendNotificationFunction
    {
        private readonly ILogger<SendNotificationFunction> _logger;

        public SendNotificationFunction(ILogger<SendNotificationFunction> logger)
        {
            _logger = logger;
        }

        [Function(nameof(SendNotificationFunction))]
        public async Task Run(
            [ServiceBusTrigger("sendnotification", Connection = "AzureServiceBusConnectionString")]
            ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions)
        {
            _logger.LogInformation("Message ID: {id}", message.MessageId);
            _logger.LogInformation("Message Body: {body}", message.Body);
            _logger.LogInformation("Message Content-Type: {contentType}", message.ContentType);

            var notification = JsonConvert.DeserializeObject<NotificationRequest>(message.Body.ToString());

            _logger.LogInformation($"This is the messaging service! \n I am going to inform {notification.Recepients[0]} about the following message: {notification.MessageBody}");
            _logger.LogInformation("Message Service - Done sending message. Have a good day!");


            // Complete the message
            await messageActions.CompleteMessageAsync(message);
        }
    }
}
