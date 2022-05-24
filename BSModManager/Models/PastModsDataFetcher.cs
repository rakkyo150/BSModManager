using BSModManager.Static;
using Octokit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static BSModManager.Models.ModCsv;

namespace BSModManager.Models
{
    public class PastModsDataFetcher
    {
        MAMods mAMods;
        LocalMods localMods;
        PastMods pastMods;
        ModCsv modCsv;
        Syncer syncer;
        GitHubApi gitHubApi;

        public PastModsDataFetcher(MAMods mam,LocalMods lm,PastMods pm,ModCsv mc,Syncer s,GitHubApi gha)
        {
            mAMods = mam;
            localMods = lm;
            pastMods = pm;
            modCsv = mc;
            syncer = s;
            gitHubApi = gha;
        }

        public async Task FetchData()
        {
            List<ModCsvIndex> previousDataList = new List<ModCsvIndex>();

            // 現在のバージョンも含む
            string[] AllPastVersion = Directory.GetDirectories(Folder.Instance.dataFolder, "*", SearchOption.TopDirectoryOnly);

            if (AllPastVersion.Count() == 0) return;

            foreach (string pastVersion in AllPastVersion)
            {
                string dataDirectory = Path.Combine(Folder.Instance.dataFolder, pastVersion);
                string modsDataCsvPath = Path.Combine(dataDirectory, "ModsData.csv");

                if (!File.Exists(modsDataCsvPath)) continue;

                List<ModCsvIndex> tempDataList = new List<ModCsvIndex>();
                tempDataList = await modCsv.Read(modsDataCsvPath);

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

            foreach (var modAssistantMod in mAMods.modAssistantAllMods)
            {
                if (!previousDataList.Any(x => x.Mod == modAssistantMod.name)) continue;
                if (!previousDataList.Find(x => x.Mod == modAssistantMod.name).Original) continue;

                UpdatePreviousDataToExistInMAVersion(previousDataList, modAssistantMod);
            }

            foreach (var localMod in localMods.LocalModsData)
            {
                if (!previousDataList.Any(x => x.Mod == localMod.Mod)) continue;
                if (!previousDataList.Find(x => x.Mod == localMod.Mod).Original == (localMod.Original == "〇" ? true : false)) continue;

                previousDataList.Remove(previousDataList.Find(x => x.Mod == localMod.Mod));
            }

            if (NoPreviousData(previousDataList)) return;

            foreach (var previousData in previousDataList)
            {
                if (previousData.Ma)
                {
                    bool existsInNowMa = Array.Exists(mAMods.modAssistantAllMods, x => x.name == previousData.Mod);

                    DateTime now = DateTime.Now;
                    DateTime mAUpdatedAt = existsInNowMa ?
                        DateTime.Parse(mAMods.modAssistantAllMods.First(x => x.name == previousData.Mod).updatedDate) : DateTime.MaxValue;
                    string updated = "?";

                    string description = existsInNowMa ?
                       mAMods.modAssistantAllMods.First(x => x.name == previousData.Mod).description : "?";

                    if (mAUpdatedAt != DateTime.MaxValue)
                    {
                        updated = (now - mAUpdatedAt).Days >= 1 ?
                        (now - mAUpdatedAt).Days + "D ago" : (now - mAUpdatedAt).Hours + "H" + (now - mAUpdatedAt).Minutes + "m ago";
                    }

                    pastMods.Add(new PastMods.PastModData(syncer)
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
                response = await gitHubApi.GetModLatestVersionAsync(previousData.Url);
                string original = previousData.Original ? "〇" : "×";

                if (response == null)
                {
                    pastMods.Add(new PastMods.PastModData(syncer)
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

                    pastMods.Add(new PastMods.PastModData(syncer)
                    {
                        Mod = previousData.Mod,
                        Latest = gitHubApi.DetectVersion(response.TagName),
                        Updated = updated,
                        Original = original,
                        MA = "×",
                        Description = response.Body,
                        Url = previousData.Url
                    });
                }
            }
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
