using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace ModernBoxes.Infrastructure.Ai
{
    /// <summary>ApiKey 用 DPAPI（CurrentUser）加密存盘，按 profile 分文件。</summary>
    public static class AiSecretStore
    {
        internal static string? SecretsDirOverride { get; set; }

        private static string SecretsDir =>
            SecretsDirOverride ?? Path.Combine(AppPaths.Root, "ai-secrets");

        public static void SaveApiKey(string profileName, string apiKey)
        {
            if (string.IsNullOrWhiteSpace(profileName))
                throw new ArgumentException("profile name required", nameof(profileName));

            Directory.CreateDirectory(SecretsDir);
            var plain = Encoding.UTF8.GetBytes(apiKey);
            var protectedBytes = ProtectedData.Protect(plain, null, DataProtectionScope.CurrentUser);
            File.WriteAllBytes(SecretPath(profileName), protectedBytes);
        }

        public static string? LoadApiKey(string profileName)
        {
            if (string.IsNullOrWhiteSpace(profileName))
                return null;

            var path = SecretPath(profileName);
            if (!File.Exists(path))
                return null;

            try
            {
                var protectedBytes = File.ReadAllBytes(path);
                var plain = ProtectedData.Unprotect(protectedBytes, null, DataProtectionScope.CurrentUser);
                return Encoding.UTF8.GetString(plain);
            }
            catch
            {
                return null;
            }
        }

        public static void DeleteApiKey(string profileName)
        {
            var path = SecretPath(profileName);
            if (File.Exists(path))
                File.Delete(path);
        }

        private static string SecretPath(string profileName) =>
            Path.Combine(SecretsDir, $"{Sanitize(profileName)}.key");

        private static string Sanitize(string name)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
                name = name.Replace(c, '_');
            return name;
        }
    }
}
