using CommunityToolkit.Mvvm.ComponentModel;
using ModernBoxes.Core.Enums;
using System;

namespace ModernBoxes.Core.Models
{
    public class TempDirModel : ObservableObject
    {
        private string tempDirPath = string.Empty;

        public string TempDirPath
        {
            get => tempDirPath;
            set { tempDirPath = value; OnPropertyChanged(nameof(TempDirPath)); }
        }

        private DirEnum tempDirImportantKind;

        public DirEnum TempDirImportantKind
        {
            get => tempDirImportantKind;
            set { tempDirImportantKind = value; OnPropertyChanged(nameof(TempDirImportantKind)); }
        }
    }
}
