using FluentAssertions;
using ModernBoxes.Infrastructure.Ai;
using Xunit;

namespace ModernBoxes.Tests.Ai;

public class AiSecretStoreTests : IDisposable
{
    private readonly string _tempDir;

    public AiSecretStoreTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "muhan-ai-test-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
        AiSecretStore.SecretsDirOverride = _tempDir;
    }

    public void Dispose()
    {
        AiSecretStore.SecretsDirOverride = null;
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    [Fact]
    public void SaveAndLoad_RoundTripsApiKey()
    {
        AiSecretStore.SaveApiKey("default", "sk-test-key-12345");
        AiSecretStore.LoadApiKey("default").Should().Be("sk-test-key-12345");
    }

    [Fact]
    public void Load_MissingProfile_ReturnsNull()
    {
        AiSecretStore.LoadApiKey("nonexistent").Should().BeNull();
    }

    [Fact]
    public void Delete_RemovesKey()
    {
        AiSecretStore.SaveApiKey("temp", "key");
        AiSecretStore.DeleteApiKey("temp");
        AiSecretStore.LoadApiKey("temp").Should().BeNull();
    }
}
