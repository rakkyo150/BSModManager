using BSModManager.Models.Structure;
using BSModManager.Models.ViewModelCommonProperty;
using Octokit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace BSModManager.Models.CoreManager
{
    public class GitHubManager : DataManager
    {
        UpdateMyselfConfirmPropertyModel updateMyselfConfirmPropertyModel;
        SettingsTabPropertyModel settingsTabPropertyModel;
        VersionManager versionManager;

        public GitHubManager(VersionManager vm,InnerData id, UpdateMyselfConfirmPropertyModel umcpm, SettingsTabPropertyModel stpm, LocalModsDataModel mdm,MainWindowPropertyModel mwpm) : base(id, stpm, umcpm, mwpm,mdm)
        {
            updateMyselfConfirmPropertyModel = umcpm;
            settingsTabPropertyModel = stpm;
            versionManager = vm;
        }

        public async Task<bool> CheckNewVersionAndDowonload()
        {
            GitHubClient github = new GitHubClient(new ProductHeaderValue("BSModManager"));
            bool update = false;
            string url = "https://github.com/rakkyo150/BSModManager";
            string destDirFullPath;

            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            System.Version rawVersion = assembly.GetName().Version;

            Version currentVersion = new Version(rawVersion.Major, rawVersion.Minor, rawVersion.Build);

            Release response = await GetGitHubModLatestVersionAsync(url);
            
            if (response == null)
            {
                return update;
            }

            updateMyselfConfirmPropertyModel.LatestMyselfVersion = DetectVersion((await GetGitHubModLatestVersionAsync(url)).TagName);
            
            if (updateMyselfConfirmPropertyModel.LatestMyselfVersion > currentVersion)
            {
                destDirFullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, updateMyselfConfirmPropertyModel.LatestMyselfVersion.ToString());
                if (!Directory.Exists(destDirFullPath))
                {
                    Directory.CreateDirectory(destDirFullPath);
                }

                await DownloadGitHubModAsync(url, currentVersion, destDirFullPath);

                string zipFileName = Path.Combine(destDirFullPath, "BSModManager.zip");
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

                    string unzipPath = Path.Combine(destDirFullPath, "BSModManager");
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

        // Initializeでも使うので第二引数が必要
        public async Task InputGitHubModInformationAsync(KeyValuePair<string, Version> fileAndVersion, List<ModInformationCsv> githubModInformationToCsv)
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

                    Version tempGitHubModVersion = DetectVersion((await GetGitHubModLatestVersionAsync(gitHubUrl)).TagName);
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

            var githubModInstance = new ModInformationCsv()
            {
                Mod = fileAndVersion.Key,
                LocalVersion = fileAndVersion.Value.ToString(),
                LatestVersion = gitHubModVersion,
                Original = originalMod,
                Url = gitHubUrl,
            };
            githubModInformationToCsv.Add(githubModInstance);
        }

        public async Task DownloadGitHubModAsync(string url, Version currentVersion, string destDirFullPath)
        {
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


                    foreach (var item in response.Assets)
                    {
                        Console.WriteLine("ダウンロード中");
                        await DownloadModHelperAsync(item.BrowserDownloadUrl, item.Name, destDirFullPath);
                        Console.WriteLine("ダウンロード成功！");
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

        public async Task<Release> GetGitHubModLatestVersionAsync(string url)
        {
            var credential = new Credentials(settingsTabPropertyModel.GitHubToken);
            GitHubClient gitHub = new GitHubClient(new ProductHeaderValue("GithubModUpdateChecker"));
            gitHub.Credentials = credential;

            string temp = url.Replace("https://github.com/", "");
            int nextSlashPosition = temp.IndexOf('/');

            if (nextSlashPosition == -1)
            {
                Console.WriteLine("GitHubのURLではないです");
                Console.WriteLine($"対象のURL : {url}");
                return null;
            }

            string owner = temp.Substring(0, nextSlashPosition);
            string name = temp.Substring(nextSlashPosition + 1);

            Version latestVersion = null;
            Release response = null;

            try
            {
                // プレリリース非対応
                response = await gitHub.Repository.Release.GetLatest(owner, name);
                latestVersion = DetectVersion(response.TagName);

                if (latestVersion == null)
                {
                    throw new Exception("バージョン情報の取得に失敗");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("リポジトリのURLではなさそうです");
                Console.WriteLine($"対象のURL : {url}");
                return null;
            }

            return response;
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

            if (tagName == null) return version;
            
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
