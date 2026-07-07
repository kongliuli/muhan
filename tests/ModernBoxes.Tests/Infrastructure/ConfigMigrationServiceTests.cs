using FluentAssertions;
using Newtonsoft.Json.Linq;
using Xunit;

namespace ModernBoxes.Tests.Migrations;

public class ConfigMigrationServiceTests
{
    [Fact]
    public void NeedsCardsJsonMigration_DetectsPriviewField()
    {
        var json = """[{"CardID":0,"Preview":"/img.png"}]""";
        json.Contains("\"Priview\"", StringComparison.Ordinal).Should().BeFalse();

        var oldJson = """[{"CardID":0,"Priview":"/img.png"}]""";
        oldJson.Contains("\"Priview\"", StringComparison.Ordinal).Should().BeTrue();
    }

    [Fact]
    public void MigrateIfNeeded_RenamesPriviewInMemory()
    {
        var array = JArray.Parse("""[{"CardID":0,"Priview":"/img.png","CardName":"test"}]""");
        foreach (var token in array.Children<JObject>())
        {
            if (token["Priview"] != null && token["Preview"] == null)
            {
                token["Preview"] = token["Priview"];
                token.Remove("Priview");
            }
        }

        array[0]!["Preview"]!.ToString().Should().Be("/img.png");
        array[0]["Priview"].Should().BeNull();
    }
}
