using FluentAssertions;
using ModernBoxes.Infrastructure.Plugins;
using Xunit;

namespace ModernBoxes.Tests.Plugins;

[Trait("Category", "Plugin")]
public class PluginManifestReaderTests
{
    [Fact]
    public void TryRead_ShouldParseValidManifest()
    {
        var path = Path.Combine(Path.GetTempPath(), $"muhan-plugin-{Guid.NewGuid():N}.json");
        File.WriteAllText(path, """
            {
              "id": "demo",
              "name": "Demo",
              "main": "Demo.dll",
              "minHostApiVersion": 1
            }
            """);

        try
        {
            var manifest = PluginManifestReader.TryRead(path);

            manifest.Should().NotBeNull();
            manifest!.Id.Should().Be("demo");
            manifest.Name.Should().Be("Demo");
            manifest.Main.Should().Be("Demo.dll");
            manifest.MinHostApiVersion.Should().Be(1);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void TryRead_ShouldReturnNull_WhenMainMissing()
    {
        var path = Path.Combine(Path.GetTempPath(), $"muhan-plugin-{Guid.NewGuid():N}.json");
        File.WriteAllText(path, """{ "id": "demo" }""");

        try
        {
            PluginManifestReader.TryRead(path).Should().BeNull();
        }
        finally
        {
            File.Delete(path);
        }
    }
}
