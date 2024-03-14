using Azure;
using Azure.Data.Tables;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VendingMachineFunctions.Models
{
    public class OrderStatus : ITableEntity
    {
        public string PartitionKey { get; set; }
        //OrderId
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; } = DateTimeOffset.Now;
        public ETag ETag { get; set; } = ETag.All;

        public OrderState OrderState { get; set; }

        public OrderState PermissionState { get; set; } = OrderState.NotApplicable;

        public OrderState SubscriptionState { get; set; } = OrderState.NotApplicable;

        public DateTime CreationDate { get; set; }

        public DateTime LastUpdate { get; set; }

        public string Requestor { get; set; }
    }
}
