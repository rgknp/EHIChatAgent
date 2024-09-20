using Newtonsoft.Json;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace EHIChatAgent
{
    public class OpenAIRequest
    {
        private const string API_KEY = "0ba307120c32453893d22d6debcd69cc"; // Set your key here

        private const string ENDPOINT = "https://hdi-openai-resource.openai.azure.com/openai/deployments/gpt-4/chat/completions?api-version=2024-02-15-preview";
        public static async Task<string> OpenAIRequestAsync(string question)
        {
            string responseDataString = null;
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Add("api-key", API_KEY);
                var payload = new
                {
                    messages = new object[]
                    {
                  new {
                      role = "system",
                      content = new object[] {
                          new {
                              type = "text",
                              text = "You are an AI assistant that helps people find information."
                          }
                      }
                  },
                  new {
                      role = "user",
                      content = new object[] {
                          new {
                              type = "text",
                              text = question
                          }
                      }
                  }
                    },
                    temperature = 0.7,
                    top_p = 0.95,
                    max_tokens = 800,
                    stream = false
                };

                var response = await httpClient.PostAsync(ENDPOINT, new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json"));

                if (response.IsSuccessStatusCode)
                {
                    var responseData = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());
                    responseDataString = responseData?.choices[0]?.message?.content.ToString();

                }
                else
                {
                    Console.WriteLine($"Error: {response.StatusCode}, {response.ReasonPhrase}");
                }
            }
            return responseDataString;
        }
    }
}
