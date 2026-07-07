using CommunityToolkit.Mvvm.ComponentModel;
using ModernBoxes.Core.Enums;
using System;

namespace ModernBoxes.Core.Models
{
    public class TempDirModel : ObservableObject
    {
        private String tempDirPath;

        public String TempDirPath
        {
            get { return tempDirPath; }
            set { tempDirPath = value; OnPropertyChanged("TempDirPath"); }
        }

        /// <summary>
        /// 红色文件夹 非常重要
        /// 黄色文件夹 重要
        /// 蓝色文件夹 一般
        /// 绿色文件夹 临时(随时可能要删除)
        /// </summary>
        private DirEnum tempDirImportantKind;

        public DirEnum TempDirImportantKind
        {
            get { return tempDirImportantKind; }
            set { tempDirImportantKind = value; OnPropertyChanged("TempDirImportantKind"); }
        }
    }
}