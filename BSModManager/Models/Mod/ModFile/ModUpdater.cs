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
        readonly GitHubApi gitHubApi;
        readonly ModDisposer modDisposer;
        readonly Refresher refresher;
        readonly ModsContainerAgent modsDataContainerAgent;

        public ModUpdater(GitHubApi gha, ModDisposer md, Refresher r,ModsContainerAgent mdca)
        {
            gitHubApi = gha;
            modDisposer = md;
            refresher = r;
            modsDataContainerAgent = mdca;
        }

        public async Task Update()
        {
            bool openMA = false;

            IEnumerable<IMod> CheckedLocalModsData = modsDataContainerAgent.LocalModsContainer.ReturnCheckedModsData();

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
                IMod checkedLocalModDataWithNewInstalledVersionAndFileHash = SetNewInstalledVersionAndFileHash(checkedLocalModData);
                modDisposer.MoveFolder(Folder.Instance.tmpFolder, Config.Instance.BSFolderPath);
                modsDataContainerAgent.LocalModsContainer.UpdateDownloadedFileHash(checkedLocalModDataWithNewInstalledVersionAndFileHash);
                modsDataContainerAgent.LocalModsContainer.UpdateInstalled(checkedLocalModDataWithNewInstalledVersionAndFileHash);
            }

            await refresher.Refresh();

            if (openMA && Config.Instance.MAExeVerification)
            {
                try
                {
                    System.Diagnostics.Process.Start(Config.Instance.MAExePath);
                }
                catch (Exception e)
                {
                    Logger.Instance.Error(e.Message);
                }
            }
        }

        private IMod SetNewInstalledVersionAndFileHash(IMod modData)
        {
            if (!Directory.Exists(Path.Combine(Folder.Instance.tmpFolder, "Plugins")))
            {
                return modData;
            }

            IMod modDataWithNewInstalledVersionAndFileHash = modData;

            DirectoryInfo dir = new DirectoryInfo(Path.Combine(Folder.Instance.tmpFolder, "Plugins"));
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                // 場合によってはPluginsフォルダの中に複数のdllファイルが入っているのでファイル名を確定させる
                if (file.Name!=$"{modData.Mod}.dll") continue;

                modDataWithNewInstalledVersionAndFileHash.Installed = modDataWithNewInstalledVersionAndFileHash.Latest;
                modDataWithNewInstalledVersionAndFileHash.DownloadedFileHash = FileHashProvider.ComputeFileHash(file.FullName);
                break;
            }

            return modDataWithNewInstalledVersionAndFileHash;
        }
    }
}
