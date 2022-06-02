using BSModManager.Models;
using BSModManager.Static;
using Prism.Mvvm;
using Prism.Navigation;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Diagnostics;
using System.IO;
using System.Reactive.Disposables;
using System.Windows.Controls;
using System.Windows.Media;

namespace BSModManager.ViewModels
{
    public class SettingsTabViewModel : BindableBase, IDestructible
    {
        readonly ConfigFileHandler configFile;
        readonly GitHubApi gitHubApi;
        readonly SettingsVerifier settingsVerifier;

        CompositeDisposable Disposables { get; } = new CompositeDisposable();

        public ReactiveCommand OpenBSModManagerRepositoryCommand { get; } = new ReactiveCommand();
        public ReactiveCommand SelectBSFolder { get; } = new ReactiveCommand();
        public ReactiveCommand OpenBSFolder { get; }
        public ReactiveCommand ChangeToken { get; } = new ReactiveCommand();
        public ReactiveCommand SelectMAExe { get; } = new ReactiveCommand();
        public ReactiveCommand OpenMAFolder { get; } = new ReactiveCommand();
        public ReactiveCommand OpenLogFolder { get; } = new ReactiveCommand();
        public ReactiveCommand OpenDataFolder { get; } = new ReactiveCommand();
        public ReactiveCommand OpenBackupFolder { get; } = new ReactiveCommand();
        public ReactiveCommand OpenModTempFolder { get; } = new ReactiveCommand();


        public ReactiveProperty<string> BSFolderPath { get; } = new ReactiveProperty<string>(Folder.Instance.BSFolderPath);
        public ReactiveProperty<string> MAExePath { get; } = new ReactiveProperty<string>(FilePath.Instance.MAExePath);

        public ReactiveProperty<bool> OpenBSFolderButtonEnable { get; } = new ReactiveProperty<bool>(true);

        public ReactiveProperty<string> VerifyBSFolder { get; } = new ReactiveProperty<string>("〇");
        public ReactiveProperty<Brush> VerifyBSFolderColor { get; } = new ReactiveProperty<Brush>(Brushes.Green);
        public ReactiveProperty<string> VerifyGitHubToken { get; } = new ReactiveProperty<string>("〇");
        public ReactiveProperty<Brush> VerifyGitHubTokenColor { get; } = new ReactiveProperty<Brush>(Brushes.Green);
        public ReactiveProperty<string> VerifyMAExe { get; } = new ReactiveProperty<string>("〇");
        public ReactiveProperty<Brush> VerifyMAExeColor { get; } = new ReactiveProperty<Brush>(Brushes.Green);

        // IContainerProviderをDIしてResolveで取ってきてもOK
        public SettingsTabViewModel(ConfigFileHandler cf, GitHubApi gha, SettingsVerifier sv)
        {
            configFile = cf;
            gitHubApi = gha;
            settingsVerifier = sv;

            BSFolderPath.AddTo(Disposables);
            MAExePath.AddTo(Disposables);
            OpenBSFolderButtonEnable.AddTo(Disposables);
            VerifyBSFolder.AddTo(Disposables);
            VerifyBSFolderColor.AddTo(Disposables);
            VerifyGitHubToken.AddTo(Disposables);
            VerifyGitHubTokenColor.AddTo(Disposables);
            VerifyMAExe.AddTo(Disposables);
            VerifyMAExeColor.AddTo(Disposables);

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

            OpenBSModManagerRepositoryCommand.Subscribe(() =>
            {
                try
                {
                    string searchUrl = $"https://github.com/rakkyo150/BSModManager";
                    ProcessStartInfo pi = new ProcessStartInfo()
                    {
                        FileName = searchUrl,
                        UseShellExecute = true,
                    };
                    Process.Start(pi);
                }
                catch (Exception ex)
                {
                    Logger.Instance.Error(ex.Message + "\nBSModManagerのリポジトリを開けませんでした");
                }
            }).AddTo(Disposables);

            SelectBSFolder.Subscribe(_ =>
            {
                Folder.Instance.BSFolderPath = Folder.Instance.Select(Folder.Instance.BSFolderPath);
                Logger.Instance.Info(Folder.Instance.BSFolderPath);
            }).AddTo(Disposables);

            OpenBSFolder = OpenBSFolderButtonEnable
                .ToReactiveCommand()
                .WithSubscribe(() =>
                {
                    Logger.Instance.Info("Open BS Folder");
                    Folder.Instance.Open(Folder.Instance.BSFolderPath);
                    Logger.Instance.Info(Folder.Instance.BSFolderPath);
                }).AddTo(Disposables);

            ChangeToken.Subscribe((x) =>
            {
                gitHubApi.GitHubToken = ((PasswordBox)x).Password;
                configFile.Generate(Folder.Instance.BSFolderPath, gitHubApi.GitHubToken, FilePath.Instance.MAExePath);
                Logger.Instance.Info("GitHub Token Changed");
            }).AddTo(Disposables);

            SelectMAExe.Subscribe(() =>
            {
                Logger.Instance.Info("Select ModAssistant.exe");
                FilePath.Instance.MAExePath = FilePath.Instance.SelectFile(FilePath.Instance.MAExePath);
            }).AddTo(Disposables);

            OpenMAFolder.Subscribe(() =>
            {
                Logger.Instance.Info("Open ModAssistant Folder");
                Folder.Instance.Open(Path.GetDirectoryName(FilePath.Instance.MAExePath));
            }).AddTo(Disposables);

            OpenLogFolder.Subscribe(_ =>
            {
                Logger.Instance.Info("Open Log Folder");
                Folder.Instance.Open(Folder.Instance.logFolder);
            }).AddTo(Disposables);

            OpenDataFolder.Subscribe(_ =>
            {
                Logger.Instance.Info("Open Data Folder");
                Folder.Instance.Open(Folder.Instance.dataFolder);
            }).AddTo(Disposables);

            OpenBackupFolder.Subscribe(_ =>
            {
                Logger.Instance.Info("Open Backup Folder");
                Folder.Instance.Open(Folder.Instance.backupFolder);
            }).AddTo(Disposables);

            OpenModTempFolder.Subscribe(_ =>
            {
                Logger.Instance.Info("Open Temp Folder");
                Folder.Instance.Open(Folder.Instance.tmpFolder);
            }).AddTo(Disposables);


            if (!settingsVerifier.BSFolder)
            {
                VerifyBSFolder.Value = "×";
                VerifyBSFolderColor.Value = Brushes.Red;
                OpenBSFolderButtonEnable.Value = false;
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
                    OpenBSFolderButtonEnable.Value = true;
                }
                else
                {
                    VerifyBSFolder.Value = "×";
                    VerifyBSFolderColor.Value = Brushes.Red;
                    OpenBSFolderButtonEnable.Value = false;
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

            Logger.Instance.Info("Settings");
        }

        public void Destroy()
        {
            Disposables.Dispose();
        }
    }
}
