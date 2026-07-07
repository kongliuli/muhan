namespace ModernBoxes.Infrastructure.Ai
{
    public sealed class AiSettings
    {
        public string ActiveProfile { get; set; } = "default";

        public List<AiProfile> Profiles { get; set; } = [new AiProfile()];
    }
}
