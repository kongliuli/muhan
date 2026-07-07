using ModernBoxes.Core.Interfaces;
using ModernBoxes.Core.Models;
using ModernBoxes.Core.Interfaces.Repositories;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ModernBoxes.Infrastructure.Data
{
    public class DualWriteProvider : IPersistenceProvider
    {
        private readonly Dictionary<string, (string FilePath, Action<object> SyncAction)> _entityMap;

        public DualWriteProvider(
            IMenuRepository menuRepo,
            IApplicationRepository appRepo,
            ITempDirRepository dirRepo,
            ITempFileRepository fileRepo,
            INoteRepository noteRepo,
            ICardConfigRepository cardConfigRepo)
        {
            _entityMap = new Dictionary<string, (string, Action<object>)>
            {
                ["menus"] = ("MenuConfig.json", data => menuRepo.SyncMenus((IEnumerable<MenuModel>)data)),
                ["applications"] = ("UsedApplicationConfig.json", data => appRepo.SyncApplications((IEnumerable<ApplicationModel>)data)),
                ["tempdirs"] = ("TempDirConfig.json", data => dirRepo.SyncTempDirs((IEnumerable<TempDirModel>)data)),
                ["tempfiles"] = ("TempFileConfig.json", data => fileRepo.SyncTempFiles((IEnumerable<TempFileModel>)data)),
                ["notes"] = ("NotesConfig.json", data => noteRepo.SyncNotes((IEnumerable<NoteModel>)data)),
                ["cardconfigs"] = ("AllCardsConfig.json", data => cardConfigRepo.SyncCardConfigs((IEnumerable<CardContentModel>)data)),
            };
        }

        public async Task SaveAsync<T>(string entityName, IEnumerable<T> data)
        {
            if (!_entityMap.TryGetValue(entityName, out var entry))
                throw new ArgumentException($"Unknown entity: {entityName}");

            string path = Path.Combine(AppContext.BaseDirectory, entry.FilePath);
            string json = JsonConvert.SerializeObject(data);
            // FileHelper.WriteFile 内部走临时文件+替换，原子写入，无需先删除
            await FileHelper.WriteFile(path, json);
            try { entry.SyncAction(data); } catch (Exception ex) { Logger.Error(ex, "SQLite sync failed"); }
        }

        public async Task<IEnumerable<T>> LoadAsync<T>(string entityName)
        {
            if (!_entityMap.TryGetValue(entityName, out var entry))
                throw new ArgumentException($"Unknown entity: {entityName}");

            string path = Path.Combine(AppContext.BaseDirectory, entry.FilePath);
            if (!File.Exists(path))
                return Array.Empty<T>();

            string json = await FileHelper.ReadFile(path);
            if (json.Length <= 8)
                return Array.Empty<T>();
            return JsonConvert.DeserializeObject<List<T>>(json) ?? new List<T>();
        }
    }
}
