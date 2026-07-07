namespace ModernBoxes.Sdk.Search
{
    /// <summary>用户输入查询，对齐 Wox/Flow Launcher Query 语义。</summary>
    public sealed class Query
    {
        public Query(string raw)
        {
            Raw = raw ?? string.Empty;
            var trimmed = Raw.TrimStart();
            var space = trimmed.IndexOf(' ');
            if (space > 0)
            {
                ActionKeyword = trimmed.Substring(0, space);
                Search = trimmed.Substring(space + 1).TrimStart();
            }
            else
            {
                ActionKeyword = string.Empty;
                Search = trimmed;
            }
        }

        public string Raw { get; }
        public string ActionKeyword { get; }
        public string Search { get; }

        public bool MatchesPlugin(string pluginKeyword)
        {
            if (string.IsNullOrEmpty(pluginKeyword) || pluginKeyword == "*")
                return true;
            return string.Equals(ActionKeyword, pluginKeyword, System.StringComparison.OrdinalIgnoreCase);
        }
    }
}
