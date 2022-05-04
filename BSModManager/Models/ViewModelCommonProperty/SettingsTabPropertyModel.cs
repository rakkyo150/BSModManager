﻿using Octokit;
using Prism.Mvvm;
using Reactive.Bindings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Media;

namespace BSModManager.Models.ViewModelCommonProperty
{
    public class SettingsTabPropertyModel : BindableBase
    {
        MainWindowPropertyModel mainWindowPropertyModel;
        VersionManager versionManager;
        ConfigFileManager configFileManager;

        public SettingsTabPropertyModel(MainWindowPropertyModel mwpm, VersionManager vm, ConfigFileManager cfm)
        {
            mainWindowPropertyModel = mwpm;
            versionManager = vm;
            configFileManager = cfm;

            Dictionary<string, string> tempDictionary = configFileManager.LoadConfigFile();
            if (tempDictionary["BSFolderPath"] != null && tempDictionary["GitHubToken"] != null)
            {
                BSFolderPath = tempDictionary["BSFolderPath"];
                GitHubToken = tempDictionary["GitHubToken"];
            }

            // https://nryblog.work/call-sync-to-async-method/
            Task.Run(() => { return CheckCredential(); }).GetAwaiter().GetResult();

            if (versionManager.GetGameVersion(BSFolderPath) == "GameVersion\n---")
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
                mainWindowPropertyModel.GameVersion = versionManager.GetGameVersion(BSFolderPath);
                configFileManager.MakeConfigFile(BSFolderPath, GitHubToken);
                if (Directory.Exists(BSFolderPath)) OpenBSFolderButtonEnable.Value = true;
                else OpenBSFolderButtonEnable.Value = false;
                if (versionManager.GetGameVersion(BSFolderPath) == "GameVersion\n---")
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
                configFileManager.MakeConfigFile(BSFolderPath, GitHubToken);

                // https://stackoverflow.com/questions/6602244/how-to-call-an-async-method-from-a-getter-or-setter
                new Task(async () =>
                {
                    await CheckCredential();
                }).Start();

                /*
                if (CheckCredential().Result)
                {
                    VerifyGitHubToken.Value = "〇";
                    VerifyGitHubTokenColor.Value = Brushes.Green;
                }
                else
                {
                    VerifyGitHubToken.Value = "×";
                    VerifyGitHubTokenColor.Value = Brushes.Red;
                }
                */
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

        public ReactiveProperty<bool> OpenBSFolderButtonEnable { get; } = new ReactiveProperty<bool>();

        public ReactiveProperty<string> VerifyBSFolder { get; } = new ReactiveProperty<string>("ー");
        public ReactiveProperty<Brush> VerifyBSFolderColor { get; } = new ReactiveProperty<Brush>(Brushes.Black);
        public ReactiveProperty<string> VerifyGitHubToken { get; } = new ReactiveProperty<string>("ー");
        public ReactiveProperty<Brush> VerifyGitHubTokenColor { get; } = new ReactiveProperty<Brush>(Brushes.Black);

        public ReactiveProperty<bool> VerifyBoth { get; } = new ReactiveProperty<bool>();
    }
}