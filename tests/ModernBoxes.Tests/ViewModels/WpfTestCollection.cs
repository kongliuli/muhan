using Xunit;

namespace ModernBoxes.Tests.ViewModels;

[CollectionDefinition("Wpf")]
public class WpfTestCollection : ICollectionFixture<WpfApplicationFixture>
{
}
