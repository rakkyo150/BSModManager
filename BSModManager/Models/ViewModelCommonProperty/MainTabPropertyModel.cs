using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Media;

namespace BSModManager.Models
{
    public class MainTabPropertyModel : BindableBase
    {
        public ObservableCollection<ModData> ModsData = new ObservableCollection<ModData>();

        public MainWindowPropertyModel mainWindowPropertyModel;

        public MainTabPropertyModel(MainWindowPropertyModel mwpm)
        {
            mainWindowPropertyModel = mwpm;
        }


        // 変更通知イベントがないとUIに反映されない
        public class ModData : BindableBase
        {
            private bool c = false;
            private string mod = "";
            private Version installed = new Version("0.0.0");
            private Version latest = new Version("0.0.0");
            private string updated = "?";
            private string original = "?";
            private string mA = "?";
            private string description = "?";
            private Brush installedColor = Brushes.Green;
            private Brush latestColor = Brushes.Red;
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
                set { SetProperty(ref installed, value); }
            }
            public Version Latest
            {
                get { return latest; }
                set { SetProperty(ref latest, value); }
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
            public Brush LatestColor
            {
                get { return latestColor; }
                set { SetProperty(ref latestColor, value); }
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
