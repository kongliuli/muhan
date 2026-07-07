using ModernBoxes.Core.Models;
using ModernBoxes.Core.Interfaces.Repositories;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ModernBoxes.Infrastructure.Data
{
    public static class MigrationService
    {
        public static void Migrate(
            IMenuRepository menuRepo,
            IApplicationRepository appRepo,
            ITempDirRepository dirRepo,
            ITempFileRepository fileRepo,
            INoteRepository noteRepo,
            ICardConfigRepository cardRepo)
        {
            try
            {
                long count = DatabaseService.Instance.GetTableCount("Menus");
                if (count > 0)
                {
                    Logger.Info("SQLite already has data, skipping migration");
                    return;
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Migration check failed");
                return;
            }

            Logger.Info("Starting JSON to SQLite migration...");

            try
            {
                MigrateMenus(menuRepo);
                MigrateApplications(appRepo);
                MigrateTempDirs(dirRepo);
                MigrateTempFiles(fileRepo);
                MigrateNotes(noteRepo);
                MigrateCardConfigs(cardRepo);
                Logger.Info("JSON to SQLite migration completed successfully");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "Migration failed");
            }
        }

        private static void MigrateMenus(IMenuRepository repo)
        {
            string path = AppPaths.Config("MenuConfig.json");
            if (!File.Exists(path)) return;
            string json = File.ReadAllText(path);
            if (json.Length <= 8) return;
            var menus = JsonConvert.DeserializeObject<List<MenuModel>>(json);
            if (menus != null && menus.Count > 0)
            {
                repo.SyncMenus(menus);
                Logger.Info($"Migrated {menus.Count} menus");
            }
        }

        private static void MigrateApplications(IApplicationRepository repo)
        {
            string path = AppPaths.Config("UsedApplicationConfig.json");
            if (!File.Exists(path)) return;
            string json = File.ReadAllText(path);
            if (json.Length <= 8) return;
            var apps = JsonConvert.DeserializeObject<List<ApplicationModel>>(json);
            if (apps != null && apps.Count > 0)
            {
                repo.SyncApplications(apps);
                Logger.Info($"Migrated {apps.Count} applications");
            }
        }

        private static void MigrateTempDirs(ITempDirRepository repo)
        {
            string path = AppPaths.Config("TempDirConfig.json");
            if (!File.Exists(path)) return;
            string json = File.ReadAllText(path);
            if (json.Length <= 8) return;
            var dirs = JsonConvert.DeserializeObject<List<TempDirModel>>(json);
            if (dirs != null && dirs.Count > 0)
            {
                repo.SyncTempDirs(dirs);
                Logger.Info($"Migrated {dirs.Count} temp dirs");
            }
        }

        private static void MigrateTempFiles(ITempFileRepository repo)
        {
            string path = AppPaths.Config("TempFileConfig.json");
            if (!File.Exists(path)) return;
            string json = File.ReadAllText(path);
            if (json.Length <= 8) return;
            var files = JsonConvert.DeserializeObject<List<TempFileModel>>(json);
            if (files != null && files.Count > 0)
            {
                repo.SyncTempFiles(files);
                Logger.Info($"Migrated {files.Count} temp files");
            }
        }

        private static void MigrateNotes(INoteRepository repo)
        {
            string path = AppPaths.Config("NotesConfig.json");
            if (!File.Exists(path)) return;
            string json = File.ReadAllText(path);
            if (json.Length <= 8) return;
            var notes = JsonConvert.DeserializeObject<List<NoteModel>>(json);
            if (notes != null && notes.Count > 0)
            {
                repo.SyncNotes(notes);
                Logger.Info($"Migrated {notes.Count} notes");
            }
        }

        private static void MigrateCardConfigs(ICardConfigRepository repo)
        {
            string path = AppPaths.Config("AllCardsConfig.json");
            if (!File.Exists(path)) return;
            string json = File.ReadAllText(path);
            if (json.Length <= 8) return;
            var cards = JsonConvert.DeserializeObject<List<CardContentModel>>(json);
            if (cards != null && cards.Count > 0)
            {
                repo.SyncCardConfigs(cards);
                Logger.Info($"Migrated {cards.Count} card configs");
            }
        }
    }
}
