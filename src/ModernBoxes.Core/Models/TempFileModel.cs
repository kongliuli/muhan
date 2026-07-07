using CommunityToolkit.Mvvm.ComponentModel;
using ModernBoxes.Core.Enums;
using System;

namespace ModernBoxes.Core.Models
{
    public class TempFileModel : ObservableObject
    {
        private string filePath = string.Empty;

        public string FilePath
        {
            get => filePath;
            set { filePath = value; OnPropertyChanged(nameof(FilePath)); }
        }

        private DirEnum fileKind;

        public DirEnum FileKind
        {
            get => fileKind;
            set { fileKind = value; OnPropertyChanged(nameof(FileKind)); }
        }
    }
}
