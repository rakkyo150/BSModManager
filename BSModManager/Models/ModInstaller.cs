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
        readonly MainModsSetter mainModsSetter;

        public ModInstaller(LocalMods lmdm, GitHubApi gha, ModDisposer md,Refresher r,SettingsVerifier sv,MainModsSetter mms)
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

            IEnumerable<IModData> ModsData = mainModsSetter.MainMods.ReturnCheckedModsData();

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

                response = await gitHubApi.GetLatestReleaseInfoAsync(a.Url);

                if (response != null)
                {
                    await gitHubApi.DownloadAsync(a.Url,Folder.Instance.tmpFolder);
                    modDisposer.Dispose(Folder.Instance.tmpFolder, Folder.Instance.BSFolderPath);
                    localModsDataModel.Add(a);
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
