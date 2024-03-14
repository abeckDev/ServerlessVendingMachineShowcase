using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using VendingMachineFunctions.Models;
using VendingMachineFunctions.Services;

namespace VendingMachineFunctions
{
    public class HandleDeadLetterFunction
    {
        private readonly ILogger<HandleDeadLetterFunction> _logger;

        public HandleDeadLetterFunction(ILogger<HandleDeadLetterFunction> logger)
        {
            _logger = logger;
        }

        [Function(nameof(HandleDeadLetterFunction))]
        public async Task Run(
            [ServiceBusTrigger("createsandbox", "UpdateAPI/$deadletterqueue", Connection = "AzureServiceBusConnectionString")]
            ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions)
        {
            _logger.LogInformation("Deadlettering Service engaged. Handling order errors.");
            _logger.LogInformation("Message ID: {id}", message.MessageId);
            _logger.LogInformation("Message Body: {body}", message.Body);
            _logger.LogInformation("Message Content-Type: {contentType}", message.ContentType);

            //Deserialie Order from message body
            var sborderobject = JsonConvert.DeserializeObject<Order>(message.Body.ToString());
            string orderId = sborderobject.OrderId;


            //Set the order State to InProgress
            var tableStorageService = new TableStorageService();
            try
            {

                //Get current state
                var order = await tableStorageService.GetOrderStatusAsync(orderId);
                //Switch state to accepted and update Table Storage
                order.OrderStatus = VendingMachineFunctions.Models.OrderState.Aborted;
                order.SubscriptionTask.SubscriptionStatus = OrderState.Aborted;
                await tableStorageService.UpdateOrderStatusAsync(order);
            }
            catch (Exception ex)
            {
                await messageActions.AbandonMessageAsync(message);
            }

            //Your code logic here -- It is mocked



            //Send bad news message
            var sbService = new ServiceBusService();
            await sbService.SendNotificationRequest(new Models.NotificationRequest()
            {
                MessageBody = $"Order {orderId} has been aborted. Please contact support for further information.",
                Recepients = new string[] { sborderobject.Requestor },
            });


            // Complete the message
            await messageActions.CompleteMessageAsync(message);
        }
    }
}
