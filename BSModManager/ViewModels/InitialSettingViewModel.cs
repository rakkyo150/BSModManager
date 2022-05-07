using BSModManager.Models;
using BSModManager.Models.CoreManager;
using BSModManager.Models.Structure;
using BSModManager.Models.ViewModelCommonProperty;
using BSModManager.Static;
using Octokit;
using Prism.Mvvm;
using Prism.Navigation;
using Prism.Services.Dialogs;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
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
        GitHubManager gitHubManager;
        UpdateTabPropertyModel updateTabPropertyModel;

        MainWindowPropertyModel mainWindowPropertyModel;

        CompositeDisposable disposables { get; } = new CompositeDisposable();
        
        public ReadOnlyReactivePropertySlim<string> VerifyBSFolder { get; }
        public ReadOnlyReactivePropertySlim<Brush> VerifyBSFolderColor { get; }

        public ReactiveProperty<string> BSFolderPath { get; }

        public ReactiveCommand SelectBSFolderCommand { get; } = new ReactiveCommand();

        public ReadOnlyReactivePropertySlim<string> VerifyGitHubToken { get; }
        public ReadOnlyReactivePropertySlim<Brush> VerifyGitHubTokenColor { get; }

        public ReadOnlyReactivePropertySlim<string> VerifyMAExe { get; }
        public ReadOnlyReactivePropertySlim<Brush> VerifyMAExeColor { get; }

        public ReactiveProperty<string> MAExePath { get; }

        public ReactiveCommand SelectMAExeCommand { get; } = new ReactiveCommand();

        public ReactiveCommand SettingFinishCommand { get; }
        public ReactiveCommand VerifyGitHubTokenCommand { get; } = new ReactiveCommand();

        public InitialSettingViewModel(SettingsTabPropertyModel stpm, DataManager dm,ModAssistantManager mam,InnerData id,MainWindowPropertyModel mwpm,GitHubManager ghm,UpdateTabPropertyModel mtpm)
        {
            settingsTabPropertyModel = stpm;
            dataManager = dm;
            modAssistantManager = mam;
            innerData = id;
            mainWindowPropertyModel = mwpm;
            gitHubManager = ghm;
            updateTabPropertyModel = mtpm;

            // https://whitedog0215.hatenablog.jp/entry/2020/03/17/221403
            BSFolderPath = settingsTabPropertyModel.ToReactivePropertyAsSynchronized(x => x.BSFolderPath).AddTo(disposables);
            MAExePath = settingsTabPropertyModel.ToReactivePropertyAsSynchronized(x => x.MAExePath).AddTo(disposables);

            VerifyBSFolder = settingsTabPropertyModel.VerifyBSFolder.ToReadOnlyReactivePropertySlim().AddTo(disposables);
            VerifyBSFolderColor = settingsTabPropertyModel.VerifyBSFolderColor.ToReadOnlyReactivePropertySlim().AddTo(disposables);

            VerifyGitHubToken = settingsTabPropertyModel.VerifyGitHubToken.ToReadOnlyReactivePropertySlim().AddTo(disposables);
            VerifyGitHubTokenColor = settingsTabPropertyModel.VerifyGitHubTokenColor.ToReadOnlyReactivePropertySlim().AddTo(disposables);

            VerifyMAExe = settingsTabPropertyModel.VerifyMAExe.ToReadOnlyReactivePropertySlim().AddTo(disposables);
            VerifyMAExeColor = settingsTabPropertyModel.VerifyMAExeColor.ToReadOnlyReactivePropertySlim().AddTo(disposables);

            SelectBSFolderCommand.Subscribe(_ => BSFolderPath.Value = FolderManager.SelectFolderCommand(BSFolderPath.Value)).AddTo(disposables);
            SelectMAExeCommand.Subscribe(_ => MAExePath.Value = FilePath.SelectFile(MAExePath.Value)).AddTo(disposables);

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

            string dataDirectory = Path.Combine(FolderManager.dataFolder, dataManager.GetGameVersion());
            string modsDataCsvPath = Path.Combine(dataDirectory, "ModsData.csv");
            List<ModInformationCsv> previousDataList;
            if (File.Exists(modsDataCsvPath))
            {
                previousDataList = Task.Run(async () => await dataManager.ReadCsv(modsDataCsvPath)).GetAwaiter().GetResult();
                foreach (var previousData in previousDataList)
                {
                    if (Array.Exists(innerData.modAssistantAllMods, x => x.name == previousData.Mod))
                    {
                        if (previousData.Original)
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

                            updateTabPropertyModel.ModsData.Add(new UpdateTabPropertyModel.ModData(settingsTabPropertyModel,mainWindowPropertyModel,dataManager)
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
                        Release response = null;
                        string original = null;
                        Task.Run(async()=> { response = await gitHubManager.GetGitHubModLatestVersionAsync(previousData.Url); }).GetAwaiter().GetResult();

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
                            updateTabPropertyModel.ModsData.Add(new UpdateTabPropertyModel.ModData(settingsTabPropertyModel,mainWindowPropertyModel,dataManager)
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

                Task.Run(() => { dataManager.GetLocalModFilesInfo(); }).GetAwaiter().GetResult();

                /*
                アップデート時などに必要なら立ち上げるのがいいかも　
                if (settingsTabPropertyModel.VerifyMAExe.Value == "〇")
                {
                    try
                    {
                        System.Diagnostics.Process.Start(settingsTabPropertyModel.MAExePath);
                    }
                    catch(Exception e)
                    {
                        mainWindowPropertyModel.Console = e.Message;
                    }
                }
                */
            }
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
