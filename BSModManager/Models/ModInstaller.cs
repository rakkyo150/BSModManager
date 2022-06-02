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

            IEnumerable<IModData> CheckedModsData = mainModsSetter.MainMods.ReturnCheckedModsData();

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
                IModData checkedModDataPlusFileHash = SetFileHashToCheckedLocalMods(checkedModData);
                modDisposer.MoveFolder(Folder.Instance.tmpFolder, Folder.Instance.BSFolderPath);
                localModsDataModel.Add(checkedModDataPlusFileHash);
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

        private IModData SetFileHashToCheckedLocalMods(IModData modData)
        {
            if (!Directory.Exists(Path.Combine(Folder.Instance.tmpFolder, "Plugins")))
            {
                return modData;
            }

            IModData modDataPlusFileHashAndInstalled = new LocalModData(refresher);

            DirectoryInfo dir = new DirectoryInfo(Path.Combine(Folder.Instance.tmpFolder, "Plugins"));
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                if (!file.Name.Contains(".dll")) continue;

                modDataPlusFileHashAndInstalled = modData;

                modDataPlusFileHashAndInstalled.DownloadedFileHash = FileHashProvider.ComputeFileHash(file.FullName);
                modDataPlusFileHashAndInstalled.Installed = modData.Latest;
                ;
            }

            return modDataPlusFileHashAndInstalled;
        }
    }
}
