using System;

namespace ModernBoxes.Core.Interfaces
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public class CardExportAttribute : Attribute
    {
        public string CardName { get; }
        public string? Author { get; set; }
        public string? Version { get; set; }
        public string? Description { get; set; }

        public CardExportAttribute(string cardName)
        {
            CardName = cardName;
        }
    }
}
