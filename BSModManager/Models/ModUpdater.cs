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

        public void Update()
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

                Task.Run(async () => response = await gitHubApi.GetModLatestVersionAsync(a.Url)).GetAwaiter().GetResult();
                if (response != null)
                {
                    Task.Run(async () => await gitHubApi.DownloadAsync(a.Url, a.Installed, Folder.Instance.tmpFolder)).GetAwaiter().GetResult();
                    Task.Run(() => modDisposer.Dispose(Folder.Instance.tmpFolder, Folder.Instance.BSFolderPath)).GetAwaiter().GetResult();
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
