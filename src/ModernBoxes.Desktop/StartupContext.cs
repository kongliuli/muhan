namespace ModernBoxes.Desktop
{
    public static class StartupContext
    {
        public static string[] Args { get; set; } = [];

        public static Infrastructure.Platform.SingleInstanceGate? Gate { get; set; }
    }
}
