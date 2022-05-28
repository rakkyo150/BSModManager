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
        readonly SettingsVerifier settingsVerifier;
        readonly GitHubApi gitHubApi;
        readonly ConfigFileHandler configFile;

        CompositeDisposable Disposables { get; } = new CompositeDisposable();

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

        internal InitialSettingViewModel(SettingsVerifier sv, ConfigFileHandler cf)
        {
            settingsVerifier = sv;
            configFile = cf;

            // https://whitedog0215.hatenablog.jp/entry/2020/03/17/221403
            BSFolderPath = Folder.Instance.ToReactivePropertyAsSynchronized(x => x.BSFolderPath).AddTo(Disposables);
            MAExePath = FilePath.Instance.ToReactivePropertyAsSynchronized(x => x.MAExePath).AddTo(Disposables);

            VerifyBSFolder = new ReactiveProperty<string>("〇").AddTo(Disposables);
            VerifyBSFolderColor = new ReactiveProperty<Brush>(Brushes.Green).AddTo(Disposables);
            VerifyGitHubToken = new ReactiveProperty<string>("〇").AddTo(Disposables);
            VerifyGitHubTokenColor = new ReactiveProperty<Brush>(Brushes.Green).AddTo(Disposables);
            VerifyBSFolderAndGitHubToken = new ReactiveProperty<bool>(true).AddTo(Disposables);
            VerifyMAExe = new ReactiveProperty<string>("〇").AddTo(Disposables);
            VerifyMAExeColor = new ReactiveProperty<Brush>(Brushes.Green).AddTo(Disposables);

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
            if (!settingsVerifier.BSFolderAndGitHubToken)
            {
                VerifyBSFolderAndGitHubToken.Value = false;
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

                VerifyBSFolderAndGitHubToken.Value = settingsVerifier.BSFolderAndGitHubToken;

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

            SelectBSFolderCommand.Subscribe(() =>
            {
                Folder.Instance.BSFolderPath = Folder.Instance.Select(Folder.Instance.BSFolderPath);
                configFile.Generate(Folder.Instance.BSFolderPath, gitHubApi.GitHubToken, FilePath.Instance.MAExePath);
            }).AddTo(Disposables);

            SelectMAExeCommand.Subscribe(() =>
            {
                FilePath.Instance.MAExePath = FilePath.Instance.SelectFile(FilePath.Instance.MAExePath);
                configFile.Generate(Folder.Instance.BSFolderPath, gitHubApi.GitHubToken, FilePath.Instance.MAExePath);
            }).AddTo(Disposables);

            SettingFinishCommand = VerifyBSFolderAndGitHubToken.ToReactiveCommand()
                .WithSubscribe(() => RequestClose.Invoke(new DialogResult(ButtonResult.OK))).AddTo(Disposables);

            VerifyGitHubTokenCommand.Subscribe((x) =>
            {
                gitHubApi.GitHubToken = ((PasswordBox)x).Password;
            }).AddTo(Disposables);
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
