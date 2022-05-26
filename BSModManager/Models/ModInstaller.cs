using BSModManager.Interfaces;
using BSModManager.Static;
using Octokit;
using System;
using System.Collections.Generic;
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

        public ModInstaller(LocalMods lmdm, GitHubApi gha, ModDisposer md,Refresher r,SettingsVerifier sv)
        {
            localModsDataModel = lmdm;
            gitHubApi = gha;
            modDisposer = md;
            refresher = r;
            settingsVerifier = sv;
        }

        public void Install(IMods sourceModData)
        {
            bool openMA = false;

            IEnumerable<IModData> ModsData = sourceModData.ReturnCheckedModsData();

            if (ModsData.Count() == 0) return;

            foreach (var a in ModsData)
            {
                Release response = null;

                if (a.Installed >= a.Latest) continue;
                
                if (a.MA == "〇")
                {
                    openMA = true;
                    continue;
                }

                Task.Run(async () => response = await gitHubApi.GetLatestReleaseAsync(a.Url)).GetAwaiter().GetResult();

                if (response != null)
                {
                    Task.Run(async () => await gitHubApi.DownloadAsync(a.Url,Folder.Instance.tmpFolder)).GetAwaiter().GetResult();
                    Task.Run(() => modDisposer.Dispose(Folder.Instance.tmpFolder, Folder.Instance.BSFolderPath)).GetAwaiter().GetResult();
                    localModsDataModel.Add(a);
                }
            }

            Task.Run(() => refresher.Refresh()).GetAwaiter().GetResult();

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
    }
}
