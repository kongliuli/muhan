using ModernBoxes.Core.Interfaces;
using ModernBoxes.Core.Models;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace ModernBoxes.Infrastructure.Services
{
    public class FileCardService : IFileCardService
    {
        private readonly IPersistenceProvider _persistence;

        public FileCardService(IPersistenceProvider persistence)
        {
            _persistence = persistence;
        }

        public async Task<IEnumerable<TempFileModel>> GetAllFiles()
        {
            return await _persistence.LoadAsync<TempFileModel>("tempfiles");
        }

        public async Task AddFile(TempFileModel file)
        {
            var files = new List<TempFileModel>(await GetAllFiles());
            files.Add(file);
            await _persistence.SaveAsync("tempfiles", files);
        }

        public async Task RemoveFile(string filePath)
        {
            var files = new List<TempFileModel>(await GetAllFiles());
            files.RemoveAll(f => f.FilePath == filePath);
            await _persistence.SaveAsync("tempfiles", files);
        }

        public async Task DeleteToRecycleBin(string filePath)
        {
            Microsoft.VisualBasic.FileIO.FileSystem.DeleteFile(filePath,
                Microsoft.VisualBasic.FileIO.UIOption.OnlyErrorDialogs,
                Microsoft.VisualBasic.FileIO.RecycleOption.SendToRecycleBin);
            await RemoveFile(filePath);
        }

        public Task OpenFile(string filePath)
        {
            var psi = new ProcessStartInfo(filePath) { UseShellExecute = true };
            Process.Start(psi);
            return Task.CompletedTask;
        }

        public Task OpenFileLocation(string filePath)
        {
            Process.Start("explorer.exe", $"/select,\"{filePath}\"");
            return Task.CompletedTask;
        }
    }
}
