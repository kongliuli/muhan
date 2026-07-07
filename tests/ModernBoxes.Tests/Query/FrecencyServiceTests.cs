using FluentAssertions;
using ModernBoxes.Infrastructure.Data;
using ModernBoxes.Infrastructure.Query;
using Xunit;

namespace ModernBoxes.Tests.Query;

[Trait("Category", "Service")]
public class FrecencyServiceTests : IClassFixture<DatabaseFixture>
{
    private readonly FrecencyService _sut;

    public FrecencyServiceTests(DatabaseFixture _)
    {
        _sut = new FrecencyService(DatabaseService.Instance);
        _sut.EnsureSchema();
    }

    [Fact]
    public void GetBoost_ReturnsZero_WhenNeverUsed()
    {
        var key = FrecencyService.MakeKey("Application", "未使用过");

        _sut.GetBoost(key).Should().Be(0);
    }

    [Fact]
    public void Record_IncreasesBoost_ForSameKey()
    {
        var key = FrecencyService.MakeKey("Application", "微信");

        _sut.Record(key);
        _sut.Record(key);

        _sut.GetBoost(key).Should().BeGreaterThan(0);
    }
}
