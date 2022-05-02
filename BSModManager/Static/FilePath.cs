using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BSModManager.Static
{
    public static class FilePath
    {
        public readonly static string configFilePath = Path.Combine(Environment.CurrentDirectory, "config.json");
        public readonly static string mAModCsvPath = Path.Combine(FolderManager.dataFolder, "ModAssistantModData.csv");
    }
}
