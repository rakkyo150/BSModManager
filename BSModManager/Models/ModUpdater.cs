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

        public ModUpdater(LocalMods lmdm, GitHubApi gha, ModDisposer md, Refresher r)
        {
            localModsDataModel = lmdm;
            gitHubApi = gha;
            modDisposer = md;
            refresher = r;
        }

        public async Task Update()
        {
            bool openMA = false;

            IEnumerable<IModData> CheckedLocalModsData = localModsDataModel.ReturnCheckedModsData();

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
                IModData checkedLocalModDataWithNewInstalledVersionAndFileHash = SetNewInstalledVersionAndFileHash(checkedLocalModData);
                modDisposer.MoveFolder(Folder.Instance.tmpFolder, Config.Instance.BSFolderPath);
                localModsDataModel.UpdateDownloadedFileHash(checkedLocalModDataWithNewInstalledVersionAndFileHash);
                localModsDataModel.UpdateInstalled(checkedLocalModDataWithNewInstalledVersionAndFileHash);
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

        private IModData SetNewInstalledVersionAndFileHash(IModData modData)
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
                if (file.Name!=$"{modData.Mod}.dll") continue;

                modDataWithNewInstalledVersionAndFileHash.Installed = modDataWithNewInstalledVersionAndFileHash.Latest;
                modDataWithNewInstalledVersionAndFileHash.DownloadedFileHash = FileHashProvider.ComputeFileHash(file.FullName);
                break;
            }

            return modDataWithNewInstalledVersionAndFileHash;
        }
    }
}
