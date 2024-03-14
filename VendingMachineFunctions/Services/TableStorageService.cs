using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure;
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

        public async Task<Azure.Response> AddOrderStatusAsync(Order order)
        {

            //Create TableItem
            var StateItem = MapOrderToState(order);
            StateItem.CreationDate = DateTime.Now;

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

        public List<Order> GetOrdersByState(OrderState state)
        {
            try
            {
                //Get TableEntities

                Pageable<OrderStatus> queryResultsFilter = tableClient.Query<OrderStatus>(filter: $"OrderState eq '{state.ToString()}'");
    
                //Create return element
                var response = new List<Order>();

                foreach (OrderStatus orderState in queryResultsFilter)
                {
                    response.Add(MapStateToOrder(orderState));
                }

                return response;
            }
            catch (Exception ex)
            {
                throw new Exception("Error in retrieving Orders from Table Storage", ex);
            }
        }

        public async Task<Azure.Response> UpdateOrderStatusAsync(Order order)
        {
            OrderStatus orderStatus = MapOrderToState(order);

            try
            {
                //Check if time is set to UTC
                if (orderStatus.CreationDate.Kind != DateTimeKind.Utc)
                {
                    orderStatus.CreationDate = orderStatus.CreationDate.ToUniversalTime();
                }

                //Check if time is set to UTC
                if (orderStatus.LastUpdate.Kind != DateTimeKind.Utc)
                {
                    orderStatus.LastUpdate = orderStatus.LastUpdate.ToUniversalTime();
                }

                //Write to Table Storage
                return await tableClient.UpdateEntityAsync(orderStatus, ETag.All, TableUpdateMode.Replace);
            }
            catch (Exception ex)
            {
                throw new Exception("Error in updating OrderStatus to Table Storage", ex);
            }
        }

        public async Task<Azure.Response> DeleteOrderStatusAsync(string orderId)
        {
            try
            {
                //Delete from Table Storage
                return await tableClient.DeleteEntityAsync("orders", orderId);
            }
            catch (Exception ex)
            {
                throw new Exception("Error in deleting OrderStatus from Table Storage", ex);
            }
        }

        //Get Status by OrderId
        public async Task<Order> GetOrderStatusAsync(string orderId)
        {
            try
            {
                //Get TableEntity
                return MapStateToOrder((await tableClient.GetEntityAsync<OrderStatus>("orders", orderId)).Value);
            }
            catch (Exception ex)
            {
                throw new Exception("Error in retrieving OrderStatus from Table Storage", ex);
            }
        }

        private OrderStatus MapOrderToState(Order order)
        {
            var returnOrder = new OrderStatus()
            {
                CreationDate = order.OrderDate,
                Requestor = order.Requestor,
                PartitionKey = "orders",
                RowKey = order.OrderId,
                OrderState = order.OrderStatus,
                LastUpdate = DateTime.Now,
                PermissionState = order.PermissionsTask.PermissionState,
                SubscriptionState = order.SubscriptionTask.SubscriptionStatus,

            };

            return returnOrder;
        }

        private Order MapStateToOrder(OrderStatus orderStatus)
        {
            return new Order()
            {
                OrderId = orderStatus.RowKey,
                Requestor = orderStatus.Requestor,
                OrderDate = orderStatus.CreationDate,
                OrderStatus = orderStatus.OrderState,
                LastChanged = orderStatus.LastUpdate,
                SubscriptionTask = new SubscriptionTask()
                {
                    SubscriptionStatus = orderStatus.SubscriptionState
                },
                PermissionsTask = new PermissionsTask()
                {
                    PermissionState = orderStatus.PermissionState
                }
                
            };
        }
    }
}
