using BSModManager.Static;
using Octokit;
using System;
using System.Threading.Tasks;

namespace BSModManager.Models
{
    public class ModUpdater
    {
        LocalMods localModsDataModel;
        GitHubApi gitHubApi;
        ModDisposer modDisposer;
        Refresher refresher;

        public ModUpdater(LocalMods lmdm, GitHubApi gha, ModDisposer md,Refresher r)
        {
            localModsDataModel = lmdm;
            gitHubApi = gha;
            modDisposer = md;
            refresher = r;
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

            if (openMA)
            {
                try
                {
                    System.Diagnostics.Process.Start(FilePath.Instance.MAExePath);
                }
                catch (Exception e)
                {
                    MainWindowLog.Instance.Debug = e.Message;
                }
            }
        }
    }
}
