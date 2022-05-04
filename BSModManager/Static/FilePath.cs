using System;
using System.IO;

namespace BSModManager.Static
{
    public static class FilePath
    {
        public readonly static string configFilePath = Path.Combine(Environment.CurrentDirectory, "config.json");
        public readonly static string mAModCsvPath = Path.Combine(FolderManager.dataFolder, "ModAssistantModData.csv");
    }
}
