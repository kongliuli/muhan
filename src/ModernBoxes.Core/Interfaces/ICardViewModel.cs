using System;

namespace ModernBoxes.Core.Interfaces
{
    public interface ICardViewModel : ICard
    {
        const int HostApiVersion = 1;

        int CardApiVersion => 1;

        System.Threading.Tasks.Task LoadAsync();
        System.Threading.Tasks.Task RefreshAsync();
    }
}
