﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using BSModManager.Models.Structure;
using BSModManager.Models.ViewModelCommonProperty;
using Octokit;

namespace BSModManager.Models.CoreManager
{
    public class GitHubManager: DataManager
    {
        ConfigFileManager configFileManager;
        UpdateMyselfConfirmPropertyModel updateMyselfConfirmPropertyModel;
        SettingsTabPropertyModel settingsTabPropertyModel;
        
        public GitHubManager(InnerData id,ConfigFileManager cfm,UpdateMyselfConfirmPropertyModel umcpm,SettingsTabPropertyModel stpm): base(id,stpm,umcpm)
        {
            configFileManager = cfm;
            updateMyselfConfirmPropertyModel = umcpm;
            settingsTabPropertyModel = stpm;
        }
        
        public async Task<bool> CheckNewVersionAndDowonload()
        {
            GitHubClient github = new GitHubClient(new ProductHeaderValue("GithubModUpdateChecker"));
            bool update = false;
            string url = "https://github.com/rakkyo150/GithubModUpdateCheckerConsole";
            string destDirFullPath;

            System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();
            System.Version currentVersion = asm.GetName().Version;

            updateMyselfConfirmPropertyModel.LatestMyselfVersion = await GetGitHubModLatestVersionAsync(url);
            if (updateMyselfConfirmPropertyModel.LatestMyselfVersion > currentVersion)
            {
                destDirFullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, updateMyselfConfirmPropertyModel.LatestMyselfVersion.ToString());
                if (!Directory.Exists(destDirFullPath))
                {
                    Directory.CreateDirectory(destDirFullPath);
                }

                await DownloadGitHubModAsync(url, currentVersion, destDirFullPath, null, null);

                string zipFileName = Path.Combine(destDirFullPath, "GitHubModUpdateCheckerConsole.zip");
                try
                {
                    using (var fs = File.Open(zipFileName, System.IO.FileMode.Open))
                    using (var zip = new ZipArchive(fs))
                    {
                        foreach (var file in zip.Entries)
                        {
                            var installPath = Path.Combine(destDirFullPath, file.FullName);
                            if (File.Exists(installPath))
                            {
                                File.Delete(installPath);
                            }
                        }
                        zip.ExtractToDirectory(destDirFullPath);
                    }
                    File.Delete(zipFileName);

                    string unzipPath = Path.Combine(destDirFullPath, "GithubModUpdateCheckerConsole");
                    if (Directory.Exists(unzipPath))
                    {
                        DirectoryCopy(unzipPath, destDirFullPath, true);
                        Directory.Delete(unzipPath, true);
                    }

                    update = true;
                    return update;
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{e}");
                    return update;
                }
            }
            else
            {
                Console.WriteLine("更新版はみつかりませんでした");
                return update;
            }
        }

        public async Task CheckCredential()
        {
            bool checkCredential = false;

            while (!checkCredential)
            {
                var credential = new Credentials(settingsTabPropertyModel.GitHubToken);
                GitHubClient gitHub = new GitHubClient(new ProductHeaderValue("GithubModUpdateChecker"));
                gitHub.Credentials = credential;

                string owner = "rakkyo150";
                string name = "GithubModUpdateCheckerConsole";

                try
                {
                    var response = await gitHub.Repository.Release.GetLatest(owner, name);
                    checkCredential = true;
                    Console.WriteLine("Tokenは有効です");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Tokenは無効です");
                    Console.WriteLine("新たなTokenを入力してください");
                    string token = Console.ReadLine();
                    settingsTabPropertyModel.GitHubToken = token;
                }
            }

            configFileManager.MakeConfigFile(settingsTabPropertyModel.BSFolderPath,settingsTabPropertyModel.GitHubToken);
        }

        // Initializeでも使うので第二引数が必要
        public async Task InputGitHubModInformationAsync(KeyValuePair<string, Version> fileAndVersion, List<GitHubModInformationCsv> githubModInformationToCsv)
        {
            Console.WriteLine($"{fileAndVersion.Key} : {fileAndVersion.Value}");

            Console.WriteLine("オリジナルModですか？ [y/n]");
            var ok = Console.ReadLine();
            bool originalMod;
            if (ok == "y")
            {
                originalMod = true;
            }
            else
            {
                originalMod = false;
            }

            string gitHubUrl = "p";
            string gitHubModVersion = "0.0.0";
            bool inputUrlFinish = false;
            while (!inputUrlFinish)
            {
                Console.WriteLine("GithubのリポジトリのURLを入力してください");
                Console.WriteLine("Google検索したい場合は\"s\"を、URLが無いような場合は\"p\"を入力してください");
                gitHubUrl = Console.ReadLine();
                if (gitHubUrl == "s")
                {
                    try
                    {
                        string searchUrl = $"https://www.google.com/search?q={fileAndVersion.Key}";
                        ProcessStartInfo pi = new ProcessStartInfo()
                        {
                            FileName = searchUrl,
                            UseShellExecute = true,
                        };
                        Process.Start(pi);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                        Console.WriteLine("Google検索できませんでした");
                    }
                }
                else if (gitHubUrl == "p")
                {
                    Console.WriteLine("最新のリリース情報を取得しません");
                    inputUrlFinish = true;
                }
                else
                {
                    Console.WriteLine("Githubの最新のリリースのタグ情報を取得します");

                    Version tempGitHubModVersion = await GetGitHubModLatestVersionAsync(gitHubUrl);
                    gitHubModVersion = tempGitHubModVersion.ToString();
                    if (gitHubModVersion == new Version("0.0.0").ToString())
                    {
                        Console.WriteLine("リリース情報が取得できませんでした");
                        Console.WriteLine("URLを修正しますか？ [y/n]");
                        var a = Console.ReadLine();
                        if (a != "y")
                        {
                            inputUrlFinish = true;
                        }
                    }
                    else
                    {
                        inputUrlFinish = true;
                    }
                }
            }

            Console.WriteLine("GithubModData.csvにデータを追加します");
            Console.WriteLine("データを書き換えたい場合、このcsvを直接書き換えてください");

            var githubModInstance = new GitHubModInformationCsv()
            {
                GithubMod = fileAndVersion.Key,
                LocalVersion = fileAndVersion.Value.ToString(),
                GithubVersion = gitHubModVersion,
                OriginalMod = originalMod,
                GithubUrl = gitHubUrl,
            };
            githubModInformationToCsv.Add(githubModInstance);
        }

        public async Task DownloadGitHubModAsync(string url, Version currentVersion, string destDirFullPath, List<GitHubModInformationCsv> gitHubModInformationToCsv, string fileName)
        {
            if (url == "p") return;

            var credential = new Credentials(settingsTabPropertyModel.GitHubToken);
            GitHubClient gitHub = new GitHubClient(new ProductHeaderValue("GitHubModUpdateChecker"));
            gitHub.Credentials = credential;

            string temp = url.Replace("https://github.com/", "");
            int nextSlashPosition = temp.IndexOf('/');

            if (nextSlashPosition == -1)
            {
                Console.WriteLine("URLにミスがあります");
                Console.WriteLine($"対象のURL : {url}");
                return;
            }

            string owner = temp.Substring(0, nextSlashPosition);
            string name = temp.Substring(nextSlashPosition + 1);

            var response = await gitHub.Repository.Release.GetLatest(owner, name);

            try
            {
                string releaseBody = response.Body;
                var releaseCreatedAt = response.CreatedAt;
                DateTimeOffset now = DateTimeOffset.UtcNow;

                Version latestVersion = DetectVersion(response.TagName);

                if (latestVersion == null)
                {
                    throw new Exception("バージョン情報の取得に失敗");
                }

                if (latestVersion > currentVersion)
                {
                    Console.WriteLine("****************************************************");
                    Console.WriteLine($"{owner}/{name}の最新バージョン:{latestVersion}が見つかりました");

                    Console.WriteLine("----------------------------------------------------");
                    if ((now - releaseCreatedAt).Days >= 1)
                    {
                        Console.WriteLine((now - releaseCreatedAt).Days + "日前にリリース");
                    }
                    else
                    {
                        Console.WriteLine((now - releaseCreatedAt).Hours + "時間" + (now - releaseCreatedAt).Minutes + "分前にリリース");
                    }
                    Console.WriteLine("リリースの説明");
                    Console.WriteLine(releaseBody);
                    Console.WriteLine("----------------------------------------------------");

                    bool downloadChoiceFinish = false;
                    while (!downloadChoiceFinish)
                    {
                        Console.WriteLine("ダウンロードしますか？ [y/n]");
                        Console.WriteLine("リポジトリを確認したい場合は\"r\"を入力してください");
                        string download = Console.ReadLine();
                        if (download == "r")
                        {
                            try
                            {
                                string searchUrl = url;
                                ProcessStartInfo pi = new ProcessStartInfo()
                                {
                                    FileName = searchUrl,
                                    UseShellExecute = true,
                                };
                                Process.Start(pi);

                                downloadChoiceFinish = false;
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(ex.Message);
                                Console.WriteLine("リポジトリが開けませんでした");
                            }
                        }
                        else if (download == "y")
                        {
                            foreach (var item in response.Assets)
                            {
                                Console.WriteLine("ダウンロード中");
                                await DownloadModHelperAsync(item.BrowserDownloadUrl, item.Name, destDirFullPath);
                                Console.WriteLine("ダウンロード成功！");
                            }

                            if (gitHubModInformationToCsv != null)
                            {
                                if (gitHubModInformationToCsv.Find(n => n.GithubMod == fileName) == null)
                                {
                                    Console.WriteLine("csvのModのバージョンを更新できませんでした");
                                }
                                else
                                {
                                    gitHubModInformationToCsv.Find(n => n.GithubMod == fileName).LocalVersion = latestVersion.ToString();
                                }
                            }
                            downloadChoiceFinish = true;
                        }
                        else
                        {
                            Console.WriteLine("ダウンロードしません");
                            downloadChoiceFinish = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("リリースが見つかりませんでした");
                Console.WriteLine($"対象のリポジトリのURL : {url}");
            }
        }

        public async Task<Version> GetGitHubModLatestVersionAsync(string url)
        {
            if (url == "p") return new Version("0.0.0");

            var credential = new Credentials(settingsTabPropertyModel.GitHubToken);
            GitHubClient gitHub = new GitHubClient(new ProductHeaderValue("GithubModUpdateChecker"));
            gitHub.Credentials = credential;

            string temp = url.Replace("https://github.com/", "");
            int nextSlashPosition = temp.IndexOf('/');

            if (nextSlashPosition == -1)
            {
                Console.WriteLine("URLにミスがあるかもしれません");
                Console.WriteLine($"対象のURL : {url}");
                return new Version("0.0.0");
            }

            string owner = temp.Substring(0, nextSlashPosition);
            string name = temp.Substring(nextSlashPosition + 1);

            Version latestVersion = null;

            try
            {
                // プレリリースを取得する場合はGetAllしかないが、効率が悪いのでプレリリースには対応しません
                var response = await gitHub.Repository.Release.GetLatest(owner, name);
                latestVersion = DetectVersion(response.TagName);

                if (latestVersion == null)
                {
                    throw new Exception("バージョン情報の取得に失敗");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("URLにミスがあるかもしれません");
                Console.WriteLine($"対象のURL : {url}");
                return new Version("0.0.0");
            }

            return latestVersion;
        }

        // Based on https://qiita.com/thrzn41/items/2754bec8ebad97ecd7fd
        public async Task DownloadModHelperAsync(string uri, string name, string destDirFullPath)
        {
            using (HttpClient httpClient = new HttpClient())
            using (var request = new HttpRequestMessage(HttpMethod.Get, new Uri(uri)))
            {
                try
                {
                    using (var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
                    {
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            using (var content = response.Content)
                            using (var stream = await content.ReadAsStreamAsync())
                            {
                                if (!Directory.Exists(destDirFullPath))
                                {
                                    Directory.CreateDirectory(destDirFullPath);
                                }
                                string pluginDownloadPath = Path.Combine(destDirFullPath, name);
                                using (var fileStream = new FileStream(pluginDownloadPath, System.IO.FileMode.Create, FileAccess.Write, FileShare.None))
                                {
                                    await stream.CopyToAsync(fileStream);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine("URLにミスがあるかもしれません");
                    Console.WriteLine($"対象のURL : {uri}");
                }
            }
        }

        public Version DetectVersion(string tagName)
        {
            Version version = null;

            // バージョン情報が始まる位置を特定
            int position = 0;
            foreach (char item in tagName)
            {
                if (item >= '0' && item <= '9')
                {
                    break;
                }
                position++;
            }

            //　バージョン情報が終わる位置を特定
            for (int i = 0; i <= tagName.Length - position - 1; i++)
            {
                char versionDetector = tagName[position + i];
                if (!(versionDetector >= '0' && versionDetector <= '9') && versionDetector != '.')
                {
                    version = new Version(tagName.Substring(position, i));
                    break;
                }
                if (i == tagName.Length - position - 1)
                {
                    version = new Version(tagName.Substring(position));
                }
            }

            return version;
        }
    }
}
