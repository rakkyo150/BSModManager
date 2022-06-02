using BSModManager.Static;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace BSModManager.Models
{
    public class ModDisposer
    {
        public void Dispose(string sourceDirFullPath, string destDirFullPath)
        {
            if (Directory.GetFiles(sourceDirFullPath).Length == 0) return;

            // https://github.com/denpadokei/LocalModAssistant/blob/b0c119f7e32a35cd15ca2010f9dc50b8267183fe/LocalModAssistant/Models/MainViewDomain.cs
            DisposeDllFile(sourceDirFullPath, destDirFullPath);
            DisposeZipFile(sourceDirFullPath, destDirFullPath);
        }

        public void MoveFolder(string sourceDirFullPath, string destDirFullPath)
        {
            DirectoryInfo sourceDir = new DirectoryInfo(sourceDirFullPath);
            
            DirectoryInfo[] dirs = sourceDir.GetDirectories();

            if (dirs.Length == 0) return;

            foreach(var dir in dirs)
            {
                try
                {
                    Folder.Instance.Copy(dir.FullName, Path.Combine(destDirFullPath,dir.Name), true);
                    Directory.Delete(dir.FullName, true);
                }
                catch(Exception ex)
                {
                    Logger.Instance.Info($"{ex.Message}\nダウンロードしたディレクトリの配置に失敗しました");
                }
            }
        }

        private static void DisposeZipFile(string sourceDirFullPath, string destDirFullPath)
        {
            IEnumerable<string> zipFilesPath = Directory.EnumerateFiles(sourceDirFullPath, "*.zip", SearchOption.TopDirectoryOnly);

            if (zipFilesPath.Count() == 0) return;
            
            foreach (var zipFileName in Directory.EnumerateFiles(sourceDirFullPath, "*.zip", SearchOption.TopDirectoryOnly))
            {
                try
                {
                    using (var fs = File.Open(zipFileName, FileMode.Open))
                    using (var zip = new ZipArchive(fs))
                    {
                        foreach (var file in zip.Entries)
                        {
                            var installPath = Path.Combine(destDirFullPath, file.FullName);
                            if (File.Exists(installPath))
                            {
                                File.Delete(installPath);
                            }
                        }
                        zip.ExtractToDirectory(destDirFullPath);
                    }
                    File.Delete(zipFileName);
                }
                catch (Exception ex)
                {
                    Logger.Instance.Error($"{ex.Message}\nダウンロードしたModの解凍を正常に行えませんでした");
                }
            }
        }

        private static void DisposeDllFile(string sourceDirFullPath, string destDirFullPath)
        {
            IEnumerable<string> dllFilesPath = Directory.EnumerateFiles(sourceDirFullPath, "*.dll", SearchOption.TopDirectoryOnly);

            if (dllFilesPath.Count() == 0) return;

            foreach (var dllFileFullPath in dllFilesPath)
            {
                if (!Directory.Exists(Path.Combine(destDirFullPath, "Plugins")))
                {
                    Directory.CreateDirectory(Path.Combine(destDirFullPath, "Plugins"));
                }
                try
                {
                    var installPath = Path.Combine(destDirFullPath, "Plugins", Path.GetFileName(dllFileFullPath));
                    if (File.Exists(installPath))
                    {
                        File.Delete(installPath);
                    }
                    File.Move(dllFileFullPath, installPath);
                }
                catch (Exception ex)
                {
                    Logger.Instance.Error($"{ex.Message}\nダウンロードしたModを正常に移動できませんでした");
                }
            }
        }
    }
}
