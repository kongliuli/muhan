using ModernBoxes.Core.Enums;
using ModernBoxes.Core.Interfaces;
using ModernBoxes.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ModernBoxes.Infrastructure.Services
{
    public class DirectoryCardService : IDirectoryCardService
    {
        private readonly IPersistenceProvider _persistence;

        public DirectoryCardService(IPersistenceProvider persistence)
        {
            _persistence = persistence;
        }

        public async Task AddDirectory(TempDirModel model)
        {
            var dirs = (await _persistence.LoadAsync<TempDirModel>("tempdirs")).ToList();
            dirs.Add(model);
            await _persistence.SaveAsync("tempdirs", dirs);
        }

        public async Task RemoveDirectory(string path)
        {
            var dirs = (await _persistence.LoadAsync<TempDirModel>("tempdirs")).ToList();
            dirs.RemoveAll(d => d.TempDirPath == path);
            await _persistence.SaveAsync("tempdirs", dirs);
        }

        public void OpenDirectory(string path)
        {
            Process.Start("explorer.exe", path.Replace('/', '\\'));
        }

        public async Task ChangeImportance(string path, DirEnum kind)
        {
            var dirs = (await _persistence.LoadAsync<TempDirModel>("tempdirs")).ToList();
            var dir = dirs.FirstOrDefault(d => d.TempDirPath == path);
            if (dir != null)
            {
                dir.TempDirImportantKind = kind;
                await _persistence.SaveAsync("tempdirs", dirs);
            }
        }

        public async Task<IEnumerable<TempDirModel>> GetAllDirectories()
        {
            return await _persistence.LoadAsync<TempDirModel>("tempdirs");
        }
    }
}
