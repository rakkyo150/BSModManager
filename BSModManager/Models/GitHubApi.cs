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
        readonly string myselfUrl = "https://github.com/rakkyo150/BSModManager";

        readonly MyselfUpdater myselfUpdater;

        public GitHubApi(MyselfUpdater u)
        {
            myselfUpdater = u;
        }

        public async Task<bool> CheckMyselfNewVersion()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            System.Version rawVersion = assembly.GetName().Version;

            Version currentVersion = new Version(rawVersion.Major, rawVersion.Minor, rawVersion.Build);

            Release response = await GetLatestReleaseInfoAsync(myselfUrl);

            if (response == null) return false;

            myselfUpdater.LatestMyselfVersion = VersionExtractor.DetectVersionFromRawVersion(response.TagName);
            myselfUpdater.SetLatestMyselfDescription(response);

            if (myselfUpdater.LatestMyselfVersion <= currentVersion)
            {
                Logger.Instance.Info("自分自身の更新版はみつかりませんでした");
                return false;
            }

            return true;
        }

        public async Task<bool> DownloadMyselfNewVersion()
        {
            string destDirFullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, myselfUpdater.LatestMyselfVersion.ToString());

            if (!Directory.Exists(destDirFullPath))
            {
                Directory.CreateDirectory(destDirFullPath);
            }

            await DownloadAsync(myselfUrl, destDirFullPath);

            string zipFileName = Path.Combine(destDirFullPath, "BSModManager.zip");

            return UnzipMyselfNewVersion(destDirFullPath, zipFileName);
        }

        private static bool UnzipMyselfNewVersion(string destDirFullPath, string zipFileName)
        {
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
            catch (Exception ex)
            {
                Logger.Instance.Debug($"{ex}");
                Logger.Instance.Info("自分自身の最新版のダウンロードに失敗しました");
                return false;
            }
        }

        public async Task DownloadAsync(string url, string destDirFullPath)
        {
            Release response = await GetLatestReleaseInfoAsync(url);
            if (response == null) return;

            foreach (var item in response.Assets)
            {
                Logger.Instance.Info($"{item.Name}をダウンロード中");
                bool success = await StreamingDownloadAsync(item.BrowserDownloadUrl, item.Name, destDirFullPath);
                if (success) Logger.Instance.Info($"{item.Name}のダウンロード成功！");
                else Logger.Instance.Info($"{item.Name}のダウンロード失敗");
            }
        }

        public async Task<Release> GetLatestReleaseInfoAsync(string url)
        {
            string owner = string.Empty;
            string name = string.Empty;

            try
            {
                GitHubClient gitHub;

                if (Config.Instance.GitHubToken == string.Empty)
                {
                    gitHub = new GitHubClient(new ProductHeaderValue("GitHubModUpdateChecker"));
                }
                else
                {
                    var credential = new Credentials(Config.Instance.GitHubToken);
                    gitHub = new GitHubClient(new ProductHeaderValue("GitHubModUpdateChecker"))
                    {
                        Credentials = credential
                    };
                }

                string temp = url.Replace("https://github.com/", string.Empty);
                int nextSlashPosition = temp.IndexOf('/');

                if (nextSlashPosition == -1)
                {
                    Logger.Instance.Info($"{url}は不正なURLです");
                    return null;
                }

                owner = temp.Substring(0, nextSlashPosition);
                name = temp.Substring(nextSlashPosition + 1);

                var response = await gitHub.Repository.Release.GetLatest(owner, name);
                return response;
            }
            catch (Exception ex)
            {
                Logger.Instance.Info($"{owner}/{name}のリリース情報を取得できませんでした");
                Logger.Instance.Debug(ex.Message);
                return null;
            }
        }

        // Based on https://qiita.com/thrzn41/items/2754bec8ebad97ecd7fd
        private async Task<bool> StreamingDownloadAsync(string uri, string name, string destDirFullPath)
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
                            Logger.Instance.Info($"{uri}のダウンロードに失敗しました");
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
                    Logger.Instance.Info($"{uri}のダウンロードに失敗しました");
                    Logger.Instance.Debug(ex.Message);
                    return false;
                }
            }
        }
    }
}
