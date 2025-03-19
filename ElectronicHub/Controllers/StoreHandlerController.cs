using ElectronicHub.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ElectronicHub.Controllers
{
    public class StoreHandlerController : Controller
    {
        public static string constring = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        //Query
        public static string AddItemData_query = "INSERT INTO Items (ItemID, ItemName, ItemQuantity, Item_Price, Item_Image1, Item_Image2, Item_Image3,Item_Description,Item_Stock_limit) " +
               "VALUES (@ItemID, @ItemName, @ItemQuantity, @Item_Price, @Item_Image1, @Item_Image2, @Item_Image3,@Item_Description,@Item_Stock_limit)";

        // GET: StoreHandler
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Add_Products_StoreHandler()
        {
            return View();
        }
        public ActionResult View_Products_StoreHandler()
        {
            return View();
        }

        public ActionResult AwaitingShipment()
        {
            return View();
        }
        public ActionResult AwaitingShipment_COD()
        {
            return View();
        }
        public ActionResult AwaitingShipment_Paypal()
        {
            return View();
        }

        [HttpPost]
        public JsonResult AddItemData(Item item)
        {
            try
            {
                byte[] imageBytes1 = null, imageBytes2 = null, imageBytes3 = null;

                // Convert images to byte arrays
                if (item.ImageFile1 != null)
                    using (var ms = new MemoryStream())
                    {
                        item.ImageFile1.InputStream.CopyTo(ms);
                        imageBytes1 = ms.ToArray();
                    }

                if (item.ImageFile2 != null)
                    using (var ms = new MemoryStream())
                    {
                        item.ImageFile2.InputStream.CopyTo(ms);
                        imageBytes2 = ms.ToArray();
                    }

                if (item.ImageFile3 != null)
                    using (var ms = new MemoryStream())
                    {
                        item.ImageFile3.InputStream.CopyTo(ms);
                        imageBytes3 = ms.ToArray();
                    }

                using (SqlConnection con = new SqlConnection(constring))
                {
                    con.Open();

                    using (SqlCommand cmd = new SqlCommand(AddItemData_query, con))
                    {
                        cmd.Parameters.AddWithValue("@ItemID", item.ItemID);
                        cmd.Parameters.AddWithValue("@ItemName", item.ItemName);
                        cmd.Parameters.AddWithValue("@ItemQuantity", item.ItemQuantity);
                        cmd.Parameters.AddWithValue("@Item_Price", item.Item_Price);
                        cmd.Parameters.AddWithValue("@Item_Description", item.Item_Description);
                        cmd.Parameters.AddWithValue("@Item_Stock_limit", item.Item_Stock_limit);
                        cmd.Parameters.Add("@Item_Image1", SqlDbType.VarBinary).Value = (object)imageBytes1 ?? DBNull.Value;
                        cmd.Parameters.Add("@Item_Image2", SqlDbType.VarBinary).Value = (object)imageBytes2 ?? DBNull.Value;
                        cmd.Parameters.Add("@Item_Image3", SqlDbType.VarBinary).Value = (object)imageBytes3 ?? DBNull.Value;

                        cmd.ExecuteNonQuery();
                    }
                    con.Close();
                }

                return Json(new { success = true, message = "Data Saved Successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        [HttpGet]
        public JsonResult GetAll_Products_Load()
        {
            List<object> products = new List<object>();

            using (SqlConnection con = new SqlConnection(constring))
            {
                con.Open();
                string query = "SELECT ID,ItemID, ItemName, ItemQuantity, Item_Price, Item_Description,Item_Stock_limit, Item_Image1, Item_Image2, Item_Image3 FROM Items";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            products.Add(new
                            {
                                ID = Convert.ToInt32(reader["ID"]),
                                ItemID = reader["ItemID"].ToString(),
                                ItemName = reader["ItemName"].ToString(),
                                ItemQuantity = reader["ItemQuantity"].ToString(),
                                Item_Price = reader["Item_Price"].ToString(),
                                Item_Description = reader["Item_Description"].ToString(),
                                Item_Stock_limit = reader["Item_Stock_limit"].ToString(),
                                Item_Image1 = reader["Item_Image1"] != DBNull.Value ? "data:image/jpeg;base64," + Convert.ToBase64String((byte[])reader["Item_Image1"]) : "",
                                Item_Image2 = reader["Item_Image2"] != DBNull.Value ? "data:image/jpeg;base64," + Convert.ToBase64String((byte[])reader["Item_Image2"]) : "",
                                Item_Image3 = reader["Item_Image3"] != DBNull.Value ? "data:image/jpeg;base64," + Convert.ToBase64String((byte[])reader["Item_Image3"]) : ""
                            });
                        }
                    }
                }
            }

            JsonResult result = Json(products, JsonRequestBehavior.AllowGet);
            result.MaxJsonLength = int.MaxValue; // Allows a very large JSON size
            return result;
        }


        public JsonResult GetAll_Avaiting_COD_Load()
        {
            List<object> Orders = new List<object>();

            using (SqlConnection con = new SqlConnection(constring))
            {
                con.Open();
                string query = "SELECT * FROM [Order] Where Paymet_Type ='COD'";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            Orders.Add(new
                            {
                                Order_ID = Convert.ToInt32(reader["Order_ID"]),
                                UserId = reader["UserId"].ToString(),
                                Address = reader["Address"].ToString(),
                                Status = reader["Status"].ToString(),
                                Date = reader["Date"].ToString(),
                                Sub_Total = reader["Sub_Total"].ToString(),
                                Paymet_Type = reader["Paymet_Type"].ToString()
                            });
                        }
                    }
                }
            }

            JsonResult result = Json(Orders, JsonRequestBehavior.AllowGet);
            result.MaxJsonLength = int.MaxValue; // Allows a very large JSON size
            return result;
        }

    }
}