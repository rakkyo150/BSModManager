using BSModManager.Models;
using BSModManager.Models.ViewModelCommonProperty;
using BSModManager.Static;
using Prism.Mvvm;
using Prism.Navigation;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.ComponentModel;
using System.IO;
using System.Reactive.Disposables;
using System.Windows.Controls;
using System.Windows.Media;

namespace BSModManager.ViewModels
{
    public class SettingsTabViewModel : BindableBase, INotifyPropertyChanged,IDestructible
    {
        MainWindowPropertyModel mainWindowPropertyModel;
        SettingsTabPropertyModel settingsTabPropertyModel;

        CompositeDisposable disposables { get; } = new CompositeDisposable();
        
        public ReactiveCommand SelectBSFolder { get; } = new ReactiveCommand();
        public ReactiveCommand OpenBSFolder { get; }
        public ReactiveCommand ChangeToken { get; } = new ReactiveCommand();
        public ReactiveCommand SelectMAExe { get; } = new ReactiveCommand();
        public ReactiveCommand OpenMAFolder { get; } = new ReactiveCommand();
        public ReactiveCommand OpenDataFolder { get; } = new ReactiveCommand();
        public ReactiveCommand OpenBackupFolder { get; } = new ReactiveCommand();
        public ReactiveCommand OpenModTempFolder { get; } = new ReactiveCommand();

        // IContainerProviderをDIしてResolveで取ってきてもOK
        public SettingsTabViewModel(MainWindowPropertyModel mwpm, SettingsTabPropertyModel stpm)
        {
            mainWindowPropertyModel = mwpm;
            settingsTabPropertyModel = stpm;

            this.BSFolderPath = settingsTabPropertyModel.ObserveProperty(x => x.BSFolderPath).ToReadOnlyReactivePropertySlim().AddTo(disposables);
            this.VerifyBSFolder = settingsTabPropertyModel.VerifyBSFolder.ToReadOnlyReactivePropertySlim().AddTo(disposables);
            this.VerifyBSFolderColor = settingsTabPropertyModel.VerifyBSFolderColor.ToReadOnlyReactivePropertySlim().AddTo(disposables);
            this.VerifyGitHubToken = settingsTabPropertyModel.VerifyGitHubToken.ToReadOnlyReactivePropertySlim().AddTo(disposables);
            this.VerifyGitHubTokenColor = settingsTabPropertyModel.VerifyGitHubTokenColor.ToReadOnlyReactivePropertySlim().AddTo(disposables);
            this.MAExePath = settingsTabPropertyModel.ObserveProperty(x => x.MAExePath).ToReadOnlyReactivePropertySlim().AddTo(disposables);
            this.VerifyMAExe = settingsTabPropertyModel.VerifyMAExe.ToReadOnlyReactivePropertySlim().AddTo(disposables);
            this.VerifyMAExeColor = settingsTabPropertyModel.VerifyMAExeColor.ToReadOnlyReactivePropertySlim().AddTo(disposables);

            SelectBSFolder.Subscribe(_ =>
            {
                settingsTabPropertyModel.BSFolderPath = FolderManager.SelectFolderCommand(settingsTabPropertyModel.BSFolderPath);
                mainWindowPropertyModel.Console = BSFolderPath.Value;
            }).AddTo(disposables);
            OpenBSFolder = settingsTabPropertyModel.OpenBSFolderButtonEnable
                .ToReactiveCommand()
                .WithSubscribe(() =>
                {
                    mainWindowPropertyModel.Console = "Open BS Folder";
                    FolderManager.OpenFolderCommand(settingsTabPropertyModel.BSFolderPath);
                    mainWindowPropertyModel.Console = BSFolderPath.Value;
                }).AddTo(disposables);
            ChangeToken.Subscribe((x) =>
            {
                settingsTabPropertyModel.GitHubToken = ((PasswordBox)x).Password;
                mainWindowPropertyModel.Console = "GitHub Token Changed";
            }).AddTo(disposables);
            SelectMAExe.Subscribe(() =>
            {
                mainWindowPropertyModel.Console = "Select ModAssistant.exe";
                settingsTabPropertyModel.MAExePath = FilePath.SelectFile(settingsTabPropertyModel.MAExePath);
            }).AddTo(disposables);
            OpenMAFolder.Subscribe(() =>
            {
                mainWindowPropertyModel.Console = "Open ModAssistant Folder";
                FolderManager.OpenFolderCommand(Path.GetDirectoryName(MAExePath.Value));
            }).AddTo(disposables);
            OpenDataFolder.Subscribe(_ =>
            {
                mainWindowPropertyModel.Console = "Open Data Folder";
                FolderManager.OpenFolderCommand(FolderManager.dataFolder);
            }).AddTo(disposables);
            OpenBackupFolder.Subscribe(_ =>
            {
                mainWindowPropertyModel.Console = "Open Backup Folder";
                FolderManager.OpenFolderCommand(FolderManager.backupFolder);
            }).AddTo(disposables);
            OpenModTempFolder.Subscribe(_ =>
            {
                mainWindowPropertyModel.Console = "Open Temp Folder";
                FolderManager.OpenFolderCommand(FolderManager.tempFolder);
            }).AddTo(disposables);

            mainWindowPropertyModel.Console = "Settings";
        }

        public ReadOnlyReactivePropertySlim<string> BSFolderPath { get; }
        public ReadOnlyReactivePropertySlim<string> VerifyBSFolder { get; }
        public ReadOnlyReactivePropertySlim<Brush> VerifyBSFolderColor { get; }

        public ReadOnlyReactivePropertySlim<string> VerifyGitHubToken { get; }
        public ReadOnlyReactivePropertySlim<Brush> VerifyGitHubTokenColor { get; }

        public ReadOnlyReactivePropertySlim<string> MAExePath { get; }
        public ReadOnlyReactivePropertySlim<string> VerifyMAExe { get; }
        public ReadOnlyReactivePropertySlim<Brush> VerifyMAExeColor { get; }

        public void Destroy()
        {
            disposables.Dispose();
        }
    }
}
