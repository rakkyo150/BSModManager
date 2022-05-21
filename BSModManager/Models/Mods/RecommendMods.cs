using BSModManager.Interfaces;
using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace BSModManager.Models
{
    public class RecommendMods : BindableBase, IModsData
    {
        public ObservableCollection<RecommendModData> RecommendModsData = new ObservableCollection<RecommendModData>();

        public void AllCheckedOrUnchecked()
        {
            Console.WriteLine(RecommendModsData.Count);

            int i = 0;
            if (RecommendModsData.Count(x => x.Checked == true) * 2 > RecommendModsData.Count)
            {
                Console.WriteLine("to false");
                foreach (var _ in RecommendModsData)
                {
                    RecommendModsData[i].Checked = false;
                    i++;
                }
            }
            else
            {
                Console.WriteLine("to true");
                foreach (var _ in RecommendModsData)
                {
                    RecommendModsData[i].Checked = true;
                    i++;
                }
            }
        }

        public void ModRepositoryOpen()
        {
            foreach (var a in RecommendModsData)
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
                        Console.WriteLine($"{a.Mod}のURL : \"{a.Url}\"を開けませんでした");
                    }
                }
            }
        }

        public class RecommendModData : BindableBase
        {
            private bool @checked = false;
            private string mod = "";
            private Version latest = new Version("0.0.0");
            private string updated = "?";
            private string original = "〇";
            private string mA = "×";
            private string description = "?";
            private string url = "";

            public bool Checked
            {
                get { return @checked; }
                set { SetProperty(ref @checked, value); }
            }
            public string Mod
            {
                get { return mod; }
                set { SetProperty(ref mod, value); }
            }

            public Version Latest
            {
                get { return latest; }
                set
                {
                    SetProperty(ref latest, value);
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
        }
    }
}
