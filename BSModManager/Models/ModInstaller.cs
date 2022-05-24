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
        LocalMods localModsDataModel;
        GitHubApi gitHubApi;
        ModDisposer modDisposer;
        Syncer localModSyncer;

        public ModInstaller(LocalMods lmdm, GitHubApi gha, ModDisposer md, Syncer lms)
        {
            localModsDataModel = lmdm;
            gitHubApi = gha;
            modDisposer = md;
            localModSyncer = lms;
        }

        public void Install(IMods sourceModData)
        {
            bool openMA = false;

            IEnumerable<IModData> ModsData = sourceModData.ReturnCheckedModsData();

            if (ModsData.Count() == 0) return;

            foreach (var a in ModsData)
            {
                Release response = null;

                if (a.MA == "〇" && a.Installed < a.Latest && openMA == false)
                {
                    openMA = true;
                }

                if (a.MA == "×")
                {
                    Task.Run(async () => response = await gitHubApi.GetModLatestVersionAsync(a.Url)).GetAwaiter().GetResult();

                    if (response != null)
                    {
                        Task.Run(async () => await gitHubApi.DownloadAsync(a.Url, a.Installed, Folder.Instance.tmpFolder)).GetAwaiter().GetResult();
                        Task.Run(() => modDisposer.Dispose(Folder.Instance.tmpFolder, Folder.Instance.BSFolderPath)).GetAwaiter().GetResult();
                        localModsDataModel.Update(a);
                        sourceModData.Remove(a);
                    }
                }
            }

            localModSyncer.Sync();

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
