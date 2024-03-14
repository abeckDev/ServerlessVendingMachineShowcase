using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using VendingMachineFunctions.Models;
using VendingMachineFunctions.Services;

namespace VendingMachineFunctions
{
    public class IntakeFunction
    {
        private readonly ILogger<IntakeFunction> _logger;

        public IntakeFunction(ILogger<IntakeFunction> logger)
        {
            _logger = logger;
        }

        [Function("IntakeFunction")]
        public async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            //Extract Order from request

            //Create sample Order
            var order = new Order()
            {
                OrderId = Guid.NewGuid().ToString(),
                OrderStatus = VendingMachineFunctions.Models.OrderState.New,
                OrderDate = DateTime.Now,
                SubscriptionTask = new SubscriptionTask()
                {
                    CostCenterId = "CC123",
                    TenantId = "T123"
                },
                PermissionsTask = new PermissionsTask()
                {
                    SubscriptionId = "S123",
                    OwnerIds = new string[] { "O123", "O124" },
                    ContributorIds = new string[] { "O123", "O124" }
                }
            };

            //Add Order to Table Storage
            var tableStorageService = new TableStorageService();

            try
            {
                var response = await tableStorageService.AddOrderStatus(order);
                if (response.IsError)
                {
                    throw new Exception("Error in adding Order to Table Storage");
                };

                //Switch state to accepted and update Table Storage
                order.OrderStatus = VendingMachineFunctions.Models.OrderState.Accepted;
                await tableStorageService.UpdateOrderStatus(order);

                //Return accepted response
                return new AcceptedResult(@"http://localhost:7111/api/GetOrderStatusFunction?orderId=" + order.OrderId, order.OrderId);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult("Error in adding Order to Table Storage - " + ex.Message);
            }
        }
    }
}
