using BSModManager.Models.Structure;
using System;
using System.Collections.Generic;

namespace BSModManager.Models.CoreManager
{
    public class InnerData
    {
        public Dictionary<string, Tuple<Version, bool, string>> nowLocalGithubModAndVersionAndOriginalBoolAndUrl { get; set; } = new Dictionary<string, Tuple<Version, bool, string>>();

        public string gameVersion { get; set; }

        public Dictionary<string, Version> nowLocalFilesInfoDictionary { get; set; }

        public List<string> installedMAMod { get; set; } = new List<string>();

        public ModAssistantModInformation[] modAssistantAllMods { get; set; }


        public List<ModInformationCsv> installedGithubModInformationToCsvForInitialize { get; set; } = new List<ModInformationCsv>();
        public List<ModInformationCsv> installedGitHubModInformationToCsvForUpdate { get; set; } = new List<ModInformationCsv>();
        public List<MAModInformationCsv> detectedModAssistantModCsvListForInitialize { get; set; } = new List<MAModInformationCsv>();
        public List<MAModInformationCsv> modAssistantModCsvListForUpdate { get; set; } = new List<MAModInformationCsv>();
    }
}
