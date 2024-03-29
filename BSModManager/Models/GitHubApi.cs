﻿using BSModManager.Static;
using Octokit;
using Prism.Mvvm;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace BSModManager.Models
{
    public class GitHubApi : BindableBase
    {
        public async Task<string> GetTopRepository(string modName)
        {
            try
            {
                GitHubClient gitHub;

                if (Config.Instance.GitHubToken == string.Empty)
                {
                    gitHub = new GitHubClient(new ProductHeaderValue("BSModManager"));
                }
                else
                {
                    Credentials credential = new Credentials(Config.Instance.GitHubToken);
                    gitHub = new GitHubClient(new ProductHeaderValue("BSModManager"))
                    {
                        Credentials = credential
                    };
                }

                SearchRepositoriesRequest srr = new SearchRepositoriesRequest(modName);
                SearchRepositoryResult result = await gitHub.Search.SearchRepo(srr);

                if (result.TotalCount == 0) return string.Empty;

                return result.Items[0].HtmlUrl;
            }
            catch (Exception ex)
            {
                Logger.Instance.Error(ex.Message);
                return string.Empty;
            }
        }

        public async Task DownloadAsync(string url, string destDirFullPath)
        {
            Release response = await GetLatestReleaseInfoAsync(url);
            if (response == null) return;

            foreach (ReleaseAsset item in response.Assets)
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
                    gitHub = new GitHubClient(new ProductHeaderValue("BSModManager"));
                }
                else
                {
                    Credentials credential = new Credentials(Config.Instance.GitHubToken);
                    gitHub = new GitHubClient(new ProductHeaderValue("BSModManager"))
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

                Release response = await gitHub.Repository.Release.GetLatest(owner, name);
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
            using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, new Uri(uri)))
            {
                try
                {
                    using (HttpResponseMessage response = await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead))
                    {
                        if (response.StatusCode != HttpStatusCode.OK)
                        {
                            Logger.Instance.Info($"{uri}のダウンロードに失敗しました");
                            return false;
                        }

                        using (HttpContent content = response.Content)
                        using (Stream stream = await content.ReadAsStreamAsync())
                        {
                            if (!Directory.Exists(destDirFullPath))
                            {
                                Directory.CreateDirectory(destDirFullPath);
                            }
                            string pluginDownloadPath = Path.Combine(destDirFullPath, name);
                            using (FileStream fileStream = new FileStream(pluginDownloadPath, System.IO.FileMode.Create, FileAccess.Write, FileShare.None))
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
