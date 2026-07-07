using CommunityToolkit.Mvvm.ComponentModel;
using ModernBoxes.Core.Interfaces;
using ModernBoxes.Core.Models;
using ModernBoxes.Infrastructure;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ModernBoxes.Presentation.ViewModels
{
    public class DirInformationDialogViewModel : ObservableObject
    {
        private readonly IPersistenceProvider _persistence;

        private DirInformationModel dirInfo = new DirInformationModel();

        public DirInformationModel DirInfo
        {
            get { return dirInfo; }
            set { dirInfo = value; }
        }

        public DirInformationDialogViewModel(IPersistenceProvider persistence, String path)
        {
            _persistence = persistence;
            DirInfo.Path = path;
            _ = init();
        }

        private async Task init()
        {
            try
            {
                DirInfo.CreateTime = Directory.GetCreationTime(DirInfo.Path).ToShortDateString();
                DirInfo.DirName = Path.GetFileName(DirInfo.Path.TrimEnd('\\'));
                DirInfo.Include = $"文件夹{Directory.GetFiles(DirInfo.Path).Length}个，子文件夹{Directory.GetDirectories(DirInfo.Path).Length}个";

                var dirs = await _persistence.LoadAsync<TempDirModel>("tempdirs");
                var match = dirs.FirstOrDefault(d => d.TempDirPath == DirInfo.Path);
                if (match != null)
                    DirInfo.DirKind = match.TempDirImportantKind;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Error loading dir information");
            }
        }
    }
}
