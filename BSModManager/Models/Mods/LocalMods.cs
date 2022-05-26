using BSModManager.Interfaces;
using BSModManager.Static;
using Prism.Mvvm;
using Prism.Navigation;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace BSModManager.Models
{
    public class LocalMods : BindableBase, IMods
    {
        public ObservableCollection<IModData> LocalModsData = new ObservableCollection<IModData>();

        public void AllCheckedOrUnchecked()
        {
            Console.WriteLine(LocalModsData.Count);

            int i = 0;
            if (LocalModsData.Count(x => x.Checked == true) * 2 > LocalModsData.Count)
            {
                Console.WriteLine("to false");
                foreach (var _ in LocalModsData)
                {
                    LocalModsData[i].Checked = false;
                    i++;
                }
                return;
            }

            Console.WriteLine("to true");
            foreach (var _ in LocalModsData)
            {
                LocalModsData[i].Checked = true;
                i++;
            }
        }

        public void ModRepositoryOpen()
        {
            foreach (var a in LocalModsData)
            {
                if (!a.Checked) continue;

                try
                {
                    string searchUrl = a.Url;
                    ProcessStartInfo pi = new ProcessStartInfo()
                    {
                        FileName = searchUrl,
                        UseShellExecute = true,
                    };
                    Process.Start(pi);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{a.Mod}のURL : \"{a.Url}\"を開けませんでした");
                    Console.WriteLine(ex.Message);
                }
            }
        }

        public void Update(IModData modData)
        {
            if (!ExisitsSameData(modData))
            {
                Console.WriteLine($"{modData}はUpdateされる予定でしたがAddに変更されます");
                Add(modData);
                return;
            }

            Remove(modData);
            LocalModsData.Add(modData);
        }

        public void Add(IModData modData)
        {
            if (ExisitsSameData(modData))
            {
                Console.WriteLine($"{modData}はAddされる予定でしたがUpdateに変更されます");
                Update(modData);
                return;
            }

            LocalModsData.Add(modData);
        }

        private bool ExisitsSameData(IModData modData)
        {
            return LocalModsData.Any(x => x.Mod == modData.Mod) && LocalModsData.Any(x => x.Original == modData.Original);
        }

        public void Remove(IModData modData)
        {
            if (!ExisitsSameData(modData))
            {
                Console.WriteLine($"{modData}のデータが存在しないため削除できませんでした");
                return;
            }

            LocalModsData.Remove(LocalModsData.First(x => x.Mod == modData.Mod));
        }

        public IEnumerable<IModData> ReturnCheckedModsData()
        {
            return LocalModsData.Where(x => x.Checked == true);
        }

        // 変更通知イベントがないとUIに反映されない
        // https://yutori-techblog.com/innerclass-private-access
        public class LocalModData : BindableBase, IDestructible, IModData
        {
            readonly Refresher refresher;

            public ReactiveCommand<string> UninstallCommand { get; } = new ReactiveCommand<string>();

            public CompositeDisposable disposables = new CompositeDisposable();


            public LocalModData(Refresher r)
            {
                refresher = r ;

                UninstallCommand.Subscribe((x) => Uninstall(x)).AddTo(disposables).AddTo(disposables);
            }

            private bool c = false;
            private string mod = "";
            private Version installed = new Version("0.0.0");
            private Version latest = new Version("0.0.0");
            private string updated = "?";
            private string original = "〇";
            private string mA = "×";
            private string description = "?";
            private Brush installedColor = Brushes.Green;
            private string url = "";

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

            public void Destroy()
            {
                disposables.Dispose();
            }

            public void Uninstall(string modName)
            {
                string modFileName = modName + ".dll";
                string modFilePath = Path.Combine(Folder.Instance.BSFolderPath, "Plugins", modFileName);
                string modPendingFilePath = Path.Combine(Folder.Instance.BSFolderPath, "IPA", "Pending", "Plugins", modFileName);

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

                    Task.Run(()=>refresher.Refresh()).GetAwaiter().GetResult();
                    Logger.Instance.Info($"Finish Deleting {modFilePath}");
                }
            }
        }
    }
}
