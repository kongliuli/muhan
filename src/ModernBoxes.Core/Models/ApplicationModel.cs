using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace ModernBoxes.Core.Models
{
    public class ApplicationModel : ObservableObject
    {
        private String fileName;

        public String FileName
        {
            get { return fileName; }
            set { fileName = value; OnPropertyChanged("FileName"); }
        }

        private String appPath;

        public String AppPath
        {
            get { return appPath; }
            set { appPath = value; OnPropertyChanged("AppPath"); }
        }

        private String icon;

        public String Icon
        {
            get { return icon; }
            set { icon = value; OnPropertyChanged("Icon"); }
        }
    }
}