using BSModManager.Static;
using System;
using System.IO;
using System.IO.Compression;

namespace BSModManager.Models
{
    public class Initializer
    {
        public void Backup()
        {
            string now = DateTime.Now.ToString("yyyyMMddHHmmss");

            // なぜかBackupフォルダで一時フォルダ作ると画面固まるのでtempで一時フォルダ作る
            string zipPath = Path.Combine(Folder.Instance.tmpFolder, $"BS{GameVersion.Version}-{now}");
            Directory.CreateDirectory(zipPath);

            Folder.Instance.Copy(Path.Combine(Folder.Instance.BSFolderPath, "Plugins"),
                Path.Combine(zipPath, "Plugins"), true);
            Folder.Instance.Copy(Folder.Instance.dataFolder, Path.Combine(zipPath, "Data"), true);
            File.Copy(FilePath.Instance.configFilePath, Path.Combine(zipPath, "config.json"), true);

            ZipFile.CreateFromDirectory(zipPath, Path.Combine(Folder.Instance.backupFolder, $"BS{GameVersion.Version}-{now}.zip"));
            Directory.Delete(zipPath, true);
        }

        public void CleanModsTemp(string path)
        {
            if (!Directory.Exists(Folder.Instance.tmpFolder))
            {
                Console.WriteLine($"{Folder.Instance.tmpFolder}がありません");
                Console.WriteLine($"{Folder.Instance.tmpFolder}を作成します");
                Directory.CreateDirectory(Folder.Instance.tmpFolder);
            }
            DirectoryInfo dir = new DirectoryInfo(Folder.Instance.tmpFolder);

            //ディレクトリ以外の全ファイルを削除
            string[] filePaths = Directory.GetFiles(path);
            foreach (string filePath in filePaths)
            {
                File.SetAttributes(filePath, FileAttributes.Normal);
                File.Delete(filePath);
            }

            //ディレクトリの中のディレクトリも再帰的に削除
            string[] directiryPaths = Directory.GetDirectories(path);
            foreach (string directoryPath in directiryPaths)
            {
                CleanModsTemp(directoryPath);
            }

            if (path != Folder.Instance.tmpFolder)
            {
                Directory.Delete(path, false);
            }
        }
    }
}
