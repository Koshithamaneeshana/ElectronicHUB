using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace ElectronicHub.Models
{
    public class Item
    {
        public string ItemID { get; set; }
        public string ItemName { get; set; }
        public string ItemQuantity { get; set; }
        public int ItemQuantityINT { get; set; }
        public string Item_Price { get; set; }
        public string Item_Description { get; set; }
        public int Item_Stock_limit { get; set; }

        public byte[] Item_Image1 { get; set; }
        public byte[] Item_Image2 { get; set; }
        public byte[] Item_Image3 { get; set; }

        [NotMapped]  // Prevent mapping to database
        public HttpPostedFileBase ImageFile1 { get; set; }

        [NotMapped]
        public HttpPostedFileBase ImageFile2 { get; set; }

        [NotMapped]
        public HttpPostedFileBase ImageFile3 { get; set; }

        public List<Review> Ratings { get; set; }
    }
}