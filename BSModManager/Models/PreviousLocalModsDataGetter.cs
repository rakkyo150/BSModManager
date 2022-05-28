using BSModManager.Models.Mods.Structures;
using BSModManager.Static;
using Octokit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using static BSModManager.Models.ModCsvHandler;

namespace BSModManager.Models
{
    public class PreviousLocalModsDataGetter
    {
        readonly LocalMods localMods;
        readonly GitHubApi gitHubApi;
        readonly MAMods mAMods;
        readonly ModCsvHandler modCsv;
        readonly Refresher refresher;
        readonly DateTime now = DateTime.Now;
        string updated = "";

        public PreviousLocalModsDataGetter(LocalMods lm, GitHubApi gha, MAMods mam, ModCsvHandler mc, Refresher r)
        {
            localMods = lm;
            gitHubApi = gha;
            mAMods = mam;
            modCsv = mc;
            refresher = r;
        }

        internal async Task GetData()
        {
            string dataDirectory = Path.Combine(Folder.Instance.dataFolder, GameVersion.Version);
            string modsDataCsvPath = Path.Combine(dataDirectory, "ModsData.csv");
            List<ModCsvIndex> previousDataList;

            if (!File.Exists(modsDataCsvPath)) return;

            previousDataList = await modCsv.Read(modsDataCsvPath);
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

                    localMods.LocalModsData.Add(new LocalModData(refresher)
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
                    localMods.LocalModsData.Add(new LocalModData(refresher)
                    {
                        Mod = previousData.Mod,
                        Latest = new Version("0.0.0"),
                        Updated = previousData.Url == "" ? "?" : "---",
                        Original = original,
                        MA = "×",
                        Description = previousData.Url == "" ? "?" : "---",
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

                localMods.LocalModsData.Add(new LocalModData(refresher)
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

        private bool ExistsModDataInMA(ModCsvIndex previousData)
        {
            return Array.Exists(mAMods.ModAssistantAllMods, x => x.name == previousData.Mod);
        }
    }
}
