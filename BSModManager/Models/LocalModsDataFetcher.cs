using BSModManager.Static;
using Octokit;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using static BSModManager.Models.ModCsv;

namespace BSModManager.Models
{
    public class LocalModsDataFetcher
    {
        LocalMods localMods;
        GitHubApi gitHubApi;
        MAMods mAMods;
        ModCsv modCsv;
        Syncer syncer;

        public LocalModsDataFetcher(LocalMods lm, GitHubApi gha, MAMods mam, ModCsv mc, Syncer s)
        {
            localMods = lm;
            gitHubApi = gha;
            mAMods = mam;
            modCsv = mc;
            syncer = s;
        }

        internal async Task FetchData()
        {
            string dataDirectory = Path.Combine(Folder.Instance.dataFolder, GameVersion.Version);
            string modsDataCsvPath = Path.Combine(dataDirectory, "ModsData.csv");
            List<ModCsvIndex> previousDataList;
            if (File.Exists(modsDataCsvPath))
            {
                previousDataList = await modCsv.Read(modsDataCsvPath);
                foreach (var previousData in previousDataList)
                {
                    if (Array.Exists(mAMods.modAssistantAllMods, x => x.name == previousData.Mod))
                    {
                        if (previousData.Original)
                        {
                            var temp = Array.Find(mAMods.modAssistantAllMods, x => x.name == previousData.Mod);

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

                            localMods.LocalModsData.Add(new LocalMods.LocalModData(syncer)
                            {
                                Mod = previousData.Mod,
                                Latest = new Version(temp.version),
                                Updated = updated,
                                Original = "〇",
                                MA = "〇",
                                Description = temp.description,
                                Url = temp.link
                            });
                        }
                    }
                    else
                    {
                        Release response = await gitHubApi.GetModLatestVersionAsync(previousData.Url);
                        string original = null;
                        if (!previousData.Original)
                        {
                            original = "×";
                        }
                        else
                        {
                            original = "〇";
                        }

                        if (response == null)
                        {
                            localMods.LocalModsData.Add(new LocalMods.LocalModData(syncer)
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

                            localMods.LocalModsData.Add(new LocalMods.LocalModData(syncer)
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
            }
        }
    }
}
