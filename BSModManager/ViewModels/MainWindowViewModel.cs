using BSModManager.Models;
using BSModManager.Models.CoreManager;
using BSModManager.Models.Structure;
using BSModManager.Models.ViewModelCommonProperty;
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
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using System.Windows;

namespace BSModManager.ViewModels
{
    public class MainWindowViewModel : BindableBase,IDestructible
    {
        CompositeDisposable disposables { get; } = new CompositeDisposable();
        
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

        private string updateOrInstall = "Update";
        public string UpdateOrInstall
        {
            get { return updateOrInstall; }
            set { SetProperty(ref updateOrInstall, value); }
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
        VersionManager versionManager;
        SettingsTabPropertyModel settingsTabPropertyModel;
        MainWindowPropertyModel mainWindowPropertyModel;
        UpdateTabPropertyModel updateTabPropertyModel;
        DataManager dataManager;
        ChangeModInfoPropertyModel changeModInfoPropertyModel;
        ModAssistantManager modAssistantManager;
        InnerData innerData;
        GitHubManager gitHubManager;

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

        public MainWindowViewModel(IRegionManager regionManager, SettingsTabPropertyModel stpm, IDialogService ds, VersionManager vm,
            MainWindowPropertyModel mwpm, UpdateTabPropertyModel mtpm, DataManager dm, ChangeModInfoPropertyModel cmipm, ModAssistantManager mam, InnerData id,GitHubManager ghm)
        {
            versionManager = vm;
            settingsTabPropertyModel = stpm;
            mainWindowPropertyModel = mwpm;
            updateTabPropertyModel = mtpm;
            dataManager = dm;
            changeModInfoPropertyModel = cmipm;
            modAssistantManager = mam;
            innerData = id;
            gitHubManager = ghm;

            dialogService = ds;

            // https://whitedog0215.hatenablog.jp/entry/2020/03/17/221403
            this.Console = mainWindowPropertyModel.ObserveProperty(x => x.Console).ToReadOnlyReactivePropertySlim().AddTo(disposables);
            this.GameVersion = mainWindowPropertyModel.ObserveProperty(x => x.GameVersion).ToReadOnlyReactivePropertySlim().AddTo(disposables);

            MyselfVersion = versionManager.GetMyselfVersion();

            RegionManager = regionManager;
            RegionManager.RegisterViewWithRegion("ContentRegion", typeof(UpdateTab));
            ShowUpdateTabViewCommand = new DelegateCommand<string>((x) =>
              {
                  mainWindowPropertyModel.Console = "Update";
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
                mainWindowPropertyModel.Console = "Install";
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
                  mainWindowPropertyModel.Console = "Settings";
                  UpdateOrInstallButtonEnable = false;
                  ModRepositoryButtonEnable = false;
                  ChangeModInfoButtonEnable = false;
                  AllCheckedButtonEnable = false;
                  RefreshButtonEnable = false;
                  RegionManager.RequestNavigate("ContentRegion", x);
              });

            AllCheckedButtonCommand.Subscribe(_ =>
            {
                updateTabPropertyModel.AllCheckedOrUnchecked();
            }).AddTo(disposables);

            UpdateOrInstallButtonCommand = new DelegateCommand<string>((x) =>
              {
                  mainWindowPropertyModel.Console = x;
              });

            ChangeModInfoButtonCommand = new DelegateCommand(() =>
              {
                  changeModInfoPropertyModel.ChangeModInfo();
              });

            ModRepositoryButtonCommand = new DelegateCommand(() =>
              {
                  updateTabPropertyModel.ModRepositoryOpen();
              });

            RefreshButtonCommand = new DelegateCommand(() =>
              {
                  dataManager.GetLocalModFilesInfo();
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
                    mainWindowPropertyModel.Console = "Start Making Backup";
                    await Task.Run(() => { dataManager.Backup(); });
                    mainWindowPropertyModel.Console = "Finish Making Backup";
                    innerData.modAssistantAllMods = await modAssistantManager.GetAllModAssistantModsAsync();
                    string dataDirectory = Path.Combine(FolderManager.dataFolder, dataManager.GetGameVersion());
                    string modsDataCsvPath = Path.Combine(dataDirectory, "ModsData.csv");
                    List<ModInformationCsv> previousDataList;
                    if (File.Exists(modsDataCsvPath))
                    {
                        previousDataList = await dataManager.ReadCsv(modsDataCsvPath);
                        foreach (var previousData in previousDataList)
                        {
                            if (Array.Exists(innerData.modAssistantAllMods,x => x.name == previousData.Mod))
                            {
                                if(previousData.Original)
                                {
                                    var temp = Array.Find(innerData.modAssistantAllMods, x => x.name == previousData.Mod);

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

                                    updateTabPropertyModel.ModsData.Add(new UpdateTabPropertyModel.ModData(settingsTabPropertyModel,mainWindowPropertyModel, dataManager)
                                    {
                                        Mod = previousData.Mod,
                                        Latest = new Version(temp.version),
                                        Updated = updated,
                                        Original="〇",
                                        MA = "〇",
                                        Description=temp.description,
                                        Url = temp.link
                                    }); 
                                }
                            }
                            else
                            {
                                Release response = await gitHubManager.GetGitHubModLatestVersionAsync(previousData.Url);
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
                                    updateTabPropertyModel.ModsData.Add(new UpdateTabPropertyModel.ModData(settingsTabPropertyModel,mainWindowPropertyModel, dataManager)
                                    {
                                        Mod = previousData.Mod,
                                        Latest = new Version("0.0.0"),
                                        Updated = "?",
                                        Original = original,
                                        MA = "×",
                                        Description = "?",
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

                                    updateTabPropertyModel.ModsData.Add(new UpdateTabPropertyModel.ModData(settingsTabPropertyModel,mainWindowPropertyModel,dataManager)
                                    {
                                        Mod = previousData.Mod,
                                        Latest = gitHubManager.DetectVersion(response.TagName),
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
                    await Task.Run(() => dataManager.GetLocalModFilesInfo());

                    /*
                    アップデート時などに必要なら立ち上げるのがいいかも　
                    if (settingsTabPropertyModel.VerifyMAExe.Value == "〇")
                    {
                        try
                        {
                            System.Diagnostics.Process.Start(settingsTabPropertyModel.MAExePath);
                        }
                        catch (Exception e)
                        {
                            mainWindowPropertyModel.Console = e.Message;
                        }
                    }
                    */
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
                else
                {
                    if (dataManager.GetGameVersion() != null)
                    {
                        string dataDirectory = Path.Combine(FolderManager.dataFolder, dataManager.GetGameVersion());
                        if (!Directory.Exists(dataDirectory))
                        {
                            Directory.CreateDirectory(dataDirectory);
                        }
                        string modsDataCsvPath = Path.Combine(dataDirectory, "ModsData.csv");
                        Task.Run(async()=>await dataManager.WriteCsv(modsDataCsvPath, updateTabPropertyModel.ModsData)).GetAwaiter().GetResult();
                    }
                }
            });


        }

        public void Destroy()
        {
            disposables.Dispose();
        }
    }
}
