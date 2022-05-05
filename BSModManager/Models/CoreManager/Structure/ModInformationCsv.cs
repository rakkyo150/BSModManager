using CsvHelper.Configuration.Attributes;

namespace BSModManager.Models.Structure
{
    public class ModInformationCsv
    {
        [Index(0)]
        public string Mod { get; set; }
        [Index(1)]
        public string LocalVersion { get; set; }
        [Index(2)]
        public string LatestVersion { get; set; }
        [Index(3)]
        public bool Original { get; set; }
        [Index(4)]
        public string Url { get; set; }
    }
}
