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
        private CompositeDisposable Disposables { get; } = new CompositeDisposable();

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


        public ReactiveProperty<string> BSFolderPath { get; } = new ReactiveProperty<string>(Config.Instance.BSFolderPath);
        public ReactiveProperty<string> MAExePath { get; } = new ReactiveProperty<string>(Config.Instance.MAExePath);

        public ReactiveProperty<bool> OpenBSFolderButtonEnable { get; } = new ReactiveProperty<bool>(true);

        public ReactiveProperty<string> VerifyBSFolder { get; } = new ReactiveProperty<string>("〇");
        public ReactiveProperty<Brush> VerifyBSFolderColor { get; } = new ReactiveProperty<Brush>(Brushes.Green);
        public ReactiveProperty<string> VerifyGitHubToken { get; } = new ReactiveProperty<string>("〇");
        public ReactiveProperty<Brush> VerifyGitHubTokenColor { get; } = new ReactiveProperty<Brush>(Brushes.Green);
        public ReactiveProperty<string> VerifyMAExe { get; } = new ReactiveProperty<string>("〇");
        public ReactiveProperty<Brush> VerifyMAExeColor { get; } = new ReactiveProperty<Brush>(Brushes.Green);

        // IContainerProviderをDIしてResolveで取ってきてもOK
        public SettingsTabViewModel()
        {
            BSFolderPath.AddTo(Disposables);
            MAExePath.AddTo(Disposables);
            OpenBSFolderButtonEnable.AddTo(Disposables);
            VerifyBSFolder.AddTo(Disposables);
            VerifyBSFolderColor.AddTo(Disposables);
            VerifyGitHubToken.AddTo(Disposables);
            VerifyGitHubTokenColor.AddTo(Disposables);
            VerifyMAExe.AddTo(Disposables);
            VerifyMAExeColor.AddTo(Disposables);

            Update();

            Config.Instance.PropertyChanged += (sender, e) =>
            {
                Update();
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
                Config.Instance.BSFolderPath = Folder.Instance.Select(Config.Instance.BSFolderPath);
                Logger.Instance.Info(Config.Instance.BSFolderPath);
            }).AddTo(Disposables);

            OpenBSFolder = OpenBSFolderButtonEnable
                .ToReactiveCommand()
                .WithSubscribe(() =>
                {
                    Logger.Instance.Info("Open BS Folder");
                    Folder.Instance.Open(Config.Instance.BSFolderPath);
                    Logger.Instance.Info(Config.Instance.BSFolderPath);
                }).AddTo(Disposables);

            ChangeToken.Subscribe((x) =>
            {
                Config.Instance.GitHubToken = ((PasswordBox)x).Password;
                Logger.Instance.Info("GitHub Token Changed");
            }).AddTo(Disposables);

            SelectMAExe.Subscribe(() =>
            {
                Logger.Instance.Info("Select ModAssistant.exe");
                Config.Instance.MAExePath = FilePath.Instance.SelectFile(Config.Instance.MAExePath);
            }).AddTo(Disposables);

            OpenMAFolder.Subscribe(() =>
            {
                Logger.Instance.Info("Open ModAssistant Folder");
                Folder.Instance.Open(Path.GetDirectoryName(Config.Instance.MAExePath));
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

            Logger.Instance.Info("Settings");
        }

        private void Update()
        {
            BSFolderPath.Value = Config.Instance.BSFolderPath;
            VerifyBSFolder.Value = Config.Instance.BSFolderVerificationString;
            VerifyBSFolderColor.Value = Config.Instance.BSFolderVerificationColor;
            OpenBSFolderButtonEnable.Value = Config.Instance.BSFolderVerification;
            VerifyGitHubToken.Value = Config.Instance.GitHubTokenVerificationString;
            VerifyGitHubTokenColor.Value = Config.Instance.GitHubTokenVerificationColor;
            MAExePath.Value = Config.Instance.MAExePath;
            VerifyMAExe.Value = Config.Instance.MAExeVerificationString;
            VerifyMAExeColor.Value = Config.Instance.MAExeVerificationColor;
            OpenBSFolderButtonEnable.Value = Config.Instance.MAExeVerification;
        }

        public void Destroy()
        {
            Disposables.Dispose();
        }
    }
}
