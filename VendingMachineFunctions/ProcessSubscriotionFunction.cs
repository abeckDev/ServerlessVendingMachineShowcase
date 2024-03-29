using System;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using VendingMachineFunctions.Models;
using VendingMachineFunctions.Services;
using Newtonsoft.Json;

namespace VendingMachineFunctions
{
    public class ProcessSubscriotionFunction
    {
        private readonly ILogger<ProcessSubscriotionFunction> _logger;

        public ProcessSubscriotionFunction(ILogger<ProcessSubscriotionFunction> logger)
        {
            _logger = logger;
        }

        [Function(nameof(ProcessSubscriotionFunction))]
        public async Task Run(
            [ServiceBusTrigger("createsandbox", "UpdateAPI", Connection = "AzureServiceBusConnectionString")]
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
                order.SubscriptionTask.SubscriptionStatus = OrderState.InProgress;
                await tableStorageService.UpdateOrderStatusAsync(order);
            }
            catch (Exception ex)
            {
                await messageActions.AbandonMessageAsync(message);
            }


            //Your code logic here -- It is mocked
            _logger.LogInformation("Order {orderId} is in progress", sborderobject.OrderId);

            //Check for debug flag 
            if (sborderobject.SubscriptionTask.CostCenterId == "42")
            {
                _logger.LogInformation("Debug flag is identified. I will now mess up the processing");
                await messageActions.AbandonMessageAsync(message);
                throw new Exception("Dont feel like workiong today. Ask again in an hour");
            }
            else
            {
                System.Threading.Tasks.Task.Delay(5000).Wait();
                _logger.LogInformation("Successfully updated SUBSCRIPTIONS (and of course did not mock anything at all...) for order {orderId}", sborderobject.OrderId);

                // Complete the message
                try
                {

                    //Get current state
                    var order = await tableStorageService.GetOrderStatusAsync(orderId);
                    //Switch state to delivered and update Table Storage
                    order.SubscriptionTask.SubscriptionStatus = OrderState.Delivered;
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
}
