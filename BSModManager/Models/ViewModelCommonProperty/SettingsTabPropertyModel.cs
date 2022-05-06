using Octokit;
using Prism.Mvvm;
using Prism.Navigation;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using System.Windows.Media;

namespace BSModManager.Models.ViewModelCommonProperty
{
    public class SettingsTabPropertyModel : BindableBase,IDestructible
    {
        MainWindowPropertyModel mainWindowPropertyModel;
        VersionManager versionManager;
        ConfigFileManager configFileManager;

        public SettingsTabPropertyModel(MainWindowPropertyModel mwpm, VersionManager vm, ConfigFileManager cfm)
        {
            mainWindowPropertyModel = mwpm;
            versionManager = vm;
            configFileManager = cfm;

            OpenBSFolderButtonEnable.AddTo(disposables);
            VerifyBSFolder.AddTo(disposables);
            VerifyBSFolderColor.AddTo(disposables);
            VerifyGitHubToken.AddTo(disposables);
            VerifyGitHubTokenColor.AddTo(disposables);
            VerifyBoth.AddTo(disposables);
            VerifyMAExe.AddTo(disposables);
            VerifyMAExeColor.AddTo(disposables);

            Dictionary<string, string> tempDictionary = configFileManager.LoadConfigFile();
            if (tempDictionary["BSFolderPath"] != null && tempDictionary["GitHubToken"] != null)
            {
                BSFolderPath = tempDictionary["BSFolderPath"];
                GitHubToken = tempDictionary["GitHubToken"];
                MAExePath = tempDictionary["MAExePath"];
            }

            // https://nryblog.work/call-sync-to-async-method/
            Task.Run(() => { return CheckCredential(); }).GetAwaiter().GetResult();

            if (versionManager.GetGameVersionStr(BSFolderPath) == "GameVersion\n---")
            {
                VerifyBSFolder.Value = "×";
                VerifyBoth.Value = false;
                VerifyBSFolderColor.Value = Brushes.Red;
            }
            else
            {
                VerifyBSFolder.Value = "〇";
                if (VerifyGitHubToken.Value == "〇")
                {
                    VerifyBoth.Value = true;
                }
                VerifyBSFolderColor.Value = Brushes.Green;
            }
        }

        // テスト用にあえてパス間違えてる
        private string bSFolderPath = @"C:\Program Files (x86)\Steam\steamapps\common\Beat Sabe";
        public string BSFolderPath
        {
            get => bSFolderPath;
            set
            {
                SetProperty(ref bSFolderPath, value);
                mainWindowPropertyModel.GameVersion = versionManager.GetGameVersionStr(BSFolderPath);
                configFileManager.MakeConfigFile(BSFolderPath, GitHubToken, MAExePath);
                if (Directory.Exists(BSFolderPath)) OpenBSFolderButtonEnable.Value = true;
                else OpenBSFolderButtonEnable.Value = false;
                if (versionManager.GetGameVersionStr(BSFolderPath) == "GameVersion\n---")
                {
                    VerifyBSFolder.Value = "×";
                    VerifyBoth.Value = false;
                    VerifyBSFolderColor.Value = Brushes.Red;
                }
                else
                {
                    VerifyBSFolder.Value = "〇";
                    if (VerifyGitHubToken.Value == "〇")
                    {
                        VerifyBoth.Value = true;
                    }
                    VerifyBSFolderColor.Value = Brushes.Green;
                }
            }
        }

        private string gitHubToken = "";
        public string GitHubToken
        {
            get => gitHubToken;
            set
            {
                SetProperty(ref gitHubToken, value);
                configFileManager.MakeConfigFile(BSFolderPath, GitHubToken, MAExePath);

                // https://stackoverflow.com/questions/6602244/how-to-call-an-async-method-from-a-getter-or-setter
                new Task(async () =>
                {
                    await CheckCredential();
                }).Start();
            }
        }

        private string mAExePath = "";
        public string MAExePath
        {
            get { return mAExePath; }
            set 
            { 
                SetProperty(ref mAExePath, value);
                configFileManager.MakeConfigFile(BSFolderPath, GitHubToken, MAExePath);

                if (Path.GetFileName(MAExePath) == "ModAssistant.exe")
                {
                    VerifyMAExe.Value = "〇";
                    VerifyMAExeColor.Value = Brushes.Green;
                }
                else
                {
                    VerifyMAExe.Value = "×";
                    VerifyMAExeColor.Value = Brushes.Red;
                }
            }
        }

        public async Task<bool> CheckCredential()
        {
            bool checkCredential = false;

            if (GitHubToken == "")
            {
                return false;
            }

            var credential = new Credentials(GitHubToken);
            GitHubClient gitHub = new GitHubClient(new ProductHeaderValue("GithubModUpdateChecker"));
            gitHub.Credentials = credential;

            string owner = "rakkyo150";
            string name = "GithubModUpdateCheckerConsole";

            try
            {
                var response = await gitHub.Repository.Release.GetLatest(owner, name);
                checkCredential = true;
                VerifyGitHubToken.Value = "〇";
                if (VerifyBSFolder.Value == "〇")
                {
                    VerifyBoth.Value = true;
                }
                else
                {
                    VerifyBoth.Value = false;
                }
                VerifyGitHubTokenColor.Value = Brushes.Green;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                VerifyGitHubToken.Value = "×";
                VerifyBoth.Value = false;
                VerifyGitHubTokenColor.Value = Brushes.Red;
            }

            return checkCredential;
        }

        public void Destroy()
        {
            disposables.Dispose();
        }

        private CompositeDisposable disposables { get; } = new CompositeDisposable();
        
        public ReactiveProperty<bool> OpenBSFolderButtonEnable { get; } = new ReactiveProperty<bool>();

        public ReactiveProperty<string> VerifyBSFolder { get; } = new ReactiveProperty<string>("ー");
        public ReactiveProperty<Brush> VerifyBSFolderColor { get; } = new ReactiveProperty<Brush>(Brushes.Black);
        public ReactiveProperty<string> VerifyGitHubToken { get; } = new ReactiveProperty<string>("ー");
        public ReactiveProperty<Brush> VerifyGitHubTokenColor { get; } = new ReactiveProperty<Brush>(Brushes.Black);

        public ReactiveProperty<bool> VerifyBoth { get; } = new ReactiveProperty<bool>();

        public ReactiveProperty<string> VerifyMAExe { get; } = new ReactiveProperty<string>("ー");
        public ReactiveProperty<Brush> VerifyMAExeColor { get; } = new ReactiveProperty<Brush>(Brushes.Black);
    }
}
