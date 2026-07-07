using FluentAssertions;
using ModernBoxes.Infrastructure.Deeplink;
using Xunit;

namespace ModernBoxes.Tests.Deeplink;

public class MuhanUriParserTests
{
    [Theory]
    [InlineData("muhan://search/dotnet", "search", "dotnet")]
    [InlineData("muhan://search?q=hello", "search", "hello")]
    [InlineData("muhan://palette", "palette", "")]
    public void TryParse_ExtractsCommandAndArgument(string raw, string command, string argument)
    {
        MuhanUriParser.TryParse(raw, out var uri).Should().BeTrue();
        var ctx = new DeeplinkContext(uri);
        ctx.Command.Should().Be(command);
        ctx.GetArgument().Should().Be(argument);
    }

    [Theory]
    [InlineData("https://example.com")]
    [InlineData("muhan://")]
    [InlineData("")]
    public void TryParse_RejectsInvalid(string raw)
    {
        MuhanUriParser.TryParse(raw, out _).Should().BeFalse();
    }
}

public class DeeplinkRegistryTests
{
    [Fact]
    public async Task TryDispatchAsync_InvokesRegisteredHandler()
    {
        var registry = new DeeplinkRegistry();
        var hit = false;
        registry.Register("ping", _ =>
        {
            hit = true;
            return Task.CompletedTask;
        });

        var ok = await registry.TryDispatchAsync("muhan://ping");
        ok.Should().BeTrue();
        hit.Should().BeTrue();
    }

    [Fact]
    public async Task TryDispatchAsync_UnknownCommandReturnsFalse()
    {
        var registry = new DeeplinkRegistry();
        var ok = await registry.TryDispatchAsync("muhan://missing");
        ok.Should().BeFalse();
    }
}
