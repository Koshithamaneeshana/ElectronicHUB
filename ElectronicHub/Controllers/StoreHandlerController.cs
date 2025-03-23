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


        public static string AwaitingShipmentCoun_Query = "SELECT Count( [Order_ID]) FROM [Order] Where Status != 'Delivered'";
        public static string CompletedShipmentCoun_Query = "SELECT Count( [Order_ID]) FROM [Order] Where Status = 'Delivered'";

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
        public ActionResult CompletedShipment()
        {
            return View();
        }
        public ActionResult CompletedShipment_Paypal()
        {
            return View();
        }
        public ActionResult CompletedShipment_COD()
        {
            return View();
        }
        public ActionResult Category()
        {
            return View();
        }
        public ActionResult Add_Category()
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
                string query = "SELECT * FROM [Order] Where Paymet_Type ='COD' AND Status != 'Delivered'";

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

        public JsonResult GetProductById(int id)
        {
            object product = null;

            using (SqlConnection con = new SqlConnection(constring))
            {
                con.Open();
                string query = "SELECT ID, ItemID, ItemName, ItemQuantity, Item_Price, Item_Description, Item_Stock_limit FROM Items WHERE ID = @ID";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@ID", id);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            product = new
                            {
                                ID = Convert.ToInt32(reader["ID"]),
                                ItemID = reader["ItemID"].ToString(),
                                ItemName = reader["ItemName"].ToString(),
                                ItemQuantity = reader["ItemQuantity"].ToString(),
                                Item_Price = reader["Item_Price"].ToString(),
                                Item_Description = reader["Item_Description"].ToString(),
                                Item_Stock_limit = reader["Item_Stock_limit"].ToString()
                            };
                        }
                    }
                }
            }

            return Json(product, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult UpdateProduct(int ID, string ItemName, int ItemQuantity, decimal Item_Price, string Item_Description, int Item_Stock_limit)
        {
            bool success = false;

            using (SqlConnection con = new SqlConnection(constring))
            {
                con.Open();
                string query = "UPDATE Items SET ItemName = @ItemName, ItemQuantity = @ItemQuantity, Item_Price = @Item_Price, Item_Description = @Item_Description, Item_Stock_limit = @Item_Stock_limit WHERE ID = @ID";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@ItemName", ItemName);
                    cmd.Parameters.AddWithValue("@ItemQuantity", ItemQuantity);
                    cmd.Parameters.AddWithValue("@Item_Price", Item_Price);
                    cmd.Parameters.AddWithValue("@Item_Description", Item_Description);
                    cmd.Parameters.AddWithValue("@Item_Stock_limit", Item_Stock_limit);
                    cmd.Parameters.AddWithValue("@ID", ID);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        success = true;
                    }
                }
            }

            return Json(new { success = success });
        }

        public JsonResult GetOrderItems(int orderId)
        {
            List<object> orderItems = new List<object>();

            using (SqlConnection con = new SqlConnection(constring))
            {
                con.Open();
                string query = "SELECT * FROM [Order_Item] WHERE Order_ID = @Order_ID";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Order_ID", orderId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            orderItems.Add(new
                            {
                                ItemName = reader["ItemName"].ToString(),
                                Quantity = Convert.ToInt32(reader["Quantity"]),
                                Unit_price = Convert.ToDouble(reader["Unit_price"]),
                            });
                        }
                    }
                }
            }

            return Json(orderItems, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult UpdateTrackingNumber(int orderId, string trackingNumber)
        {
            bool success = false;

            using (SqlConnection con = new SqlConnection(constring))
            {
                con.Open();
                string query = "UPDATE [Order] SET Tracking_Number = @Tracking_Number, Status = 'Delivered' WHERE Order_ID = @Order_ID";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Order_ID", orderId);
                    cmd.Parameters.AddWithValue("@Tracking_Number", trackingNumber);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    success = rowsAffected > 0;
                }
            }

            return Json(new { success = success });
        }


        public JsonResult GetAll_Completed_COD_Load()
        {
            List<object> Orders = new List<object>();

            using (SqlConnection con = new SqlConnection(constring))
            {
                con.Open();
                string query = "SELECT * FROM [Order] Where Paymet_Type ='COD' AND Status = 'Delivered'";

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


        #region ctaegory

        public JsonResult GetCategoryDetails(int categoryID)
        {
            using (SqlConnection con = new SqlConnection(constring))
            {
                con.Open();
                string query = "SELECT * FROM [Category] WHERE CategoryID = @CategoryID";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@CategoryID", categoryID);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var category = new
                            {
                                CategoryID = reader["CategoryID"],
                                CategoryName = reader["CategoryName"].ToString(),
                                Description = reader["Description"].ToString()
                            };
                            return Json(category, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json(null, JsonRequestBehavior.AllowGet); // Return null if category not found
                        }
                    }
                }
            }
        }

        [HttpPost]
        public JsonResult UpdateCategory(int categoryID, string categoryName, string description)
        {
            using (SqlConnection con = new SqlConnection(constring))
            {
                con.Open();
                string query = "UPDATE [Category] SET CategoryName = @CategoryName, Description = @Description WHERE CategoryID = @CategoryID";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@CategoryID", categoryID);
                    cmd.Parameters.AddWithValue("@CategoryName", categoryName);
                    cmd.Parameters.AddWithValue("@Description", description);

                    int rowsAffected = cmd.ExecuteNonQuery();
                    if (rowsAffected > 0)
                    {
                        return Json(new { success = true }, JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json(new { success = false }, JsonRequestBehavior.AllowGet);
                    }
                }
            }
        }


        [HttpPost]
        public JsonResult InsertCategory(string CategoryName, string Description)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(constring))
                {
                    con.Open();

                    // Prepare the SQL query to insert the new category
                    string query = "INSERT INTO Category (CategoryName, Description) VALUES (@CategoryName, @Description)";

                    // Execute the SQL query
                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        // Add parameters to prevent SQL injection
                        cmd.Parameters.AddWithValue("@CategoryName", CategoryName);
                        cmd.Parameters.AddWithValue("@Description", Description);

                        int result = cmd.ExecuteNonQuery();

                        // If rows are affected, the insert was successful
                        if (result > 0)
                        {
                            return Json(new { success = true });
                        }
                        else
                        {
                            return Json(new { success = false });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log error and return failure response
                return Json(new { success = false, message = ex.Message });
            }
        }

        public JsonResult GetAll_Category_Load()
        {
            List<object> categories = new List<object>();

            using (SqlConnection con = new SqlConnection(constring))
            {
                con.Open();
                string query = "SELECT * FROM Category";  

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            categories.Add(new
                            {
                                CategoryID = Convert.ToInt32(reader["CategoryID"]),
                                CategoryName = reader["CategoryName"].ToString(),
                                Description = reader["Description"].ToString()
                            });
                        }
                    }
                }
            }

            JsonResult result = Json(categories, JsonRequestBehavior.AllowGet);
            result.MaxJsonLength = int.MaxValue;  // Allows a very large JSON size
            return result;
        }

        #endregion

        #region Count

        public JsonResult AwaitingShipmentCount()
        {
            try
            {
                string constr = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                List<object> NotificationCount = new List<object>();
                using (SqlConnection con = new SqlConnection(constr))
                {
                    using (SqlCommand cmd = new SqlCommand(AwaitingShipmentCoun_Query, con))
                    {
                        cmd.CommandType = CommandType.Text;
                        con.Open();
                        using (SqlDataReader sdr = cmd.ExecuteReader())
                        {
                            while (sdr.Read())
                            {
                                NotificationCount.Add(new object[]
                                {
                                    sdr[0]
                                    });
                            }
                        }

                        con.Close();
                    }
                }

                return Json(NotificationCount);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return Json("0");
            }

        }

        public JsonResult CompletedShipmentCount()
        {
            try
            {
                string constr = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                List<object> NotificationCount = new List<object>();
                using (SqlConnection con = new SqlConnection(constr))
                {
                    using (SqlCommand cmd = new SqlCommand(CompletedShipmentCoun_Query, con))
                    {
                        cmd.CommandType = CommandType.Text;
                        con.Open();
                        using (SqlDataReader sdr = cmd.ExecuteReader())
                        {
                            while (sdr.Read())
                            {
                                NotificationCount.Add(new object[]
                                {
                                    sdr[0]
                                    });
                            }
                        }

                        con.Close();
                    }
                }

                return Json(NotificationCount);

            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return Json("0");
            }

        }

        #endregion

    }
}