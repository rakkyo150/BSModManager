using BSModManager.Models;
using BSModManager.Models.CoreManager;
using BSModManager.Models.ViewModelCommonProperty;
using BSModManager.Views;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Threading.Tasks;
using System.Windows;

namespace BSModManager.ViewModels
{
    public class MainWindowViewModel : BindableBase
    {
        // プロパティにしないとバインドされない
        public ReactiveCommand AllCheckedButtonCommand { get; } = new ReactiveCommand();

        // https://whitedog0215.hatenablog.jp/entry/2020/03/17/221403
        public ReadOnlyReactivePropertySlim<string> Console { get; }
        public ReadOnlyReactivePropertySlim<string> GameVersion { get; }

        private string title = "BSModManager";
        public string Title
        {
            get { return title; }
            set { SetProperty(ref title, value); }
        }

        private string myselfVersion = "Version\n---";
        public string MyselfVersion
        {
            get { return myselfVersion; }
            set { SetProperty(ref myselfVersion, value); }
        }

        private bool installButtonEnable = true;
        public bool InstallButtonEnable
        {
            get { return installButtonEnable; }
            set { SetProperty(ref installButtonEnable, value); }
        }

        private bool modRepositoryButtonEnable = true;
        public bool ModRepositoryButtonEnable
        {
            get { return modRepositoryButtonEnable; }
            set { SetProperty(ref modRepositoryButtonEnable, value); }
        }

        private bool changeModInfoButtonEnable = true;
        public bool ChangeModInfoButtonEnable
        {
            get { return changeModInfoButtonEnable; }
            set { SetProperty(ref changeModInfoButtonEnable, value); }
        }

        private bool allCheckedButtonEnable = true;
        public bool AllCheckedButtonEnable
        {
            get { return allCheckedButtonEnable; }
            set { SetProperty(ref allCheckedButtonEnable, value); }
        }

        IDialogService dialogService;
        VersionManager versionManager;
        SettingsTabPropertyModel settingsTabPropertyModel;
        MainWindowPropertyModel mainWindowPropertyModel;
        MainTabPropertyModel mainTabPropertyModel;
        DataManager dataManager;
        ChangeModInfoPropertyModel changeModInfoPropertyModel;
        ModAssistantManager modAssistantManager;
        InnerData innerData;

        public IRegionManager RegionManager { get; private set; }
        public DelegateCommand<string> ShowMainTabViewCommand { get; private set; }
        public DelegateCommand<string> ShowSettingsTabViewCommand { get; private set; }
        public DelegateCommand ChangeModInfoButtonCommand { get; private set; }
        public DelegateCommand LoadedCommand { get; }
        public DelegateCommand<System.ComponentModel.CancelEventArgs> ClosingCommand { get; }

        public MainWindowViewModel(IRegionManager regionManager, SettingsTabPropertyModel stpm, IDialogService ds, VersionManager vm, 
            MainWindowPropertyModel mwpm, MainTabPropertyModel mtpm, DataManager dm, ChangeModInfoPropertyModel cmipm,ModAssistantManager mam,InnerData id)
        {
            versionManager = vm;
            settingsTabPropertyModel = stpm;
            mainWindowPropertyModel = mwpm;
            mainTabPropertyModel = mtpm;
            dataManager = dm;
            changeModInfoPropertyModel = cmipm;
            modAssistantManager = mam;
            innerData = id;

            dialogService = ds;

            // https://whitedog0215.hatenablog.jp/entry/2020/03/17/221403
            this.Console = mainWindowPropertyModel.ObserveProperty(x => x.Console).ToReadOnlyReactivePropertySlim();
            this.GameVersion = mainWindowPropertyModel.ObserveProperty(x => x.GameVersion).ToReadOnlyReactivePropertySlim();

            MyselfVersion = versionManager.GetMyselfVersion();

            RegionManager = regionManager;
            RegionManager.RegisterViewWithRegion("ContentRegion", typeof(MainTab));
            ShowMainTabViewCommand = new DelegateCommand<string>((x) =>
              {
                  mainWindowPropertyModel.Console = "Main";
                  InstallButtonEnable = true;
                  ModRepositoryButtonEnable = true;
                  ChangeModInfoButtonEnable = true;
                  AllCheckedButtonEnable = true;
                  RegionManager.RequestNavigate("ContentRegion", x);
              });
            ShowSettingsTabViewCommand = new DelegateCommand<string>((x) =>
              {
                  mainWindowPropertyModel.Console = "Settings";
                  InstallButtonEnable = false;
                  ModRepositoryButtonEnable = false;
                  ChangeModInfoButtonEnable = false;
                  AllCheckedButtonEnable = false;
                  RegionManager.RequestNavigate("ContentRegion", x);
              });

            AllCheckedButtonCommand.Subscribe(_ =>
            {
                mainTabPropertyModel.AllCheckedOrUnchecked();
            });

            ChangeModInfoButtonCommand = new DelegateCommand(() =>
              {
                  changeModInfoPropertyModel.ChangeModInfo();
              });

            LoadedCommand = new DelegateCommand(async () =>
            {
                mainWindowPropertyModel.Console = "Start Initializing";
                if (!settingsTabPropertyModel.VerifyBoth.Value)
                {
                    System.Diagnostics.Debug.WriteLine(settingsTabPropertyModel.VerifyBoth.Value);
                    dialogService.ShowDialog("InitialSetting");
                }
                else
                {
                    innerData.modAssistantAllMods = await modAssistantManager.GetAllModAssistantModsAsync();
                    await Task.Run(() => dataManager.GetLocalModFilesInfo());
                    mainWindowPropertyModel.Console = "Start Making Backup";
                    dataManager.Backup();
                    mainWindowPropertyModel.Console = "Finish Making Backup";
                }
            });

            ClosingCommand = new DelegateCommand<System.ComponentModel.CancelEventArgs>((x) =>
            {
                // https://araramistudio.jimdo.com/2016/10/12/wpf%E3%81%A7window%E3%82%92%E9%96%89%E3%81%98%E3%82%8B%E5%89%8D%E3%81%AB%E7%A2%BA%E8%AA%8D%E3%81%99%E3%82%8B/
                if (MessageBoxResult.Yes != MessageBox.Show("画面を閉じます。よろしいですか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Information))
                {
                    x.Cancel = true;
                    return;
                }
            });
        }
    }
}
