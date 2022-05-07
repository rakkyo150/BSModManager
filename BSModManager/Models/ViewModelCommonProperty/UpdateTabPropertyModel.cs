using BSModManager.Models.CoreManager;
using BSModManager.Models.ViewModelCommonProperty;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Windows;
using System.Windows.Media;

namespace BSModManager.Models
{
    public class UpdateTabPropertyModel : BindableBase
    {
        public ObservableCollection<ModData> ModsData = new ObservableCollection<ModData>();

        public MainWindowPropertyModel mainWindowPropertyModel;
        public SettingsTabPropertyModel settingTabPropertyModel;

        public UpdateTabPropertyModel(MainWindowPropertyModel mwpm,SettingsTabPropertyModel stpm)
        {
            mainWindowPropertyModel = mwpm;
        }


        // 変更通知イベントがないとUIに反映されない
        // https://yutori-techblog.com/innerclass-private-access
        public class ModData : BindableBase, IDestructible
        {
            public SettingsTabPropertyModel settingTabPropertyModel;
            public MainWindowPropertyModel mainWindowPropertyModel;
            public DataManager dataManager;

            public ReactiveCommand<string> UninstallCommand { get; } = new ReactiveCommand<string>();

            public CompositeDisposable disposables = new CompositeDisposable();

            public ModData(SettingsTabPropertyModel stpm,MainWindowPropertyModel mwpm,DataManager dm)
            {
                settingTabPropertyModel = stpm;
                mainWindowPropertyModel = mwpm;
                dataManager = dm;

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
                    SetProperty(ref installed,new Version(value.Major,value.Minor,value.Build));
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
                string modFilePath = Path.Combine(settingTabPropertyModel.BSFolderPath,"Plugins" ,modFileName);

                if (MessageBoxResult.Yes == MessageBox.Show($"{modName}を削除します。よろしいですか？", "確認", MessageBoxButton.YesNo, MessageBoxImage.Information))
                {
                    if (File.Exists(modFilePath))
                    {
                        File.Delete(modFilePath);
                        dataManager.GetLocalModFilesInfo();
                        mainWindowPropertyModel.Console = $"Finish Deleting {modFilePath}";
                    }
                    else
                    {
                        mainWindowPropertyModel.Console = $"Fail to Delete {modFilePath}";
                    }
                }
            }
        }
        
        public void AllCheckedOrUnchecked()
        {
            Console.WriteLine(ModsData.Count);

            int i = 0;
            if (ModsData.Count(x => x.Checked == true) * 2 > ModsData.Count)
            {
                Console.WriteLine("to false");
                foreach (var _ in ModsData)
                {
                    ModsData[i].Checked = false;
                    i++;
                }
            }
            else
            {
                Console.WriteLine("to true");
                foreach (var _ in ModsData)
                {
                    ModsData[i].Checked = true;
                    i++;
                }
            }
        }

        public void ModRepositoryOpen()
        {
            foreach(var a in ModsData)
            {
                if (a.Checked)
                {
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
                        mainWindowPropertyModel.Console = $"{a.Mod}のURL : \"{a.Url}\"を開けませんでした";
                    }
                }
            }
        }
    }
}
