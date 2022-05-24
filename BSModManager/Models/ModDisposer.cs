using System;
using System.IO;
using System.IO.Compression;

namespace BSModManager.Models
{
    public class ModDisposer
    {
        public void Dispose(string sourceDirFullPath, string destDirFullPath)
        {
            if (Directory.GetFiles(sourceDirFullPath).Length > 0)
            {
                // https://github.com/denpadokei/LocalModAssistant/blob/b0c119f7e32a35cd15ca2010f9dc50b8267183fe/LocalModAssistant/Models/MainViewDomain.cs
                foreach (var dllFileFullPath in Directory.EnumerateFiles(sourceDirFullPath, "*.dll", SearchOption.TopDirectoryOnly))
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
                    catch (Exception e)
                    {
                        Console.WriteLine($"{e}");
                    }
                }
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
                    catch (Exception e)
                    {
                        Console.WriteLine($"{e}");
                    }
                }
            }
        }
    }
}
