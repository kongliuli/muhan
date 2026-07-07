using FluentAssertions;
using ModernBoxes.Infrastructure;
using Xunit;

namespace ModernBoxes.Tests.Infrastructure;

public class AppPathsPortableTests : IDisposable
{
    private readonly string _tempDir;
    private readonly bool? _previousPortable;

    public AppPathsPortableTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), "muhan-portable-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_tempDir);
        _previousPortable = AppPaths.PortableModeOverride;
    }

    public void Dispose()
    {
        AppPaths.PortableModeOverride = _previousPortable;
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    [Fact]
    public void IsPortableMode_TrueWhenOverride()
    {
        AppPaths.PortableModeOverride = true;
        AppPaths.IsPortableMode.Should().BeTrue();
        AppPaths.UpdatesDisabled.Should().BeTrue();
    }

    [Fact]
    public void Root_UsesDataSubdirInPortableMode()
    {
        AppPaths.PortableModeOverride = true;
        AppPaths.Root.Should().EndWith(Path.Combine("Data"));
    }
}
