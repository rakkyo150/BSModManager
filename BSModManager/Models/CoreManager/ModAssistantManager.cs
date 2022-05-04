﻿using BSModManager.Models.Structure;
using BSModManager.Models.ViewModelCommonProperty;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace BSModManager.Models.CoreManager
{
    public class ModAssistantManager : DataManager
    {
        public ModAssistantManager(InnerData id, SettingsTabPropertyModel stpm, UpdateMyselfConfirmPropertyModel umcpm, MainTabPropertyModel mtpm) : base(id, stpm, umcpm, mtpm)
        {

        }

        public async Task<ModAssistantModInformation[]> GetAllModAssistantModsAsync()
        {
            ModAssistantModInformation[] modAssistantMod = null;

            string gameVersion = GetGameVersion();

            string modAssistantModInformationUrl = $"https://beatmods.com/api/v1/mod?status=approved&gameVersion={gameVersion}";

            using (HttpClient httpClient = new HttpClient())
            {
                try
                {
                    var resp = await httpClient.GetStringAsync(modAssistantModInformationUrl);
                    modAssistantMod = JsonConvert.DeserializeObject<ModAssistantModInformation[]>(resp);

                    // Console.WriteLine("Fisnish GetAllMods");

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