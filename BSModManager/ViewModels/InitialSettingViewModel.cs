using BSModManager.Models;
using BSModManager.Models.ViewModelCommonProperty;
using BSModManager.Static;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace BSModManager.ViewModels
{
    public class InitialSettingViewModel : BindableBase,IDialogAware
    {
        IDialogService dialogService;
        MainWindowPropertyModel mainWindowPropertyModel;
        SettingsTabPropertyModel settingsTabPropertyModel;

        public ReactiveProperty<string> VerifyBSFolder { get; }
        public ReactiveProperty<Brush> VerifyBSFolderColor { get; }

        public ReactiveProperty<string> BSFolderPath { get; }

        public ReactiveCommand SelectBSFolder { get; } = new ReactiveCommand();

        public ReactiveProperty<string> VerifyGitHubToken { get; }
        public ReactiveProperty<Brush> VerifyGitHubTokenColor { get; }

        public ReactiveCommand VerifyAllCommand { get; } = new ReactiveCommand();
        public ReactiveCommand SettingFinishCommand { get; } = new ReactiveCommand();

        public InitialSettingViewModel(IDialogService ds,MainWindowPropertyModel mwpm,SettingsTabPropertyModel stpm)
        {
            dialogService = ds;
            mainWindowPropertyModel = mwpm;
            settingsTabPropertyModel = stpm;

            // https://whitedog0215.hatenablog.jp/entry/2020/03/17/221403
            BSFolderPath = settingsTabPropertyModel.ToReactivePropertyAsSynchronized(x=> x.BSFolderPath);

            SelectBSFolder.Subscribe(_ => BSFolderPath.Value = FolderManager.SelectFolderCommand(BSFolderPath.Value));

            if (mainWindowPropertyModel.GameVersion== "GameVersion\n---")
            {
                dialogService.ShowDialog("InitialSetting");
            }


        }

        public string Title => "Initial Setting";

        public event Action<IDialogResult> RequestClose;

        public bool CanCloseDialog() => true;

        public void OnDialogClosed() { }

        public void OnDialogOpened(IDialogParameters _)
        {
        }
    }
}
