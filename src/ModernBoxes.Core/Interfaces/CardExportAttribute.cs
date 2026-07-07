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

        /// <summary>插件卡片对应的 WPF 视图类型（需继承 FrameworkElement）。</summary>
        public Type? ViewType { get; set; }

        /// <summary>插件要求的最低宿主 API 版本。</summary>
        public int MinHostApiVersion { get; set; } = 1;

        public CardExportAttribute(string cardName)
        {
            CardName = cardName;
        }
    }
}
