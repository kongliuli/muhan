using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace ModernBoxes.Core.Models
{
    public class CardContentModel : ObservableObject
    {
        private int cardId;

        public int CardID
        {
            get => cardId;
            set => cardId = value;
        }

        private string cardName = string.Empty;

        public string CardName
        {
            get => cardName;
            set => cardName = value;
        }

        private object? cardContent;

        public object? CardContent
        {
            get => cardContent;
            set => cardContent = value;
        }

        private double cardHeight = 200;

        public double CardHeight
        {
            get => cardHeight;
            set { cardHeight = value; OnPropertyChanged(nameof(CardHeight)); }
        }

        private string preview = string.Empty;

        public string Preview
        {
            get => preview;
            set => preview = value;
        }

        private bool isChecked;

        public bool IsChecked
        {
            get => isChecked;
            set { isChecked = value; OnPropertyChanged(nameof(IsChecked)); }
        }
    }
}
