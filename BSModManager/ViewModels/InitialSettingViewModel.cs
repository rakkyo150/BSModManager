using BSModManager.Models;
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
using static BSModManager.Models.ModCsvHandler;

namespace BSModManager.ViewModels
{
    public class InitialSettingViewModel : BindableBase, IDialogAware, IDestructible
    {
        SettingsVerifier settingsVerifier;

        GitHubApi gitHubApi;
        LocalMods modsDataModel;
        ModCsvHandler modCsv;
        InitialDirectorySetup initializer;
        MAMods mAMod;
        ConfigFileHandler configFile;
        Refresher refresher;

        CompositeDisposable disposables { get; } = new CompositeDisposable();

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

        internal InitialSettingViewModel(Refresher r,GitHubApi ghm, LocalMods mdm, ModCsvHandler mc,
            InitialDirectorySetup i, MAMods mam, SettingsVerifier sv, ConfigFileHandler cf)
        {
            refresher = r;
            gitHubApi = ghm;
            modsDataModel = mdm;
            modCsv = mc;
            initializer = i;
            mAMod = mam;
            settingsVerifier = sv;
            configFile = cf;

            // https://whitedog0215.hatenablog.jp/entry/2020/03/17/221403
            BSFolderPath = Folder.Instance.ToReactivePropertyAsSynchronized(x => x.BSFolderPath).AddTo(disposables);
            MAExePath = FilePath.Instance.ToReactivePropertyAsSynchronized(x => x.MAExePath).AddTo(disposables);

            VerifyBSFolder = new ReactiveProperty<string>("〇").AddTo(disposables);
            VerifyBSFolderColor = new ReactiveProperty<Brush>(Brushes.Green).AddTo(disposables);
            VerifyGitHubToken = new ReactiveProperty<string>("〇").AddTo(disposables);
            VerifyGitHubTokenColor = new ReactiveProperty<Brush>(Brushes.Green).AddTo(disposables);
            VerifyBSFolderAndGitHubToken = new ReactiveProperty<bool>(true).AddTo(disposables);
            VerifyMAExe = new ReactiveProperty<string>("〇").AddTo(disposables);
            VerifyMAExeColor = new ReactiveProperty<Brush>(Brushes.Green).AddTo(disposables);

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
            }).AddTo(disposables);

            SelectMAExeCommand.Subscribe(() =>
            {
                FilePath.Instance.MAExePath = FilePath.Instance.SelectFile(FilePath.Instance.MAExePath);
                configFile.Generate(Folder.Instance.BSFolderPath, gitHubApi.GitHubToken, FilePath.Instance.MAExePath);
            }).AddTo(disposables);

            SettingFinishCommand = VerifyBSFolderAndGitHubToken.ToReactiveCommand()
                .WithSubscribe(() => RequestClose.Invoke(new DialogResult(ButtonResult.OK))).AddTo(disposables);

            VerifyGitHubTokenCommand.Subscribe((x) =>
            {
                gitHubApi.GitHubToken = ((PasswordBox)x).Password;
            }).AddTo(disposables);
        }

        public string Title => "Initial Setting";

        public event Action<IDialogResult> RequestClose;

        public bool CanCloseDialog() => SettingFinishCommand.CanExecute();

        public void OnDialogClosed()
        {
            Task.Run(() =>
            {
                MainWindowLog.Instance.Debug = "Start Making Backup";
                initializer.Backup();
                MainWindowLog.Instance.Debug = "Finish Making Backup";
            }).GetAwaiter().GetResult();
            Task.Run(() => { mAMod.modAssistantAllMods = mAMod.GetAllAsync().Result; }).GetAwaiter().GetResult();

            string dataDirectory = Path.Combine(Folder.Instance.dataFolder, GameVersion.Version);
            string modsDataCsvPath = Path.Combine(dataDirectory, "ModsData.csv");
            List<ModCsvIndex> previousDataList;
            if (File.Exists(modsDataCsvPath))
            {
                previousDataList = Task.Run(async () => await modCsv.Read(modsDataCsvPath)).GetAwaiter().GetResult();
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

                            modsDataModel.LocalModsData.Add(new LocalMods.LocalModData(refresher)
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
                        Task.Run(async () => { response = await gitHubApi.GetModLatestVersionAsync(previousData.Url); }).GetAwaiter().GetResult();

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
                            modsDataModel.LocalModsData.Add(new LocalMods.LocalModData(refresher)
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

                            modsDataModel.LocalModsData.Add(new LocalMods.LocalModData(refresher)
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

                Task.Run(() => refresher.Refresh()).GetAwaiter().GetResult();
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
