using ElectronicHub.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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

        private readonly string openAiApiKey = "";  // Replace with your OpenAI API key

        [HttpPost]
        public async Task<JsonResult> GetResponse(ChatRequest request)
        {
            if (string.IsNullOrEmpty(request.Message))
                return Json("Please enter a message.");

            string aiResponse = await GetChatGPTResponse(request.Message);
            return Json(aiResponse);
        }

        private async Task<string> GetChatGPTResponse(string userMessage)
        {
            string openAiEndpoint = "https://api.openai.com/v1/chat/completions";

            var requestBody = new
            {
                model = "gpt-4", // Use GPT-4 or another available model
                messages = new[] { new { role = "user", content = userMessage } },
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

    }
}