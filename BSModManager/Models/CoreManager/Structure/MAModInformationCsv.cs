using CsvHelper.Configuration.Attributes;

namespace BSModManager.Models.Structure
{
    public class MAModInformationCsv
    {
        [Index(0)]
        public string ModAssistantMod { get; set; }
        [Index(1)]
        public string LocalVersion { get; set; }
        [Index(2)]
        public string ModAssistantVersion { get; set; }
    }
}
