using CommunityToolkit.Mvvm.ComponentModel;
using ModernBoxes.Core.Models;
using ModernBoxes.Infrastructure;
using System;
using System.IO;

namespace ModernBoxes.Presentation.ViewModels
{
    public class FilePropertyDialogViewModel : ObservableObject
    {
        private FileInformationModel fileInformation = new FileInformationModel();

        public FileInformationModel FileInformation
        {
            get { return fileInformation; }
            set { fileInformation = value; OnPropertyChanged("FileInformation"); }
        }

        public FilePropertyDialogViewModel(String FilePath)
        {
            FileInformation.FilePath = FilePath;
            FileInformation.CreateTime = File.GetCreationTime(FilePath).ToString();
            FileInformation.ChangeTime = File.GetLastWriteTime(FilePath).ToString();
            FileInformation.Size = FileHelper.getFileSize(FilePath).ToString() + " Byte";
        }
    }
}