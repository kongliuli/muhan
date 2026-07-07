using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Windows;
using System.Windows.Forms;
using ModernBoxes.Infrastructure;
using MessageBox = System.Windows.MessageBox;

namespace ModernBoxes.Presentation.ViewModels
{
    public class BackupViewModel
    {
        private static readonly string[] ConfigFiles =
        {
            "AllCardsConfig.json",
            "MenuConfig.json",
            "TempDirConfig.json",
            "TempFileConfig.json",
            "UsedApplicationConfig.json",
            "NotesConfig.json",
            "App.config"
        };

        private static string AppDir => AppPaths.Root;

        public void ExportData()
        {
            var outputPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                $"ModernBoxes_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.mhbak");

            var tempDir = Path.Combine(Path.GetTempPath(), $"mhbak_export_{Guid.NewGuid():N}");
            Directory.CreateDirectory(tempDir);

            try
            {
                foreach (var file in ConfigFiles)
                {
                    var src = Path.Combine(AppDir, file);
                    if (File.Exists(src))
                        File.Copy(src, Path.Combine(tempDir, file), true);
                }

                if (File.Exists(outputPath))
                    File.Delete(outputPath);

                ZipFile.CreateFromDirectory(tempDir, outputPath, CompressionLevel.Optimal, false);

                MessageBox.Show($"备份成功！\n\n文件已保存至：\n{outputPath}", "备份完成");
                Process.Start("explorer.exe", $"/select,\"{outputPath}\"");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"备份失败：{ex.Message}", "错误");
            }
            finally
            {
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }

        public void ImportData()
        {
            var dialog = new OpenFileDialog
            {
                Filter = "ModernBoxes 备份文件 (*.mhbak)|*.mhbak",
                Title = "选择备份文件"
            };

            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            var backupPath = dialog.FileName;
            var tempDir = Path.Combine(Path.GetTempPath(), $"mhbak_import_{Guid.NewGuid():N}");
            Directory.CreateDirectory(tempDir);

            try
            {
                ZipFile.ExtractToDirectory(backupPath, tempDir);

                foreach (var file in ConfigFiles)
                {
                    if (!File.Exists(Path.Combine(tempDir, file)))
                    {
                        MessageBox.Show($"备份文件不完整：缺少 {file}", "验证失败");
                        return;
                    }
                }

                var rollbackDir = Path.Combine(Path.GetTempPath(), $"mhbak_rollback_{Guid.NewGuid():N}");
                Directory.CreateDirectory(rollbackDir);

                try
                {
                    foreach (var file in ConfigFiles)
                    {
                        var dest = Path.Combine(AppDir, file);
                        var rollbackFile = Path.Combine(rollbackDir, file + ".bak");
                        if (File.Exists(dest))
                        {
                            if (File.Exists(rollbackFile))
                                File.Delete(rollbackFile);
                            File.Move(dest, rollbackFile);
                        }
                        File.Copy(Path.Combine(tempDir, file), dest, true);
                    }
                }
                catch
                {
                    foreach (var file in ConfigFiles)
                    {
                        var rollbackFile = Path.Combine(rollbackDir, file + ".bak");
                        var dest = Path.Combine(AppDir, file);
                        if (File.Exists(rollbackFile) && !File.Exists(dest))
                            File.Move(rollbackFile, dest);
                    }
                    throw;
                }

                MessageBox.Show("恢复成功，部分设置可能需要重启后生效", "恢复完成");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"恢复失败：{ex.Message}", "错误");
            }
            finally
            {
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }
    }
}
