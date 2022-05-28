using BSModManager.Models;
using BSModManager.Static;
using BSModManager.Views;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using Prism.Regions;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using System.Windows;

namespace BSModManager.ViewModels
{
    public class MainWindowViewModel : BindableBase, IDestructible
    {
        CompositeDisposable Disposables { get; } = new CompositeDisposable();

        // https://whitedog0215.hatenablog.jp/entry/2020/03/17/221403
        public ReadOnlyReactivePropertySlim<string> Debug { get; }

        private string title = "BSModManager";
        public string Title
        {
            get { return title; }
            set { SetProperty(ref title, value); }
        }

        private string displayedGameVersion = GameVersion.DisplayedVersion;
        public string DisplayedGameVersion
        {
            get { return displayedGameVersion; }
            set { SetProperty(ref displayedGameVersion, value); }
        }

        private string myselfVersion = "Version\n---";
        public string MyselfVersion
        {
            get { return myselfVersion; }
            set { SetProperty(ref myselfVersion, value); }
        }

        private string updateOrInstall = "Update";
        public string UpdateOrInstall
        {
            get { return updateOrInstall; }
            set { SetProperty(ref updateOrInstall, value); }
        }

        private bool showInstallTabViewEnable = true;
        public bool ShowInstallTabViewEnable
        {
            get { return showInstallTabViewEnable; }
            set { SetProperty(ref showInstallTabViewEnable, value); }
        }

        private bool showUpdateTabViewEnable = true;
        public bool ShowUpdateTabViewEnable
        {
            get { return showUpdateTabViewEnable; }
            set { SetProperty(ref showUpdateTabViewEnable, value); }
        }

        private bool showSettingsTabViewEnable = true;
        public bool ShowSettingsTabViewEnable
        {
            get { return showSettingsTabViewEnable; }
            set { SetProperty(ref showSettingsTabViewEnable, value); }
        }

        private bool updateOrInstallButtonEnable = true;
        public bool UpdateOrInstallButtonEnable
        {
            get { return updateOrInstallButtonEnable; }
            set { SetProperty(ref updateOrInstallButtonEnable, value); }
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

        private bool refreshButtonEnable = true;
        public bool RefreshButtonEnable
        {
            get { return refreshButtonEnable; }
            set { SetProperty(ref refreshButtonEnable, value); }
        }

        readonly IDialogService dialogService;
        readonly LocalMods localMods;
        readonly ChangeModInfoModel changeModInfoModel;
        readonly GitHubApi gitHubApi;
        readonly ModCsvHandler modCsv;
        readonly InitialDirectorySetup initializer;
        readonly MyselfUpdater mySelfUpdater;
        readonly ModUpdater modUpdater;
        readonly MAMods mAMod;
        readonly ConfigFileHandler configFile;
        readonly SettingsVerifier settingsVerifier;
        readonly ModInstaller modInstaller;
        readonly PreviousLocalModsDataGetter localModsDataFetcher;
        readonly Refresher refresher;
        readonly MainModsSetter mainModsChanger;

        public IRegionManager RegionManager { get; private set; }
        public DelegateCommand<string> ShowUpdateTabViewCommand { get; private set; }
        public DelegateCommand<string> ShowInstallTabViewCommand { get; private set; }
        public DelegateCommand<string> ShowSettingsTabViewCommand { get; private set; }
        public DelegateCommand<string> UpdateOrInstallButtonCommand { get; private set; }

        private DelegateCommand allCheckedButtonCommand;
        public DelegateCommand AllCheckedButtonCommand
        {
            get { return allCheckedButtonCommand; }
            private set { SetProperty(ref allCheckedButtonCommand, value); }
        }
        public DelegateCommand ChangeModInfoButtonCommand { get; private set; }

        private DelegateCommand modRepositoryButtonCommand;
        public DelegateCommand ModRepositoryButtonCommand
        {
            get { return modRepositoryButtonCommand; }
            private set { SetProperty(ref modRepositoryButtonCommand, value); }
        }
        public DelegateCommand RefreshButtonCommand { get; private set; }
        public DelegateCommand LoadedCommand { get; }
        public DelegateCommand<System.ComponentModel.CancelEventArgs> ClosingCommand { get; }

        public MainWindowViewModel(IRegionManager regionManager, IDialogService ds,
            ModInstaller mi, Refresher r,ChangeModInfoModel cmim,MainModsSetter mmc,
            GitHubApi gha, LocalMods lmdm, ConfigFileHandler cf, SettingsVerifier sv, PreviousLocalModsDataGetter lmdf,
            ModCsvHandler mc, InitialDirectorySetup i, MyselfUpdater u, ModUpdater mu, MAMods mam)
        {
            localMods = lmdm;
            gitHubApi = gha;
            localModsDataFetcher = lmdf;
            modCsv = mc;
            initializer = i;
            mySelfUpdater = u;
            modUpdater = mu;
            mAMod = mam;
            configFile = cf;
            settingsVerifier = sv;
            modInstaller = mi;
            refresher = r;
            changeModInfoModel = cmim;
            mainModsChanger = mmc;

            dialogService = ds;

            // あんまりいい方法なさそうなので
            // https://stackoverflow.com/questions/3683450/handling-the-window-closing-event-with-wpf-mvvm-light-toolkit
            System.Windows.Application.Current.MainWindow.Closing += new CancelEventHandler(ClosingCommand);

            // https://whitedog0215.hatenablog.jp/entry/2020/03/17/221403
            this.Debug = Logger.Instance.ObserveProperty(x => x.InfoLog).ToReadOnlyReactivePropertySlim().AddTo(Disposables);

            Folder.Instance.PropertyChanged += (sender, e) =>
            {
                DisplayedGameVersion = GameVersion.DisplayedVersion;
            };

            mainModsChanger.ChangeModInfoButtonEnable = this.ToReactivePropertyAsSynchronized(x => x.ChangeModInfoButtonEnable).AddTo(Disposables);
            
            SetMyselfVersion();

            ButtonCommandSubscribe(regionManager);

            Dictionary<string, string> tempDictionary = configFile.Load();
            if (tempDictionary["BSFolderPath"] != string.Empty && tempDictionary["GitHubToken"] != string.Empty)
            {
                Folder.Instance.BSFolderPath = tempDictionary["BSFolderPath"];
                gitHubApi.GitHubToken = tempDictionary["GitHubToken"];
                FilePath.Instance.MAExePath = tempDictionary["MAExePath"];
            }

            LoadedCommand = new DelegateCommand(async () =>
            {
                // 少なくとも最初にスッと落ちるのは避けたいので
                try
                {
                    Logger.Instance.Info("Start Initializing");

                    AllButtonDisable();

                    Logger.Instance.Info("Check Myself Latest Version");

                    await MyselfUpdateCheck();

                    if (!settingsVerifier.BSFolderAndGitHubToken)
                    {
                        dialogService.ShowDialog("InitialSetting");
                    }

                    Logger.Instance.Info("Start Making Backup");
                    initializer.Backup();
                    Logger.Instance.Info("Finish Making Backup");
                    Logger.Instance.Info("Start Cleanup ModsTemp");
                    initializer.CleanModsTemp(Folder.Instance.tmpFolder);
                    Logger.Instance.Info("Finish Cleanup ModsTemp");

                    mAMod.ModAssistantAllMods = await mAMod.GetAllAsync();

                    await localModsDataFetcher.GetData();

                    await refresher.Refresh();

                    AllButtonEnable();
                }
                catch(Exception ex)
                {
                    Logger.Instance.Error(ex.Message);
                }
            });


            void ClosingCommand(object sender, CancelEventArgs e)
            {
                // https://araramistudio.jimdo.com/2016/10/12/wpf%E3%81%A7window%E3%82%92%E9%96%89%E3%81%98%E3%82%8B%E5%89%8D%E3%81%AB%E7%A2%BA%E8%AA%8D%E3%81%99%E3%82%8B/
                if (MessageBoxResult.Yes != MessageBox.Show("画面を閉じます。よろしいですか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Information))
                {
                    e.Cancel = true;
                    return;
                }

                if (GameVersion.Version == "---") return;

                SaveModsData();

                configFile.Generate(Folder.Instance.BSFolderPath, gitHubApi.GitHubToken, FilePath.Instance.MAExePath);

                Logger.Instance.GenerateLogFile();
            };
        }

        private void SetMyselfVersion()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            System.Version version = assembly.GetName().Version;
            MyselfVersion = "Version\n" + version.ToString(3);
        }

        private void SaveModsData()
        {
            string dataDirectory = Path.Combine(Folder.Instance.dataFolder, GameVersion.Version);
            if (!Directory.Exists(dataDirectory))
            {
                Directory.CreateDirectory(dataDirectory);
            }
            string modsDataCsvPath = Path.Combine(dataDirectory, "ModsData.csv");
            modCsv.Write(modsDataCsvPath, localMods.LocalModsData);
        }

        private async Task MyselfUpdateCheck()
        {
            bool canUpdate = await gitHubApi.CheckMyselfNewVersion();

            if (!canUpdate) return;
                
            if (MessageBoxResult.Yes != MessageBox.Show("更新版を発見しました。更新しますか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Information))
            {
                return;
            }

            bool hasDownloaded = await gitHubApi.DownloadMyselfNewVersion();

            if (hasDownloaded && File.Exists(Path.Combine(Environment.CurrentDirectory, "Updater.exe")))
            {
                mySelfUpdater.UpdateUpdater();

                ProcessStartInfo processStartInfo = new ProcessStartInfo
                {
                    Arguments = mySelfUpdater.LatestMyselfVersion.ToString(),
                    FileName = Path.Combine(Environment.CurrentDirectory, "Updater.exe")
                };
                Process.Start(processStartInfo);
                Environment.Exit(0);
            }
        }

        private void ButtonCommandSubscribe(IRegionManager regionManager)
        {
            RegionManager = regionManager;
            RegionManager.RegisterViewWithRegion("ContentRegion", typeof(UpdateTab));

            AllCheckedButtonCommand = new DelegateCommand(() =>
            {
                mainModsChanger.MainMods.AllCheckedOrUnchecked();
            });

            UpdateOrInstallButtonCommand = new DelegateCommand<string>(async (x) =>
            {
                Logger.Instance.Debug(x + " Button pushed");
                if (x == "Update")
                {
                    await modUpdater.Update();
                }
                else
                {
                    await modInstaller.Install();
                }
            });

            ChangeModInfoButtonCommand = new DelegateCommand(() =>
            {
                changeModInfoModel.ChangeInfo();
            });

            ModRepositoryButtonCommand = new DelegateCommand(() =>
            {
                mainModsChanger.MainMods.ModRepositoryOpen();
            });

            RefreshButtonCommand = new DelegateCommand(() =>
            {
                Task.Run(() => refresher.Refresh()).GetAwaiter().GetResult();
            });

            ShowUpdateTabViewCommand = new DelegateCommand<string>((x) =>
            {
                mainModsChanger.SetLocalMods();

                Logger.Instance.Info("Update");
                UpdateOrInstall = "Update";
                AllButtonEnable();
                RegionManager.RequestNavigate("ContentRegion", x);
            });

            ShowInstallTabViewCommand = new DelegateCommand<string>((x) =>
            {
                AllButtonEnable();
                Logger.Instance.Info("Install");
                UpdateOrInstall = "Install";
                RegionManager.RequestNavigate("ContentRegion", x);

                if (mainModsChanger.InstallTabIndex.Value == 0)
                {
                    mainModsChanger.SetPastMods();
                    return;
                }

                mainModsChanger.SetRecommendMods();
                ChangeModInfoButtonEnable = false;
            });

            ShowSettingsTabViewCommand = new DelegateCommand<string>((x) =>
            {
                Logger.Instance.Info("Settings");
                AllButtonDisable();
                ShowInstallTabViewEnable = true;
                ShowSettingsTabViewEnable = true;
                ShowUpdateTabViewEnable = true;
                RegionManager.RequestNavigate("ContentRegion", x);
            });
        }

        private void AllButtonDisable()
        {
            ShowInstallTabViewEnable = false;
            ShowSettingsTabViewEnable = false;
            ShowUpdateTabViewEnable = false;
            UpdateOrInstallButtonEnable = false;
            ModRepositoryButtonEnable = false;
            ChangeModInfoButtonEnable = false;
            AllCheckedButtonEnable = false;
            RefreshButtonEnable = false;
        }

        private void AllButtonEnable()
        {
            ShowInstallTabViewEnable = true;
            ShowSettingsTabViewEnable = true;
            ShowUpdateTabViewEnable = true;
            UpdateOrInstallButtonEnable = true;
            ModRepositoryButtonEnable = true;
            ChangeModInfoButtonEnable = true;
            AllCheckedButtonEnable = true;
            RefreshButtonEnable = true;
        }

        public void Destroy()
        {
            Disposables.Dispose();
        }
    }
}
