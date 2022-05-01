using BSModManager.Models;
using BSModManager.Static;
using Prism.Mvvm;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Controls;

namespace ModManager.ViewModels
{
    public class SettingsTabViewModel : BindableBase, INotifyPropertyChanged
    {
        MainWindowPropertyModel mainWindowPropertyModel;
        ConfigFileManager configFileManager;
        VersionManager versionManager;

        public ReactiveCommand SelectBSFolder { get; } = new ReactiveCommand();
        public ReactiveCommand OpenBSFolder { get; } = new ReactiveCommand();
        public ReactiveCommand ChangeToken { get; } = new ReactiveCommand();
        public ReactiveCommand OpenDataFolder { get; } = new ReactiveCommand();
        public ReactiveCommand OpenBackupFolder { get; } = new ReactiveCommand();
        public ReactiveCommand OpenModTempFolder { get; } = new ReactiveCommand();

        // IContainerProviderをDIしてResolveで取ってきてもOK
        public SettingsTabViewModel(ConfigFileManager cfm, VersionManager vm,MainWindowPropertyModel mwpm)
        {
            configFileManager = cfm;
            versionManager = vm;
            mainWindowPropertyModel = mwpm;

            Dictionary<string, string> tempDictionary = configFileManager.LoadConfigFile();
            if (tempDictionary["BSFolderPath"] != null && tempDictionary["GitHubToken"] != null)
            {
                BSFolderPath = tempDictionary["BSFolderPath"];
                GitHubToken = tempDictionary["GitHubToken"];
            }

            SelectBSFolder.Subscribe(_ => BSFolderPath = FolderManager.SelectFolderCommand(BSFolderPath));
            OpenBSFolder.Subscribe(_ =>
            {
                mainWindowPropertyModel.Console = "Open BS Folder";
                FolderManager.OpenFolderCommand(BSFolderPath);
            });
            ChangeToken.Subscribe((x) =>
            {
                GitHubToken = ((PasswordBox)x).Password;
            });
            OpenDataFolder.Subscribe(_ =>
            {
                mainWindowPropertyModel.Console = "Open Data Folder";
                FolderManager.OpenFolderCommand(FolderManager.dataFolder);
            });
            OpenBackupFolder.Subscribe(_ =>
            {
                mainWindowPropertyModel.Console = "Open Backup Folder";
                FolderManager.OpenFolderCommand(FolderManager.backupFolder);
            });
            OpenModTempFolder.Subscribe(_ =>
            {
                mainWindowPropertyModel.Console = "Open Temp Folder";
                FolderManager.OpenFolderCommand(FolderManager.modTempFolder);
            });

            mainWindowPropertyModel.Console = "Settings";
        }

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
                if (Directory.Exists(BSFolderPath)) OpenBSFolderButton = true;
                else OpenBSFolderButton = false;
            }
        }

        private bool openBSFolderButton = false;
        public bool OpenBSFolderButton
        {
            get => openBSFolderButton;
            set => SetProperty(ref openBSFolderButton, value);
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
    }
}
