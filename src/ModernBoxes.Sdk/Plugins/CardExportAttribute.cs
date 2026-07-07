using System;

namespace ModernBoxes.Sdk.Plugins
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class CardExportAttribute : Attribute
    {
        public string CardName { get; }
        public string? Author { get; set; }
        public string? Version { get; set; }
        public string? Description { get; set; }
        public Type? ViewType { get; set; }
        public int MinHostApiVersion { get; set; } = 1;

        public CardExportAttribute(string cardName)
        {
            CardName = cardName;
        }
    }
}
