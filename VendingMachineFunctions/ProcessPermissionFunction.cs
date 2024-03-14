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
    public class ProcessPermissionFunction
    {
        private readonly ILogger<ProcessPermissionFunction> _logger;

        public ProcessPermissionFunction(ILogger<ProcessPermissionFunction> logger)
        {
            _logger = logger;
        }

        [Function(nameof(ProcessPermissionFunction))]
        public async Task Run(
            [ServiceBusTrigger("createsandbox", "UpdateCosmosDB", Connection = "AzureServiceBusConnectionString")]
            ServiceBusReceivedMessage message,
            ServiceBusMessageActions messageActions)
        {

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
                order.OrderStatus = VendingMachineFunctions.Models.OrderState.InProgress;
                order.PermissionsTask.PermissionState = OrderState.InProgress;
                await tableStorageService.UpdateOrderStatusAsync(order);
            }
            catch (Exception ex)
            {
                await messageActions.AbandonMessageAsync(message);
            }


            //Your code logic here -- It is mocked
            _logger.LogInformation("Order {orderId} is in progress", sborderobject.OrderId);
            System.Threading.Tasks.Task.Delay(10000).Wait();
            _logger.LogInformation("Successfully updated PERMISSIONS (and of course did not mock anything at all...) for order {orderId}", sborderobject.OrderId);

            // Complete the message
            try
            {

                //Get current state
                var order = await tableStorageService.GetOrderStatusAsync(orderId);
                //Switch state to delivered and update Table Storage
                order.PermissionsTask.PermissionState = OrderState.Delivered;
                await tableStorageService.UpdateOrderStatusAsync(order);
            }
            catch (Exception ex)
            {
                await messageActions.AbandonMessageAsync(message);
            }
            await messageActions.CompleteMessageAsync(message);
        }
    }
}
