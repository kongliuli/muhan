using System.Threading.Tasks;

namespace ModernBoxes.Core.Interfaces
{
    public interface ICardViewModel : ICard
    {
        Task LoadAsync();
        Task RefreshAsync();
    }
}
