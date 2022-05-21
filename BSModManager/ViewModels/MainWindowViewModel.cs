using BSModManager.Models;
using BSModManager.Static;
using BSModManager.Views;
using Octokit;
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
using static BSModManager.Models.ModCsv;

namespace BSModManager.ViewModels
{
    public class MainWindowViewModel : BindableBase, IDestructible
    {
        CompositeDisposable disposables { get; } = new CompositeDisposable();

        // プロパティにしないとバインドされない
        public ReactiveCommand AllCheckedButtonCommand { get; } = new ReactiveCommand();

        // https://whitedog0215.hatenablog.jp/entry/2020/03/17/221403
        public ReadOnlyReactivePropertySlim<string> Console { get; }

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
        LocalMods localModsDataModel;
        LocalModSyncer localModSyncer;
        ChangeModInfoModel changeModInfoPropertyModel;
        GitHubApi gitHubApi;
        PastMods pastModsDataModel;
        ModCsv modCsv;
        Initializer initializer;
        MyselfUpdater mySelfUpdater;
        ModUpdater modUpdater;
        MAMods mAMod;
        ConfigFile configFile;
        SettingsVerifier settingsVerifier;

        public IRegionManager RegionManager { get; private set; }
        public DelegateCommand<string> ShowUpdateTabViewCommand { get; private set; }
        public DelegateCommand<string> ShowInstallTabViewCommand { get; private set; }
        public DelegateCommand<string> ShowSettingsTabViewCommand { get; private set; }
        public DelegateCommand<string> UpdateOrInstallButtonCommand { get; private set; }
        public DelegateCommand ChangeModInfoButtonCommand { get; private set; }
        public DelegateCommand ModRepositoryButtonCommand { get; private set; }
        public DelegateCommand RefreshButtonCommand { get; private set; }
        public DelegateCommand LoadedCommand { get; }
        public DelegateCommand<System.ComponentModel.CancelEventArgs> ClosingCommand { get; }

        public MainWindowViewModel(IRegionManager regionManager, IDialogService ds,
            LocalModSyncer dm, ChangeModInfoModel cmipm,
            GitHubApi gha, LocalMods lmdm, ConfigFile cf,SettingsVerifier sv,
            PastMods pmdm, ModCsv mc,Initializer i,MyselfUpdater u,ModUpdater mu,MAMods mam)
        {
            localModsDataModel = lmdm;
            localModSyncer = dm;
            changeModInfoPropertyModel = cmipm;
            gitHubApi = gha;
            pastModsDataModel = pmdm;
            modCsv = mc;
            initializer = i;
            mySelfUpdater = u;
            modUpdater = mu;
            mAMod = mam;
            configFile = cf;
            settingsVerifier = sv;

            dialogService = ds;

            // あんまりいい方法なさそうなので
            // https://stackoverflow.com/questions/3683450/handling-the-window-closing-event-with-wpf-mvvm-light-toolkit
            System.Windows.Application.Current.MainWindow.Closing += new CancelEventHandler(ClosingCommand);

            // https://whitedog0215.hatenablog.jp/entry/2020/03/17/221403
            this.Console = MainWindowLog.Instance.ObserveProperty(x => x.Debug).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            Folder.Instance.PropertyChanged += (sender, e) =>
            {
                DisplayedGameVersion = GameVersion.DisplayedVersion;
            };

            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            System.Version version = assembly.GetName().Version;
            MyselfVersion = "Version\n" + version.ToString(3);

            RegionManager = regionManager;
            RegionManager.RegisterViewWithRegion("ContentRegion", typeof(UpdateTab));
            ShowUpdateTabViewCommand = new DelegateCommand<string>((x) =>
              {
                  MainWindowLog.Instance.Debug = "Update";
                  UpdateOrInstall = "Update";
                  UpdateOrInstallButtonEnable = true;
                  ModRepositoryButtonEnable = true;
                  ChangeModInfoButtonEnable = true;
                  AllCheckedButtonEnable = true;
                  RefreshButtonEnable = true;
                  RegionManager.RequestNavigate("ContentRegion", x);
              });
            ShowInstallTabViewCommand = new DelegateCommand<string>((x) =>
            {
                MainWindowLog.Instance.Debug = "Install";
                UpdateOrInstall = "Install";
                UpdateOrInstallButtonEnable = true;
                ModRepositoryButtonEnable = true;
                ChangeModInfoButtonEnable = false;
                AllCheckedButtonEnable = true;
                RefreshButtonEnable = true;
                RegionManager.RequestNavigate("ContentRegion", x);
            });
            ShowSettingsTabViewCommand = new DelegateCommand<string>((x) =>
              {
                  MainWindowLog.Instance.Debug = "Settings";
                  UpdateOrInstallButtonEnable = false;
                  ModRepositoryButtonEnable = false;
                  ChangeModInfoButtonEnable = false;
                  AllCheckedButtonEnable = false;
                  RefreshButtonEnable = false;
                  RegionManager.RequestNavigate("ContentRegion", x);
              });

            AllCheckedButtonCommand.Subscribe(_ =>
            {
                localModsDataModel.AllCheckedOrUnchecked();
            }).AddTo(disposables);

            UpdateOrInstallButtonCommand = new DelegateCommand<string>((x) =>
              {
                  MainWindowLog.Instance.Debug = x;
                  if (x == "Update")
                  {
                      modUpdater.Update();
                  }
                  else
                  {

                  }
              });

            ChangeModInfoButtonCommand = new DelegateCommand(() =>
              {
                  changeModInfoPropertyModel.ChangeModInfo();
              });

            ModRepositoryButtonCommand = new DelegateCommand(() =>
              {
                  localModsDataModel.ModRepositoryOpen();
              });

            RefreshButtonCommand = new DelegateCommand(() =>
              {
                  localModSyncer.Sync();
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

                ShowInstallTabViewEnable = false;
                ShowSettingsTabViewEnable = false;
                UpdateOrInstallButtonEnable = false;
                ModRepositoryButtonEnable = false;
                ChangeModInfoButtonEnable = false;
                AllCheckedButtonEnable = false;
                RefreshButtonEnable = false;

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

                    // ModAssistantのPopulateModsListを使った方がいい
                    mAMod.modAssistantAllMods = await mAMod.GetAllAsync();

                    string dataDirectory = Path.Combine(Folder.Instance.dataFolder, GameVersion.Version);
                    string modsDataCsvPath = Path.Combine(dataDirectory, "ModsData.csv");
                    List<ModCsvIndex> previousDataList;
                    if (File.Exists(modsDataCsvPath))
                    {
                        previousDataList = await modCsv.Read(modsDataCsvPath);
                        foreach (var previousData in previousDataList)
                        {
                            if (Array.Exists(mAMod.modAssistantAllMods, x => x.name == previousData.Mod))
                            {
                                if (previousData.Original)
                                {
                                    var temp = Array.Find(mAMod.modAssistantAllMods, x => x.name == previousData.Mod);

                                    DateTime now = DateTime.Now;
                                    DateTime mAUpdatedAt = DateTime.Parse(temp.updatedDate);
                                    string updated = null;
                                    if ((now - mAUpdatedAt).Days >= 1)
                                    {
                                        updated = (now - mAUpdatedAt).Days + "D ago";
                                    }
                                    else
                                    {
                                        updated = (now - mAUpdatedAt).Hours + "H" + (now - mAUpdatedAt).Minutes + "m ago";
                                    }

                                    localModsDataModel.LocalModsData.Add(new LocalMods.LocalModData(localModSyncer)
                                    {
                                        Mod = previousData.Mod,
                                        Latest = new Version(temp.version),
                                        Updated = updated,
                                        Original = "〇",
                                        MA = "〇",
                                        Description = temp.description,
                                        Url = temp.link
                                    });
                                }
                            }
                            else
                            {
                                Release response = await gitHubApi.GetModLatestVersionAsync(previousData.Url);
                                string original = null;
                                if (!previousData.Original)
                                {
                                    original = "×";
                                }
                                else
                                {
                                    original = "〇";
                                }

                                if (response == null)
                                {
                                    localModsDataModel.LocalModsData.Add(new LocalMods.LocalModData(localModSyncer)
                                    {
                                        Mod = previousData.Mod,
                                        Latest = new Version("0.0.0"),
                                        Updated = previousData.Url == "" ? "?" : "---",
                                        Original = original,
                                        MA = "×",
                                        Description = previousData.Url == "" ? "?" : "---",
                                        Url = previousData.Url
                                    });
                                }
                                else
                                {
                                    DateTime now = DateTime.Now;
                                    string updated = null;
                                    if ((now - response.CreatedAt).Days >= 1)
                                    {
                                        updated = (now - response.CreatedAt).Days + "D ago";
                                    }
                                    else
                                    {
                                        updated = (now - response.CreatedAt).Hours + "H" + (now - response.CreatedAt).Minutes + "m ago";
                                    }

                                    localModsDataModel.LocalModsData.Add(new LocalMods.LocalModData(localModSyncer)
                                    {
                                        Mod = previousData.Mod,
                                        Latest = gitHubApi.DetectVersion(response.TagName),
                                        Updated = updated,
                                        Original = original,
                                        MA = "×",
                                        Description = response.Body,
                                        Url = previousData.Url
                                    });
                                }
                            }
                        }
                    }

                    await Task.Run(() => localModSyncer.Sync());
                }

                await pastModsDataModel.Initialize();

                ShowInstallTabViewEnable = true;
                ShowSettingsTabViewEnable = true;
                UpdateOrInstallButtonEnable = true;
                ModRepositoryButtonEnable = true;
                ChangeModInfoButtonEnable = true;
                AllCheckedButtonEnable = true;
                RefreshButtonEnable = true;
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
                    Task.Run(async () => await modCsv.Write(modsDataCsvPath, localModsDataModel.LocalModsData)).GetAwaiter().GetResult();
                }
            };


        }

        public void Destroy()
        {
            disposables.Dispose();
        }
    }
}
