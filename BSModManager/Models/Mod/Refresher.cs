﻿using BSModManager.Interfaces;
using BSModManager.Models.Mods.Structures;
using BSModManager.Static;
using Newtonsoft.Json;
using Octokit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static BSModManager.Models.MA;
using static BSModManager.Models.ModsDataCsv;

namespace BSModManager.Models
{
    public class Refresher
    {
        readonly MA mAMods;
        readonly GitHubApi gitHubApi;
        readonly ModsContainerAgent modsDataContainerAgent;
        readonly ModsDataCsv modsDataCsv;
        readonly List<IMod> removedPastModsData = new List<IMod>();
        readonly List<IMod> removedRecommendModsData = new List<IMod>();


        public Refresher(MA mam, ModsDataCsv mch, GitHubApi gha, ModsContainerAgent mdca)
        {
            mAMods = mam;
            modsDataCsv = mch;
            gitHubApi = gha;
            modsDataContainerAgent = mdca;
        }

        public async Task Refresh()
        {
            LocalModsDataRefresh();
            await PastModsDataRefresh();
            await RecommendModDataRefreash();

            await AssistLocalModDataByRemovedPastOrRecommendMods();
        }

        private async Task AssistLocalModDataByRemovedPastOrRecommendMods()
        {
            foreach (var localMod in modsDataContainerAgent.LocalModsContainer.LocalModsData)
            {
                if (localMod.Url != string.Empty) continue;

                if (!removedPastModsData.Any(x => x.Mod == localMod.Mod) && !removedRecommendModsData.Any(x => x.Mod == localMod.Mod))
                    continue;

                if (removedPastModsData.Any(x => x.Mod == localMod.Mod) && !removedRecommendModsData.Any(x => x.Mod == localMod.Mod))
                {
                    await SetRemovedPastModsDataToLocalMods(localMod);
                    continue;
                }
                else if (!removedPastModsData.Any(x => x.Mod == localMod.Mod) && removedRecommendModsData.Any(x => x.Mod == localMod.Mod))
                {
                    await SetRemovedRecommendModsDataToLocalMods(localMod);
                    continue;
                }

                if (removedPastModsData.First(x => x.Mod == localMod.Mod).Url == string.Empty)
                {
                    await SetRemovedRecommendModsDataToLocalMods(localMod);
                    continue;
                }

                await SetRemovedPastModsDataToLocalMods(localMod);
            }
        }

        private async Task SetRemovedRecommendModsDataToLocalMods(IMod localMod)
        {
            IMod modData = removedRecommendModsData.First(x => x.Mod == localMod.Mod);

            Release response = await gitHubApi.GetLatestReleaseInfoAsync(modData.Url);

            modsDataContainerAgent.LocalModsContainer.UpdateURL(modData);
            modsDataContainerAgent.LocalModsContainer.UpdateOriginal(modData);

            if (response == null) return;

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

            IMod assignedModData = new LocalMod()
            {
                Mod = localMod.Mod,
                Latest = VersionExtractor.DetectVersionFromRawVersion(response.TagName),
                Updated = updated,
                Description = response.Body
            };


            modsDataContainerAgent.LocalModsContainer.UpdateLatest(assignedModData);
            modsDataContainerAgent.LocalModsContainer.UpdateUpdated(assignedModData);
            modsDataContainerAgent.LocalModsContainer.UpdateDescription(assignedModData);
        }

        private async Task SetRemovedPastModsDataToLocalMods(IMod localMod)
        {
            IMod modData = removedPastModsData.First(x => x.Mod == localMod.Mod);

            Release response = await gitHubApi.GetLatestReleaseInfoAsync(modData.Url);

            modsDataContainerAgent.LocalModsContainer.UpdateURL(modData);
            modsDataContainerAgent.LocalModsContainer.UpdateOriginal(modData);

            if (response == null) return;
            
            // リリース情報に不備がある場合
            if (response.TagName == null) return;

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

            IMod assignedModData = new LocalMod()
            {
                Mod = localMod.Mod,
                Latest = VersionExtractor.DetectVersionFromRawVersion(response.TagName),
                Updated = updated,
                Description = response.Body
            };


            modsDataContainerAgent.LocalModsContainer.UpdateLatest(assignedModData);
            modsDataContainerAgent.LocalModsContainer.UpdateUpdated(assignedModData);
            modsDataContainerAgent.LocalModsContainer.UpdateDescription(assignedModData);
        }

        private async Task RecommendModDataRefreash()
        {
            List<ModsDataCsvIndex> notInstalledRecommendList = new List<ModsDataCsvIndex>();

            System.Reflection.Assembly myAssembly = System.Reflection.Assembly.GetExecutingAssembly();
            System.IO.StreamReader sr =
                new System.IO.StreamReader(
                myAssembly.GetManifestResourceStream("BSModManager.Models.Mod.ModsContainer.RecommendMods.json"),
                    System.Text.Encoding.GetEncoding("shift-jis"));
            string s = sr.ReadToEnd();
            sr.Close();

            List<RecommendMod> rawRecommendModData = JsonConvert.DeserializeObject<List<RecommendMod>>(s);
            foreach (var a in rawRecommendModData)
            {
                notInstalledRecommendList.Add(new ModsDataCsvIndex()
                {
                    Mod = a.Mod,
                    LocalVersion = a.Installed.ToString(),
                    LatestVersion = a.Latest.ToString(),
                    Original = a.Original == "true",
                    Ma = a.MA == "〇",
                    Url = a.Url
                });
            }

            foreach (var modAssistantMod in mAMods.ModAssistantAllMods)
            {
                if (!notInstalledRecommendList.Any(x => x.Mod == modAssistantMod.name)) continue;
                if (!notInstalledRecommendList.Find(x => x.Mod == modAssistantMod.name).Original) continue;

                UpdateDataToExistsInMAVersion(notInstalledRecommendList, modAssistantMod);
            }

            foreach (var localMod in modsDataContainerAgent.LocalModsContainer.LocalModsData)
            {
                if (!notInstalledRecommendList.Any(x => x.Mod == localMod.Mod)) continue;
                if (!(notInstalledRecommendList.First(x => x.Mod == localMod.Mod).Original == (localMod.Original == "〇"))) continue;

                removedRecommendModsData.Add(new RecommendMod(this)
                {
                    Mod = localMod.Mod,
                    Original = notInstalledRecommendList.First(x => x.Mod == localMod.Mod).Original ? "〇" : "×",
                    Url = notInstalledRecommendList.First(x => x.Mod == localMod.Mod).Url
                });

                notInstalledRecommendList.Remove(notInstalledRecommendList.First(x => x.Mod == localMod.Mod));

                if (!modsDataContainerAgent.RecommendModsContainer.ExistsSameModNameData(localMod)) continue;

                modsDataContainerAgent.RecommendModsContainer.Remove(localMod);
            }

            await AddRemoteDataToModsData(modsDataContainerAgent.RecommendModsContainer, notInstalledRecommendList);

            foreach (var a in rawRecommendModData)
            {
                if (modsDataContainerAgent.RecommendModsContainer.ExistsSameModNameData(a))
                {
                    modsDataContainerAgent.RecommendModsContainer.UpdateDescription(new RecommendMod(this)
                    {
                        Mod = a.Mod,
                        Description = a.Description
                    });
                }
            }

            modsDataContainerAgent.RecommendModsContainer.SortByName();
        }

        private async Task PastModsDataRefresh()
        {
            List<ModsDataCsvIndex> previousDataListAddedToPastModsDataCue = new List<ModsDataCsvIndex>();

            // 現在のバージョンも含む
            string[] AllPastVersion = Directory.GetDirectories(Folder.Instance.dataFolder, "*", SearchOption.TopDirectoryOnly);

            if (AllPastVersion.Count() == 0) return;

            await GetPreviousDataList(previousDataListAddedToPastModsDataCue, AllPastVersion);

            foreach (var localMod in modsDataContainerAgent.LocalModsContainer.LocalModsData)
            {
                if (!previousDataListAddedToPastModsDataCue.Any(x => x.Mod == localMod.Mod)) continue;
                if (!previousDataListAddedToPastModsDataCue.Find(x => x.Mod == localMod.Mod).Original == (localMod.Original == "〇")) continue;

                removedPastModsData.Add(new RecommendMod(this)
                {
                    Mod = localMod.Mod,
                    Original = previousDataListAddedToPastModsDataCue.First(x => x.Mod == localMod.Mod).Original ? "〇" : "×",
                    Url = previousDataListAddedToPastModsDataCue.First(x => x.Mod == localMod.Mod).Url
                });

                previousDataListAddedToPastModsDataCue.Remove(previousDataListAddedToPastModsDataCue.Find(x => x.Mod == localMod.Mod));

                if (!modsDataContainerAgent.PastModsContainer.ExistsSameModNameData(localMod)) continue;

                // MAや手動でMod追加したときの更新のため
                modsDataContainerAgent.PastModsContainer.Remove(localMod);
            }

            if (NoPreviousData(previousDataListAddedToPastModsDataCue)) return;

            await AddRemoteDataToModsData(modsDataContainerAgent.PastModsContainer, previousDataListAddedToPastModsDataCue);

            modsDataContainerAgent.PastModsContainer.SortByName();
        }

        private async Task GetPreviousDataList(List<ModsDataCsvIndex> previousDataList, string[] AllPastVersion)
        {
            foreach (string pastVersion in AllPastVersion)
            {
                string dataDirectory = Path.Combine(Folder.Instance.dataFolder, pastVersion);
                string modsDataCsvPath = Path.Combine(dataDirectory, "ModsData.csv");

                if (!File.Exists(modsDataCsvPath)) continue;

                List<ModsDataCsvIndex> tempDataList = new List<ModsDataCsvIndex>();
                tempDataList = await modsDataCsv.Read(modsDataCsvPath);

                var exceptDataList = tempDataList.Except(previousDataList);

                foreach (ModsDataCsvIndex exceptData in exceptDataList)
                {
                    bool existsSameModNameAtPrevioudsDataList = previousDataList.Any(x => x.Mod == exceptData.Mod);
                    bool sameMA = previousDataList.Any(x => x.Ma == exceptData.Ma);
                    bool nowMA = true;

                    if (!existsSameModNameAtPrevioudsDataList)
                    {
                        previousDataList.Add(exceptData);
                        continue;
                    }

                    nowMA = previousDataList.Find(x => x.Mod == exceptData.Mod).Ma;

                    if (nowMA && !sameMA)
                    {
                        previousDataList.Find(x => x.Mod == exceptData.Mod).Ma = exceptData.Ma;
                        previousDataList.Find(x => x.Mod == exceptData.Mod).Url = exceptData.Url;
                        continue;
                    }

                    if (previousDataList.Find(x => x.Mod == exceptData.Mod).Url == string.Empty && exceptData.Url != string.Empty)
                    {
                        previousDataList.Find(x => x.Mod == exceptData.Mod).Url = exceptData.Url;
                    }
                }
            }

            foreach (var modAssistantMod in mAMods.ModAssistantAllMods)
            {
                if (!previousDataList.Any(x => x.Mod == modAssistantMod.name)) continue;
                if (!previousDataList.Find(x => x.Mod == modAssistantMod.name).Original) continue;

                UpdateDataToExistsInMAVersion(previousDataList, modAssistantMod);
            }
        }

        private async Task AddRemoteDataToModsData(IModsContainer mods, List<ModsDataCsvIndex> localDataList)
        {
            foreach (var localData in localDataList)
            {
                if (localData.Ma)
                {
                    DateTime now = DateTime.Now;
                    DateTime mAUpdatedAt = mAMods.ExistsData(localData.Mod) ?
                        DateTime.Parse(mAMods.ModAssistantAllMods.First(x => x.name == localData.Mod).updatedDate) : DateTime.MaxValue;
                    string updated = "?";

                    string description = mAMods.ExistsData(localData.Mod) ?
                       mAMods.ModAssistantAllMods.First(x => x.name == localData.Mod).description : "?";

                    if (mAUpdatedAt != DateTime.MaxValue)
                    {
                        updated = (now - mAUpdatedAt).Days >= 1 ?
                        (now - mAUpdatedAt).Days + "D ago" : (now - mAUpdatedAt).Hours + "H" + (now - mAUpdatedAt).Minutes + "m ago";
                    }

                    mods.Add(new PastMod(this)
                    {
                        Mod = localData.Mod,
                        Latest = new Version(localData.LatestVersion),
                        Updated = updated,
                        Original = "〇",
                        MA = "〇",
                        Description = description,
                        Url = localData.Url
                    });

                    continue;
                }

                Release response = null;
                response = await gitHubApi.GetLatestReleaseInfoAsync(localData.Url);
                string original = localData.Original ? "〇" : "×";

                if (response == null)
                {
                    mods.Add(new PastMod(this)
                    {
                        Mod = localData.Mod,
                        Latest = new Version("0.0.0"),
                        Updated = localData.Url == string.Empty ? "?" : "---",
                        Original = original,
                        MA = "×",
                        Description = localData.Url == string.Empty ? "?" : "---",
                        Url = localData.Url
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

                    mods.Add(new PastMod(this)
                    {
                        Mod = localData.Mod,
                        Latest = VersionExtractor.DetectVersionFromRawVersion(response.TagName),
                        Updated = updated,
                        Original = original,
                        MA = "×",
                        Description = response.Body,
                        Url = localData.Url
                    });
                }
            }
        }

        private void LocalModsDataRefresh()
        {
            List<LocalModFile> localModFilesData = GetLocalModFilesData();

            UpdateLocalModsData(localModFilesData);

            RemoveNotExistingModsData(localModFilesData);

            modsDataContainerAgent.LocalModsContainer.SortByName();
        }

        private void UpdateLocalModsData(List<LocalModFile> localModsFileData)
        {
            foreach (LocalModFile localModFileData in localModsFileData)
            {
                if (modsDataContainerAgent.LocalModsContainer.ShouldChangeInstalledVersionToFileItselfVersion(new LocalMod()
                {
                    Mod = localModFileData.ModName,
                    DownloadedFileHash = localModFileData.FileHash
                }))
                {
                    modsDataContainerAgent.LocalModsContainer.UpdateInstalled(new LocalMod()
                    {
                        Mod = localModFileData.ModName,
                        Installed = localModFileData.Version
                    });

                    continue;
                }

                if (!mAMods.ExistsData(localModFileData.ModName))
                {
                    modsDataContainerAgent.LocalModsContainer.Add(new LocalMod()
                    {
                        Mod = localModFileData.ModName,
                        Installed = localModFileData.Version
                    });

                    continue;
                }

                var temp = Array.Find(mAMods.ModAssistantAllMods, x => x.name == localModFileData.ModName);

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

                modsDataContainerAgent.LocalModsContainer.Add(new LocalMod()
                {
                    Mod = localModFileData.ModName,
                    Installed = localModFileData.Version,
                    Latest = new Version(temp.version),
                    Updated = updated,
                    Original = "〇",
                    MA = "〇",
                    Description = temp.description,
                    Url = temp.link
                });
            }
        }

        private void RemoveNotExistingModsData(List<LocalModFile> localModFilesData)
        {
            if (NoLocalModsData()) return;

            List<IMod> removeList = new List<IMod>();
            foreach (var data in modsDataContainerAgent.LocalModsContainer.LocalModsData)
            {
                if (HasRemovedFromLocal(localModFilesData, data))
                {
                    removeList.Add(data);
                }
            }

            if (removeList.Count == 0) return;

            foreach (var removeData in removeList)
            {
                modsDataContainerAgent.LocalModsContainer.Remove(removeData);
            }
        }

        private static List<LocalModFile> GetLocalModFilesData()
        {
            string pluginFolderPath = Path.Combine(Config.Instance.BSFolderPath, "Plugins");
            string pendingPluginFolderPath = Path.Combine(Config.Instance.BSFolderPath, "IPA", "Pending", "Plugins");

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


            List<LocalModFile> localModFilesData = new List<LocalModFile>();

            if (filesName != null)
            {
                foreach (System.IO.FileInfo f in filesName)
                {
                    string pluginPath = Path.Combine(pluginFolderPath, f.Name);

                    System.Diagnostics.FileVersionInfo vi = System.Diagnostics.FileVersionInfo.GetVersionInfo(pluginPath);
                    Version installedModVersion = new Version(vi.FileVersion);
                    string fileHash = FileHashProvider.ComputeFileHash(pluginPath);

                    localModFilesData.Add(new LocalModFile(f.Name.Replace(".dll", string.Empty), installedModVersion, fileHash));
                }
            }

            if (pendingFilesName != null)
            {
                foreach (System.IO.FileInfo pendingF in pendingFilesName)
                {
                    string pendingPluginPath = Path.Combine(pendingPluginFolderPath, pendingF.Name);

                    System.Diagnostics.FileVersionInfo pendingVi = System.Diagnostics.FileVersionInfo.GetVersionInfo(pendingPluginPath);
                    Version pendingInstalledModVersion = new Version(pendingVi.FileVersion);
                    string pendingFileHash = FileHashProvider.ComputeFileHash(pendingPluginPath);

                    if (!localModFilesData.Any(x => x.ModName == pendingF.Name.Replace(".dll", string.Empty)))
                    {
                        localModFilesData.Add(new LocalModFile(pendingF.Name.Replace(".dll", string.Empty), pendingInstalledModVersion, pendingFileHash));
                        continue;
                    }

                    LocalModFile sameModNameFile = localModFilesData.Find(x => x.ModName == pendingF.Name.Replace(".dll", string.Empty));

                    if (pendingInstalledModVersion > sameModNameFile.Version)
                    {
                        localModFilesData.Remove(sameModNameFile);
                        localModFilesData.Add(new LocalModFile(pendingF.Name.Replace(".dll", string.Empty), pendingInstalledModVersion, pendingFileHash));
                    }
                }
            }

            return localModFilesData;
        }

        private bool NoLocalModsData()
        {
            return modsDataContainerAgent.LocalModsContainer.LocalModsData.Count == 0;
        }

        private bool NoPreviousData(List<ModsDataCsvIndex> previousDataListAddedToPastModsData)
        {
            return previousDataListAddedToPastModsData.Count == 0;
        }


        private bool HasRemovedFromLocal(List<LocalModFile> localModFilesData, IMod data)
        {
            return !localModFilesData.Any(x => x.ModName == data.Mod);
        }

        private static void UpdateDataToExistsInMAVersion(List<ModsDataCsvIndex> previousDataList, MAMod modAssistantMod)
        {
            previousDataList.Find(x => x.Mod == modAssistantMod.name).Ma = true;
            previousDataList.Find(x => x.Mod == modAssistantMod.name).LatestVersion = modAssistantMod.version;
            previousDataList.Find(x => x.Mod == modAssistantMod.name).Url = modAssistantMod.link;
        }
    }
}