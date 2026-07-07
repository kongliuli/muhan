using FluentAssertions;
using ModernBoxes.Infrastructure.Search;
using Xunit;

namespace ModernBoxes.Tests.Services;

[Trait("Category", "Service")]
public class ChinesePinyinHelperTests
{
    [Fact]
    public void Matches_ShouldHitDirectSubstring()
    {
        ChinesePinyinHelper.Matches("微信", "微信 PC版").Should().BeTrue();
    }

    [Fact]
    public void Matches_ShouldHitPinyinInitials()
    {
        ChinesePinyinHelper.Matches("wx", "微信").Should().BeTrue();
    }

    [Fact]
    public void Matches_ShouldMissUnrelatedQuery()
    {
        ChinesePinyinHelper.Matches("abc", "微信").Should().BeFalse();
    }

    [Fact]
    public void GetInitials_ShouldIncludeAsciiLetters()
    {
        ChinesePinyinHelper.GetInitials("App微信").Should().Contain("app");
    }
}
