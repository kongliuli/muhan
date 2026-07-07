using CommunityToolkit.Mvvm.ComponentModel;
using System.Threading.Tasks;

namespace ModernBoxes.Core.Interfaces
{
    public abstract class CardBase<TModel> : ObservableObject, ICardViewModel
    {
        protected TModel Model;

        private string _cardID = string.Empty;
        public virtual string CardID
        {
            get => _cardID;
            set => SetProperty(ref _cardID, value);
        }

        private string _cardName = string.Empty;
        public virtual string CardName
        {
            get => _cardName;
            set => SetProperty(ref _cardName, value);
        }

        private object _cardContent = new();
        public virtual object CardContent
        {
            get => _cardContent;
            set => SetProperty(ref _cardContent, value);
        }

        private double _cardHeight = 200;
        public virtual double CardHeight
        {
            get => _cardHeight;
            set => SetProperty(ref _cardHeight, value);
        }

        private string _preview = string.Empty;
        public virtual string Preview
        {
            get => _preview;
            set => SetProperty(ref _preview, value);
        }

        private bool _isChecked;
        public virtual bool IsChecked
        {
            get => _isChecked;
            set => SetProperty(ref _isChecked, value);
        }

        protected CardBase(TModel model)
        {
            Model = model;
        }

        public abstract Task LoadAsync();
        public abstract Task RefreshAsync();
    }
}
