using ElectronicHub.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ElectronicHub.Controllers
{
    public class CustomerController : Controller
    {
        public static string constring = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        public static string AddToCart_query = "INSERT INTO [dbo].[Cart_Item] ([ItemID] ,[ItemName],[Quantity],[Unit_Price],[UserId]) VALUES (@ItemID,@ItemName,@Quantity,@Unit_Price,@UserId)";
        public static string Get_Item_Data_By_ID_Query = "SELECT TOP (1) [ID] ,[ItemID],[ItemName] ,[ItemQuantity],[Item_Price],[Item_Description],[Item_Stock_limit]FROM [Items] WHere ItemID =@ItemID";


        // GET: Customer
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Profile()
        {
            return View();
        }

        public ActionResult Checkout()
        {
            return View();
        }

        public JsonResult AddToCart(String ProductId)
        {
            try
            {
                List<Item> item = Get_Item_Data_By_ID(ProductId);

                if(item.Count > 0)
                {
                    if (Session["username"] != null && !string.IsNullOrEmpty(Session["username"].ToString()))
                    {
                        using (SqlConnection con = new SqlConnection(constring))
                        {
                            con.Open();

                            using (SqlCommand cmd = new SqlCommand(AddToCart_query, con))
                            {
                                for (int i = 0; i < item.Count; i++)
                                {
                                    cmd.Parameters.AddWithValue("@ItemID", item[i].ItemID);
                                    cmd.Parameters.AddWithValue("@ItemName", item[i].ItemName);
                                    cmd.Parameters.AddWithValue("@Quantity", 1);
                                    cmd.Parameters.AddWithValue("@Unit_Price", item[i].Item_Price);
                                    cmd.Parameters.AddWithValue("@UserId", Session["username"].ToString());
                                    cmd.ExecuteNonQuery();
                                }

                            }
                            con.Close();

                            return Json(new { success = true, message = "Product Added to Cart" });
                        }
                    }
                    else
                    {
                        // Return data for JavaScript to handle Local Storage
                        return Json(new
                        {
                            success = true,
                            message = "Product Added to Local Storage",
                            localStorageData = new
                            {
                                ItemID = item[0].ItemID,
                                ItemName = item[0].ItemName,
                                ItemQuantity = 1,  // Default quantity
                                Item_Price = item[0].Item_Price
                            }
                        });
                    }
                }
                else
                {
                    return Json(new { success = false, message = "No Products Found" });
                }

                
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        public List<Item> Get_Item_Data_By_ID(string ItemID)
        {
            List<Item> products = new List<Item>();

            try
            {
                using (SqlConnection con = new SqlConnection(constring))
                {
                    using (SqlCommand cmd = new SqlCommand(Get_Item_Data_By_ID_Query, con))
                    {
                        cmd.Parameters.AddWithValue("@ItemID", ItemID);
                        cmd.CommandType = CommandType.Text;
                        con.Open();

                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                products.Add(new Item
                                {
                                    ItemID = reader["ItemID"].ToString(),
                                    ItemName = reader["ItemName"].ToString(),
                                    Item_Price = reader["Item_Price"].ToString(),
                                    Item_Description = reader["Item_Description"].ToString(),
                                    Item_Stock_limit = Convert.ToInt32(reader["Item_Stock_limit"]),

                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error properly for debugging
                Console.WriteLine("Error: " + ex.Message);
            }

            return products; // Always return a valid list, even if empty
        }

        [HttpGet]
        public JsonResult GetCartCount()
        {
            try
            {
                if (Session["username"] != null && !string.IsNullOrEmpty(Session["username"].ToString()))
                {
                    int cartCount = 0;
                    string userId = Session["username"].ToString();

                    using (SqlConnection con = new SqlConnection(constring))
                    {
                        con.Open();
                        string query = "SELECT SUM(Quantity) FROM Cart_Item WHERE UserId = @UserId";  // Modify based on your DB structure
                        using (SqlCommand cmd = new SqlCommand(query, con))
                        {
                            cmd.Parameters.AddWithValue("@UserId", userId);
                            var result = cmd.ExecuteScalar();
                            cartCount = result != DBNull.Value ? Convert.ToInt32(result) : 0;
                        }
                        con.Close();
                    }
                    return Json(new { success = true, count = cartCount }, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new { success = false, count = 0 }, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // Load Cart Items
        public JsonResult GetCartItems()
        {
            if (Session["username"] != null)
            {
                using (SqlConnection con = new SqlConnection(constring))
                {
                    string query = "  SELECT cart.Cart_ID,cart.ItemID,cart.ItemName,cart.Quantity,cart.Unit_Price,cart.UserId , items.Item_Image1 FROM Cart_Item as cart  left Join Items as items ON cart.ItemID = items.ItemID  WHERE UserId = @UserId";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@UserId", Session["username"].ToString());

                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();
                    List<object> cart = new List<object>();

                    while (reader.Read())
                    {
                        cart.Add(new 
                        {
                            ItemID = reader["ItemID"].ToString(),
                            ItemName = reader["ItemName"].ToString(),
                            Item_Price = reader["Unit_Price"].ToString(),
                            ItemQuantity = reader["Quantity"].ToString(),
                            Item_Image1 = reader["Item_Image1"] != DBNull.Value ? "data:image/jpeg;base64," + Convert.ToBase64String((byte[])reader["Item_Image1"]) : "",
                        });
                    }

                    JsonResult result = Json(new { sessionAvailable = true, data = cart }, JsonRequestBehavior.AllowGet);
                    result.MaxJsonLength = int.MaxValue; // Allows a very large JSON size
                    return result;

                    //return Json(new { sessionAvailable = true, data = cart }, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json(new { sessionAvailable = false }, JsonRequestBehavior.AllowGet);
            }
        }

        public JsonResult Get_Item_Image(string ItemID)
        {

            using (SqlConnection con = new SqlConnection(constring))
            {
                string query = @"SELECT TOP (1) [Item_Image1] FROM [Items] Where ItemID =@ItemID";

                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@ItemID", ItemID);

                con.Open();
                SqlDataReader reader = cmd.ExecuteReader();

                string imageBase64 = ""; // Default empty image string

                if (reader.Read()) // Only fetch the first image
                {
                    if (reader["Item_Image1"] != DBNull.Value)
                    {
                        byte[] imageBytes = (byte[])reader["Item_Image1"];
                        imageBase64 = "data:image/jpeg;base64," + Convert.ToBase64String(imageBytes);
                    }
                }
                con.Close();

                // Return only one image
                return Json(new { sessionAvailable = true, image = imageBase64 }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult GetImagesForItems(List<string> itemIds)
        {
            try
            {
                List<object> images = new List<object>();

                using (SqlConnection con = new SqlConnection(constring))
                {
                    string query = "SELECT ItemID, Item_Image1 FROM Items WHERE ItemID IN (" + string.Join(",", itemIds.Select(id => "'" + id + "'")) + ")";
                    SqlCommand cmd = new SqlCommand(query, con);

                    con.Open();
                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        images.Add(new
                        {
                            ItemID = reader["ItemID"].ToString(),
                            Item_Image1 = reader["Item_Image1"] != DBNull.Value
                                ? "data:image/jpeg;base64," + Convert.ToBase64String((byte[])reader["Item_Image1"])
                                : ""
                        });
                    }
                    con.Close();
                }

                JsonResult result = Json(images, JsonRequestBehavior.AllowGet);
                result.MaxJsonLength = int.MaxValue; // Allows a very large JSON size
                return result;
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        // Update Quantity
        public JsonResult UpdateQuantity(string ItemID, int Change)
        {
            if (Session["username"] != null)
            {
                using (SqlConnection con = new SqlConnection(constring))
                {
                    string query = "UPDATE Cart_Item SET Quantity = Quantity + @Change WHERE ItemID = @ItemID AND UserId = @UserId";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@Change", Change);
                    cmd.Parameters.AddWithValue("@ItemID", ItemID);
                    cmd.Parameters.AddWithValue("@UserId", Session["username"].ToString());

                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            return Json(new { success = true, sessionAvailable = Session["username"] != null });
        }

        // Remove Item
        public JsonResult RemoveItem(string ItemID)
        {
            if (Session["username"] != null)
            {
                using (SqlConnection con = new SqlConnection(constring))
                {
                    string query = "DELETE FROM Cart_Item WHERE ItemID = @ItemID AND UserId = @UserId";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@ItemID", ItemID);
                    cmd.Parameters.AddWithValue("@UserId", Session["username"].ToString());

                    con.Open();
                    cmd.ExecuteNonQuery();
                }
            }
            return Json(new { success = true, sessionAvailable = Session["username"] != null });
        }

    }
}