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

        public async Task Install(IMods sourceModData)
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

                response = await gitHubApi.GetLatestReleaseAsync(a.Url);

                if (response != null)
                {
                    await gitHubApi.DownloadAsync(a.Url,Folder.Instance.tmpFolder);
                    modDisposer.Dispose(Folder.Instance.tmpFolder, Folder.Instance.BSFolderPath);
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
