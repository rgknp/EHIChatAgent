using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Text.Json;
using Markdig;


namespace EHIChatAgent.Pages
{
    public class ChatModel : PageModel
    {
        //private readonly OpenAIRequest _openAIRequest;
        private readonly OpenAIChat _openAIChat;
        // A static list to store chat messages during the session.
        public static List<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();

        //public ChatModel(OpenAIRequest openAIRequest)
        //{
        //    _openAIRequest = openAIRequest;
        //}

        public ChatModel(OpenAIChat openAiChat)
        {
            _openAIChat = openAiChat;
        }

        [BindProperty]
        public string Message { get; set; }

        public void OnGet()
        {
            // Retrieve the chat history from the session if it exists.
            var chatData = HttpContext.Session.GetString("ChatMessages");
            if (chatData != null)
            {
                ChatMessages = JsonSerializer.Deserialize<List<ChatMessage>>(chatData);
            }
        }

        public IActionResult OnPost()
        {
            // Retrieve existing messages from the session.
            var chatData = HttpContext.Session.GetString("ChatMessages");
            if (chatData != null)
            {
                ChatMessages = JsonSerializer.Deserialize<List<ChatMessage>>(chatData);
            }

            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();

            // Add the new message to the list.
            if (!string.IsNullOrEmpty(Message))
            {
                string openAIResponseContent = _openAIChat.OpenAIRequestAsync(Message).Result.content;
                string openAIResponseIntent = _openAIChat.OpenAIRequestAsync(Message).Result.intent;

                ChatMessages.Add(new ChatMessage
                {
                    Sender = "You",
                    Content = Markdig.Markdown.ToHtml(openAIResponseContent, pipeline)
                });

                if (!string.IsNullOrEmpty(openAIResponseIntent))
                {
                    ChatMessages.Add(new ChatMessage
                    {
                        Sender = "Intent",
                        Content = Markdig.Markdown.ToHtml(openAIResponseIntent, pipeline)
                    });
                }

                if (_openAIChat.OpenAIRequestAsync(Message).Result.citations != null)
                {
                    _openAIChat.OpenAIRequestAsync(Message).Result.citations.ForEach(citation =>
                    {
                        ChatMessages.Add(new ChatMessage
                        {
                            Sender = "Citation",
                            Content = Markdig.Markdown.ToHtml(citation, pipeline)
                        });
                    });
                }

            }

            // Save the updated chat messages back to the session.
            HttpContext.Session.SetString("ChatMessages", JsonSerializer.Serialize(ChatMessages));

            // Redirect to avoid form resubmission issue
            return RedirectToPage();
        }
    }

    // Helper class to store each chat message.
    public class ChatMessage
    {
        public string Sender { get; set; }
        public string Content { get; set; }
    }
}
