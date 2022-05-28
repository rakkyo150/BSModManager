using BSModManager.Static;
using Octokit;
using System;
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

        public ModUpdater(LocalMods lmdm, GitHubApi gha, ModDisposer md,Refresher r,SettingsVerifier sv)
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

            foreach (var a in localModsDataModel.LocalModsData)
            {
                Release response = null;

                if (!a.Checked) continue;

                if (a.Installed >= a.Latest) continue;

                if (a.MA == "〇")
                {
                    openMA = true;
                    continue;
                }

                response = await gitHubApi.GetLatestReleaseInfoAsync(a.Url);
                if (response != null)
                {
                    await gitHubApi.DownloadAsync(a.Url, Folder.Instance.tmpFolder);
                    modDisposer.Dispose(Folder.Instance.tmpFolder, Folder.Instance.BSFolderPath);
                }
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
    }
}
