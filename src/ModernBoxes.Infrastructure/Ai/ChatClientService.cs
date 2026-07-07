using Microsoft.Extensions.AI;
using OpenAI;
using System.ClientModel;
using System.Collections.Concurrent;

namespace ModernBoxes.Infrastructure.Ai
{
    public sealed class ChatClientService
    {
        private readonly ConcurrentDictionary<string, IChatClient> _cache = new(StringComparer.OrdinalIgnoreCase);

        public IChatClient? GetClient(string? profileName = null)
        {
            var settings = AiSettingsStore.Load();
            var name = profileName ?? settings.ActiveProfile;
            var profile = settings.Profiles.Find(p =>
                string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
            if (profile == null)
                return null;

            var apiKey = AiSecretStore.LoadApiKey(profile.Name);
            if (string.IsNullOrWhiteSpace(apiKey))
                return null;

            return _cache.GetOrAdd(profile.Name, _ => CreateClient(profile, apiKey));
        }

        public void InvalidateCache() => _cache.Clear();

        private static IChatClient CreateClient(AiProfile profile, string apiKey)
        {
            var credential = new ApiKeyCredential(apiKey);
            OpenAI.Chat.ChatClient chat;
            if (string.IsNullOrWhiteSpace(profile.Endpoint) ||
                profile.Endpoint.Contains("api.openai.com", StringComparison.OrdinalIgnoreCase))
            {
                chat = new OpenAI.Chat.ChatClient(profile.Model, credential);
            }
            else
            {
                var options = new OpenAIClientOptions
                {
                    Endpoint = new Uri(profile.Endpoint.TrimEnd('/') + "/"),
                };
                chat = new OpenAI.Chat.ChatClient(profile.Model, credential, options);
            }

            return chat.AsIChatClient();
        }
    }
}
