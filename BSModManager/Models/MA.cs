using BSModManager.Static;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace BSModManager.Models
{
    public class MA
    {
        public class MAMod
        {
            public string name;
            public string version;
            public string gameVersion;
            public string _id;
            public string status;
            public string authorId;
            public string uploadedDate;
            public string updatedDate;
            public Author author;
            public string description;
            public string link;
            public string category;
            public DownloadLink[] downloads;
            public bool required;
            public Dependency[] dependencies;
            public List<MAMod> Dependents = new List<MAMod>();

            public class Author
            {
                public string _id;
                public string username;
                public string lastLogin;
            }

            public class DownloadLink
            {
                public string type;
                public string url;
                public FileHashes[] hashMd5;
            }

            public class FileHashes
            {
                public string hash;
                public string file;
            }

            public class Dependency
            {
                public string name;
                public string _id;
                public MAMod Mod;
            }
        }

        public MAMod[] ModAssistantAllMods { get; set; }

        public async Task<MAMod[]> GetAllAsync()
        {
            MAMod[] modAssistantMod = null;

            string gameVersion = VersionExtractor.GameVersion;

            string modAssistantModInformationUrl = $"https://beatmods.com/api/v1/mod?status=approved&gameVersion={gameVersion}";

            using (HttpClient httpClient = new HttpClient())
            {
                try
                {
                    string resp = await httpClient.GetStringAsync(modAssistantModInformationUrl);
                    modAssistantMod = JsonConvert.DeserializeObject<MAMod[]>(resp);

                    Version retryGameVersion = new Version(gameVersion);

                    while (modAssistantMod.Length == 0)
                    {
                        if (retryGameVersion.Build > 0)
                        {
                            retryGameVersion = new Version(retryGameVersion.Major, retryGameVersion.Minor, retryGameVersion.Build - 1);
                        }
                        else
                        {
                            retryGameVersion = new Version(retryGameVersion.Major, retryGameVersion.Minor - 1, 9);
                        }

                        string retryModAssistantModInformationUrl = $"https://beatmods.com/api/v1/mod?status=approved&gameVersion={retryGameVersion}";

                        string retryResp = await httpClient.GetStringAsync(retryModAssistantModInformationUrl);
                        modAssistantMod = JsonConvert.DeserializeObject<MAMod[]>(retryResp);
                    }

                    foreach (MAMod mod in modAssistantMod)
                    {
                        // Mod名とファイル名が違う、よく使うModに対応
                        if (mod.name == "BeatSaberMarkupLanguage")
                        {
                            mod.name = "BSML";
                        }
                        else if (mod.name == "BS Utils")
                        {
                            mod.name = "BS_Utils";
                        }
                        mod.name = mod.name.Replace(" ", string.Empty);
                    }
                }
                catch (Exception ex) { Logger.Instance.Error($"{ex.Message}\nModAssistantのデータの取得に失敗しました"); }
            }

            return modAssistantMod;
        }

        internal bool ExistsData(string modName)
        {
            return Array.Exists(ModAssistantAllMods, x => x.name == modName);
        }
    }
}
