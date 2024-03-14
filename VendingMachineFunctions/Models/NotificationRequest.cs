using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VendingMachineFunctions.Models
{
    public class NotificationRequest
    {
        public string MessageBody { get; set; }

        public string[] Recepients { get; set; }
    }
}
