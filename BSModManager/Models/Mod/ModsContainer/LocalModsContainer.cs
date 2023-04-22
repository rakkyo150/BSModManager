using BSModManager.Interfaces;
using BSModManager.Models.Mods.Structures;
using BSModManager.Static;
using Octokit;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static BSModManager.Models.ModsDataCsv;

namespace BSModManager.Models
{
    public class LocalModsContainer : BindableBase, IModsContainer
    {
        public ObservableCollection<IMod> LocalModsData = new ObservableCollection<IMod>();

        readonly GitHubApi gitHubApi;
        readonly MA mAMods;
        readonly ModsDataCsv modsDataCsv;
        readonly DateTime now = DateTime.Now;
        string updated = string.Empty;

        public LocalModsContainer(GitHubApi gha, MA mam, ModsDataCsv mdc)
        {
            gitHubApi = gha;
            mAMods = mam;
            modsDataCsv = mdc;
        }

        public void AllCheckedOrUnchecked()
        {
            int i = 0;
            if (LocalModsData.Count(x => x.Checked == true) * 2 > LocalModsData.Count)
            {
                foreach (var _ in LocalModsData)
                {
                    LocalModsData[i].Checked = false;
                    i++;
                }
                return;
            }

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
            var sorted = this.LocalModsData.OrderBy(x => x.Mod).ToList();
            this.LocalModsData.Clear();
            foreach (var item in sorted) this.LocalModsData.Add(item);
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
            foreach (var previousData in previousDataList)
            {
                if (ExistsModDataInMA(previousData))
                {
                    if (!previousData.Original) continue;

                    var temp = Array.Find(mAMods.ModAssistantAllMods, x => x.name == previousData.Mod);

                    DateTime mAUpdatedAt = DateTime.Parse(temp.updatedDate);
                    if ((now - mAUpdatedAt).Days >= 1)
                    {
                        updated = (now - mAUpdatedAt).Days + "D ago";
                    }
                    else
                    {
                        updated = (now - mAUpdatedAt).Hours + "H" + (now - mAUpdatedAt).Minutes + "m ago";
                    }

                    LocalModsData.Add(new LocalMod()
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
                    LocalModsData.Add(new LocalMod()
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
                    LocalModsData.Add(new LocalMod()
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

                LocalModsData.Add(new LocalMod()
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
    }
}
