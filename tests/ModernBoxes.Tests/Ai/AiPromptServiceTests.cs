using FluentAssertions;
using ModernBoxes.Infrastructure.Ai;
using Xunit;

namespace ModernBoxes.Tests.Ai;

public class AiPromptServiceTests
{
    [Fact]
    public void IsAvailable_FalseWhenNoApiKey()
    {
        var chat = new ChatClientService();
        var service = new AiPromptService(chat);
        service.IsAvailable.Should().BeFalse();
    }
}
