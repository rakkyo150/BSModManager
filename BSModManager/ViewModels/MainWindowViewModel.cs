﻿using BSModManager.Models;
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
        CompositeDisposable disposables { get; } = new CompositeDisposable();

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

        IDialogService dialogService;
        LocalMods localMods;
        LocalModsDataSyncer syncer;
        ChangeModInfoModel changeModInfoPropertyModel;
        GitHubApi gitHubApi;
        PastMods pastMods;
        PastModsDataHandler pastModsDataFetcher;
        ModCsvHandler modCsv;
        InitialDirectorySetup initializer;
        MyselfUpdater mySelfUpdater;
        ModUpdater modUpdater;
        MAMods mAMod;
        ConfigFileHandler configFile;
        SettingsVerifier settingsVerifier;
        ModInstaller modInstaller;
        PreviousLocalModsDataFetcher localModsDataFetcher;

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
            LocalModsDataSyncer dm, ChangeModInfoModel cmipm, ModInstaller mi, PastModsDataHandler pmdf,
            GitHubApi gha, LocalMods lmdm, ConfigFileHandler cf, SettingsVerifier sv, PreviousLocalModsDataFetcher lmdf,
            PastMods pmdm, ModCsvHandler mc, InitialDirectorySetup i, MyselfUpdater u, ModUpdater mu, MAMods mam)
        {
            localMods = lmdm;
            syncer = dm;
            changeModInfoPropertyModel = cmipm;
            gitHubApi = gha;
            pastMods = pmdm;
            pastModsDataFetcher = pmdf;
            localModsDataFetcher = lmdf;
            modCsv = mc;
            initializer = i;
            mySelfUpdater = u;
            modUpdater = mu;
            mAMod = mam;
            configFile = cf;
            settingsVerifier = sv;
            modInstaller = mi;

            dialogService = ds;

            // あんまりいい方法なさそうなので
            // https://stackoverflow.com/questions/3683450/handling-the-window-closing-event-with-wpf-mvvm-light-toolkit
            System.Windows.Application.Current.MainWindow.Closing += new CancelEventHandler(ClosingCommand);

            // https://whitedog0215.hatenablog.jp/entry/2020/03/17/221403
            this.Debug = MainWindowLog.Instance.ObserveProperty(x => x.Debug).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            Folder.Instance.PropertyChanged += (sender, e) =>
            {
                DisplayedGameVersion = GameVersion.DisplayedVersion;
            };

            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            System.Version version = assembly.GetName().Version;
            MyselfVersion = "Version\n" + version.ToString(3);

            RegionManager = regionManager;
            RegionManager.RegisterViewWithRegion("ContentRegion", typeof(UpdateTab));

            AllCheckedButtonCommand = new DelegateCommand(() =>
              {
                  localMods.AllCheckedOrUnchecked();
              });

            UpdateOrInstallButtonCommand = new DelegateCommand<string>((x) =>
              {
                  MainWindowLog.Instance.Debug = x;
                  if (x == "Update")
                  {
                      modUpdater.Update();
                  }
                  else
                  {
                      Console.WriteLine("test");
                      modInstaller.Install(pastMods);
                  }
              });

            ChangeModInfoButtonCommand = new DelegateCommand(() =>
              {
                  changeModInfoPropertyModel.ChangeModInfo();
              });

            ModRepositoryButtonCommand = new DelegateCommand(() =>
              {
                  localMods.ModRepositoryOpen();
              });

            RefreshButtonCommand = new DelegateCommand(() =>
              {
                  syncer.Sync();
                  Task.Run(() => pastModsDataFetcher.FetchAndSync()).GetAwaiter().GetResult();
              });

            ShowUpdateTabViewCommand = new DelegateCommand<string>((x) =>
            {
                AllCheckedButtonCommand = new DelegateCommand(() =>
                {
                    localMods.AllCheckedOrUnchecked();
                });
                ModRepositoryButtonCommand = new DelegateCommand(() =>
                {
                    localMods.ModRepositoryOpen(); ;
                });
                MainWindowLog.Instance.Debug = "Update";
                UpdateOrInstall = "Update";
                AllButtonEnable();
                RegionManager.RequestNavigate("ContentRegion", x);
            });

            ShowInstallTabViewCommand = new DelegateCommand<string>((x) =>
            {
                AllCheckedButtonCommand = new DelegateCommand(() =>
                {
                    pastMods.AllCheckedOrUnchecked();
                });
                ModRepositoryButtonCommand = new DelegateCommand(() =>
                {
                    pastMods.ModRepositoryOpen(); ;
                });
                MainWindowLog.Instance.Debug = "Install";
                UpdateOrInstall = "Install";
                AllButtonEnable();
                RegionManager.RequestNavigate("ContentRegion", x);
            });

            ShowSettingsTabViewCommand = new DelegateCommand<string>((x) =>
            {
                MainWindowLog.Instance.Debug = "Settings";
                AllButtonDisable();
                ShowInstallTabViewEnable = true;
                ShowSettingsTabViewEnable = true;
                ShowUpdateTabViewEnable = true;
                RegionManager.RequestNavigate("ContentRegion", x);
            });

            Dictionary<string, string> tempDictionary = configFile.Load();
            if (tempDictionary["BSFolderPath"] != null && tempDictionary["GitHubToken"] != null)
            {
                Folder.Instance.BSFolderPath = tempDictionary["BSFolderPath"];
                gitHubApi.GitHubToken = tempDictionary["GitHubToken"];
                FilePath.Instance.MAExePath = tempDictionary["MAExePath"];
            }

            LoadedCommand = new DelegateCommand(async () =>
            {
                MainWindowLog.Instance.Debug = "Start Initializing";

                AllButtonDisable();

                MainWindowLog.Instance.Debug = "Check Myself Latest Version";
                bool update = await gitHubApi.CheckNewVersionAndDowonload();

                if (update)
                {
                    if (MessageBoxResult.Yes != MessageBox.Show("更新版を発見しました。更新しますか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Information))
                    {
                        if (File.Exists(Path.Combine(Environment.CurrentDirectory, "Updater.exe")))
                        {
                            mySelfUpdater.UpdateUpdater();

                            ProcessStartInfo processStartInfo = new ProcessStartInfo();
                            processStartInfo.Arguments = mySelfUpdater.LatestMyselfVersion.ToString();
                            processStartInfo.FileName = Path.Combine(Environment.CurrentDirectory, "Updater.exe");
                            Process process = Process.Start(processStartInfo);
                            Environment.Exit(0);
                        }
                    }
                }

                if (!settingsVerifier.BSFolderAndGitHubToken)
                {
                    dialogService.ShowDialog("InitialSetting");
                }
                else
                {
                    MainWindowLog.Instance.Debug = "Start Making Backup";
                    await Task.Run(() => { initializer.Backup(); });
                    MainWindowLog.Instance.Debug = "Finish Making Backup";
                    MainWindowLog.Instance.Debug = "Start Cleanup ModsTemp";
                    await Task.Run(() => { initializer.CleanModsTemp(Folder.Instance.tmpFolder); });
                    MainWindowLog.Instance.Debug = "Finish Cleanup ModsTemp";

                    mAMod.modAssistantAllMods = await mAMod.GetAllAsync();

                    await localModsDataFetcher.FetchData();

                    await Task.Run(() => syncer.Sync());
                }

                await pastModsDataFetcher.FetchAndSync();

                AllButtonEnable();
            });


            void ClosingCommand(object sender, CancelEventArgs e)
            {
                // https://araramistudio.jimdo.com/2016/10/12/wpf%E3%81%A7window%E3%82%92%E9%96%89%E3%81%98%E3%82%8B%E5%89%8D%E3%81%AB%E7%A2%BA%E8%AA%8D%E3%81%99%E3%82%8B/
                if (MessageBoxResult.Yes != MessageBox.Show("画面を閉じます。よろしいですか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Information))
                {
                    e.Cancel = true;
                    return;
                }
                else
                {
                    if (GameVersion.Version == "---") return;

                    string dataDirectory = Path.Combine(Folder.Instance.dataFolder, GameVersion.Version);
                    if (!Directory.Exists(dataDirectory))
                    {
                        Directory.CreateDirectory(dataDirectory);
                    }
                    string modsDataCsvPath = Path.Combine(dataDirectory, "ModsData.csv");
                    Task.Run(async () => await modCsv.Write(modsDataCsvPath, localMods.LocalModsData)).GetAwaiter().GetResult();
                }
            };
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
            disposables.Dispose();
        }
    }
}
