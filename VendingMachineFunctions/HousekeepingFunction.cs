using System;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using VendingMachineFunctions.Services;

namespace VendingMachineFunctions
{
    public class HousekeepingFunction
    {
        private readonly ILogger _logger;

        public HousekeepingFunction(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<HousekeepingFunction>();
        }

        [Function("HousekeepingFunction")]
        public async Task RunAsync([TimerTrigger("*/5 * * * * *")] TimerInfo myTimer)
        {
            _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");


            var tableStorageService = new TableStorageService();
            var sbService = new ServiceBusService();


            //Checking for succeeded orders 
            var completedOrders = tableStorageService.GetOrdersByState(Models.OrderState.InProgress);

            foreach (var order in completedOrders)
            {
                //Check if all steps are done
                if(order.SubscriptionTask.SubscriptionStatus == Models.OrderState.Delivered &&
                   order.PermissionsTask.PermissionState == Models.OrderState.Delivered)
                {
                    order.OrderStatus = Models.OrderState.Delivered;
                    await tableStorageService.UpdateOrderStatusAsync(order);
                    
                    //CreateNotification that order is done
                    await sbService.SendNotificationRequest(new Models.NotificationRequest()
                    {
                        MessageBody = $"Order {order.OrderId} has been fully processed. Thanks for choosing our vending machine today!",
                        Recepients = [order.Requestor],
                    });
                }
            }



            //Filtering for orders that are done or aborted 
            var succeededTasks = tableStorageService.GetOrdersByState(Models.OrderState.Delivered);
            succeededTasks.AddRange(tableStorageService.GetOrdersByState(Models.OrderState.Aborted));
            _logger.LogInformation($"Found {succeededTasks.Count} succeeded and aborted orders");

            //Check wich one of these are last updated longer than a day 
            int deleteCounter = 0;
            foreach (var order in succeededTasks)
            {
                if (DateTime.UtcNow - order.LastChanged > TimeSpan.FromMinutes(5))
                {
                    //Delete the findings from the table storage
                    await tableStorageService.DeleteOrderStatusAsync(order.OrderId);
                    deleteCounter++;
                }
            }
            _logger.LogInformation($"Deleted {deleteCounter} orders");
            

            
            if (myTimer.ScheduleStatus is not null)
            {
                _logger.LogInformation($"Next timer schedule at: {myTimer.ScheduleStatus.Next}");
            }
        }
    }
}
