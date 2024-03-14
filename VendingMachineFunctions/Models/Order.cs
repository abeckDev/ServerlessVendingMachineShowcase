using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VendingMachineFunctions.Models
{
    class Order
    {
        public string OrderId { get; set; } = Guid.NewGuid().ToString();
        public DateTime OrderDate { get; set; } = DateTime.Now;
        public DateTime LastChanged { get; set; } = DateTime.Now;
        public OrderState OrderStatus { get; set; } = OrderState.New;
        public SubscriptionTask SubscriptionTask { get; set; } = new SubscriptionTask();
        public PermissionsTask PermissionsTask { get; set; } = new PermissionsTask();

    }

    public class SubscriptionTask
    {
        public string CostCenterId { get; set; } = "CC123";
        public string TenantId { get; set; } = "T123";
    }

    class PermissionsTask
    {
        public string SubscriptionId { get; set; } = "S123";
        public string[] OwnerIds { get; set; } = new string[] { "O123", "O124" };
        public string[] ContributorIds { get; set; } = new string[] { "O123", "O124" };
    }

    public enum OrderState
    {
        New,
        Accepted,
        Dispatched,
        InProgress,
        Delivered,
        Aborted
    }
}
