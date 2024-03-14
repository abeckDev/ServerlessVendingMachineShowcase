using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using VendingMachineFunctions.Services;

namespace VendingMachineFunctions
{
    public class GetOrderStatusFunction
    {
        private readonly ILogger<GetOrderStatusFunction> _logger;

        public GetOrderStatusFunction(ILogger<GetOrderStatusFunction> logger)
        {
            _logger = logger;
        }

        [Function("GetOrderStatusFunction")]
        public async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            
            //GetOrderId from request query
            string orderId = req.Query["orderId"];

            //Check if empty
            if (string.IsNullOrEmpty(orderId))
            {
                return new BadRequestObjectResult("Please pass an orderId on the query string");
            }

            //InitialeTableStorageService
            var tableStorageService = new TableStorageService();

            //Debug
            if (orderId == "debug")
            {
                return new OkObjectResult(tableStorageService.GetOrdersByState(Models.OrderState.Accepted));
            }



            try
            {
                //Get OrderStatus from Table Storage
                var OrderStatus = await tableStorageService.GetOrderStatusAsync(orderId);
                
                //Return OrderStatus as Json object response
                return new OkObjectResult(new 
                {
                    orderId = OrderStatus.OrderId,
                    orderState = OrderStatus.OrderStatus.ToString(),
                    created = OrderStatus.OrderDate,
                    lastUpdate = OrderStatus.LastChanged,
                    permissionState = OrderStatus.PermissionsTask.PermissionState.ToString(),
                    subscriptionState = OrderStatus.SubscriptionTask.SubscriptionStatus.ToString()

                });
            }
            catch (Exception ex)
            {

                return new NotFoundObjectResult("Error: " + ex.Message);
            }
        }
    }
}
