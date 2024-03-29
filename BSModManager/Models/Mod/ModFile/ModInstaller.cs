﻿using BSModManager.Interfaces;
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
        private readonly GitHubApi gitHubApi;
        private readonly ModDisposer modDisposer;
        private readonly Refresher refresher;
        private readonly ModsContainerAgent modsDataContainerAgent;

        public ModInstaller(GitHubApi gha, ModDisposer md, Refresher r, ModsContainerAgent mms)
        {
            gitHubApi = gha;
            modDisposer = md;
            refresher = r;
            modsDataContainerAgent = mms;
        }

        public async Task Install()
        {
            bool openMA = false;

            IEnumerable<IMod> CheckedLocalModsData = modsDataContainerAgent.ActiveMods.ReturnCheckedModsData();

            if (CheckedLocalModsData.Count() == 0) return;

            foreach (IMod checkedLocalModData in CheckedLocalModsData)
            {
                if (checkedLocalModData.Installed >= checkedLocalModData.Latest) continue;

                if (checkedLocalModData.MA == "〇")
                {
                    openMA = true;
                    continue;
                }

                await gitHubApi.DownloadAsync(checkedLocalModData.Url, Folder.Instance.tmpFolder);
                modDisposer.Dispose(Folder.Instance.tmpFolder, Folder.Instance.tmpFolder);
                IMod checkedLocalModDataWithNewInstalledVersionAndFileHash = SetInstalledVersionAndFileHash(checkedLocalModData);
                modDisposer.MoveFolder(Folder.Instance.tmpFolder, Config.Instance.BSFolderPath);
                modsDataContainerAgent.LocalModsContainer.Add(checkedLocalModDataWithNewInstalledVersionAndFileHash);
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

        private IMod SetInstalledVersionAndFileHash(IMod modData)
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
                if (file.Name != $"{modData.Mod}.dll") continue;

                modDataWithNewInstalledVersionAndFileHash.Installed = modDataWithNewInstalledVersionAndFileHash.Latest;
                modDataWithNewInstalledVersionAndFileHash.DownloadedFileHash = FileHashProvider.ComputeFileHash(file.FullName);
                break;
            }

            return modDataWithNewInstalledVersionAndFileHash;
        }
    }
}
