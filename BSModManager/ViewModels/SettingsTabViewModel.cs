using BSModManager.Models;
using BSModManager.Models.ViewModelPropertyModel;
using BSModManager.Static;
using Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Controls;

namespace BSModManager.ViewModels
{
    public class SettingsTabViewModel : BindableBase, INotifyPropertyChanged
    {
        MainWindowPropertyModel mainWindowPropertyModel;
        SettingsTabPropertyModel settingsTabPropertyModel;

        public ReactiveCommand SelectBSFolder { get; } = new ReactiveCommand();
        public ReactiveCommand OpenBSFolder { get; } = new ReactiveCommand();
        public ReactiveCommand ChangeToken { get; } = new ReactiveCommand();
        public ReactiveCommand OpenDataFolder { get; } = new ReactiveCommand();
        public ReactiveCommand OpenBackupFolder { get; } = new ReactiveCommand();
        public ReactiveCommand OpenModTempFolder { get; } = new ReactiveCommand();

        // IContainerProviderをDIしてResolveで取ってきてもOK
        public SettingsTabViewModel(MainWindowPropertyModel mwpm,SettingsTabPropertyModel stpm)
        {
            mainWindowPropertyModel = mwpm;
            settingsTabPropertyModel = stpm;

            this.BSFolderPath = settingsTabPropertyModel.ObserveProperty(x=>x.BSFolderPath).ToReadOnlyReactivePropertySlim();
            this.GitHubToken = settingsTabPropertyModel.ObserveProperty(x => x.GitHubToken).ToReadOnlyReactivePropertySlim();

            SelectBSFolder.Subscribe(_ => settingsTabPropertyModel.BSFolderPath = FolderManager.SelectFolderCommand(settingsTabPropertyModel.BSFolderPath));
            OpenBSFolder = settingsTabPropertyModel.OpenBSFolderButtonEnable
                .ToReactiveCommand()
                .WithSubscribe(() =>
                {
                    mainWindowPropertyModel.Console = "Open BS Folder";
                    FolderManager.OpenFolderCommand(settingsTabPropertyModel.BSFolderPath);
                });
            ChangeToken.Subscribe((x) =>
            {
                settingsTabPropertyModel.GitHubToken = ((PasswordBox)x).Password;
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

        public ReadOnlyReactivePropertySlim<string> BSFolderPath { get; }
        public ReadOnlyReactivePropertySlim<string> GitHubToken { get; }


    }
}
