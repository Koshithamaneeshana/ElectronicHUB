using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ElectronicHub.Models
{
    public class OrderItem
    {
        public string Name { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public string itemID { get; set; }
    }
}