using Prism.Mvvm;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BSModManager.Models.ViewModelPropertyModel
{
    public class SettingsTabPropertyModel: BindableBase
    {
        MainWindowPropertyModel mainWindowPropertyModel;
        VersionManager versionManager;
        ConfigFileManager configFileManager;
        
        public SettingsTabPropertyModel(MainWindowPropertyModel mwpm,VersionManager vm,ConfigFileManager cfm)
        {
            mainWindowPropertyModel = mwpm;
            versionManager = vm;
            configFileManager = cfm;

            Dictionary<string, string> tempDictionary = configFileManager.LoadConfigFile();
            if (tempDictionary["BSFolderPath"] != null && tempDictionary["GitHubToken"] != null)
            {
                BSFolderPath = tempDictionary["BSFolderPath"];
                GitHubToken = tempDictionary["GitHubToken"];
            }
        }
        
        // テスト用にあえてパス間違えてる
        private string bSFolderPath = @"C:\Program Files (x86)\Steam\steamapps\common\Beat Sabe";
        public string BSFolderPath
        {
            get => bSFolderPath;
            set
            {
                SetProperty(ref bSFolderPath, value);
                mainWindowPropertyModel.GameVersion = versionManager.GetGameVersion(BSFolderPath);
                configFileManager.MakeConfigFile(BSFolderPath, GitHubToken);
                mainWindowPropertyModel.Console = BSFolderPath;
                if (Directory.Exists(BSFolderPath)) OpenBSFolderButtonEnable.Value = true;
                else OpenBSFolderButtonEnable.Value = false;
            }
        }

        private string gitHubToken = "";
        public string GitHubToken
        {
            get => gitHubToken;
            set
            {
                SetProperty(ref gitHubToken, value);
                configFileManager.MakeConfigFile(BSFolderPath, GitHubToken);
                mainWindowPropertyModel.Console = "GitHub Token Changed";
            }
        }

        public ReactiveProperty<bool> OpenBSFolderButtonEnable { get; private set; } = new ReactiveProperty<bool>();
    }
}
