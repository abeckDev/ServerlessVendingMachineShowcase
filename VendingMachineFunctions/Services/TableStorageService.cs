using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure.Data.Tables;
using Azure.Data.Tables.Models;
using VendingMachineFunctions.Models;

namespace VendingMachineFunctions.Services
{
    class TableStorageService
    {
        public TableClient tableClient;
        public TableStorageService()
        {
            //GetStorageAccount ServiceClient via ConnectionString
            var serviceClient = new TableServiceClient(System.Environment.GetEnvironmentVariable("AzureStorageAccountConnectionString"));
            
            //CheckIfTableExists
            serviceClient.CreateTableIfNotExists("orders");

            //Get TabelClient
            this.tableClient = serviceClient.GetTableClient("orders");
        }

        public async Task<Azure.Response> AddOrderStatus(Order order)
        {

            //Create TableItem
            var StateItem = new OrderStatus()
            {
                PartitionKey = "orders",
                RowKey = order.OrderId,
                OrderState = order.OrderStatus,
                CreationDate = DateTime.Now,
                LastUpdate = DateTime.Now
            };

            try
            {
                //Check if time is set to UTC
                if (StateItem.CreationDate.Kind != DateTimeKind.Utc)
                {
                    StateItem.CreationDate = StateItem.CreationDate.ToUniversalTime();
                }

                //Check if time is set to UTC
                if (StateItem.LastUpdate.Kind != DateTimeKind.Utc)
                {
                    StateItem.LastUpdate = StateItem.LastUpdate.ToUniversalTime();
                }

                //Write to Table Storage
                return await tableClient.AddEntityAsync(StateItem);
            }
            catch (Exception ex)
            {

                throw new Exception("Error in adding OrderStatus to Table Storage", ex);
            }
        } 


        //Get Status by OrderId
        public async Task<OrderStatus> GetOrderStatus(string orderId)
        {
            try
            {
                //Get TableEntity
                return (await tableClient.GetEntityAsync<OrderStatus>("orders", orderId)).Value;
            }
            catch (Exception ex)
            {
                throw new Exception("Error in retrieving OrderStatus from Table Storage", ex);
            }
        }
    }
}
