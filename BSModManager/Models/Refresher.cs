using BSModManager.Interfaces;
using BSModManager.Models.Mods.Structures;
using BSModManager.Static;
using Octokit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BSModManager.Models.ModCsvHandler;

namespace BSModManager.Models
{
    public class Refresher
    {
        readonly MAMods mAMods;
        readonly ModCsvHandler modCsvHandler;
        readonly LocalMods localMods;
        readonly PastMods pastMods;
        readonly GitHubApi gitHubApi;

        public Refresher(MAMods mam, LocalMods lm, ModCsvHandler mch, PastMods pm, GitHubApi gha)
        {
            mAMods = mam;
            localMods = lm;
            modCsvHandler = mch;
            pastMods = pm;
            gitHubApi = gha;
        }

        public async Task Refresh()
        {
            LocalModsDataRefresh();
            await PastModsDataRefresh();
        }

        private async Task PastModsDataRefresh()
        {
            List<ModCsvIndex> previousDataListAddedToPastModsData = new List<ModCsvIndex>();

            // 現在のバージョンも含む
            string[] AllPastVersion = Directory.GetDirectories(Folder.Instance.dataFolder, "*", SearchOption.TopDirectoryOnly);

            if (AllPastVersion.Count() == 0) return;

            await GetPreciousDataList(previousDataListAddedToPastModsData, AllPastVersion);

            List<IModData> removeList = new List<IModData>();

            foreach (var localMod in localMods.LocalModsData)
            {
                if (!previousDataListAddedToPastModsData.Any(x => x.Mod == localMod.Mod)) continue;
                if (!previousDataListAddedToPastModsData.Find(x => x.Mod == localMod.Mod).Original == (localMod.Original == "〇")) continue;

                previousDataListAddedToPastModsData.Remove(previousDataListAddedToPastModsData.Find(x => x.Mod == localMod.Mod));

                // MAや手動でMod追加したときの更新のため
                removeList.Add(localMod);
            }

            if (NoPreviousData(previousDataListAddedToPastModsData)) return;
            await AddPastModsData(previousDataListAddedToPastModsData);

            if (removeList.Count == 0) return;

            foreach (var removeModData in removeList)
            {
                pastMods.Remove(removeModData);
            }

            pastMods.SortByName();
        }   

        private async Task GetPreciousDataList(List<ModCsvIndex> previousDataList, string[] AllPastVersion)
        {
            foreach (string pastVersion in AllPastVersion)
            {
                string dataDirectory = Path.Combine(Folder.Instance.dataFolder, pastVersion);
                string modsDataCsvPath = Path.Combine(dataDirectory, "ModsData.csv");

                if (!File.Exists(modsDataCsvPath)) continue;

                List<ModCsvIndex> tempDataList = new List<ModCsvIndex>();
                tempDataList = await modCsvHandler.Read(modsDataCsvPath);

                var exceptDataList = tempDataList.Except(previousDataList);

                foreach (ModCsvIndex a in exceptDataList)
                {
                    bool existsSameModNameAtPrevioudsDataList = previousDataList.Any(x => x.Mod == a.Mod);
                    bool sameMA = previousDataList.Any(x => x.Ma == a.Ma);
                    bool nowMA = true;

                    if (existsSameModNameAtPrevioudsDataList) nowMA = previousDataList.Find(x => x.Mod == a.Mod).Ma;

                    if (existsSameModNameAtPrevioudsDataList && (sameMA || nowMA == false)) continue;

                    if (existsSameModNameAtPrevioudsDataList && nowMA == true && !sameMA)
                    {
                        previousDataList.Find(x => x.Mod == a.Mod).Ma = a.Ma;
                        previousDataList.Find(x => x.Mod == a.Mod).Url = a.Url;
                        continue;
                    }

                    previousDataList.Add(a);
                }
            }

            foreach (var modAssistantMod in mAMods.ModAssistantAllMods)
            {
                if (!previousDataList.Any(x => x.Mod == modAssistantMod.name)) continue;
                if (!previousDataList.Find(x => x.Mod == modAssistantMod.name).Original) continue;

                UpdatePreviousDataToExistsInMAVersion(previousDataList, modAssistantMod);
            }
        }

        private async Task AddPastModsData(List<ModCsvIndex> previousDataList)
        {
            foreach (var previousData in previousDataList)
            {
                if (previousData.Ma)
                {
                    DateTime now = DateTime.Now;
                    DateTime mAUpdatedAt = mAMods.ExistsData(new MAModData() { name=previousData.Mod}) ?
                        DateTime.Parse(mAMods.ModAssistantAllMods.First(x => x.name == previousData.Mod).updatedDate) : DateTime.MaxValue;
                    string updated = "?";

                    string description = mAMods.ExistsData(new MAModData() { name = previousData.Mod }) ?
                       mAMods.ModAssistantAllMods.First(x => x.name == previousData.Mod).description : "?";

                    if (mAUpdatedAt != DateTime.MaxValue)
                    {
                        updated = (now - mAUpdatedAt).Days >= 1 ?
                        (now - mAUpdatedAt).Days + "D ago" : (now - mAUpdatedAt).Hours + "H" + (now - mAUpdatedAt).Minutes + "m ago";
                    }

                    pastMods.Add(new PastModData(this)
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
                response = await gitHubApi.GetLatestReleaseAsync(previousData.Url);
                string original = previousData.Original ? "〇" : "×";

                if (response == null)
                {
                    pastMods.Add(new PastModData(this)
                    {
                        Mod = previousData.Mod,
                        Latest = new Version("0.0.0"),
                        Updated = previousData.Url == "" ? "?" : "---",
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

                    pastMods.Add(new PastModData(this)
                    {
                        Mod = previousData.Mod,
                        Latest = gitHubApi.DetectVersionFromTagName(response.TagName),
                        Updated = updated,
                        Original = original,
                        MA = "×",
                        Description = response.Body,
                        Url = previousData.Url
                    });
                }
            }
        }

        private void LocalModsDataRefresh()
        {
            Dictionary<string, Version> localModNameAndVersionDic = GetLocalModsNameAndVersion();
            
            AddModsData(localModNameAndVersionDic);

            RemoveNotExistingModsData(localModNameAndVersionDic);

            localMods.SortByName();
        }

        private void AddModsData(Dictionary<string, Version> localModNameAndVersionDic)
        {
            foreach (KeyValuePair<string, Version> localModNameAndVersion in localModNameAndVersionDic)
            {
                if (localMods.ExistsSameModData(new LocalModData(this){Mod = localModNameAndVersion.Key}))
                {
                    localMods.UpdateInstalled(new LocalModData(this)
                    {
                        Mod = localModNameAndVersion.Key,
                        Installed = localModNameAndVersion.Value
                    });

                    continue;
                }

                if (!mAMods.ExistsData(new MAModData(){name=localModNameAndVersion.Key }))
                {
                    localMods.Add(new LocalModData(this)
                    {
                        Mod = localModNameAndVersion.Key,
                        Installed = localModNameAndVersion.Value
                    });

                    continue;
                }

                var temp = Array.Find(mAMods.ModAssistantAllMods, x => x.name == localModNameAndVersion.Key);

                DateTime now = DateTime.Now;
                DateTime mAUpdatedAt = DateTime.Parse(temp.updatedDate);
                string updated = null;
                if ((now - mAUpdatedAt).Days >= 1)
                {
                    updated = (now - mAUpdatedAt).Days + "D ago";
                }
                else
                {
                    updated = (now - mAUpdatedAt).Hours + "H" + (now - mAUpdatedAt).Minutes + "m ago";
                }

                localMods.Add(new LocalModData(this)
                {
                    Mod = localModNameAndVersion.Key,
                    Installed = localModNameAndVersion.Value,
                    Latest = new Version(temp.version),
                    Updated = updated,
                    Original = "〇",
                    MA = "〇",
                    Description = temp.description,
                    Url = temp.link
                });
            }
        }

        private void RemoveNotExistingModsData(Dictionary<string, Version> localModNameAndVersionDic)
        {
            if (NoLocalModsData()) return;

            List<IModData> removeList = new List<IModData>();
            foreach (var data in localMods.LocalModsData)
            {
                if (HasRemovedFromLocal(localModNameAndVersionDic, data))
                {
                    removeList.Add(data);
                }
            }

            if (removeList.Count == 0) return;

            foreach (var removeData in removeList)
            {
                localMods.Remove(removeData);
            }
        }

        private static Dictionary<string, Version> GetLocalModsNameAndVersion()
        {
            string pluginFolderPath = Path.Combine(Folder.Instance.BSFolderPath, "Plugins");
            string pendingPluginFolderPath = Path.Combine(Folder.Instance.BSFolderPath, "IPA", "Pending", "Plugins");

            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(pluginFolderPath);
            System.IO.DirectoryInfo pendingDi = new System.IO.DirectoryInfo(pendingPluginFolderPath);

            IEnumerable<System.IO.FileInfo> filesName = null;
            IEnumerable<System.IO.FileInfo> pendingFilesName = null;

            if (Directory.Exists(pluginFolderPath))
            {
                filesName = di.EnumerateFiles("*.dll", System.IO.SearchOption.TopDirectoryOnly);
            }
            if (Directory.Exists(pendingDi.FullName))
            {
                pendingFilesName = pendingDi.EnumerateFiles("*.dll", System.IO.SearchOption.TopDirectoryOnly);
            }


            Dictionary<string, Version> localModNameAndVersionDic = new Dictionary<string, Version>();

            if (filesName != null)
            {
                foreach (System.IO.FileInfo f in filesName)
                {
                    string pluginPath = Path.Combine(pluginFolderPath, f.Name);

                    System.Diagnostics.FileVersionInfo vi = System.Diagnostics.FileVersionInfo.GetVersionInfo(pluginPath);
                    Version installedModVersion = new Version(vi.FileVersion);

                    localModNameAndVersionDic.Add(f.Name.Replace(".dll", ""), installedModVersion);
                }
            }

            if (pendingFilesName != null)
            {
                foreach (System.IO.FileInfo pendingF in pendingFilesName)
                {
                    string pendingPluginPath = Path.Combine(pendingPluginFolderPath, pendingF.Name);

                    System.Diagnostics.FileVersionInfo pendingVi = System.Diagnostics.FileVersionInfo.GetVersionInfo(pendingPluginPath);
                    Version pendingInstalledModVersion = new Version(pendingVi.FileVersion);

                    if (!localModNameAndVersionDic.ContainsKey(pendingF.Name.Replace(".dll", "")))
                    {
                        localModNameAndVersionDic.Add(pendingF.Name.Replace(".dll", ""), pendingInstalledModVersion);
                        continue;
                    }

                    if (pendingInstalledModVersion > localModNameAndVersionDic[pendingF.Name.Replace(".dll", "")])
                    {
                        localModNameAndVersionDic[pendingF.Name.Replace(".dll", "")] = pendingInstalledModVersion;
                    }
                }
            }

            return localModNameAndVersionDic;
        }

        private bool NoLocalModsData()
        {
            return localMods.LocalModsData.Count == 0;
        }

        private bool NoPreviousData(List<ModCsvIndex> previousDataListAddedToPastModsData)
        {
            return previousDataListAddedToPastModsData.Count == 0;
        }


        private bool HasRemovedFromLocal(Dictionary<string, Version> localModNameAndVersionDic, IModData data)
        {
            return !localModNameAndVersionDic.Keys.Any(x => x == data.Mod);
        }

        private static void UpdatePreviousDataToExistsInMAVersion(List<ModCsvIndex> previousDataList, MAModData modAssistantMod)
        {
            previousDataList.Find(x => x.Mod == modAssistantMod.name).Ma = true;
            previousDataList.Find(x => x.Mod == modAssistantMod.name).LatestVersion = modAssistantMod.version;
            previousDataList.Find(x => x.Mod == modAssistantMod.name).Url = modAssistantMod.link;
        }
    }
}
