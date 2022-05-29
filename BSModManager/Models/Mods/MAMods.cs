using BSModManager.Models.Mods.Structures;
using BSModManager.Static;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace BSModManager.Models
{
    public class MAMods
    {
        public MAModData[] ModAssistantAllMods { get; set; }

        public async Task<MAModData[]> GetAllAsync()
        {
            MAModData[] modAssistantMod = null;

            string gameVersion = GameVersion.Version;

            string modAssistantModInformationUrl = $"https://beatmods.com/api/v1/mod?status=approved&gameVersion={gameVersion}";

            using (HttpClient httpClient = new HttpClient())
            {
                try
                {
                    var resp = await httpClient.GetStringAsync(modAssistantModInformationUrl);
                    modAssistantMod = JsonConvert.DeserializeObject<MAModData[]>(resp);

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

                        var retryResp = await httpClient.GetStringAsync(retryModAssistantModInformationUrl);
                        modAssistantMod = JsonConvert.DeserializeObject<MAModData[]>(retryResp);
                    }

                    foreach (var mod in modAssistantMod)
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

        internal bool ExistsData(MAModData mAModData)
        {
            return Array.Exists(ModAssistantAllMods, x => x.name == mAModData.name);
        }
    }
}
