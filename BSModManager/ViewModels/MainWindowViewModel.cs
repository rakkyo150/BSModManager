using BSModManager.Models;
using ModManager.Views;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using System.Collections.Generic;

namespace ModManager.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        private string title = "BSModManager";
        public string Title
        {
            get { return title; }
            set { SetProperty(ref title, value); }
        }

        private string console = "Hello World";

        public string Console
        {
            get { return console; }
            set { SetProperty(ref console, value); }
        }

        private string gameVersion = "GameVersion\n---";
        public string GameVersion
        {
            get { return gameVersion; }
            set { SetProperty(ref gameVersion, value); }
        }

        private string myselfVersion = "Version\n---";
        public string MyselfVersion
        {
            get { return myselfVersion; }
            set { SetProperty(ref myselfVersion, value); }
        }

        ConfigFileManager configFileManager;
        VersionManager versionManager;

        public IRegionManager RegionManager { get; private set; }
        public DelegateCommand<string> ShowMainTabViewCommand { get; private set; }
        public DelegateCommand<string> ShowSettingsTabViewCommand { get; private set; }

        public MainWindowViewModel(IRegionManager regionManager, ConfigFileManager cfm, VersionManager vm)
        {
            configFileManager = cfm;
            versionManager = vm;

            Dictionary<string, string> tempDictionary = configFileManager.LoadConfigFile();
            if (tempDictionary["BSFolderPath"] != null)
            {
                GameVersion = versionManager.GetGameVersion(tempDictionary["BSFolderPath"]);
            }

            MyselfVersion = versionManager.GetMyselfVersion();

            RegionManager = regionManager;
            regionManager.RegisterViewWithRegion("ContentRegion", typeof(MainTab));
            ShowMainTabViewCommand = new DelegateCommand<string>((x) =>
              {
                  Console = "Main";
                  RegionManager.RequestNavigate("ContentRegion", x);
              });
            ShowSettingsTabViewCommand = new DelegateCommand<string>((x) =>
              {
                  Console = "Settings";
                  RegionManager.RequestNavigate("ContentRegion", x);
              });
        }
    }
}
