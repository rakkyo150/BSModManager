using BSModManager.Interfaces;
using BSModManager.Models.Mods.Structures;
using BSModManager.Static;
using Newtonsoft.Json;
using Octokit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        readonly RecommendMods recommendMods;
        readonly List<IModData> removedPastModsData = new List<IModData>();
        readonly List<IModData> removedRecommendModsData = new List<IModData>();


        public Refresher(MAMods mam, LocalMods lm, ModCsvHandler mch, PastMods pm, GitHubApi gha, RecommendMods rm)
        {
            mAMods = mam;
            localMods = lm;
            modCsvHandler = mch;
            pastMods = pm;
            gitHubApi = gha;
            recommendMods = rm;
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
            foreach (var localMod in localMods.LocalModsData)
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

        private async Task SetRemovedRecommendModsDataToLocalMods(IModData localMod)
        {
            IModData modData = removedRecommendModsData.First(x => x.Mod == localMod.Mod);

            Release response = await gitHubApi.GetLatestReleaseInfoAsync(modData.Url);

            localMods.UpdateURL(modData);
            localMods.UpdateOriginal(modData);

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

            IModData assignedModData = new LocalModData(this)
            {
                Mod = localMod.Mod,
                Latest = gitHubApi.DetectVersionFromTagName(response.TagName),
                Updated = updated,
                Description = response.Body
            };


            localMods.UpdateLatest(assignedModData);
            localMods.UpdateUpdated(assignedModData);
            localMods.UpdateDescription(assignedModData);
        }

        private async Task SetRemovedPastModsDataToLocalMods(IModData localMod)
        {
            IModData modData = removedPastModsData.First(x => x.Mod == localMod.Mod);

            Release response = await gitHubApi.GetLatestReleaseInfoAsync(modData.Url);

            localMods.UpdateURL(modData);
            localMods.UpdateOriginal(modData);

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

            IModData assignedModData = new LocalModData(this)
            {
                Mod = localMod.Mod,
                Latest = gitHubApi.DetectVersionFromTagName(response.TagName),
                Updated = updated,
                Description = response.Body
            };


            localMods.UpdateLatest(assignedModData);
            localMods.UpdateUpdated(assignedModData);
            localMods.UpdateDescription(assignedModData);
        }

        private async Task RecommendModDataRefreash()
        {
            List<ModCsvIndex> notInstalledRecommendList = new List<ModCsvIndex>();

            System.Reflection.Assembly myAssembly = System.Reflection.Assembly.GetExecutingAssembly();
            System.IO.StreamReader sr =
                new System.IO.StreamReader(
                myAssembly.GetManifestResourceStream("BSModManager.Models.Mods.RecommendMods.json"),
                    System.Text.Encoding.GetEncoding("shift-jis"));
            string s = sr.ReadToEnd();
            sr.Close();

            List<RecommendModData> rawRecommendModData = JsonConvert.DeserializeObject<List<RecommendModData>>(s);
            foreach (var a in rawRecommendModData)
            {
                notInstalledRecommendList.Add(new ModCsvIndex()
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

            foreach (var localMod in localMods.LocalModsData)
            {
                if (!notInstalledRecommendList.Any(x => x.Mod == localMod.Mod)) continue;
                if (!(notInstalledRecommendList.First(x => x.Mod == localMod.Mod).Original == (localMod.Original == "〇"))) continue;

                removedRecommendModsData.Add(new RecommendModData(this)
                {
                    Mod = localMod.Mod,
                    Original = notInstalledRecommendList.First(x => x.Mod == localMod.Mod).Original ? "〇" : "×",
                    Url = notInstalledRecommendList.First(x => x.Mod == localMod.Mod).Url
                });

                notInstalledRecommendList.Remove(notInstalledRecommendList.First(x => x.Mod == localMod.Mod));

                if (!recommendMods.ExistsSameModNameData(localMod)) continue;

                recommendMods.Remove(localMod);
            }

            await AddRemoteDataToModsData(recommendMods, notInstalledRecommendList);

            foreach (var a in rawRecommendModData)
            {
                if (recommendMods.ExistsSameModNameData(a))
                {
                    recommendMods.UpdateDescription(new RecommendModData(this)
                    {
                        Mod = a.Mod,
                        Description = a.Description
                    });
                }
            }

            recommendMods.SortByName();
        }

        private async Task PastModsDataRefresh()
        {
            List<ModCsvIndex> previousDataListAddedToPastModsDataCue = new List<ModCsvIndex>();

            // 現在のバージョンも含む
            string[] AllPastVersion = Directory.GetDirectories(Folder.Instance.dataFolder, "*", SearchOption.TopDirectoryOnly);

            if (AllPastVersion.Count() == 0) return;

            await GetPreviousDataList(previousDataListAddedToPastModsDataCue, AllPastVersion);

            foreach (var localMod in localMods.LocalModsData)
            {
                if (!previousDataListAddedToPastModsDataCue.Any(x => x.Mod == localMod.Mod)) continue;
                if (!previousDataListAddedToPastModsDataCue.Find(x => x.Mod == localMod.Mod).Original == (localMod.Original == "〇")) continue;

                removedPastModsData.Add(new RecommendModData(this)
                {
                    Mod = localMod.Mod,
                    Original = previousDataListAddedToPastModsDataCue.First(x => x.Mod == localMod.Mod).Original ? "〇" : "×",
                    Url = previousDataListAddedToPastModsDataCue.First(x => x.Mod == localMod.Mod).Url
                });

                previousDataListAddedToPastModsDataCue.Remove(previousDataListAddedToPastModsDataCue.Find(x => x.Mod == localMod.Mod));

                if (!pastMods.ExistsSameModNameData(localMod)) continue;

                // MAや手動でMod追加したときの更新のため
                pastMods.Remove(localMod);
            }

            if (NoPreviousData(previousDataListAddedToPastModsDataCue)) return;

            await AddRemoteDataToModsData(pastMods, previousDataListAddedToPastModsDataCue);

            pastMods.SortByName();
        }

        private async Task GetPreviousDataList(List<ModCsvIndex> previousDataList, string[] AllPastVersion)
        {
            foreach (string pastVersion in AllPastVersion)
            {
                string dataDirectory = Path.Combine(Folder.Instance.dataFolder, pastVersion);
                string modsDataCsvPath = Path.Combine(dataDirectory, "ModsData.csv");

                if (!File.Exists(modsDataCsvPath)) continue;

                List<ModCsvIndex> tempDataList = new List<ModCsvIndex>();
                tempDataList = await modCsvHandler.Read(modsDataCsvPath);

                var exceptDataList = tempDataList.Except(previousDataList);

                foreach (ModCsvIndex exceptData in exceptDataList)
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

        private async Task AddRemoteDataToModsData(IMods mods, List<ModCsvIndex> localDataList)
        {
            foreach (var localData in localDataList)
            {
                if (localData.Ma)
                {
                    DateTime now = DateTime.Now;
                    DateTime mAUpdatedAt = mAMods.ExistsData(new MAModData() { name = localData.Mod }) ?
                        DateTime.Parse(mAMods.ModAssistantAllMods.First(x => x.name == localData.Mod).updatedDate) : DateTime.MaxValue;
                    string updated = "?";

                    string description = mAMods.ExistsData(new MAModData() { name = localData.Mod }) ?
                       mAMods.ModAssistantAllMods.First(x => x.name == localData.Mod).description : "?";

                    if (mAUpdatedAt != DateTime.MaxValue)
                    {
                        updated = (now - mAUpdatedAt).Days >= 1 ?
                        (now - mAUpdatedAt).Days + "D ago" : (now - mAUpdatedAt).Hours + "H" + (now - mAUpdatedAt).Minutes + "m ago";
                    }

                    mods.Add(new PastModData(this)
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
                    mods.Add(new PastModData(this)
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

                    mods.Add(new PastModData(this)
                    {
                        Mod = localData.Mod,
                        Latest = gitHubApi.DetectVersionFromTagName(response.TagName),
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

            localMods.SortByName();
        }

        private void UpdateLocalModsData(List<LocalModFile> localModsFileData)
        {
            foreach (LocalModFile localModFileData in localModsFileData)
            {
                if (localMods.ShouldChangeInstalledVersionToFileItselfVersion(new LocalModData(this)
                {
                    Mod = localModFileData.ModName,
                    DownloadedFileHash = localModFileData.FileHash
                }))
                {
                    localMods.UpdateInstalled(new LocalModData(this)
                    {
                        Mod = localModFileData.ModName,
                        Installed = localModFileData.Version
                    });

                    continue;
                }

                if (!mAMods.ExistsData(new MAModData() { name = localModFileData.ModName }))
                {
                    localMods.Add(new LocalModData(this)
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

                localMods.Add(new LocalModData(this)
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

            List<IModData> removeList = new List<IModData>();
            foreach (var data in localMods.LocalModsData)
            {
                if (HasRemovedFromLocal(localModFilesData, data))
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

        private static List<LocalModFile> GetLocalModFilesData()
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
            return localMods.LocalModsData.Count == 0;
        }

        private bool NoPreviousData(List<ModCsvIndex> previousDataListAddedToPastModsData)
        {
            return previousDataListAddedToPastModsData.Count == 0;
        }


        private bool HasRemovedFromLocal(List<LocalModFile> localModFilesData, IModData data)
        {
            return !localModFilesData.Any(x => x.ModName == data.Mod);
        }

        private static void UpdateDataToExistsInMAVersion(List<ModCsvIndex> previousDataList, MAModData modAssistantMod)
        {
            previousDataList.Find(x => x.Mod == modAssistantMod.name).Ma = true;
            previousDataList.Find(x => x.Mod == modAssistantMod.name).LatestVersion = modAssistantMod.version;
            previousDataList.Find(x => x.Mod == modAssistantMod.name).Url = modAssistantMod.link;
        }
    }
}
