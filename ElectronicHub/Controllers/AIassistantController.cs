using ElectronicHub.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace ElectronicHub.Controllers
{
    public class AIassistantController : Controller
    {
        // GET: AIassistant
        public ActionResult Index()
        {
            return View();
        }

        //private readonly string openAiApiKey = "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";  // Replace with your OpenAI API key

        //[HttpPost]
        //public async Task<JsonResult> GetResponse(ChatRequest request)
        //{
        //    if (string.IsNullOrEmpty(request.Message))
        //        return Json("Please enter a message.");

        //    string aiResponse = await GetChatGPTResponse(request.Message);
        //    return Json(aiResponse);
        //}

        //private async Task<string> GetChatGPTResponse(string userMessage)
        //{
        //    string openAiEndpoint = "https://api.openai.com/v1/chat/completions";

        //    var requestBody = new
        //    {
        //        model = "gpt-4", // Use GPT-4 or another available model
        //        messages = new[] { new { role = "user", content = userMessage } },
        //        temperature = 0.7
        //    };

        //    using (HttpClient client = new HttpClient())
        //    {
        //        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {openAiApiKey}");
        //        var jsonContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
        //        HttpResponseMessage response = await client.PostAsync(openAiEndpoint, jsonContent);

        //        if (response.IsSuccessStatusCode)
        //        {
        //            var responseJson = await response.Content.ReadAsStringAsync();
        //            dynamic responseData = JsonConvert.DeserializeObject(responseJson);
        //            return responseData.choices[0].message.content;
        //        }
        //        else
        //        {
        //            return "Error: Unable to fetch AI response.";
        //        }
        //    }
        //}


        //New Code 

        private readonly string openAiApiKey = System.Configuration.ConfigurationManager.AppSettings["OpenAiApiKey"];
        public static string connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

        [HttpPost]
        public async Task<JsonResult> GetResponse(ChatRequest request)
        {
            if (string.IsNullOrEmpty(request.Message))
                return Json("Please enter a message.");

            // Fetch store details from database
            string storeDetails = GetStoreDetails();
            string productDetails = GetProductDetails();

            // Get AI Response
            string aiResponse = await GetChatGPTResponse(request.Message, productDetails, storeDetails);

            // Save chat history in database
            SaveChatHistory(request.Message, aiResponse);

            return Json(aiResponse);
        }

        private async Task<string> GetChatGPTResponse(string userMessage, string productDetails, string storeDetails)
        {
            string openAiEndpoint = "https://api.openai.com/v1/chat/completions";

            // Determine context dynamically
            string context;
            if (IsProductRelated(userMessage))
            {
                context = "You are a store assistant. Here are the available products:\n" + productDetails;
            }
            else
            {
                context = "You are a store assistant. Here is the store information:\n" + storeDetails;
            }

            var requestBody = new
            {
                model = "gpt-4",
                messages = new[]
                {
                    new { role = "system", content = "You are a store assistant. Available products:\n" + storeDetails },
                    new { role = "user", content = userMessage }
                },
                temperature = 0.7
            };

            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {openAiApiKey}");
                var jsonContent = new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json");
                HttpResponseMessage response = await client.PostAsync(openAiEndpoint, jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    dynamic responseData = JsonConvert.DeserializeObject(responseJson);
                    return responseData.choices[0].message.content;
                }
                else
                {
                    return "Error: Unable to fetch AI response.";
                }
            }
        }

        private string GetProductDetails()
        {
            StringBuilder productData = new StringBuilder();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "SELECT ItemName as Name, Item_Price as Price, ItemQuantity as Quantity FROM Items";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            productData.AppendLine("Available products:");
                            while (reader.Read())
                            {
                                productData.AppendLine($"- {reader["Name"]}: Rs. {reader["Price"]} (Stock: {reader["Quantity"]})");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return "Error fetching product data: " + ex.Message;
            }

            return productData.ToString();
        }

        private string GetStoreDetails()
        {
            StringBuilder storeData = new StringBuilder();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    string query = "SELECT StoreName, Location, CourierCharges, PaymentOptions, DeliveryPeriod FROM StoreInfo";
                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        conn.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                storeData.AppendLine($"Store Name: {reader["StoreName"]}");
                                storeData.AppendLine($"Location: {reader["Location"]}");
                                storeData.AppendLine($"Courier Charges: {reader["CourierCharges"]}");
                                storeData.AppendLine($"Payment Options: {reader["PaymentOptions"]}");
                                storeData.AppendLine($"Delivery Period: {reader["DeliveryPeriod"]}");
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return "Error fetching store details: " + ex.Message;
            }

            return storeData.ToString();
        }

        private void SaveChatHistory(string userMessage, string aiResponse)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "INSERT INTO AI_Assistant (UserMessage, AIResponse ) VALUES (@UserMessage, @AIResponse )";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UserMessage", userMessage);
                    cmd.Parameters.AddWithValue("@AIResponse", aiResponse);
                    //cmd.Parameters.AddWithValue("@UserId", Session["username"].ToString());
                    conn.Open();
                    cmd.ExecuteNonQuery();
                }
            }
        }

        public JsonResult GetPreviousChats()
        {
            List<object> chatHistory = new List<object>();

            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                string query = "SELECT UserMessage, AIResponse, Timestamp FROM AI_Assistant ORDER BY Timestamp DESC";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    //cmd.Parameters.AddWithValue("@UserId", Session["username"].ToString());

                    conn.Open();
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            chatHistory.Add(new
                            {
                                UserMessage = reader["UserMessage"].ToString(),
                                AIResponse = reader["AIResponse"].ToString(),
                                Timestamp = reader["Timestamp"].ToString()
                            });
                        }
                    }
                }
            }

            return Json(chatHistory, JsonRequestBehavior.AllowGet);
        }

        private bool IsProductRelated(string message)
        {
            // Check if the message contains keywords related to products
            string[] productKeywords = { "price", "stock", "buy", "available", "cost", "product", "sell" };
            foreach (var keyword in productKeywords)
            {
                if (message.ToLower().Contains(keyword))
                {
                    return true;
                }
            }
            return false;
        }

    }
}