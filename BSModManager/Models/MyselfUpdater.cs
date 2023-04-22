using BSModManager.Static;
using Octokit;
using Prism.Mvvm;
using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using System.Windows;

namespace BSModManager.Models
{
    public class MyselfUpdater : BindableBase
    {
        private readonly string myselfUrl = "https://github.com/rakkyo150/BSModManager";
        private Version latestMyselfVersion = new Version("0.0.0");
        private readonly GitHubApi gitHubApi;

        public MyselfUpdater(GitHubApi gha)
        {
            gitHubApi = gha;
        }

        public Version LatestMyselfVersion
        {
            get { return latestMyselfVersion; }
            set { SetProperty(ref latestMyselfVersion, value); }
        }

        private string latestMyselfDescription = string.Empty;
        public string LatestMyselfDescription
        {
            get { return latestMyselfDescription; }
        }

        public void SetLatestMyselfDescription(Release release)
        {
            latestMyselfDescription = release.Body;
        }

        public async Task<bool> CheckMyselfNewVersion()
        {
            System.Reflection.Assembly assembly = System.Reflection.Assembly.GetExecutingAssembly();
            System.Version rawVersion = assembly.GetName().Version;

            Version currentVersion = new Version(rawVersion.Major, rawVersion.Minor, rawVersion.Build);

            Release response = await gitHubApi.GetLatestReleaseInfoAsync(myselfUrl);

            if (response == null) return false;

            LatestMyselfVersion = VersionExtractor.DetectVersionFromRawVersion(response.TagName);
            SetLatestMyselfDescription(response);

            if (LatestMyselfVersion <= currentVersion)
            {
                Logger.Instance.Info("自分自身の更新版はみつかりませんでした");
                return false;
            }

            return true;
        }

        public async Task<bool> DownloadMyselfNewVersion()
        {
            string destDirFullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, LatestMyselfVersion.ToString());

            if (!Directory.Exists(destDirFullPath))
            {
                Directory.CreateDirectory(destDirFullPath);
            }

            await gitHubApi.DownloadAsync(myselfUrl, destDirFullPath);

            string zipFileName = Path.Combine(destDirFullPath, "BSModManager.zip");

            return UnzipMyselfNewVersion(destDirFullPath, zipFileName);
        }

        private static bool UnzipMyselfNewVersion(string destDirFullPath, string zipFileName)
        {
            try
            {
                using (FileStream fs = File.Open(zipFileName, System.IO.FileMode.Open))
                using (ZipArchive zip = new ZipArchive(fs))
                {
                    foreach (ZipArchiveEntry file in zip.Entries)
                    {
                        string installPath = Path.Combine(destDirFullPath, file.FullName);
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

        public void UpdateUpdater()
        {
            string downloadPath = Path.Combine(Environment.CurrentDirectory, LatestMyselfVersion.ToString());

            try
            {
                if (!Directory.Exists(downloadPath))
                {
                    MessageBox.Show("Updaterのアップデートができませんでした",
                        "アップデート失敗", MessageBoxButton.OK, MessageBoxImage.Error);
                    Environment.Exit(0);
                    return;
                }

                DirectoryInfo dir = new DirectoryInfo(downloadPath);

                FileInfo[] files = dir.GetFiles();
                foreach (FileInfo file in files)
                {
                    if (!file.Name.Contains("Updater") || file.Name.Contains("Setup")) continue;

                    string tempPath = Path.Combine(Environment.CurrentDirectory, file.Name);
                    file.CopyTo(tempPath, true);
                }
                Logger.Instance.Info("Updaterのアップデート完了");
            }
            catch (Exception ex)
            {
                Logger.Instance.Error($"{ex.Message}\nUpdaterのアップデートができませんでした" +
                    $"\n最新バージョンのフォルダが生成されているはずなので、手動で中身を上書きコピペしてください");
                Environment.Exit(0);
            }
        }
    }
}
