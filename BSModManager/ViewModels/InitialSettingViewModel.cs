using BSModManager.Models;
using BSModManager.Models.CoreManager;
using BSModManager.Models.ViewModelCommonProperty;
using BSModManager.Static;
using Prism.Mvvm;
using Prism.Navigation;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;

namespace BSModManager.ViewModels
{
    public class InitialSettingViewModel : BindableBase, IDialogAware,IDestructible
    {
        // 別のクラスから呼んでもらう必要あり
        SettingsTabPropertyModel settingsTabPropertyModel;

        DataManager dataManager;

        ModAssistantManager modAssistantManager;
        InnerData innerData;

        MainWindowPropertyModel mainWindowPropertyModel;

        CompositeDisposable disposables { get; } = new CompositeDisposable();
        
        public ReadOnlyReactivePropertySlim<string> VerifyBSFolder { get; }
        public ReadOnlyReactivePropertySlim<Brush> VerifyBSFolderColor { get; }

        public ReactiveProperty<string> BSFolderPath { get; }

        public ReactiveCommand SelectBSFolderCommand { get; } = new ReactiveCommand();

        public ReadOnlyReactivePropertySlim<string> VerifyGitHubToken { get; }
        public ReadOnlyReactivePropertySlim<Brush> VerifyGitHubTokenColor { get; }
        public ReactiveCommand SettingFinishCommand { get; }
        public ReactiveCommand VerifyGitHubTokenCommand { get; } = new ReactiveCommand();

        public InitialSettingViewModel(SettingsTabPropertyModel stpm, DataManager dm,ModAssistantManager mam,InnerData id,MainWindowPropertyModel mwpm)
        {
            settingsTabPropertyModel = stpm;
            dataManager = dm;
            modAssistantManager = mam;
            innerData = id;
            mainWindowPropertyModel = mwpm;

            // https://whitedog0215.hatenablog.jp/entry/2020/03/17/221403
            BSFolderPath = settingsTabPropertyModel.ToReactivePropertyAsSynchronized(x => x.BSFolderPath).AddTo(disposables);

            VerifyBSFolder = settingsTabPropertyModel.VerifyBSFolder.ToReadOnlyReactivePropertySlim().AddTo(disposables);
            VerifyBSFolderColor = settingsTabPropertyModel.VerifyBSFolderColor.ToReadOnlyReactivePropertySlim().AddTo(disposables);

            VerifyGitHubToken = settingsTabPropertyModel.VerifyGitHubToken.ToReadOnlyReactivePropertySlim().AddTo(disposables);
            VerifyGitHubTokenColor = settingsTabPropertyModel.VerifyGitHubTokenColor.ToReadOnlyReactivePropertySlim().AddTo(disposables);

            SelectBSFolderCommand.Subscribe(_ => BSFolderPath.Value = FolderManager.SelectFolderCommand(BSFolderPath.Value)).AddTo(disposables);

            SettingFinishCommand = settingsTabPropertyModel.VerifyBoth.ToReactiveCommand().WithSubscribe(() => RequestClose.Invoke(new DialogResult(ButtonResult.OK))).AddTo(disposables);

            VerifyGitHubTokenCommand.Subscribe((x) =>
            {
                settingsTabPropertyModel.GitHubToken = ((PasswordBox)x).Password;
            }).AddTo(disposables);
        }

        public string Title => "Initial Setting";

        public event Action<IDialogResult> RequestClose;

        public bool CanCloseDialog() => SettingFinishCommand.CanExecute();

        public void OnDialogClosed()
        {
            Task.Run(() => 
            {
                mainWindowPropertyModel.Console = "Start Making Backup";
                dataManager.Backup();
                mainWindowPropertyModel.Console = "Finish Making Backup";
            }).GetAwaiter().GetResult();
            Task.Run(() => { innerData.modAssistantAllMods = modAssistantManager.GetAllModAssistantModsAsync().Result; }).GetAwaiter().GetResult();
            Task.Run(() => { dataManager.GetLocalModFilesInfo(); }).GetAwaiter().GetResult();
        }

        public void OnDialogOpened(IDialogParameters _)
        {
        }

        public void Destroy()
        {
            disposables.Dispose();
        }
    }
}
