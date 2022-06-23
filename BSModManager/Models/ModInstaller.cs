using BSModManager.Interfaces;
using BSModManager.Models.Mods.Structures;
using BSModManager.Static;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BSModManager.Models
{
    public class ModInstaller
    {
        readonly LocalMods localModsDataModel;
        readonly GitHubApi gitHubApi;
        readonly ModDisposer modDisposer;
        readonly Refresher refresher;
        readonly SettingsVerifier settingsVerifier;
        readonly MainModsSetter mainModsSetter;

        public ModInstaller(LocalMods lmdm, GitHubApi gha, ModDisposer md, Refresher r, SettingsVerifier sv, MainModsSetter mms)
        {
            localModsDataModel = lmdm;
            gitHubApi = gha;
            modDisposer = md;
            refresher = r;
            settingsVerifier = sv;
            mainModsSetter = mms;
        }

        public async Task Install()
        {
            bool openMA = false;

            IEnumerable<IModData> CheckedLocalModsData = mainModsSetter.MainMods.ReturnCheckedModsData();

            if (CheckedLocalModsData.Count() == 0) return;

            foreach (var checkedLocalModData in CheckedLocalModsData)
            {
                if (checkedLocalModData.Installed >= checkedLocalModData.Latest) continue;

                if (checkedLocalModData.MA == "〇")
                {
                    openMA = true;
                    continue;
                }

                await gitHubApi.DownloadAsync(checkedLocalModData.Url, Folder.Instance.tmpFolder);
                modDisposer.Dispose(Folder.Instance.tmpFolder, Folder.Instance.tmpFolder);
                IModData checkedLocalModDataWithNewInstalledVersionAndFileHash = SetInstalledVersionAndFileHash(checkedLocalModData);
                modDisposer.MoveFolder(Folder.Instance.tmpFolder, Folder.Instance.BSFolderPath);
                localModsDataModel.Add(checkedLocalModDataWithNewInstalledVersionAndFileHash);
            }

            await refresher.Refresh();

            if (openMA && settingsVerifier.MAExe)
            {
                try
                {
                    System.Diagnostics.Process.Start(FilePath.Instance.MAExePath);
                }
                catch (Exception e)
                {
                    Logger.Instance.Error(e.Message);
                }
            }
        }

        private IModData SetInstalledVersionAndFileHash(IModData modData)
        {
            if (!Directory.Exists(Path.Combine(Folder.Instance.tmpFolder, "Plugins")))
            {
                return modData;
            }

            IModData modDataWithNewInstalledVersionAndFileHash = modData;

            DirectoryInfo dir = new DirectoryInfo(Path.Combine(Folder.Instance.tmpFolder, "Plugins"));
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                // 場合によってはPluginsフォルダの中に複数のdllファイルが入っているのでファイル名を確定させる
                if (file.Name != $"{modData.Mod}.dll") continue;

                modDataWithNewInstalledVersionAndFileHash.Installed = modDataWithNewInstalledVersionAndFileHash.Latest;
                modDataWithNewInstalledVersionAndFileHash.DownloadedFileHash = FileHashProvider.ComputeFileHash(file.FullName);
                break;
            }

            return modDataWithNewInstalledVersionAndFileHash;
        }
    }
}
