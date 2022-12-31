using BSModManager.Interfaces;
using BSModManager.Static;
using Prism.Mvvm;
using Prism.Navigation;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.IO;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace BSModManager.Models.Mods.Structures
{
    public class RecommendModData : BindableBase, IModData, IDestructible
    {
        private bool c = false;
        private string mod = string.Empty;
        private Version installed = new Version("0.0.0");
        private Version latest = new Version("0.0.0");
        private string downloadedFileHash = string.Empty;
        private string updated = "?";
        private string original = "〇";
        private string mA = "×";
        private string description = "?";
        private Brush installedColor = Brushes.Green;
        private string url = string.Empty;
        readonly Refresher refresher;

        public ReactiveCommand<string> UninstallCommand { get; } = new ReactiveCommand<string>();

        public CompositeDisposable disposables = new CompositeDisposable();

        public RecommendModData(Refresher r)
        {
            refresher = r;

            UninstallCommand.Subscribe((x) => Uninstall(x)).AddTo(disposables);
        }

        public bool Checked
        {
            get { return c; }
            set { SetProperty(ref c, value); }
        }
        public string Mod
        {
            get { return mod; }
            set { SetProperty(ref mod, value); }
        }
        public Version Installed
        {
            get { return installed; }
            set
            {
                SetProperty(ref installed, new Version(value.Major, value.Minor, value.Build));
                if (Installed == Latest) InstalledColor = Brushes.Green;
                else if (Installed < Latest) InstalledColor = Brushes.Red;
                else if (Installed > Latest) InstalledColor = Brushes.Orange;

                if (Latest == new Version("0.0.0")) InstalledColor = Brushes.Blue;
            }
        }
        public Version Latest
        {
            get { return latest; }
            set
            {
                SetProperty(ref latest, value);
                if (Installed == Latest) InstalledColor = Brushes.Green;
                else if (Installed < Latest) InstalledColor = Brushes.Red;
                else if (Installed > Latest) InstalledColor = Brushes.Orange;

                if (Latest == new Version("0.0.0")) InstalledColor = Brushes.Blue;
            }
        }
        public string DownloadedFileHash
        {
            get { return downloadedFileHash; }
            set
            {
                SetProperty(ref downloadedFileHash, value);
            }
        }
        public string Original
        {
            get { return original; }
            set { SetProperty(ref original, value); }
        }
        public string Updated
        {
            get { return updated; }
            set { SetProperty(ref updated, value); }
        }
        public string MA
        {
            get { return mA; }
            set { SetProperty(ref mA, value); }
        }
        public string Description
        {
            get { return description; }
            set { SetProperty(ref description, value); }
        }
        public string Url
        {
            get { return url; }
            set { SetProperty(ref url, value); }
        }
        public Brush InstalledColor
        {
            get { return installedColor; }
            set { SetProperty(ref installedColor, value); }
        }

        public void Uninstall(string modName)
        {
            string modFileName = modName + ".dll";
            string modFilePath = Path.Combine(Config.Instance.BSFolderPath, "Plugins", modFileName);
            string modPendingFilePath = Path.Combine(Config.Instance.BSFolderPath, "IPA", "Pending", "Plugins", modFileName);

            if (MessageBoxResult.Yes == MessageBox.Show($"{modName}を削除します。よろしいですか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Information))
            {
                if (!File.Exists(modFilePath) && !File.Exists(modPendingFilePath))
                {
                    Logger.Instance.Error($"Fail to Delete {modFilePath} or {modPendingFilePath}");
                    return;
                }

                if (File.Exists(modFilePath))
                {
                    File.Delete(modFilePath);
                }

                if (File.Exists(modPendingFilePath))
                {
                    File.Delete(modPendingFilePath);
                }

                Task.Run(() => refresher.Refresh()).GetAwaiter().GetResult();
                Logger.Instance.Info($"Finish Deleting {modFilePath}");
            }
        }

        public void Destroy()
        {
            disposables.Dispose();
        }
    }
}
