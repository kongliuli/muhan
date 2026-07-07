using ModernBoxes.Core.Enums;
using ModernBoxes.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ModernBoxes.Core.Interfaces
{
    public interface IDirectoryCardService
    {
        Task AddDirectory(TempDirModel model);
        Task RemoveDirectory(string path);
        void OpenDirectory(string path);
        Task ChangeImportance(string path, DirEnum kind);
        Task<IEnumerable<TempDirModel>> GetAllDirectories();
    }
}
