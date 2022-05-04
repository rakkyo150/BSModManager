using BSModManager.Models.Structure;
using BSModManager.Models.ViewModelCommonProperty;
using BSModManager.Static;
using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BSModManager.Models.CoreManager
{
    public class DataManager
    {
        InnerData innerData;
        SettingsTabPropertyModel settingsTabPropertyModel;
        UpdateMyselfConfirmPropertyModel updateMyselfConfirmPropertyModel;
        MainTabPropertyModel mainTabPropertyModel;
        
        public DataManager(InnerData id,SettingsTabPropertyModel stpm,UpdateMyselfConfirmPropertyModel umcpm, MainTabPropertyModel mtpm)
        {
            innerData = id;
            settingsTabPropertyModel = stpm;
            updateMyselfConfirmPropertyModel = umcpm;
            mainTabPropertyModel = mtpm;
        }

        /// <summary>
        /// <para>ローカル情報取得</para>
        /// 第二引数はcsvを基にModのバージョンを取得する(nullならファイルバージョンを利用)
        /// </summary>
        /// <param name="pluginsFolderPath"></param>
        /// <returns></returns>
        public void GetLocalModFilesInfo()
        {
            // Console.WriteLine("Start Getting FileInfo");

            string pluginFolderPath = Path.Combine(settingsTabPropertyModel.BSFolderPath, "Plugins");
            
            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(pluginFolderPath);
            IEnumerable<System.IO.FileInfo> filesName = di.EnumerateFiles("*.dll", System.IO.SearchOption.AllDirectories);
            foreach (System.IO.FileInfo f in filesName)
            {
                string pluginPath = Path.Combine(pluginFolderPath, f.Name);
                System.Diagnostics.FileVersionInfo vi = System.Diagnostics.FileVersionInfo.GetVersionInfo(pluginPath);
                Version installedModVersion = new Version(vi.FileVersion);
                if (mainTabPropertyModel.ModsData != null && !mainTabPropertyModel.ModsData.Any(x=>x.Mod==f.Name.Replace(".dll", "")))
                {
                    mainTabPropertyModel.ModsData.Add(new MainTabPropertyModel.ModData()
                    {
                        Mod = f.Name.Replace(".dll", ""),
                        Installed = installedModVersion
                    });
                }
            }
        }

        /// <summary>
        /// イニシャライズ時の処理で、ModAssistantのModの処理
        /// </summary>
        /// <param name="item"></param>
        /// <param name="fileAndVersion"></param>
        /// <param name="loopBreaklocalFileSearchLoopBreak"></param>
        /// <returns></returns>
        public bool DetectMAModAndRemoveFromManagementForInitialize(ModAssistantModInformation item, KeyValuePair<string, Version> fileAndVersion, out bool loopBreaklocalFileSearchLoopBreak)
        {
            loopBreaklocalFileSearchLoopBreak = false;
            bool passInputGithubModInformation = false;


            if (item.name == fileAndVersion.Key)
            {
                loopBreaklocalFileSearchLoopBreak = true;

                Version modAssistantModVersion = new Version(item.version);
                if (modAssistantModVersion >= fileAndVersion.Value)
                {
                    Console.WriteLine(item.name + "はModAssistantにあります");
                }
                else
                {
                    Console.WriteLine(item.name + "はModAssistantにありますが、ローカルにあるのは改造版の可能性が高いです");
                }

                Console.WriteLine(item.name + "をModAssistantで管理しますか？ [y/n]");
                string manageInModAssistant = Console.ReadLine();

                if (manageInModAssistant == "y")
                {
                    Console.WriteLine("ModAssistantModData.csvにデータを追加します");
                    Console.WriteLine("データを書き換えたい場合、このcsvを直接書き換えてください");

                    var modAssistantCsvInstance = new MAModInformationCsv()
                    {
                        ModAssistantMod = fileAndVersion.Key,
                        LocalVersion = fileAndVersion.Value.ToString(),
                        ModAssistantVersion = item.version,
                    };
                   innerData.detectedModAssistantModCsvListForInitialize.Add(modAssistantCsvInstance);

                    passInputGithubModInformation = true;
                }
            }

            return passInputGithubModInformation;
        }

        /// <summary>
        /// アップデート時の処理で、前回実行時から追加されたModAssistantのModの処理
        /// </summary>
        /// <param name="item"></param>
        public void DetectAddedMAModForUpdate(ModAssistantModInformation item)
        {
            if (innerData.nowLocalGithubModAndVersionAndOriginalBoolAndUrl.ContainsKey(item.name))
            {
                if (innerData.nowLocalGithubModAndVersionAndOriginalBoolAndUrl[item.name].Item2)
                {
                    Console.WriteLine(item.name + "はオリジナルModとして登録されており、かつModAssistantにあります");
                    Console.WriteLine($"よって、{ item.name} を管理から外します");

                    innerData.nowLocalGithubModAndVersionAndOriginalBoolAndUrl.Remove(item.name);
                    innerData.installedGitHubModInformationToCsvForUpdate.Remove(innerData.installedGitHubModInformationToCsvForUpdate.Find(n => n.GithubMod == item.name));
                }
            }
        }

        /// <summary>
        /// アップデート時の処理で、ローカル増加分でModAssistantにあるModの処理
        /// </summary>
        /// <param name="item"></param>
        /// <param name="fileAndVersion"></param>
        /// <returns></returns>
        public bool DetectMAModAndRemoveFromManagementForUpdate(ModAssistantModInformation item, KeyValuePair<string, Version> fileAndVersion)
        {
            bool passInputGithubModInformation = true;


            if (!innerData.installedMAMod.Contains(item.name) && item.name == fileAndVersion.Key)
            {
                Version modAssistantModVersion = new Version(item.version);
                if (modAssistantModVersion >= fileAndVersion.Value)
                {
                    Console.WriteLine(item.name + "はModAssistantにあります");
                }
                else
                {
                    Console.WriteLine(item.name + "はModAssistantにありますが、ローカルにあるのは改造版の可能性が高いです");
                }

                Console.WriteLine(item.name + "をModAssistantで管理しますか？ [y/n]");
                string manageInModAssistant = Console.ReadLine();

                if (manageInModAssistant == "y")
                {
                    passInputGithubModInformation = true;
                }
                else
                {
                    passInputGithubModInformation = false;
                }
            }

            return passInputGithubModInformation;
        }


        /// <summary>
        /// ローカルファイルの差分を取得
        /// </summary>
        /// <param name="gitHubManager"></param>
        public async Task ManageLocalPluginsDiffAsync(GitHubManager gitHubManager)
        {
            // ローカルファイル減少分
            foreach (var a in innerData.nowLocalGithubModAndVersionAndOriginalBoolAndUrl)
            {
                if (!innerData.nowLocalFilesInfoDictionary.ContainsKey(a.Key))
                {
                    innerData.nowLocalGithubModAndVersionAndOriginalBoolAndUrl.Remove(a.Key);
                    innerData.installedGitHubModInformationToCsvForUpdate.Remove(innerData.installedGitHubModInformationToCsvForUpdate.Find(n => n.GithubMod == a.Key));
                }
            }
            // ローカルファイル増加分
            foreach (var a in innerData.nowLocalFilesInfoDictionary)
            {
                if (!innerData.nowLocalGithubModAndVersionAndOriginalBoolAndUrl.ContainsKey(a.Key) && !Array.Exists(innerData.modAssistantAllMods, element => element.name == a.Key))
                {
                    await gitHubManager.InputGitHubModInformationAsync(new KeyValuePair<string, Version>(a.Key, a.Value), innerData.installedGitHubModInformationToCsvForUpdate);
                    Tuple<Version, bool, string> tempGithubModInformation = new Tuple<Version, bool, string>(
                        new Version(innerData.installedGitHubModInformationToCsvForUpdate.Find(n => n.GithubMod == a.Key).LocalVersion),
                        innerData.installedGitHubModInformationToCsvForUpdate.Find(n => n.GithubMod == a.Key).OriginalMod,
                        innerData.installedGitHubModInformationToCsvForUpdate.Find(n => n.GithubMod == a.Key).GithubUrl
                    );
                    innerData.nowLocalGithubModAndVersionAndOriginalBoolAndUrl[a.Key] = tempGithubModInformation;
                }
            }
        }

        /// <summary>
        /// Beat Saberのバージョンを取得
        /// </summary>
        /// <returns></returns>
        public string GetGameVersion()
        {
            string filename = Path.Combine(settingsTabPropertyModel.BSFolderPath, "Beat Saber_Data", "globalgamemanagers");
            using (var stream = File.OpenRead(filename))
            using (var reader = new BinaryReader(stream, Encoding.UTF8))
            {
                const string key = "public.app-category.games";
                int pos = 0;

                while (stream.Position < stream.Length && pos < key.Length)
                {
                    if (reader.ReadByte() == key[pos]) pos++;
                    else pos = 0;
                }

                if (stream.Position == stream.Length) // we went through the entire stream without finding the key
                    return null;

                while (stream.Position < stream.Length)
                {
                    var current = (char)reader.ReadByte();
                    if (char.IsDigit(current))
                        break;
                }

                var rewind = -sizeof(int) - sizeof(byte);
                stream.Seek(rewind, SeekOrigin.Current); // rewind to the string length

                var strlen = reader.ReadInt32();
                var strbytes = reader.ReadBytes(strlen);

                return Encoding.UTF8.GetString(strbytes);
            }
        }

        /// <summary>
        /// csvにリストの情報を書き込み
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="csvPath"></param>
        /// <param name="list"></param>
        public void WriteCsv<T>(string csvPath, List<T> list)
        {
            using (var writer = new StreamWriter(csvPath, false))
            using (var csv = new CsvWriter(writer, new CultureInfo("ja-JP", false)))
            {
                csv.WriteRecords(list);
            }
        }

        public void ReadCsv<T>(string csvPath, out List<T> listOutput)
        {
            using (var reader = new StreamReader(csvPath))
            using (var csv = new CsvReader(reader, new CultureInfo("ja-JP", false)))
            {
                listOutput = csv.GetRecords<T>().ToList();
            }
        }

        public void UpdateModAssistantModCsv()
        {
            if (innerData.modAssistantAllMods != null)
            {
                foreach (var a in innerData.modAssistantAllMods)
                {
                    if (!innerData.nowLocalGithubModAndVersionAndOriginalBoolAndUrl.ContainsKey(a.name) &&innerData.nowLocalFilesInfoDictionary.ContainsKey(a.name))
                    {
                        MAModInformationCsv modAssistantCsvInstance = new MAModInformationCsv()
                        {
                            ModAssistantMod = a.name,
                            LocalVersion = innerData.nowLocalFilesInfoDictionary[a.name].ToString(),
                            ModAssistantVersion = a.version,
                        };

                        innerData.modAssistantModCsvListForUpdate.Add(modAssistantCsvInstance);
                    }
                }

                WriteCsv(FilePath.mAModCsvPath, innerData.modAssistantModCsvListForUpdate);
            }
        }

        // https://docs.microsoft.com/ja-jp/dotnet/standard/io/how-to-copy-directories
        /// <summary>
        /// ディレクトリ内のディレクトリとファイルコピー(上書き)
        /// </summary>
        /// <param name="sourceDirName"></param>
        /// <param name="destDirName"></param>
        /// <param name="copySubDirs"></param>
        /// <exception cref="DirectoryNotFoundException"></exception>
        public void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();

            // If the destination directory doesn't exist, create it.       
            Directory.CreateDirectory(destDirName);

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string tempPath = Path.Combine(destDirName, file.Name);
                file.CopyTo(tempPath, true);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string tempPath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, tempPath, copySubDirs);
                }
            }
        }

        // sourceDirFullPathはModが一時的に保存される場所、destDirFullPathはBeat Saberディレクトリと同じ構造のもの
        /// <summary>
        /// ダウンロードしたModのフォルダ構造をローカルにインストール可能な状態に組み替えてコピーする
        /// </summary>
        /// <param name="sourceDirFullPath"></param>
        /// <param name="destDirFullPath"></param>
        /// <returns></returns>
        public void OrganizeDownloadFileStructure(string sourceDirFullPath, string destDirFullPath)
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

        public void Backup()
        {
            if (!Directory.Exists(FolderManager.backupFolder))
            {
                Console.WriteLine($"{FolderManager.backupFolder}がありません");
                Console.WriteLine($"{FolderManager.backupFolder}を作成します");
                Directory.CreateDirectory(FolderManager.backupFolder);
            }
            if (!Directory.Exists(FolderManager.dataFolder))
            {
                Console.WriteLine($"{FolderManager.dataFolder}がありません");
                Console.WriteLine($"{FolderManager.dataFolder}を作成します");
                Directory.CreateDirectory(FolderManager.dataFolder);
            }

            innerData.gameVersion = GetGameVersion();
            string now = DateTime.Now.ToString("yyyyMMddHHmmss");
            string zipPath = Path.Combine(FolderManager.backupFolder, $"BS{innerData.gameVersion}-{now}");
            Directory.CreateDirectory(zipPath);

            DirectoryCopy(Path.Combine(settingsTabPropertyModel.BSFolderPath, "Plugins"),
                Path.Combine(zipPath, "Plugins"), true);
            DirectoryCopy(FolderManager.dataFolder, Path.Combine(zipPath, "Data"), true);
            File.Copy(FilePath.configFilePath, Path.Combine(zipPath, "config.json"), true);

            ZipFile.CreateFromDirectory(zipPath, Path.Combine(FolderManager.backupFolder, $"BS{innerData.gameVersion}-{now}.zip"));
            Directory.Delete(zipPath, true);
        }

        public void CleanModsTemp(string path)
        {
            if (!Directory.Exists(FolderManager.modTempFolder))
            {
                Console.WriteLine($"{FolderManager.modTempFolder}がありません");
                Console.WriteLine($"{FolderManager.modTempFolder}を作成します");
                Directory.CreateDirectory(FolderManager.modTempFolder);
            }
            DirectoryInfo dir = new DirectoryInfo(FolderManager.modTempFolder);

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

            if (path != FolderManager.modTempFolder)
            {
                Directory.Delete(path, false);
            }
        }

        public void UpdateUpdater()
        {
            string downloadPath = Path.Combine(Environment.CurrentDirectory, updateMyselfConfirmPropertyModel.LatestMyselfVersion.ToString());
            try
            {
                if (Directory.Exists(downloadPath))
                {
                    DirectoryInfo dir = new DirectoryInfo(downloadPath);

                    FileInfo[] files = dir.GetFiles();
                    foreach (FileInfo file in files)
                    {
                        if (file.Name.Contains("Updater") && !file.Name.Contains("GithubModUpdateCheckerConsole"))
                        {
                            string tempPath = Path.Combine(Environment.CurrentDirectory, file.Name);
                            file.CopyTo(tempPath, true);
                        }
                    }
                    Console.WriteLine("Updaterのアップデート完了");
                }
                else
                {
                    Console.WriteLine("Updaterのアップデートができませんでした");
                    Console.WriteLine("最新バージョンのフォルダが生成されているはずなので、手動で中身を上書きコピペしてください");
                    Console.ReadLine();
                    Environment.Exit(0);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.WriteLine("正常にアップデートができませんでした");
                Console.WriteLine("最新バージョンのフォルダが生成されているはずなので、手動で中身を上書きコピペしてください");
                Console.ReadLine();
                Environment.Exit(0);
            }
        }
    }
}
