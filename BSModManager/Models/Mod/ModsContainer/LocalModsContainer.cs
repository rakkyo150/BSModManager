using BSModManager.Interfaces;
using BSModManager.Models.Mods.Structures;
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
using System.Threading.Tasks;
using static BSModManager.Models.ModsDataCsv;

namespace BSModManager.Models
{
    public class LocalModsContainer : BindableBase, IModsContainer
    {
        public ObservableCollection<IMod> LocalModsData = new ObservableCollection<IMod>();
        public ObservableCollection<IMod> ShowedLocalModsData = new ObservableCollection<IMod>();
        private readonly GitHubApi gitHubApi;
        private readonly MA mAMods;
        private readonly ModsDataCsv modsDataCsv;
        private readonly DateTime now = DateTime.Now;
        private string updated = string.Empty;
        private string searchWords = string.Empty;
        private List<string> Keywords = new List<string>();

        public LocalModsContainer(GitHubApi gha, MA mam, ModsDataCsv mdc)
        {
            gitHubApi = gha;
            mAMods = mam;
            modsDataCsv = mdc;

            // LocalModsDataに変更があったらShowedLocalModsDataにも反映する
            LocalModsData.CollectionChanged += (sender, e) =>
            {
                UpdateShowedLocalModsData();
            };
        }

        public string SearchWords
        {
            get => searchWords;
            set
            {
                SetProperty(ref searchWords, value);
                UpdateShowedLocalModsData();
            }
        }

        public void UpdateShowedLocalModsData()
        {
            // searchWordを空白文字ごとに分割してkeywordsリストをクリアしてから追加する
            Keywords.Clear();
            Keywords.AddRange(searchWords.Split(' '));
            Keywords.RemoveAll(x => x == "");
            ShowedLocalModsData.Clear();

            foreach (IMod mod in LocalModsData)
            {              
                if (Keywords.Count() == 0)
                {
                    ShowedLocalModsData.Add(mod);
                    continue;
                }

                if(Keywords.Any(x => x.StartsWith("@")))
                {
                    List<string> colors = Keywords.Where(x => x.StartsWith("@")).ToList();

                    // @から始まる文字列を削除する
                    colors.ForEach(x => Keywords.Remove(x));
                    // @から始まる文字列をすべて含むmodをShowedLocalModsDataに追加する
                    if (colors.Any(x => mod.InstalledColor.ToString() == x.Replace("@", "")) &&
                                                                    Keywords.All(x => mod.Mod.ToLower().Contains(x.ToLower())
                    || mod.Url.ToLower().Contains(x.ToLower())
                    || mod.Description.ToLower().Contains(x.ToLower()))
                                            )
                    {
                        ShowedLocalModsData.Add(mod);
                        // @から始まる文字列を戻す
                        colors.ForEach(x => Keywords.Add(x));
                        continue;
                    }
                    else
                    {
                        // @から始まる文字列を戻す
                        colors.ForEach(x => Keywords.Add(x));
                        continue;
                    }
                }

                // Keywordsをすべて含むmodをShowedLocalModsDataに追加する
                if (Keywords.All(x => mod.Mod.ToLower().Contains(x.ToLower()) 
                    || mod.Url.ToLower().Contains(x.ToLower()) 
                    || mod.Description.ToLower().Contains(x.ToLower()))
                )
                {
                    ShowedLocalModsData.Add(mod);
                }
            }
        }

        public void AddOrRemoveColorWord2SearchWords(string color)
        {
            if (SearchWords.Contains($" @{color}"))
            {
                SearchWords = SearchWords.Replace($" @{color}", "");
            }
            else
            {
                SearchWords += $" @{color}";
            }
        }

        public void AllCheckedOrUnchecked()
        {
            int i = 0;
            if (LocalModsData.Count(x => x.Checked == true) * 2 > LocalModsData.Count)
            {
                foreach (IMod _ in LocalModsData)
                {
                    LocalModsData[i].Checked = false;
                    i++;
                }
                return;
            }

            foreach (IMod _ in LocalModsData)
            {
                LocalModsData[i].Checked = true;
                i++;
            }
        }

        public void ModRepositoryOpen()
        {
            foreach (IMod a in LocalModsData)
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
                    Logger.Instance.Error($"{ex.Message}\n{a.Mod}のURL : \"{a.Url}\"を開けませんでした");
                }
            }
        }

        public List<IMod> AllCheckedMod()
        {
            return LocalModsData.Where(x => x.Checked == true).ToList();
        }

        public void SortByName()
        {
            List<IMod> sorted = this.LocalModsData.OrderBy(x => x.Mod).ToList();
            LocalModsData.Clear();
            foreach (IMod item in sorted) this.LocalModsData.Add(item);
        }

        public async Task SortByNameAndSupplementUrl()
        {
            List<IMod> sorted = LocalModsData.OrderBy(x => x.Mod).ToList();
            LocalModsData.Clear();
            foreach (IMod sortedMod in sorted)
            {
                if (sortedMod.Url != string.Empty || sortedMod.MA != "×")
                {
                    LocalModsData.Add(sortedMod);
                    continue;
                }

                sortedMod.Url = await gitHubApi.GetTopRepository(sortedMod.Mod);

                Release response = await gitHubApi.GetLatestReleaseInfoAsync(sortedMod.Url);

                if (response == null)
                {
                    LocalModsData.Add(sortedMod);

                    continue;
                }

                if ((now - response.CreatedAt).Days >= 1)
                {
                    sortedMod.Updated = (now - response.CreatedAt).Days + "D ago";
                }
                else
                {
                    sortedMod.Updated = (now - response.CreatedAt).Hours + "H" + (now - response.CreatedAt).Minutes + "m ago";
                }

                sortedMod.Latest = VersionExtractor.DetectVersionFromRawVersion(response.TagName);
                LocalModsData.Add(sortedMod);
            }
        }

        public void Add(IMod modData)
        {
            if (ExistsSameModNameData(modData))
            {
                Logger.Instance.Debug($"{modData.Mod}と被るデータがあるためAddはキャンセルされます");
                return;
            }

            LocalModsData.Add(modData);
        }

        public void UpdateInstalled(IMod modData)
        {
            if (!ExistsSameModNameData(modData))
            {
                Add(modData);
                return;
            }
            LocalModsData.First(x => x.Mod == modData.Mod).Installed = modData.Installed;
        }

        public void UpdateLatest(IMod modData)
        {
            if (!ExistsSameModNameData(modData))
            {
                Add(modData);
                return;
            }
            LocalModsData.First(x => x.Mod == modData.Mod).Latest = modData.Latest;
        }

        public void UpdateDownloadedFileHash(IMod modData)
        {
            if (!ExistsSameModNameData(modData))
            {
                Add(modData);
                return;
            }
            LocalModsData.First(x => x.Mod == modData.Mod).DownloadedFileHash = modData.DownloadedFileHash;
        }

        public void UpdateOriginal(IMod modData)
        {
            if (!ExistsSameModNameData(modData))
            {
                Add(modData);
                return;
            }
            LocalModsData.First(x => x.Mod == modData.Mod).Original = modData.Original;
        }

        public void UpdateUpdated(IMod modData)
        {
            if (!ExistsSameModNameData(modData))
            {
                Add(modData);
                return;
            }
            LocalModsData.First(x => x.Mod == modData.Mod).Updated = modData.Updated;
        }

        public void UpdateMA(IMod modData)
        {
            if (!ExistsSameModNameData(modData))
            {
                Add(modData);
                return;
            }
            LocalModsData.First(x => x.Mod == modData.Mod).MA = modData.MA;
        }

        public void UpdateDescription(IMod modData)
        {
            if (!ExistsSameModNameData(modData))
            {
                Add(modData);
                return;
            }
            LocalModsData.First(x => x.Mod == modData.Mod).Description = modData.Description;
        }

        public void UpdateURL(IMod modData)
        {
            if (!ExistsSameModNameData(modData))
            {
                Add(modData);
                return;
            }
            LocalModsData.First(x => x.Mod == modData.Mod).Url = modData.Url;
        }

        public void Remove(IMod modData)
        {
            if (!ExistsSameModNameData(modData))
            {
                Logger.Instance.Debug($"{modData.Mod}は{LocalModsData}に存在しないので削除できません");
                return;
            }

            LocalModsData.Remove(LocalModsData.First(x => x.Mod == modData.Mod));
        }

        internal async Task InitializeFromCsvData()
        {
            string dataDirectory = Path.Combine(Folder.Instance.dataFolder, VersionExtractor.GameVersion);
            string modsDataCsvPath = Path.Combine(dataDirectory, "ModsData.csv");
            List<ModsDataCsvIndex> previousDataList;

            if (!File.Exists(modsDataCsvPath)) return;

            previousDataList = modsDataCsv.Read(modsDataCsvPath);
            foreach (ModsDataCsvIndex previousData in previousDataList)
            {
                if (ExistsModDataInMA(previousData))
                {
                    if (!previousData.Original) continue;

                    MA.MAMod temp = Array.Find(mAMods.ModAssistantAllMods, x => x.name == previousData.Mod);

                    DateTime mAUpdatedAt = DateTime.Parse(temp.updatedDate);
                    if ((now - mAUpdatedAt).Days >= 1)
                    {
                        updated = (now - mAUpdatedAt).Days + "D ago";
                    }
                    else
                    {
                        updated = (now - mAUpdatedAt).Hours + "H" + (now - mAUpdatedAt).Minutes + "m ago";
                    }

                    LocalModsData.Add(new LocalMod(this)
                    {
                        Mod = previousData.Mod,
                        Latest = new Version(temp.version),
                        Updated = updated,
                        Original = "〇",
                        MA = "〇",
                        Description = temp.description,
                        Url = temp.link
                    });

                    continue;
                }

                Release response = await gitHubApi.GetLatestReleaseInfoAsync(previousData.Url);
                string original = null;

                original = previousData.Original ? "〇" : "×";


                if (response == null)
                {
                    LocalModsData.Add(new LocalMod(this)
                    {
                        Mod = previousData.Mod,
                        Latest = new Version("0.0.0"),
                        Updated = previousData.Url == string.Empty ? "?" : "---",
                        Original = original,
                        MA = "×",
                        Description = previousData.Url == string.Empty ? "?" : "---",
                        Url = previousData.Url
                    });

                    continue;
                }

                if ((now - response.CreatedAt).Days >= 1)
                {
                    updated = (now - response.CreatedAt).Days + "D ago";
                }
                else
                {
                    updated = (now - response.CreatedAt).Hours + "H" + (now - response.CreatedAt).Minutes + "m ago";
                }

                if (previousData.DownloadedFileHash != string.Empty)
                {
                    LocalModsData.Add(new LocalMod(this)
                    {
                        Mod = previousData.Mod,
                        Installed = new Version(previousData.LatestVersion),
                        Latest = VersionExtractor.DetectVersionFromRawVersion(response.TagName),
                        DownloadedFileHash = previousData.DownloadedFileHash,
                        Updated = updated,
                        Original = original,
                        MA = "×",
                        Description = response.Body,
                        Url = previousData.Url
                    });

                    continue;
                }

                LocalModsData.Add(new LocalMod(this)
                {
                    Mod = previousData.Mod,
                    Latest = VersionExtractor.DetectVersionFromRawVersion(response.TagName),
                    Updated = updated,
                    Original = original,
                    MA = "×",
                    Description = response.Body,
                    Url = previousData.Url
                });
            }
        }

        private bool ExistsModDataInMA(ModsDataCsvIndex previousData)
        {
            if (previousData.Original != true) return false;

            return Array.Exists(mAMods.ModAssistantAllMods, x => x.name == previousData.Mod);
        }

        public bool ExistsSameModNameData(IMod modData)
        {
            return LocalModsData.Any(x => x.Mod == modData.Mod);
        }

        public bool ShouldChangeInstalledVersionToFileItselfVersion(IMod modData)
        {
            if (!LocalModsData.Any(x => x.Mod == modData.Mod)) return false;

            // 初期化が必要なので
            if (LocalModsData.First(x => x.Mod == modData.Mod).DownloadedFileHash == string.Empty) return true;

            return !LocalModsData.Any(x => x.DownloadedFileHash == modData.DownloadedFileHash);
        }

        public IEnumerable<IMod> ReturnCheckedModsData()
        {
            return LocalModsData.Where(x => x.Checked == true);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing) { }

            LocalModsData.CollectionChanged -= (sender, e) =>
            { 
                UpdateShowedLocalModsData(); 
            };
        }

        ~LocalModsContainer()
        {
            Dispose(false);
        }
    }
}
