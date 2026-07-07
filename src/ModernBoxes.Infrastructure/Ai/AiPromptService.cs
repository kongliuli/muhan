using Microsoft.Extensions.AI;
using ModernBoxes.Infrastructure;

namespace ModernBoxes.Infrastructure.Ai
{
    public sealed class AiPromptService
    {
        private readonly ChatClientService _chat;

        public AiPromptService(ChatClientService chat)
        {
            _chat = chat;
        }

        public bool IsAvailable => _chat.GetClient() != null;

        public Task<string?> PolishNoteAsync(string content, CancellationToken cancellationToken = default) =>
            CompleteAsync(
                "你是便签助手。润色用户便签：保持原意，修正错别字和语病，简洁清晰。只输出润色后的正文，不要解释。",
                content,
                cancellationToken);

        public Task<string?> SummarizeNoteAsync(string content, CancellationToken cancellationToken = default) =>
            CompleteAsync(
                "你是便签助手。用一两句话总结便签要点。只输出总结，不要解释。",
                content,
                cancellationToken);

        public Task<string?> AnswerSearchAsync(string query, CancellationToken cancellationToken = default) =>
            CompleteAsync(
                "你是木函桌面助手的搜索兜底。用户未在本地找到结果，请简明回答问题。",
                query,
                cancellationToken);

        public Task<string?> TranslateAsync(string text, CancellationToken cancellationToken = default) =>
            CompleteAsync(
                "你是翻译助手。将用户文本翻译为简体中文（若已是中文则译为英文）。只输出译文，不要解释。",
                text,
                cancellationToken);

        private async Task<string?> CompleteAsync(
            string systemPrompt,
            string userPrompt,
            CancellationToken cancellationToken)
        {
            var client = _chat.GetClient();
            if (client == null || string.IsNullOrWhiteSpace(userPrompt))
                return null;

            try
            {
                var messages = new List<ChatMessage>
                {
                    new(ChatRole.System, systemPrompt),
                    new(ChatRole.User, userPrompt),
                };
                var response = await client
                    .GetResponseAsync(messages, cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
                var text = response.Text?.Trim();
                return string.IsNullOrEmpty(text) ? null : text;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "AI completion failed");
                return null;
            }
        }
    }
}
