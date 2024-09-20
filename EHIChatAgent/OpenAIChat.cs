using System;
using System.ClientModel;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure;
using Azure.AI.OpenAI;
using Azure.AI.OpenAI.Chat;
using OpenAI.Chat;
using static System.Environment;

namespace EHIChatAgent
{
    #pragma warning disable AOAI001
    public class OpenAIChat
    {
        private readonly string endpoint = "https://hdi-openai-resource.openai.azure.com/";
        private readonly string deploymentName = "gpt-35-turbo";
        private readonly string searchEndpoint = "https://ehisearchindex.search.windows.net";
        private readonly string searchIndex = "customschemaindex";
        private readonly string semanticConfiguration = "customschemasemantic";
        string openAiApiKey = "set your Azure OpenAI key here";
        private string searchResourceKey = "set your Azure Search API Key here";

        public async Task<ChatResponse> OpenAIRequestAsync(string question)
        {
            ChatResponse response = new ChatResponse();

            try
            {
                //AzureKeyCredential credential = new AzureKeyCredential(openAiApiKey);

                ApiKeyCredential credential = new ApiKeyCredential(openAiApiKey);

                AzureOpenAIClient azureClient = new AzureOpenAIClient(new Uri(endpoint), credential);
                //AzureOpenAIClient azureClient = new AzureOpenAIClient(new Uri(endpoint), new DefaultAzureCredential());
                
                ChatClient chatClient = azureClient.GetChatClient(deploymentName);

                var messages = new List<ChatMessage>
                    {
                        new SystemChatMessage("You are a helpful assistant in a health care professional setting. Use data source customschemaindex for the context."),
                        new UserChatMessage(question)
                    };

                ChatCompletionOptions options = new ChatCompletionOptions();
                options.AddDataSource(new AzureSearchChatDataSource()
                {
                    Endpoint = new Uri(searchEndpoint),
                    IndexName = searchIndex,
                    Authentication = DataSourceAuthentication.FromApiKey(searchResourceKey),
                    Strictness = 4,
                    TopNDocuments = 20,
                    SemanticConfiguration = semanticConfiguration,
                    QueryType = new DataSourceQueryType("semantic")
                });
                options.FrequencyPenalty = 0;
                options.PresencePenalty = 0;
                options.MaxTokens = 800;
                options.Temperature = (float?)0.7;
                options.TopP = (float?)0.95;

                ChatCompletion completion = await chatClient.CompleteChatAsync(messages, options);

                response.content = completion.Content[0].Text;

                AzureChatMessageContext onYourDataContext = completion.GetAzureMessageContext();
                if (onYourDataContext?.Intent is not null)
                {
                    response.intent = onYourDataContext.Intent;
                }
                foreach (AzureChatCitation citation in onYourDataContext?.Citations ?? new List<AzureChatCitation>())
                {
                    response.citations.Add(citation.Content);
                }
            }
            catch (RequestFailedException ex)
            {
                // Handle specific exceptions
                Console.WriteLine($"Request failed: {ex.Message}");
                response.content = "An error occurred while processing the request.";
            }
            catch (Exception ex)
            {
                // Handle general exceptions
                Console.WriteLine($"An error occurred: {ex.Message}");
                response.content = "An error occurred while processing the request.";
            }

        #pragma warning restore AOAI001 // Restore the diagnostic warning

            return response;
        }
    }

    public class ChatResponse
    {
        public string content { get; set; }
        public string intent { get; set; }
        public List<string> citations { get; set; }

        public ChatResponse()
        {
            citations = new List<string>();
        }
    }
}
