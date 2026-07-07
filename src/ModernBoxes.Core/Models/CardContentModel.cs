using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace ModernBoxes.Core.Models
{
    public class CardContentModel : ObservableObject
    {
        private int cardId;

        public int CardID
        {
            get { return cardId; }
            set { cardId = value; }
        }

        /// <summary>
        /// ø®∆¨√˚≥∆
        /// </summary>
        private String cardName;

        public String CardName
        {
            get { return cardName; }
            set { cardName = value; }
        }

        /// <summary>
        /// ø®∆¨ƒ⁄»›
        /// </summary>
        private Object cardContent;

        public Object CardContent
        {
            get { return cardContent; }
            set { cardContent = value; }
        }

        /// <summary>
        /// ø®∆¨∏ﬂ∂»
        /// </summary>
        private Double cardHeight = 200;

        public Double CardHeight
        {
            get { return cardHeight; }
            set { cardHeight = value; OnPropertyChanged("CardHeight"); }
        }

        /// <summary>
        /// ‘§¿¿Õº
        /// </summary>
        private String preview;

        public String Preview
        {
            get { return preview; }
            set { preview = value; }
        }

        /// <summary>
        ///  «∑Ò—°÷–
        /// </summary>
        private Boolean isChecked;

        public Boolean IsChecked
        {
            get { return isChecked; }
            set { isChecked = value; OnPropertyChanged("IsChecked"); }
        }
    }
}