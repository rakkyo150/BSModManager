using BSModManager.Models;
using BSModManager.Static;
using Prism.Mvvm;
using Prism.Navigation;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Reactive.Disposables;
using System.Windows.Controls;
using System.Windows.Media;

namespace BSModManager.ViewModels
{
    public class InitialSettingViewModel : BindableBase, IDialogAware, IDestructible
    {
        private CompositeDisposable Disposables { get; } = new CompositeDisposable();

        public ReactiveProperty<string> VerifyBSFolder { get; }
        public ReactiveProperty<Brush> VerifyBSFolderColor { get; }

        public ReactiveProperty<string> BSFolderPath { get; }

        public ReactiveCommand SelectBSFolderCommand { get; } = new ReactiveCommand();

        public ReactiveProperty<string> VerifyGitHubToken { get; }
        public ReactiveProperty<Brush> VerifyGitHubTokenColor { get; }

        public ReactiveProperty<bool> VerifyBSFolderAndGitHubToken { get; }

        public ReactiveProperty<string> VerifyMAExe { get; }
        public ReactiveProperty<Brush> VerifyMAExeColor { get; }

        public ReactiveProperty<string> MAExePath { get; }

        public ReactiveCommand SelectMAExeCommand { get; } = new ReactiveCommand();

        public ReactiveCommand SettingFinishCommand { get; }
        public ReactiveCommand VerifyGitHubTokenCommand { get; } = new ReactiveCommand();

        internal InitialSettingViewModel()
        {
            // https://whitedog0215.hatenablog.jp/entry/2020/03/17/221403
            BSFolderPath = Config.Instance.ToReactivePropertyAsSynchronized(x => x.BSFolderPath).AddTo(Disposables);
            MAExePath = Config.Instance.ToReactivePropertyAsSynchronized(x => x.MAExePath).AddTo(Disposables);

            VerifyBSFolder = new ReactiveProperty<string>("〇").AddTo(Disposables);
            VerifyBSFolderColor = new ReactiveProperty<Brush>(Brushes.Green).AddTo(Disposables);
            VerifyGitHubToken = new ReactiveProperty<string>("〇").AddTo(Disposables);
            VerifyGitHubTokenColor = new ReactiveProperty<Brush>(Brushes.Green).AddTo(Disposables);
            VerifyBSFolderAndGitHubToken = new ReactiveProperty<bool>(true).AddTo(Disposables);
            VerifyMAExe = new ReactiveProperty<string>("〇").AddTo(Disposables);
            VerifyMAExeColor = new ReactiveProperty<Brush>(Brushes.Green).AddTo(Disposables);

            Update();

            Config.Instance.PropertyChanged += (sender, e) =>
            {
                Update();
            };

            SelectBSFolderCommand.Subscribe(() =>
            {
                Config.Instance.BSFolderPath = Folder.Instance.Select(Config.Instance.BSFolderPath);
                Config.Instance.Update();
            }).AddTo(Disposables);

            SelectMAExeCommand.Subscribe(() =>
            {
                Config.Instance.MAExePath = FilePath.Instance.SelectFile(Config.Instance.MAExePath);
                Config.Instance.Update();
            }).AddTo(Disposables);

            SettingFinishCommand = VerifyBSFolderAndGitHubToken.ToReactiveCommand()
                .WithSubscribe(() => RequestClose.Invoke(new DialogResult(ButtonResult.OK))).AddTo(Disposables);

            VerifyGitHubTokenCommand.Subscribe((x) =>
            {
                Config.Instance.GitHubToken = ((PasswordBox)x).Password;
            }).AddTo(Disposables);
        }

        private void Update()
        {
            BSFolderPath.Value = Config.Instance.BSFolderPath;
            VerifyBSFolder.Value = Config.Instance.BSFolderVerificationString;
            VerifyBSFolderColor.Value = Config.Instance.BSFolderVerificationColor;
            VerifyGitHubToken.Value = Config.Instance.GitHubTokenVerificationString;
            VerifyGitHubTokenColor.Value = Config.Instance.GitHubTokenVerificationColor;
            MAExePath.Value = Config.Instance.MAExePath;
            VerifyMAExe.Value = Config.Instance.MAExeVerificationString;
            VerifyMAExeColor.Value = Config.Instance.MAExeVerificationColor;

            VerifyBSFolderAndGitHubToken.Value = Config.Instance.BSFolderAndGitHubTokenVerification;
        }

        public string Title => "Initial Setting";

        public event Action<IDialogResult> RequestClose;

        public bool CanCloseDialog() => SettingFinishCommand.CanExecute();

        public void OnDialogClosed()
        {
        }

        public void OnDialogOpened(IDialogParameters _)
        {
        }

        public void Destroy()
        {
            Disposables.Dispose();
        }
    }
}
