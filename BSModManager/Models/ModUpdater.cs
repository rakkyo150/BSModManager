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
    public class ModUpdater
    {
        readonly LocalMods localModsDataModel;
        readonly GitHubApi gitHubApi;
        readonly ModDisposer modDisposer;
        readonly Refresher refresher;
        readonly SettingsVerifier settingsVerifier;

        public ModUpdater(LocalMods lmdm, GitHubApi gha, ModDisposer md, Refresher r, SettingsVerifier sv)
        {
            localModsDataModel = lmdm;
            gitHubApi = gha;
            modDisposer = md;
            refresher = r;
            settingsVerifier = sv;
        }

        public async Task Update()
        {
            bool openMA = false;

            IEnumerable<IModData> CheckedModsData = localModsDataModel.ReturnCheckedModsData();

            if (CheckedModsData.Count() == 0) return;

            foreach (var checkedModData in CheckedModsData)
            {
                if (checkedModData.Installed >= checkedModData.Latest) continue;

                if (checkedModData.MA == "〇")
                {
                    openMA = true;
                    continue;
                }

                await gitHubApi.DownloadAsync(checkedModData.Url, Folder.Instance.tmpFolder);
                modDisposer.Dispose(Folder.Instance.tmpFolder, Folder.Instance.tmpFolder);
                IModData checkedModDataPlusFileHash = SetFileHashToLocalMods(checkedModData);
                modDisposer.MoveFolder(Folder.Instance.tmpFolder, Folder.Instance.BSFolderPath);
                localModsDataModel.UpdateDownloadedFileHash(checkedModDataPlusFileHash);
                localModsDataModel.UpdateInstalled(new LocalModData(refresher)
                {
                    Mod = checkedModDataPlusFileHash.Mod,
                    Installed = checkedModDataPlusFileHash.Latest
                });
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

        private IModData SetFileHashToLocalMods(IModData modData)
        {
            if (!Directory.Exists(Path.Combine(Folder.Instance.tmpFolder, "Plugins")))
            {
                return modData;
            }

            IModData modDataPlusFileHash = new LocalModData(refresher);

            DirectoryInfo dir = new DirectoryInfo(Path.Combine(Folder.Instance.tmpFolder, "Plugins"));
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                if (!file.Name.Contains(".dll")) continue;

                modDataPlusFileHash = modData;

                modDataPlusFileHash.DownloadedFileHash = FileHashProvider.ComputeFileHash(file.FullName);
            }

            return modDataPlusFileHash;
        }
    }
}
