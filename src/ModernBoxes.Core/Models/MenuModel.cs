using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace ModernBoxes.Core.Models
{
    public class MenuModel : ObservableObject
    {
        private String menuName = "";

        public String MenuName
        {
            get { return menuName; }
            set { menuName = value; OnPropertyChanged("MenuName"); }
        }

        private String icon = "";

        public String Icon
        {
            get { return icon; }
            set { icon = value; OnPropertyChanged("Target"); }
        }

        private String target = "";

        public String Target
        {
            get { return target; }
            set { target = value; OnPropertyChanged("Target"); }
        }
    }
}