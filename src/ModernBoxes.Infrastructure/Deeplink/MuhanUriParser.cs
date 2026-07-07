namespace ModernBoxes.Infrastructure.Deeplink
{
    public static class MuhanUriParser
    {
        public static bool TryParse(string? raw, out Uri uri)
        {
            uri = null!;
            if (string.IsNullOrWhiteSpace(raw))
                return false;

            raw = raw.Trim().Trim('"');
            if (!raw.StartsWith("muhan://", StringComparison.OrdinalIgnoreCase))
                return false;

            if (!Uri.TryCreate(raw, UriKind.Absolute, out var parsed))
                return false;

            if (!parsed.Scheme.Equals("muhan", StringComparison.OrdinalIgnoreCase))
                return false;

            if (string.IsNullOrEmpty(parsed.Host))
                return false;

            uri = parsed;
            return true;
        }

        public static IEnumerable<Uri> ParseAll(IEnumerable<string> args)
        {
            foreach (var arg in args)
            {
                if (TryParse(arg, out var uri))
                    yield return uri;
            }
        }
    }
}
