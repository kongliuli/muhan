using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace ModernBoxes.Core.Models
{
    public class ApplicationModel : ObservableObject
    {
        private string fileName = string.Empty;

        public string FileName
        {
            get => fileName;
            set { fileName = value; OnPropertyChanged(nameof(FileName)); }
        }

        private string appPath = string.Empty;

        public string AppPath
        {
            get => appPath;
            set { appPath = value; OnPropertyChanged(nameof(AppPath)); }
        }

        private string icon = string.Empty;

        public string Icon
        {
            get => icon;
            set { icon = value; OnPropertyChanged(nameof(Icon)); }
        }
    }
}
