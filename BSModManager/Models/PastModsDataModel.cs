using BSModManager.Interfaces;
using BSModManager.Models.CoreManager;
using BSModManager.Models.Structure;
using BSModManager.Models.ViewModelCommonProperty;
using BSModManager.Static;
using Octokit;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BSModManager.Models
{
    public class PastModsDataModel : BindableBase, IModsData
    {
        public ObservableCollection<PastModData> PastModsData = new ObservableCollection<PastModData>();

        LocalModsDataModel currentModsDataModel;
        InnerData innerData;
        DataManager dataManager;
        GitHubManager gitHubManager;

        public PastModsDataModel(InnerData id, DataManager dm, GitHubManager ghm, LocalModsDataModel mdm)
        {
            innerData = id;
            dataManager = dm;
            gitHubManager = ghm;
            currentModsDataModel = mdm;
        }

        public async Task Initialize()
        {
            List<ModInformationCsv> previousDataList = new List<ModInformationCsv>();

            // 現在のバージョンも含む
            string[] AllPastVersion = Directory.GetDirectories(FolderManager.dataFolder, "*", SearchOption.TopDirectoryOnly);

            foreach (string pastVersion in AllPastVersion)
            {
                string dataDirectory = Path.Combine(FolderManager.dataFolder, pastVersion);
                string modsDataCsvPath = Path.Combine(dataDirectory, "ModsData.csv");

                if (!File.Exists(modsDataCsvPath)) continue;

                List<ModInformationCsv> tempDataList = new List<ModInformationCsv>();
                tempDataList = await dataManager.ReadCsv(modsDataCsvPath);

                var exceptDataList = tempDataList.Except(previousDataList);

                foreach (ModInformationCsv a in exceptDataList)
                {
                    bool existsModName = previousDataList.Any(x => x.Mod == a.Mod);
                    bool sameMA = previousDataList.Any(x => x.Ma == a.Ma);
                    bool nowMA = true;
                    if (existsModName) nowMA = previousDataList.Find(x => x.Mod == a.Mod).Ma;

                    if (existsModName && (sameMA || nowMA == false)) continue;
                    if (existsModName && nowMA == true && !sameMA)
                    {
                        previousDataList.Find(x => x.Mod == a.Mod).Ma = a.Ma;
                        previousDataList.Find(x => x.Mod == a.Mod).Url = a.Url;
                        continue;
                    }

                    previousDataList.Add(a);
                }
            }

            foreach (var modAssistantMod in innerData.modAssistantAllMods)
            {
                if (!previousDataList.Any(x => x.Mod == modAssistantMod.name)) continue;
                if (!previousDataList.Find(x => x.Mod == modAssistantMod.name).Original) continue;

                previousDataList.Find(x => x.Mod == modAssistantMod.name).Ma = true;
                previousDataList.Find(x => x.Mod == modAssistantMod.name).LatestVersion = modAssistantMod.version;
                previousDataList.Find(x => x.Mod == modAssistantMod.name).Url = modAssistantMod.link;
            }

            foreach (var localMod in currentModsDataModel.LocalModsData)
            {
                if (!previousDataList.Any(x => x.Mod == localMod.Mod)) continue;
                if (!previousDataList.Any(x => x.Url == localMod.Url)) continue;

                previousDataList.Remove(previousDataList.Find(x => x.Mod == localMod.Mod));
            }

            if (previousDataList.Count == 0) return;
            
            foreach (var previousData in previousDataList)
            {
                if (previousData.Ma)
                {
                    bool existsInNowMa = Array.Exists(innerData.modAssistantAllMods, x => x.name == previousData.Mod);

                    DateTime now = DateTime.Now;
                    DateTime mAUpdatedAt = existsInNowMa ? 
                        DateTime.Parse(innerData.modAssistantAllMods.First(x=>x.name==previousData.Mod).updatedDate) : DateTime.MaxValue;
                    string updated = "?";
                    
                    string description = existsInNowMa ?
                       innerData.modAssistantAllMods.First(x => x.name == previousData.Mod).description : "?";

                    if (mAUpdatedAt != DateTime.MaxValue)
                    {
                        updated = (now - mAUpdatedAt).Days >= 1 ?
                        (now - mAUpdatedAt).Days + "D ago" : (now - mAUpdatedAt).Hours + "H" + (now - mAUpdatedAt).Minutes + "m ago";
                    }

                    PastModsData.Add(new PastModsDataModel.PastModData()
                    {
                        Mod = previousData.Mod,
                        Latest = new Version(previousData.LatestVersion),
                        Updated = updated,
                        Original = "〇",
                        MA = "〇",
                        Description = description,
                        Url = previousData.Url
                    });

                    continue;
                }

                Release response = null;
                response = await gitHubManager.GetGitHubModLatestVersionAsync(previousData.Url);
                string original = previousData.Original ? "〇" : "×";

                if (response == null)
                {
                    PastModsData.Add(new PastModsDataModel.PastModData()
                    {
                        Mod = previousData.Mod,
                        Latest = new Version("0.0.0"),
                        Updated = previousData.Url=="" ? "?" : "---",
                        Original = original,
                        MA = "×",
                        Description = previousData.Url == "" ? "?" : "---",
                        Url = previousData.Url
                    });
                }
                else
                {
                    DateTime now = DateTime.Now;
                    string updated = null;
                    if ((now - response.CreatedAt).Days >= 1)
                    {
                        updated = (now - response.CreatedAt).Days + "D ago";
                    }
                    else
                    {
                        updated = (now - response.CreatedAt).Hours + "H" + (now - response.CreatedAt).Minutes + "m ago";
                    }

                    PastModsData.Add(new PastModsDataModel.PastModData()
                    {
                        Mod = previousData.Mod,
                        Latest = gitHubManager.DetectVersion(response.TagName),
                        Updated = updated,
                        Original = original,
                        MA = "×",
                        Description = response.Body,
                        Url = previousData.Url
                    });
                }
            }
        }

        public void AllCheckedOrUnchecked()
        {
            Console.WriteLine(PastModsData.Count);

            int i = 0;
            if (PastModsData.Count(x => x.Checked == true) * 2 > PastModsData.Count)
            {
                Console.WriteLine("to false");
                foreach (var _ in PastModsData)
                {
                    PastModsData[i].Checked = false;
                    i++;
                }
            }
            else
            {
                Console.WriteLine("to true");
                foreach (var _ in PastModsData)
                {
                    PastModsData[i].Checked = true;
                    i++;
                }
            }
        }

        public void ModRepositoryOpen()
        {
            foreach (var a in PastModsData)
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

        public class PastModData : BindableBase
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
