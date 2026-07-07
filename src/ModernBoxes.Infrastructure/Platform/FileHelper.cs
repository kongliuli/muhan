using System;
using System.IO;
using System.Threading.Tasks;

namespace ModernBoxes.Infrastructure
{
    public class FileHelper
    {
        /// <summary>
        /// 原子写入：先写临时文件再替换，避免写入中途崩溃导致数据丢失。
        /// </summary>
        public static async Task<bool> WriteFile(String path, String Content)
        {
            var tempPath = path + ".tmp";
            try
            {
                await File.WriteAllTextAsync(tempPath, Content);
                File.Move(tempPath, path, overwrite: true);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Error writing file: {path}");
                try { if (File.Exists(tempPath)) File.Delete(tempPath); } catch { /* ponytail: best-effort cleanup */ }
                return false;
            }
        }

        public static Task WriteConfig(String path, String content) => WriteFile(path, content);

        public static async Task<String> ReadFile(String path)
        {
            using var fileStream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var streamReader = new StreamReader(fileStream);
            return await streamReader.ReadToEndAsync();
        }

        /// <summary>
        /// 递归复制文件夹（源文件夹本身会作为子目录复制到目标下）
        /// </summary>
        public static void CopyFolder(string strFromPath, string strToPath)
        {
            if (!Directory.Exists(strFromPath))
            {
                Directory.CreateDirectory(strFromPath);
            }
            string strFolderName = Path.GetFileName(strFromPath.TrimEnd('\\'));
            string destRoot = Path.Combine(strToPath, strFolderName);
            if (!Directory.Exists(destRoot))
            {
                Directory.CreateDirectory(destRoot);
            }
            foreach (var file in Directory.GetFiles(strFromPath))
            {
                File.Copy(file, Path.Combine(destRoot, Path.GetFileName(file)), true);
            }
            foreach (var dir in Directory.GetDirectories(strFromPath))
            {
                CopyFolder(dir, destRoot);
            }
        }

        public static long getFileSize(String FilePath)
        {
            long size = 0;
            if (File.Exists(FilePath))
            {
                size = new FileInfo(FilePath).Length;
            }
            return size;
        }
    }
}
