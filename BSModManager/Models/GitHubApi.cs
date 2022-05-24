using BSModManager.Static;
using Octokit;
using Prism.Mvvm;
using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace BSModManager.Models
{
    public class GitHubApi : BindableBase
    {
        private string gitHubToken = "";
        public string GitHubToken
        {
            get => gitHubToken;
            set
            {
                SetProperty(ref gitHubToken, value);
            }
        }

        MyselfUpdater myselfUpdater;

        public GitHubApi(MyselfUpdater u)
        {
            myselfUpdater = u;
        }

        public async Task<bool> CheckNewVersionAndDowonload()
        {
            GitHubClient github = new GitHubClient(new ProductHeaderValue("BSModManager"));
            string url = "https://github.com/rakkyo150/BSModManager";
            string destDirFullPath;

            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            System.Version rawVersion = assembly.GetName().Version;

            Version currentVersion = new Version(rawVersion.Major, rawVersion.Minor, rawVersion.Build);

            Release response = await GetModLatestVersionAsync(url);

            if (response == null) return false;

            myselfUpdater.LatestMyselfVersion = DetectVersion((await GetModLatestVersionAsync(url)).TagName);

            if (myselfUpdater.LatestMyselfVersion <= currentVersion)
            {
                Console.WriteLine("更新版はみつかりませんでした");
                return false;
            }

            destDirFullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, myselfUpdater.LatestMyselfVersion.ToString());
            
            if (!Directory.Exists(destDirFullPath))
            {
                Directory.CreateDirectory(destDirFullPath);
            }

            await DownloadAsync(url, currentVersion, destDirFullPath);

            string zipFileName = Path.Combine(destDirFullPath, "BSModManager.zip");
            try
            {
                using (var fs = File.Open(zipFileName, System.IO.FileMode.Open))
                using (var zip = new ZipArchive(fs))
                {
                    foreach (var file in zip.Entries)
                    {
                        var installPath = Path.Combine(destDirFullPath, file.FullName);
                        if (!File.Exists(installPath)) continue;
                                
                        File.Delete(installPath);
                    }
                    zip.ExtractToDirectory(destDirFullPath);
                }
                File.Delete(zipFileName);

                string unzipPath = Path.Combine(destDirFullPath, "BSModManager");

                if (!Directory.Exists(unzipPath)) return false;

                Folder.Instance.Copy(unzipPath, destDirFullPath, true);
                Directory.Delete(unzipPath, true);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine($"{e}");
                return false;
            }
        }

        public async Task DownloadAsync(string url, Version currentVersion, string destDirFullPath)
        {
            var credential = new Credentials(GitHubToken);
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

                if (latestVersion <= currentVersion) return;

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
                    await DownloadHelperAsync(item.BrowserDownloadUrl, item.Name, destDirFullPath);
                    Console.WriteLine("ダウンロード成功！");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("リリースが見つかりませんでした");
                Console.WriteLine($"対象のリポジトリのURL : {url}");
            }
        }

        public async Task<Release> GetModLatestVersionAsync(string url)
        {
            var credential = new Credentials(GitHubToken);
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
        public async Task<bool> DownloadHelperAsync(string uri, string name, string destDirFullPath)
        {
            using (HttpClient httpClient = new HttpClient())
            using (var request = new HttpRequestMessage(HttpMethod.Get, new Uri(uri)))
            {
                try
                {
                    using (var response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
                    {
                        if (response.StatusCode != HttpStatusCode.OK)
                        {
                            Console.WriteLine("失敗しました");
                            Console.WriteLine($"対象のURL : {uri}");
                            return false;
                        }

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

                            return true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                    Console.WriteLine("URLにミスがあるかもしれません");
                    Console.WriteLine($"対象のURL : {uri}");
                    return false;
                }
            }
        }

        public Version DetectVersion(string tagName)
        {
            Version version = null;

            if (tagName == null) return version;

            int versionInfoStartPosition = 0;
            foreach (char item in tagName)
            {
                if (item >= '0' && item <= '9')
                {
                    break;
                }
                versionInfoStartPosition++;
            }

            for (int versionInfoFinishPosition = 0; versionInfoFinishPosition <= tagName.Length - versionInfoStartPosition - 1; versionInfoFinishPosition++)
            {
                char versionDetector = tagName[versionInfoStartPosition + versionInfoFinishPosition];
                if (!(versionDetector >= '0' && versionDetector <= '9') && versionDetector != '.')
                {
                    version = new Version(tagName.Substring(versionInfoStartPosition, versionInfoFinishPosition));
                    return version;
                }
            }

            version = new Version(tagName.Substring(versionInfoStartPosition));
            return version;
        }

        public async Task<bool> CheckCredential()
        {
            if (GitHubToken == "") return false;

            var credential = new Credentials(GitHubToken);
            GitHubClient gitHub = new GitHubClient(new ProductHeaderValue("GithubModUpdateChecker"));
            gitHub.Credentials = credential;

            string owner = "rakkyo150";
            string name = "GithubModUpdateCheckerConsole";

            try
            {
                var response = await gitHub.Repository.Release.GetLatest(owner, name);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
        }
    }
}
