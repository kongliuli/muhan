using ModernBoxes.Core.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ModernBoxes.Core.Interfaces
{
    public interface IFileCardService
    {
        Task AddFile(TempFileModel file);
        Task RemoveFile(string filePath);
        Task DeleteToRecycleBin(string filePath);
        Task OpenFile(string filePath);
        Task OpenFileLocation(string filePath);
        Task<IEnumerable<TempFileModel>> GetAllFiles();
    }
}
