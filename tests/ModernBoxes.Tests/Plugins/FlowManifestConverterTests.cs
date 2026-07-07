using FluentAssertions;
using ModernBoxes.Infrastructure.Plugins;
using System;
using System.IO;
using Xunit;

namespace ModernBoxes.Tests.Plugins;

[Trait("Category", "Plugin")]
public class FlowManifestConverterTests
{
    [Fact]
    public void TryFromFlowDirectory_ShouldConvertCSharpPlugin()
    {
        var dir = Path.Combine(Path.GetTempPath(), "muhan-flow-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dir);
        File.WriteAllText(Path.Combine(dir, "plugin.json"), """
            {
              "ID": "abc-123",
              "Name": "Demo",
              "Language": "csharp",
              "ActionKeyword": "demo"
            }
            """);
        File.WriteAllText(Path.Combine(dir, "Flow.Launcher.Plugin.Demo.dll"), "fake");

        try
        {
            var manifest = FlowManifestConverter.TryFromFlowDirectory(dir);

            manifest.Should().NotBeNull();
            manifest!.Id.Should().Be("abc-123");
            manifest.Type.Should().Be("wox");
            manifest.Main.Should().Be("Flow.Launcher.Plugin.Demo.dll");
            manifest.ActionKeyword.Should().Be("demo");
        }
        finally
        {
            Directory.Delete(dir, recursive: true);
        }
    }

    [Fact]
    public void SanitizeFolderName_ShouldStripInvalidChars()
    {
        FlowManifestConverter.SanitizeFolderName("A/B:C*123").Should().Be("abc123");
    }
}
