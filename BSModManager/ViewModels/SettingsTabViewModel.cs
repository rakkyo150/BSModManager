using BSModManager.Models;
using BSModManager.Static;
using Prism.Mvvm;
using Prism.Navigation;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace BSModManager.ViewModels
{
    public class SettingsTabViewModel : BindableBase, IDestructible
    {
        ConfigFile configFile;
        GitHubApi gitHubApi;
        SettingsVerifier settingsVerifier;

        CompositeDisposable disposables { get; } = new CompositeDisposable();

        public ReactiveCommand SelectBSFolder { get; } = new ReactiveCommand();
        public ReactiveCommand OpenBSFolder { get; }
        public ReactiveCommand ChangeToken { get; } = new ReactiveCommand();
        public ReactiveCommand SelectMAExe { get; } = new ReactiveCommand();
        public ReactiveCommand OpenMAFolder { get; } = new ReactiveCommand();
        public ReactiveCommand OpenDataFolder { get; } = new ReactiveCommand();
        public ReactiveCommand OpenBackupFolder { get; } = new ReactiveCommand();
        public ReactiveCommand OpenModTempFolder { get; } = new ReactiveCommand();


        public ReactiveProperty<string> BSFolderPath { get; } = new ReactiveProperty<string>(Folder.Instance.BSFolderPath);
        public ReactiveProperty<string> MAExePath { get; } = new ReactiveProperty<string>(FilePath.Instance.MAExePath);
        
        public ReactiveProperty<bool> OpenBSFolderButtonEnable { get; } = new ReactiveProperty<bool>();

        public ReactiveProperty<string> VerifyBSFolder { get; } = new ReactiveProperty<string>("〇");
        public ReactiveProperty<Brush> VerifyBSFolderColor { get; } = new ReactiveProperty<Brush>(Brushes.Green);
        public ReactiveProperty<string> VerifyGitHubToken { get; } = new ReactiveProperty<string>("〇");
        public ReactiveProperty<Brush> VerifyGitHubTokenColor { get; } = new ReactiveProperty<Brush>(Brushes.Green);
        public ReactiveProperty<string> VerifyMAExe { get; } = new ReactiveProperty<string>("〇");
        public ReactiveProperty<Brush> VerifyMAExeColor { get; } = new ReactiveProperty<Brush>(Brushes.Green);

        // IContainerProviderをDIしてResolveで取ってきてもOK
        public SettingsTabViewModel(ConfigFile cf,GitHubApi gha,SettingsVerifier sv)
        {
            configFile = cf;
            gitHubApi = gha;
            settingsVerifier = sv;

            BSFolderPath.AddTo(disposables);
            MAExePath.AddTo(disposables);
            OpenBSFolderButtonEnable.AddTo(disposables);
            VerifyBSFolder.AddTo(disposables);
            VerifyBSFolderColor.AddTo(disposables);
            VerifyGitHubToken.AddTo(disposables);
            VerifyGitHubTokenColor.AddTo(disposables);
            VerifyMAExe.AddTo(disposables);
            VerifyMAExeColor.AddTo(disposables);

            Folder.Instance.PropertyChanged += (sender, e) =>
            {
                BSFolderPath.Value = Folder.Instance.BSFolderPath;
                configFile.Generate(Folder.Instance.BSFolderPath, gitHubApi.GitHubToken, FilePath.Instance.MAExePath);
                if (Directory.Exists(Folder.Instance.BSFolderPath)) OpenBSFolderButtonEnable.Value = true;
                else OpenBSFolderButtonEnable.Value = false;
            };

            FilePath.Instance.PropertyChanged += (sender, e) =>
            {
                MAExePath.Value = FilePath.Instance.MAExePath;
                configFile.Generate(Folder.Instance.BSFolderPath, gitHubApi.GitHubToken, FilePath.Instance.MAExePath);
            };

            SelectBSFolder.Subscribe(_ =>
            {
                Folder.Instance.BSFolderPath = Folder.Instance.Select(Folder.Instance.BSFolderPath);
                MainWindowLog.Instance.Debug = Folder.Instance.BSFolderPath;
            }).AddTo(disposables);
            OpenBSFolder = OpenBSFolderButtonEnable
                .ToReactiveCommand()
                .WithSubscribe(() =>
                {
                    MainWindowLog.Instance.Debug = "Open BS Folder";
                    Folder.Instance.Open(Folder.Instance.BSFolderPath);
                    MainWindowLog.Instance.Debug = Folder.Instance.BSFolderPath;
                }).AddTo(disposables);
            ChangeToken.Subscribe((x) =>
            {
                gitHubApi.GitHubToken = ((PasswordBox)x).Password;
                MainWindowLog.Instance.Debug = "GitHub Token Changed";
            }).AddTo(disposables);
            SelectMAExe.Subscribe(() =>
            {
                MainWindowLog.Instance.Debug = "Select ModAssistant.exe";
                FilePath.Instance.MAExePath = FilePath.Instance.SelectFile(FilePath.Instance.MAExePath);
            }).AddTo(disposables);
            OpenMAFolder.Subscribe(() =>
            {
                MainWindowLog.Instance.Debug = "Open ModAssistant Folder";
                Folder.Instance.Open(Path.GetDirectoryName(FilePath.Instance.MAExePath));
            }).AddTo(disposables);
            OpenDataFolder.Subscribe(_ =>
            {
                MainWindowLog.Instance.Debug = "Open Data Folder";
                Folder.Instance.Open(Folder.Instance.dataFolder);
            }).AddTo(disposables);
            OpenBackupFolder.Subscribe(_ =>
            {
                MainWindowLog.Instance.Debug = "Open Backup Folder";
                Folder.Instance.Open(Folder.Instance.backupFolder);
            }).AddTo(disposables);
            OpenModTempFolder.Subscribe(_ =>
            {
                MainWindowLog.Instance.Debug = "Open Temp Folder";
                Folder.Instance.Open(Folder.Instance.tmpFolder);
            }).AddTo(disposables);


            if (!settingsVerifier.BSFolder)
            {
                VerifyBSFolder.Value = "×";
                VerifyBSFolderColor.Value = Brushes.Red;
            }
            if (!settingsVerifier.GitHubToken)
            {
                VerifyGitHubToken.Value = "×";
                VerifyGitHubTokenColor.Value = Brushes.Red;
            }
            if (!settingsVerifier.MAExe)
            {
                VerifyMAExe.Value = "×";
                VerifyMAExeColor.Value = Brushes.Red;
            }

            settingsVerifier.PropertyChanged += (sender, e) =>
            {
                if (settingsVerifier.BSFolder)
                {
                    VerifyBSFolder.Value = "〇";
                    VerifyBSFolderColor.Value = Brushes.Green;
                }
                else
                {
                    VerifyBSFolder.Value = "×";
                    VerifyBSFolderColor.Value = Brushes.Red;
                }

                if (settingsVerifier.GitHubToken)
                {
                    VerifyGitHubToken.Value = "〇";
                    VerifyGitHubTokenColor.Value = Brushes.Green;
                }
                else
                {
                    VerifyGitHubToken.Value = "×";
                    VerifyGitHubTokenColor.Value = Brushes.Red;
                }

                if (settingsVerifier.MAExe)
                {
                    VerifyMAExe.Value = "〇";
                    VerifyMAExeColor.Value = Brushes.Green;
                }
                else
                {
                    VerifyMAExe.Value = "×";
                    VerifyMAExeColor.Value = Brushes.Red;
                }
            };

            MainWindowLog.Instance.Debug = "Settings";
        }

        public void Destroy()
        {
            disposables.Dispose();
        }
    }
}
