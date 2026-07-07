namespace ModernBoxes.Infrastructure.Ai
{
    public sealed class AiProfile
    {
        public string Name { get; set; } = "default";

        public string Endpoint { get; set; } = "https://api.openai.com/v1";

        public string Model { get; set; } = "gpt-4o-mini";
    }
}
