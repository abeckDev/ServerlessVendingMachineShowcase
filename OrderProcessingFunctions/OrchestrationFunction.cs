using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using OrderProcessingFunctions.Models;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.Functions.Worker.Extensions;
using Azure.Data.Tables;
using System.Text;

namespace OrderProcessingFunctions
{
    public class OrchestrationFunction
    {
        private readonly ILogger _logger;

        public OrchestrationFunction(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<OrchestrationFunction>();
        }

        [Function("OrchestrationFunction")]
        [ServiceBusOutput("processing", Connection = "AzureServiceBusConnectionString")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");

            //Mock an Order
            var order = new Order();

            //Create Status in the Table storage for tracking 
            var tableClient = new TableServiceClient(
                new Uri("https://storageaccountdemoalbec.table.core.windows.net"),
                new TableSharedKeyCredential("storageaccountdemoalbec", System.Environment.GetEnvironmentVariable("AzureStorageAccountKey"))
            ).GetTableClient("statusTable");



            tableClient.

            


            //Publish tasks to Service Bus

            var serviceBusSender = new ServiceBusClient(System.Environment.GetEnvironmentVariable("AzureServiceBusConnectionString")).CreateSender("processing");
            ServiceBusMessage message = new ServiceBusMessage(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(order)));
            await serviceBusSender.SendMessageAsync(message);

            //Update Status in the Table storage for tracking (Dispatched)




            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            response.WriteString("Welcome to Azure Functions!");

            return response;
        }
    }
}
