using BSModManager.Interfaces;
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
        
        public Refresher(MAMods mam,LocalMods lm,ModCsvHandler mch,PastMods pm,GitHubApi gha)
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
            List<ModCsvIndex> previousDataList = new List<ModCsvIndex>();

            List<IModData> removeList = new List<IModData>();

            // 現在のバージョンも含む
            string[] AllPastVersion = Directory.GetDirectories(Folder.Instance.dataFolder, "*", SearchOption.TopDirectoryOnly);

            if (AllPastVersion.Count() == 0) return;
            
            await GetPreciousDataList(previousDataList, AllPastVersion);

            foreach (var localMod in localMods.LocalModsData)
            {
                if (!previousDataList.Any(x => x.Mod == localMod.Mod)) continue;
                if (!previousDataList.Find(x => x.Mod == localMod.Mod).Original == (localMod.Original == "〇")) continue;

                previousDataList.Remove(previousDataList.Find(x => x.Mod == localMod.Mod));

                // MAや手動でMod追加したときの更新のため
                removeList.Add(localMod);
            }

            if (NoPreviousData(previousDataList)) return;
            await AddPastModsData(previousDataList);

            if (removeList.Count == 0) return;

            foreach (var removeModData in removeList)
            {
                pastMods.Remove(removeModData);
            }
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

                UpdatePreviousDataToExistInMAVersion(previousDataList, modAssistantMod);
            }
        }

        private async Task AddPastModsData(List<ModCsvIndex> previousDataList)
        {
            foreach (var previousData in previousDataList)
            {
                if (previousData.Ma)
                {
                    bool existsInNowMa = Array.Exists(mAMods.ModAssistantAllMods, x => x.name == previousData.Mod);

                    DateTime now = DateTime.Now;
                    DateTime mAUpdatedAt = existsInNowMa ?
                        DateTime.Parse(mAMods.ModAssistantAllMods.First(x => x.name == previousData.Mod).updatedDate) : DateTime.MaxValue;
                    string updated = "?";

                    string description = existsInNowMa ?
                       mAMods.ModAssistantAllMods.First(x => x.name == previousData.Mod).description : "?";

                    if (mAUpdatedAt != DateTime.MaxValue)
                    {
                        updated = (now - mAUpdatedAt).Days >= 1 ?
                        (now - mAUpdatedAt).Days + "D ago" : (now - mAUpdatedAt).Hours + "H" + (now - mAUpdatedAt).Minutes + "m ago";
                    }

                    pastMods.Add(new PastMods.PastModData(this)
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
                    pastMods.Add(new PastMods.PastModData(this)
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

                    pastMods.Add(new PastMods.PastModData(this)
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
        }

        private void AddModsData(Dictionary<string, Version> localModNameAndVersionDic)
        {
            foreach (KeyValuePair<string, Version> localModNameAndVersion in localModNameAndVersionDic)
            {
                if (ExistsDataInLocalModsData(localModNameAndVersion))
                {
                    localMods.Update(new LocalMods.LocalModData(this)
                    {
                        Mod = localModNameAndVersion.Key,
                        Installed = localModNameAndVersion.Value
                    });
                }

                if (!ExistsDataInMAMod(localModNameAndVersion))
                {
                    localMods.Add(new LocalMods.LocalModData(this)
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

                localMods.Add(new LocalMods.LocalModData(this)
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
                if (RemovedFromLocal(localModNameAndVersionDic, data))
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


        private static bool RemovedFromLocal(Dictionary<string, Version> localModNameAndVersionDic, IModData data)
        {
            return !localModNameAndVersionDic.Keys.Any(x => x.Replace(".dll", "") == data.Mod);
        }

        private bool ExistsDataInMAMod(KeyValuePair<string, Version> modNameAndVersion)
        {
            return Array.Exists(mAMods.ModAssistantAllMods, x => x.name == modNameAndVersion.Key);
        }

        private bool ExistsDataInLocalModsData(KeyValuePair<string, Version> modNameAndVersion)
        {
            return localMods.LocalModsData.Any(x => x.Mod == modNameAndVersion.Key);
        }
        private static bool NoPreviousData(List<ModCsvIndex> previousDataList)
        {
            return previousDataList.Count == 0;
        }

        private static void UpdatePreviousDataToExistInMAVersion(List<ModCsvIndex> previousDataList, MAMods.MAModStructure modAssistantMod)
        {
            previousDataList.Find(x => x.Mod == modAssistantMod.name).Ma = true;
            previousDataList.Find(x => x.Mod == modAssistantMod.name).LatestVersion = modAssistantMod.version;
            previousDataList.Find(x => x.Mod == modAssistantMod.name).Url = modAssistantMod.link;
        }
    }
}
