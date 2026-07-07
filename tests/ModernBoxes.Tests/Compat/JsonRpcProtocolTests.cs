using FluentAssertions;
using ModernBoxes.Infrastructure.Compat;
using Xunit;

namespace ModernBoxes.Tests.Compat;

[Trait("Category", "Plugin")]
public class JsonRpcProtocolTests
{
    [Fact]
    public void ParseQueryResponse_ShouldReadResultArray()
    {
        var response = """
            {"jsonrpc":"2.0","id":1,"result":[{"Title":"A","SubTitle":"B","Score":70}]}
            """;

        var items = JsonRpcProtocol.ParseQueryResponse(response);

        items.Should().ContainSingle();
        items[0].Title.Should().Be("A");
        items[0].SubTitle.Should().Be("B");
        items[0].Score.Should().Be(70);
    }

    [Fact]
    public void BuildQueryRequest_ShouldIncludeSearchParams()
    {
        var json = JsonRpcProtocol.BuildQueryRequest(2, "wx", "", "wx");

        json.Should().Contain("\"search\":\"wx\"");
        json.Should().Contain("\"method\":\"query\"");
    }
}
