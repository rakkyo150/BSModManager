using BSModManager.Models.CoreManager;
using BSModManager.Models.ViewModelCommonProperty;
using BSModManager.Static;
using Octokit;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace BSModManager.Models
{
    public class UpdateTabPropertyModel : BindableBase
    {
        public MainWindowPropertyModel mainWindowPropertyModel;
        public SettingsTabPropertyModel settingsTabPropertyModel;
        public GitHubManager gitHubManager;
        public ModsDataModel modsDataModel;
        DataManager dataManager;

        public UpdateTabPropertyModel(MainWindowPropertyModel mwpm,SettingsTabPropertyModel stpm, GitHubManager gm,ModsDataModel mdm,DataManager dm)
        {
            mainWindowPropertyModel = mwpm;
            settingsTabPropertyModel = stpm;
            gitHubManager = gm;
            modsDataModel = mdm;
            dataManager = dm;
        }

        public void Update()
        {
            bool openMA = false;

            foreach (var a in modsDataModel.ModsData)
            {
                Release response=null;

                if (a.Checked)
                {
                    if (a.MA == "〇" && a.Installed < a.Latest && openMA == false)
                    {
                        openMA = true;
                    }

                    if(a.MA == "×")
                    {
                        Task.Run(async() => response = await gitHubManager.GetGitHubModLatestVersionAsync(a.Url)).GetAwaiter().GetResult();
                        if (response != null)
                        {
                            Task.Run(async() => await gitHubManager.DownloadGitHubModAsync(a.Url, a.Installed, FolderManager.tempFolder, a.Mod)).GetAwaiter().GetResult();
                            Task.Run(()=>dataManager.OrganizeDownloadFileStructure(FolderManager.tempFolder, settingsTabPropertyModel.BSFolderPath)).GetAwaiter().GetResult();
                        }
                    }
                }
            }

            dataManager.GetLocalModFilesInfo();

            if (openMA)
            {
                try
                {
                    System.Diagnostics.Process.Start(settingsTabPropertyModel.MAExePath);
                }
                catch (Exception e)
                {
                    mainWindowPropertyModel.Console = e.Message;
                }
            }
        }
    }
}
