namespace ModernBoxes.Infrastructure.Deeplink
{
    public sealed class DeeplinkContext
    {
        public DeeplinkContext(Uri uri) => Uri = uri;

        public Uri Uri { get; }

        public string Command => Uri.Host;

        public string Path => Uri.AbsolutePath.TrimStart('/');

        public string GetArgument()
        {
            var q = GetQuery("q");
            if (!string.IsNullOrEmpty(q))
                return q;

            if (!string.IsNullOrEmpty(Path))
                return Uri.UnescapeDataString(Path.Replace('/', ' ').Trim());

            return string.Empty;
        }

        public string? GetQuery(string name)
        {
            if (string.IsNullOrEmpty(Uri.Query))
                return null;

            var query = Uri.Query.TrimStart('?');
            foreach (var part in query.Split('&', StringSplitOptions.RemoveEmptyEntries))
            {
                var kv = part.Split('=', 2);
                if (kv.Length == 2 &&
                    kv[0].Equals(name, StringComparison.OrdinalIgnoreCase))
                    return Uri.UnescapeDataString(kv[1]);
            }

            return null;
        }
    }
}
