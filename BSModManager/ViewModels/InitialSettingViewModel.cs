using BSModManager.Models.CoreManager;
using BSModManager.Models.ViewModelCommonProperty;
using BSModManager.Static;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace BSModManager.ViewModels
{
    public class InitialSettingViewModel : BindableBase, IDialogAware
    {
        // 別のクラスから呼んでもらう必要あり
        SettingsTabPropertyModel settingsTabPropertyModel;

        DataManager dataManager;

        public ReadOnlyReactivePropertySlim<string> VerifyBSFolder { get; }
        public ReadOnlyReactivePropertySlim<Brush> VerifyBSFolderColor { get; }

        public ReactiveProperty<string> BSFolderPath { get; }

        public ReactiveCommand SelectBSFolderCommand { get; } = new ReactiveCommand();

        public ReadOnlyReactivePropertySlim<string> VerifyGitHubToken { get; }
        public ReadOnlyReactivePropertySlim<Brush> VerifyGitHubTokenColor { get; }
        public ReactiveCommand SettingFinishCommand { get; }
        public ReactiveCommand VerifyGitHubTokenCommand { get; } = new ReactiveCommand();

        public InitialSettingViewModel(SettingsTabPropertyModel stpm, DataManager dm)
        {
            settingsTabPropertyModel = stpm;
            dataManager = dm;

            // https://whitedog0215.hatenablog.jp/entry/2020/03/17/221403
            BSFolderPath = settingsTabPropertyModel.ToReactivePropertyAsSynchronized(x => x.BSFolderPath);

            VerifyBSFolder = settingsTabPropertyModel.VerifyBSFolder.ToReadOnlyReactivePropertySlim();
            VerifyBSFolderColor = settingsTabPropertyModel.VerifyBSFolderColor.ToReadOnlyReactivePropertySlim();

            VerifyGitHubToken = settingsTabPropertyModel.VerifyGitHubToken.ToReadOnlyReactivePropertySlim();
            VerifyGitHubTokenColor = settingsTabPropertyModel.VerifyGitHubTokenColor.ToReadOnlyReactivePropertySlim();

            SelectBSFolderCommand.Subscribe(_ => BSFolderPath.Value = FolderManager.SelectFolderCommand(BSFolderPath.Value));

            SettingFinishCommand = settingsTabPropertyModel.VerifyBoth.ToReactiveCommand().WithSubscribe(() => RequestClose.Invoke(new DialogResult(ButtonResult.OK)));

            VerifyGitHubTokenCommand.Subscribe((x) =>
            {
                settingsTabPropertyModel.GitHubToken = ((PasswordBox)x).Password;
            });
        }

        public string Title => "Initial Setting";

        public event Action<IDialogResult> RequestClose;

        public bool CanCloseDialog() => SettingFinishCommand.CanExecute();

        public void OnDialogClosed()
        {
            Task.Run(() => { dataManager.GetLocalModFilesInfo(); }).GetAwaiter().GetResult();
        }

        public void OnDialogOpened(IDialogParameters _)
        {
        }
    }
}
