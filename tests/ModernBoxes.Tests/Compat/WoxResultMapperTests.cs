using FluentAssertions;
using ModernBoxes.Infrastructure.Compat;
using ModernBoxes.Sdk.Search;
using System.Reflection;
using Xunit;

namespace ModernBoxes.Tests.Compat;

[Trait("Category", "Plugin")]
public class WoxResultMapperTests
{
    private sealed class FakeWoxResult
    {
        public string Title { get; set; } = "Hello";
        public string SubTitle { get; set; } = "World";
        public string IcoPath { get; set; } = "icon.png";
        public int Score { get; set; } = 88;
        public Func<bool> Action { get; set; } = () => true;
    }

    [Fact]
    public void MapResults_ShouldConvertWoxResultFields()
    {
        var mapped = WoxResultMapper.MapResults(new[] { new FakeWoxResult() });

        mapped.Should().ContainSingle();
        mapped[0].Title.Should().Be("Hello");
        mapped[0].SubTitle.Should().Be("World");
        mapped[0].IcoPath.Should().Be("icon.png");
        mapped[0].Score.Should().Be(88);
        mapped[0].Action.Should().NotBeNull();
        mapped[0].Action!().Should().BeTrue();
    }

    [Fact]
    public void CreateWoxQuery_ShouldUseShim_WhenWoxTypeMissing()
    {
        var query = new Sdk.Search.Query("app hello");
        var shim = WoxResultMapper.CreateWoxQuery(Assembly.GetExecutingAssembly(), query);

        shim.Should().BeOfType<WoxResultMapper.WoxQueryShim>();
        var typed = (WoxResultMapper.WoxQueryShim)shim;
        typed.ActionKeyword.Should().Be("app");
        typed.Search.Should().Be("hello");
    }
}
