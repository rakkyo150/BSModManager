using Prism.Mvvm;
using System.Collections.Generic;

namespace BSModManager.Models
{
    public class MainWindowPropertyModel : BindableBase
    {
        private VersionManager versionManager;
        private ConfigFileManager configFileManager;

        private string console = "Hello World";
        public string Console
        {
            get { return console; }
            set { SetProperty(ref console, value); }
        }

        private string gameVersion;
        public string GameVersion
        {
            get { return gameVersion; }
            set { SetProperty(ref gameVersion, value); }
        }

        public MainWindowPropertyModel(VersionManager vm, ConfigFileManager cfm)
        {
            versionManager = vm;
            configFileManager = cfm;

            Dictionary<string, string> tempDictionary = configFileManager.LoadConfigFile();
            if (tempDictionary["BSFolderPath"] != null)
            {
                GameVersion = versionManager.GetGameVersionStr(tempDictionary["BSFolderPath"]);
            }
        }
    }
}
