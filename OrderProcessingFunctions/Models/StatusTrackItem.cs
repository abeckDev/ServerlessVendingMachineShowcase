using Azure;
using Azure.Data.Tables;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderProcessingFunctions.Models
{
    public class StatusTrackItem : ITableEntity
    {
        public DateTime creationDate { get; set; } = DateTime.Now;

        public string SubscriptionStatus { get; set; }
        public string PermissionsStatus { get; set; }
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        
    }
}
