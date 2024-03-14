using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Newtonsoft.Json;
using VendingMachineFunctions.Models;

namespace VendingMachineFunctions.Services
{
    public class ServiceBusService
    {
        ServiceBusClient sbClient;
        public ServiceBusService()
        {
            sbClient = new ServiceBusClient(System.Environment.GetEnvironmentVariable("AzureServiceBusConnectionString"));
        }

        public async Task SendMessageToQueueAsync(string queueName, object objectToSend)
        {
            var sender = sbClient.CreateSender(queueName);
            
            string messageBody = JsonConvert.SerializeObject(objectToSend);
            var message = new ServiceBusMessage(messageBody)
            {
                ContentType = "application/json",
            };
            
            await sender.SendMessageAsync(message);
        }

        public async Task SendNotificationRequest(NotificationRequest notificationRequest)
        {
            await SendMessageToQueueAsync("sendnotification", notificationRequest);
        }

        public async Task SendOrderMessage(Order order)
        {
            await SendMessageToQueueAsync("createsandbox", order);
        }

    }
}
