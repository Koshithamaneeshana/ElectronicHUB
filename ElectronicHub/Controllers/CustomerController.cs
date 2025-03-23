using ElectronicHub.Models;
using Newtonsoft.Json;
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


        //Order

        public static string SaveOrderToDatabase_Query = @" INSERT INTO [Order] (UserId, Address, Status, Date, Sub_Total, Paymet_Type, Tracking_Number) 
                                                            VALUES (@UserId, @Address, @Status, @Date, @Sub_Total, @Paymet_Type, @Tracking_Number);
                                                            
                                                            SELECT SCOPE_IDENTITY();";

        // GET: Customer
        public ActionResult Index()
        {
            return View();
        }
        public ActionResult User_profile_Handling()
        {
            return View();
        }
        public ActionResult Addresses_Handling()
        {
            return View();
        }
        public ActionResult Orders_Handling()
        {
            return View();
        }

        public ActionResult Profile()
        {
            if (Session["UserId"] == null || string.IsNullOrEmpty(Session["UserId"].ToString()))
            {
                return RedirectToAction("Login", "Login");
            }
            else
            {
                return View();
            }            
        }

        public ActionResult Checkout()
        {
            return View();
        }

        public JsonResult AddToCart(string ProductId)
        {
            try
            {
                List<Item> item = Get_Item_Data_By_ID(ProductId);

                if (item.Count > 0)
                {
                    if (Session["username"] != null && !string.IsNullOrEmpty(Session["username"].ToString()))
                    {
                        string userId = Session["UserId"].ToString();

                        using (SqlConnection con = new SqlConnection(constring))
                        {
                            con.Open();

                            foreach (var product in item)
                            {
                                // 1️⃣ Check if item already exists in cart
                                string checkQuery = "SELECT Quantity FROM Cart_Item WHERE UserId = @UserId AND ItemID = @ItemID";
                                using (SqlCommand checkCmd = new SqlCommand(checkQuery, con))
                                {
                                    checkCmd.Parameters.AddWithValue("@UserId", userId);
                                    checkCmd.Parameters.AddWithValue("@ItemID", product.ItemID);

                                    object existingQuantity = checkCmd.ExecuteScalar();

                                    if (existingQuantity != null)  // ✅ If item exists, update quantity
                                    {
                                        string updateQuery = "UPDATE Cart_Item SET Quantity = Quantity + 1 WHERE UserId = @UserId AND ItemID = @ItemID";
                                        using (SqlCommand updateCmd = new SqlCommand(updateQuery, con))
                                        {
                                            updateCmd.Parameters.AddWithValue("@UserId", userId);
                                            updateCmd.Parameters.AddWithValue("@ItemID", product.ItemID);
                                            updateCmd.ExecuteNonQuery();
                                        }
                                    }
                                    else  // ❌ If item does NOT exist, insert a new row
                                    {
                                        string insertQuery = @"
                                    INSERT INTO Cart_Item (ItemID, ItemName, Quantity, Unit_Price, UserId) 
                                    VALUES (@ItemID, @ItemName, @Quantity, @Unit_Price, @UserId)";

                                        using (SqlCommand insertCmd = new SqlCommand(insertQuery, con))
                                        {
                                            insertCmd.Parameters.AddWithValue("@ItemID", product.ItemID);
                                            insertCmd.Parameters.AddWithValue("@ItemName", product.ItemName);
                                            insertCmd.Parameters.AddWithValue("@Quantity", 1);
                                            insertCmd.Parameters.AddWithValue("@Unit_Price", product.Item_Price);
                                            insertCmd.Parameters.AddWithValue("@UserId", userId);
                                            insertCmd.ExecuteNonQuery();
                                        }
                                    }
                                }
                            }
                        }

                        return Json(new { success = true, message = "Product added to cart" });
                    }
                    else
                    {

                        return Json(new
                        {
                            success = true,
                            message = "Product Added to Local Storage",
                            localStorageData = new
                            {
                                ItemID = item[0].ItemID,
                                ItemName = item[0].ItemName,
                                ItemQuantity = 1,  // Default quantity
                                Item_Price = item[0].Item_Price,
                                StockQuantity = item[0].ItemQuantityINT
                            }
                        });
                    }
                }
                else
                {
                    return Json(new { success = false, message = "No products found" });
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
                                    ItemQuantityINT = Convert.ToInt32(reader["ItemQuantity"]),

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
                    string userId = Session["UserId"].ToString();

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
                    string query = "  SELECT cart.Cart_ID,cart.ItemID,cart.ItemName,cart.Quantity,cart.Unit_Price,cart.UserId , items.Item_Image1 ,items.ItemQuantity FROM Cart_Item as cart  left Join Items as items ON cart.ItemID = items.ItemID  WHERE UserId = @UserId";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@UserId", Session["UserId"].ToString());

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
                            StockQuantity = reader["ItemQuantity"].ToString()
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
        public JsonResult UpdateQuantity(string ItemID, int Change )
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

        public JsonResult ProcessOrder(string paymentMethod, decimal totalAmount, string cartItems , string fullAddress)
        {
            try
            {
                // Deserialize JSON string to List<CartItem>
                List<OrderItem> cartItemList = JsonConvert.DeserializeObject<List<OrderItem>>(cartItems);

                // Process order (Save to database or perform further actions)
                string Order_Number = SaveOrderToDatabase(paymentMethod, totalAmount, fullAddress);

                if(Order_Number != "0")
                {
                    bool isOrderItemsSaved = SaveOrderItemsToDatabase(paymentMethod, totalAmount, cartItemList , Order_Number);
                    

                    if (isOrderItemsSaved)
                    {
                        bool isUpdateItemsStock = UpdateItemsStockDatabase(paymentMethod, totalAmount, cartItemList);

                        if (isUpdateItemsStock)
                        {
                            bool empty_Cart = DeleteItemsFromCartDatabase(paymentMethod, totalAmount, cartItemList);

                            if (empty_Cart)
                            {
                                return Json(new { success = false, message = "Cart Empty Failed" });
                            }
                            else
                            {
                                return Json(new { success = true, message = "Order placed successfully!" });
                            }
                            
                        }
                        else
                        {
                            return Json(new { success = false, message = "Stocks Update Failed" });
                        }                      
                    }
                    else
                    {
                        return Json(new { success = false, message = "Order Item Saving Error" });
                    }
                }
                else
                {
                    return Json(new { success = false, message = "Order Saving Error" });
                }

            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }


        private bool SaveOrderItemsToDatabase(string paymentMethod, decimal totalAmount, List<OrderItem> cartItems, string Order_ID)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(constring))
                {
                    con.Open();

                    string query = @"
                INSERT INTO Order_Item (Order_ID, ItemID, ItemName, Quantity, Unit_price) 
                VALUES (@Order_ID, @ItemID, @ItemName, @Quantity, @Unit_price);";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        foreach (var item in cartItems)
                        {
                            cmd.Parameters.Clear();  

                            cmd.Parameters.AddWithValue("@Order_ID", Order_ID);
                            cmd.Parameters.AddWithValue("@ItemID", item.itemID);
                            cmd.Parameters.AddWithValue("@ItemName", item.Name);
                            cmd.Parameters.AddWithValue("@Quantity", item.Quantity);
                            cmd.Parameters.AddWithValue("@Unit_price", item.UnitPrice);

                            cmd.ExecuteNonQuery();  
                        }
                    }
                }

                return true;  
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error saving order items: " + ex.Message);
                return false;  
            }
        }

        private string SaveOrderToDatabase(string paymentMethod, decimal totalAmount, string fullAddress)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(constring))
                {
                    con.Open();

                    using (SqlCommand cmd = new SqlCommand(SaveOrderToDatabase_Query, con))
                    {
                        cmd.Parameters.AddWithValue("@UserId", Session["UserId"].ToString());
                        cmd.Parameters.AddWithValue("@Address", fullAddress);
                        cmd.Parameters.AddWithValue("@Status", "Pending Shipment");
                        cmd.Parameters.AddWithValue("@Date", DateTime.Now);
                        cmd.Parameters.AddWithValue("@Sub_Total", totalAmount);
                        cmd.Parameters.AddWithValue("@Paymet_Type", paymentMethod);
                        cmd.Parameters.AddWithValue("@Tracking_Number", "Pending");

                        // Retrieve the new Order_ID
                        object result = cmd.ExecuteScalar();
                        int newOrderID = Convert.ToInt32(result);

                        return newOrderID.ToString();

                    }

                }
            }
            catch (Exception e03)
            {
                return "0";
            }
        }

        private bool UpdateItemsStockDatabase(string paymentMethod, decimal totalAmount, List<OrderItem> cartItems)
        {
            try
            {
                using (SqlConnection con = new SqlConnection(constring))
                {
                    con.Open();

                    string query = @"
                                    UPDATE Items 
                                    SET ItemQuantity = ItemQuantity - @Quantity 
                                    WHERE ItemID = @ItemID;";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        foreach (var item in cartItems)
                        {
                            cmd.Parameters.Clear();  

                            cmd.Parameters.AddWithValue("@ItemID", item.itemID);
                            cmd.Parameters.AddWithValue("@Quantity", item.Quantity);

                            cmd.ExecuteNonQuery();  
                        }
                    }
                }

                return true;  
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error updating item stock: " + ex.Message);
                return false;  
            }
        }

        private bool DeleteItemsFromCartDatabase(string paymentMethod, decimal totalAmount, List<OrderItem> cartItems)
        {
            try
            {
                if (Session["UserId"] == null || string.IsNullOrEmpty(Session["UserId"].ToString()))
                {
                    Console.WriteLine("❌ Error: User is not logged in.");
                    return false;
                }

                using (SqlConnection con = new SqlConnection(constring))
                {
                    con.Open();

                    string query = @"DELETE FROM Cart_Item WHERE ItemID = @ItemID AND UserId = @UserId;";

                    using (SqlCommand cmd = new SqlCommand(query, con))
                    {
                        foreach (var item in cartItems)
                        {
                            cmd.Parameters.Clear(); // Clear previous parameters

                            cmd.Parameters.AddWithValue("@ItemID", item.itemID);
                            cmd.Parameters.AddWithValue("@UserId", Session["UserId"].ToString());

                            cmd.ExecuteNonQuery(); // Execute deletion
                        }
                    }
                }

                return true; // 
            }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Error deleting items from cart: " + ex.Message);
                return false;
            }
        }


        //public JsonResult GetAll_Products_Details(string productId)
        //{
        //    List<object> products = new List<object>();

        //    using (SqlConnection con = new SqlConnection(constring))
        //    {
        //        con.Open();
        //        string query = "SELECT ID,ItemID, ItemName, ItemQuantity, Item_Price, Item_Description,Item_Stock_limit, Item_Image1, Item_Image2, Item_Image3 FROM Items Where ItemID =@ItemID";

        //        using (SqlCommand cmd = new SqlCommand(query, con))
        //        {
        //            cmd.Parameters.AddWithValue("@ItemID", productId);

        //            using (SqlDataReader reader = cmd.ExecuteReader())
        //            {
        //                while (reader.Read())
        //                {
        //                    products.Add(new
        //                    {
        //                        ID = Convert.ToInt32(reader["ID"]),
        //                        ItemID = reader["ItemID"].ToString(),
        //                        ItemName = reader["ItemName"].ToString(),
        //                        ItemQuantity = reader["ItemQuantity"].ToString(),
        //                        Item_Price = reader["Item_Price"].ToString(),
        //                        Item_Description = reader["Item_Description"].ToString(),
        //                        Item_Stock_limit = reader["Item_Stock_limit"].ToString(),
        //                        Item_Image1 = reader["Item_Image1"] != DBNull.Value ? "data:image/jpeg;base64," + Convert.ToBase64String((byte[])reader["Item_Image1"]) : "",
        //                        Item_Image2 = reader["Item_Image2"] != DBNull.Value ? "data:image/jpeg;base64," + Convert.ToBase64String((byte[])reader["Item_Image2"]) : "",
        //                        Item_Image3 = reader["Item_Image3"] != DBNull.Value ? "data:image/jpeg;base64," + Convert.ToBase64String((byte[])reader["Item_Image3"]) : ""
        //                    });
        //                }
        //            }
        //        }
        //    }

        //    JsonResult result = Json(products, JsonRequestBehavior.AllowGet);
        //    result.MaxJsonLength = int.MaxValue; // Allows a very large JSON size
        //    return result;
        //}

        public JsonResult GetAll_Products_Details(int productId)
        {
            Item product = null;
            List<Review> reviews = new List<Review>();

            using (SqlConnection con = new SqlConnection(constring))
            {
                con.Open();

                // Fetch product details
                using (SqlCommand cmd = new SqlCommand("SELECT ID,ItemID, ItemName, ItemQuantity, Item_Price, Item_Description,Item_Stock_limit, Item_Image1, Item_Image2, Item_Image3 FROM Items Where ItemID =@ItemID", con))
                {
                    cmd.Parameters.AddWithValue("@ItemID", productId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            product = new Item
                            {
                                ItemID = reader["ItemID"].ToString(),
                                ItemName = reader["ItemName"].ToString(),
                                ItemQuantity = reader["ItemQuantity"].ToString(),
                                Item_Price = reader["Item_Price"].ToString(),
                                Item_Description = reader["Item_Description"].ToString(),
                                Item_Stock_limit = Convert.ToInt32(reader["Item_Stock_limit"]),

                                // Read images as byte array (NULL check handled)
                                Item_Image1 = reader["Item_Image1"] != DBNull.Value ? (byte[])reader["Item_Image1"] : null,
                                Item_Image2 = reader["Item_Image2"] != DBNull.Value ? (byte[])reader["Item_Image2"] : null,
                                Item_Image3 = reader["Item_Image3"] != DBNull.Value ? (byte[])reader["Item_Image3"] : null
                            };
                        }
                    }
                }

                // Fetch product reviews
                using (SqlCommand cmd = new SqlCommand("SELECT Rating, Comment FROM Review WHERE ItemID = @ItemID", con))
                {
                    cmd.Parameters.AddWithValue("@ItemID", productId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            reviews.Add(new Review
                            {
                                Rating = Convert.ToInt32(reader["Rating"]),
                                Comment = reader["Comment"].ToString()
                            });
                        }
                    }
                }
            }

            if (product != null)
            {
                product.Ratings = reviews;
            }

            // Creating the JSON response
            var jsonData = new
            {
                product.ItemID,
                product.ItemName,
                product.ItemQuantity,
                product.Item_Price,
                product.Item_Description,
                product.Item_Stock_limit,

                // Convert byte array to Base64-encoded image
                Item_Image1 = product.Item_Image1 != null ? "data:image/jpeg;base64," + Convert.ToBase64String(product.Item_Image1) : "",
                Item_Image2 = product.Item_Image2 != null ? "data:image/jpeg;base64," + Convert.ToBase64String(product.Item_Image2) : "",
                Item_Image3 = product.Item_Image3 != null ? "data:image/jpeg;base64," + Convert.ToBase64String(product.Item_Image3) : "",

                Ratings = product.Ratings
            };

            // Return JSON with extended MaxJsonLength
            JsonResult result = Json(jsonData, JsonRequestBehavior.AllowGet);
            result.MaxJsonLength = int.MaxValue; // Allows large JSON responses
            return result;
        }


        #region UserProfile

        [HttpGet]
        public JsonResult GetUserProfile()
        {
            try
            {
                string userId = Session["UserId"]?.ToString();
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "User not found." }, JsonRequestBehavior.AllowGet);
                }

                User user = GetUserById(userId);
                return Json(new { success = true, user }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        // 🚀 Update User Profile via AJAX
        [HttpPost]
        public JsonResult UpdateUserProfile(User user)
        {
            try
            {
                // Check if email is already taken by another user
                if (IsEmailExists(user.Email, user.UserId))
                {
                    return Json(new { success = false, message = "Email already exists!" });
                }

                using (SqlConnection con = new SqlConnection(constring))
                {
                    con.Open();
                    SqlCommand cmd = new SqlCommand(@"UPDATE Login 
                        SET LastName = @LastName,FirstName=@FirstName, [Email] = @Email, StreetAddress = @StreetAddress, City=@City ,PostalCode =@PostalCode Phone = @Phone 
                        WHERE UserId = @UserId", con);

                    cmd.Parameters.AddWithValue("@UserId", user.UserId);
                    cmd.Parameters.AddWithValue("@Name", user.Name);
                    cmd.Parameters.AddWithValue("@FirstName", user.FirstName);
                    cmd.Parameters.AddWithValue("@LastName", user.LastName);
                    cmd.Parameters.AddWithValue("@Email", user.Email);
                    cmd.Parameters.AddWithValue("@StreetAddress", user.StreetAddress);
                    cmd.Parameters.AddWithValue("@City", user.City);
                    cmd.Parameters.AddWithValue("@PostalCode", user.PostalCode);
                    cmd.Parameters.AddWithValue("@Phone", user.Phone);

                    cmd.ExecuteNonQuery();
                }
                return Json(new { success = true, message = "Profile updated successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // 🚀 Helper Method: Get User by ID
        private User GetUserById(string userId)
        {
            User user = new User();
            using (SqlConnection con = new SqlConnection(constring))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("SELECT * FROM Login WHERE UserId = @UserId", con);
                cmd.Parameters.AddWithValue("@UserId", userId);
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    user.UserId = reader["UserId"].ToString();
                    user.Username = reader["Username"].ToString();
                    user.Password = reader["Password"].ToString();
                    user.FirstName = reader["FirstName"].ToString();
                    user.LastName = reader["LastName"].ToString();
                    user.Email = reader["Email"].ToString();
                    user.StreetAddress = reader["StreetAddress"].ToString();
                    user.City = reader["City"].ToString();
                    user.PostalCode = reader["PostalCode"].ToString();
                    user.Phone = reader["Phone"].ToString();
                }
            }
            return user;
        }

        // 🚀 Helper Method: Check if Email Already Exists
        private bool IsEmailExists(string email, string userId)
        {
            using (SqlConnection con = new SqlConnection(constring))
            {
                con.Open();
                SqlCommand cmd = new SqlCommand("SELECT COUNT(*) FROM Login WHERE Email = @Email AND UserId <> @UserId", con);
                cmd.Parameters.AddWithValue("@Email", email);
                cmd.Parameters.AddWithValue("@UserId", userId);
                int count = (int)cmd.ExecuteScalar();
                return count > 0;
            }
        }

        // 🚀 Change Password
        [HttpPost]
        public JsonResult ChangePassword(string CurrentPassword, string NewPassword)
        {
            try
            {
                string userId = Session["UserId"]?.ToString();
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { success = false, message = "User not found." });
                }

                using (SqlConnection con = new SqlConnection(constring))
                {
                    con.Open();

                    // Check Current Password
                    SqlCommand checkCmd = new SqlCommand("SELECT Password FROM Login WHERE UserId = @UserId", con);
                    checkCmd.Parameters.AddWithValue("@UserId", userId);
                    string dbPassword = (string)checkCmd.ExecuteScalar();

                    if (dbPassword != CurrentPassword)
                    {
                        return Json(new { success = false, message = "Current password is incorrect." });
                    }

                    // Update New Password
                    SqlCommand updateCmd = new SqlCommand("UPDATE Login SET Password = @NewPassword WHERE UserId = @UserId", con);
                    updateCmd.Parameters.AddWithValue("@NewPassword", NewPassword);
                    updateCmd.Parameters.AddWithValue("@UserId", userId);
                    updateCmd.ExecuteNonQuery();
                }

                return Json(new { success = true, message = "Password changed successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        #endregion


        #region RegisterUser

        [HttpPost]
        public JsonResult RegisterUser(string username, string firstName, string lastName, string email, string password, string address, string city, string postalCode, string phone)
        {
            string message = "Registration failed!";
            bool success = false;

            try
            {
                using (SqlConnection con = new SqlConnection(constring))
                {
                    string query = "INSERT INTO Login (Username, Name, FirstName, LastName, Email, Password, StreetAddress, City, PostalCode, Phone ,Role) VALUES (@Username, @Name, @FirstName, @LastName, @Email, @Password, @StreetAddress, @City, @PostalCode, @Phone ,@Role)";
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.Parameters.AddWithValue("@Username", username);
                    cmd.Parameters.AddWithValue("@Name", firstName +" " + lastName);
                    cmd.Parameters.AddWithValue("@FirstName", firstName);
                    cmd.Parameters.AddWithValue("@LastName", lastName);
                    cmd.Parameters.AddWithValue("@Email", email);
                    cmd.Parameters.AddWithValue("@Password", password);
                    cmd.Parameters.AddWithValue("@StreetAddress", address);
                    cmd.Parameters.AddWithValue("@City", city);
                    cmd.Parameters.AddWithValue("@PostalCode", postalCode);
                    cmd.Parameters.AddWithValue("@Phone", phone);
                    cmd.Parameters.AddWithValue("@Role", "1");

                    con.Open();
                    int rowsAffected = cmd.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        success = true;
                        message = "Registration successful!";
                    }
                }
            }
            catch (Exception ex)
            {
                message = "Error: " + ex.Message;
            }

            return Json(new { success, message });
        }

        // AJAX: Check if Email is already registered
        [HttpPost]
        public JsonResult CheckEmail(string email)
        {
            bool exists = false;

            using (SqlConnection con = new SqlConnection(constring))
            {
                string query = "SELECT COUNT(1) FROM Login WHERE Email = @Email"; 
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.Parameters.AddWithValue("@Email", email);
                con.Open();
                exists = (int)cmd.ExecuteScalar() > 0;
            }

            return Json(new { isAvailable = !exists });
        }

        #endregion

        public JsonResult GetAll_Customer_Orders()
        {
            List<object> Orders = new List<object>();

            using (SqlConnection con = new SqlConnection(constring))
            {
                con.Open();
                string query = "SELECT * FROM [Order] WHERE UserId = @UserId";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@UserId", Session["UserId"].ToString());
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

        public JsonResult GetOrderItems(int orderId)
        {
            List<object> OrderItems = new List<object>();

            using (SqlConnection con = new SqlConnection(constring))
            {
                con.Open();
                string query = "SELECT * FROM Order_Item WHERE Order_ID = @Order_ID";

                using (SqlCommand cmd = new SqlCommand(query, con))
                {
                    cmd.Parameters.AddWithValue("@Order_ID", orderId);
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            OrderItems.Add(new
                            {
                                ItemName = reader["ItemName"].ToString(),
                                Unit_price = Convert.ToDouble(reader["Unit_price"]),
                                Quantity = Convert.ToInt32(reader["Quantity"]),
                                ItemID = reader["ItemID"].ToString()
                            });
                        }
                    }
                }
            }

            return Json(OrderItems, JsonRequestBehavior.AllowGet);
        }
    }
}