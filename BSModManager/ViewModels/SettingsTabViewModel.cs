using BSModManager.Models;
using BSModManager.Models.ViewModelCommonProperty;
using BSModManager.Static;
using Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Media;

namespace BSModManager.ViewModels
{
    public class SettingsTabViewModel : BindableBase, INotifyPropertyChanged
    {
        MainWindowPropertyModel mainWindowPropertyModel;
        SettingsTabPropertyModel settingsTabPropertyModel;

        public ReactiveCommand SelectBSFolder { get; } = new ReactiveCommand();
        public ReactiveCommand OpenBSFolder { get; }
        public ReactiveCommand ChangeToken { get; } = new ReactiveCommand();
        public ReactiveCommand OpenDataFolder { get; } = new ReactiveCommand();
        public ReactiveCommand OpenBackupFolder { get; } = new ReactiveCommand();
        public ReactiveCommand OpenModTempFolder { get; } = new ReactiveCommand();

        // IContainerProviderをDIしてResolveで取ってきてもOK
        public SettingsTabViewModel(MainWindowPropertyModel mwpm, SettingsTabPropertyModel stpm)
        {
            mainWindowPropertyModel = mwpm;
            settingsTabPropertyModel = stpm;

            this.BSFolderPath = settingsTabPropertyModel.ObserveProperty(x => x.BSFolderPath).ToReadOnlyReactivePropertySlim();
            this.VerifyBSFolder = settingsTabPropertyModel.VerifyBSFolder.ToReadOnlyReactivePropertySlim();
            this.VerifyBSFolderColor = settingsTabPropertyModel.VerifyBSFolderColor.ToReadOnlyReactivePropertySlim();
            this.VerifyGitHubToken = settingsTabPropertyModel.VerifyGitHubToken.ToReadOnlyReactivePropertySlim();
            this.VerifyGitHubTokenColor = settingsTabPropertyModel.VerifyGitHubTokenColor.ToReadOnlyReactivePropertySlim();

            SelectBSFolder.Subscribe(_ =>
            {
                settingsTabPropertyModel.BSFolderPath = FolderManager.SelectFolderCommand(settingsTabPropertyModel.BSFolderPath);
                mainWindowPropertyModel.Console = BSFolderPath.Value;
            });
            OpenBSFolder = settingsTabPropertyModel.OpenBSFolderButtonEnable
                .ToReactiveCommand()
                .WithSubscribe(() =>
                {
                    mainWindowPropertyModel.Console = "Open BS Folder";
                    FolderManager.OpenFolderCommand(settingsTabPropertyModel.BSFolderPath);
                    mainWindowPropertyModel.Console = BSFolderPath.Value;
                });
            ChangeToken.Subscribe((x) =>
            {
                settingsTabPropertyModel.GitHubToken = ((PasswordBox)x).Password;
                mainWindowPropertyModel.Console = "GitHub Token Changed";
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
                FolderManager.OpenFolderCommand(FolderManager.tempFolder);
            });

            mainWindowPropertyModel.Console = "Settings";
        }

        public ReadOnlyReactivePropertySlim<string> BSFolderPath { get; }
        public ReadOnlyReactivePropertySlim<string> VerifyBSFolder { get; }
        public ReadOnlyReactivePropertySlim<Brush> VerifyBSFolderColor { get; }

        public ReadOnlyReactivePropertySlim<string> VerifyGitHubToken { get; }
        public ReadOnlyReactivePropertySlim<Brush> VerifyGitHubTokenColor { get; }
    }
}
