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


            try
            {
                //Get OrderStatus from Table Storage
                var OrderStatus = await tableStorageService.GetOrderStatus(orderId);
                
                //Return OrderStatus as Json object response
                return new OkObjectResult(new 
                {
                    orderId = OrderStatus.RowKey,
                    orderState = OrderStatus.OrderState.ToString(),
                    created = OrderStatus.CreationDate,
                    lastUpdate = OrderStatus.LastUpdate

                });
            }
            catch (Exception ex)
            {

                return new NotFoundObjectResult("Error: " + ex.Message);
            }
        }
    }
}
