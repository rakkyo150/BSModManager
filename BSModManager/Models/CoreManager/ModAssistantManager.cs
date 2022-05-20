using BSModManager.Models.Structure;
using BSModManager.Models.ViewModelCommonProperty;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace BSModManager.Models.CoreManager
{
    public class ModAssistantManager : DataManager
    {
        public ModAssistantManager(InnerData id, SettingsTabPropertyModel stpm, UpdateMyselfConfirmPropertyModel umcpm,MainWindowPropertyModel mwpm ,LocalModsDataModel mdm) : base(id, stpm, umcpm,mwpm,mdm)
        {

        }

        public async Task<ModAssistantModInformation[]> GetAllModAssistantModsAsync()
        {
            ModAssistantModInformation[] modAssistantMod = null;

            string gameVersion = GetGameVersion();

            // 一時的に1.21.0にしておく
            string modAssistantModInformationUrl = $"https://beatmods.com/api/v1/mod?status=approved&gameVersion={gameVersion}";

            using (HttpClient httpClient = new HttpClient())
            {
                try
                {
                    var resp = await httpClient.GetStringAsync(modAssistantModInformationUrl);
                    modAssistantMod = JsonConvert.DeserializeObject<ModAssistantModInformation[]>(resp);

                    Version retryGameVersion = new Version(gameVersion);

                    while (modAssistantMod.Length==0)
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
                        modAssistantMod = JsonConvert.DeserializeObject<ModAssistantModInformation[]>(retryResp);
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
                        mod.name = mod.name.Replace(" ", "");
                    }
                }
                catch (Exception ex) { Console.WriteLine(ex.Message); }
            }

            return modAssistantMod;
        }
    }
}
