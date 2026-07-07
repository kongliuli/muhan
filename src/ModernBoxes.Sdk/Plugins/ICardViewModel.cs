using System.Threading.Tasks;

namespace ModernBoxes.Sdk.Plugins
{
    public interface ICardViewModel : ICard
    {
        int CardApiVersion { get; }

        Task LoadAsync();
        Task RefreshAsync();
    }
}
