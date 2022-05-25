using BSModManager.Interfaces;
using CsvHelper;
using CsvHelper.Configuration.Attributes;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BSModManager.Models
{
    public class ModCsvHandler
    {
        public async Task Write(string csvPath, IEnumerable<IModData> e)
        {
            List<ModCsvIndex> modInformationCsvList = new List<ModCsvIndex>();

            foreach (var a in e)
            {
                var githubModInstance = new ModCsvIndex()
                {
                    Mod = a.Mod,
                    LocalVersion = a.Installed.ToString(),
                    LatestVersion = a.Latest.ToString(),
                    Original = a.Original != "×",
                    Ma = a.MA != "×",
                    Url = a.Url,
                };
                modInformationCsvList.Add(githubModInstance);
            }

            using (var writer = new StreamWriter(csvPath, false))
            using (var csv = new CsvWriter(writer, new CultureInfo("ja-JP", false)))
            {
                await csv.WriteRecordsAsync(modInformationCsvList);
            }
        }

        public async Task<List<ModCsvIndex>> Read(string csvPath)
        {
            List<ModCsvIndex> output = null;

            using (var reader = new StreamReader(csvPath))
            using (var csv = new CsvReader(reader, new CultureInfo("ja-JP", false)))
            {
                await Task.Run(() =>
                {
                    output = csv.GetRecords<ModCsvIndex>().ToList();
                });
            }

            return output;
        }

        public class ModCsvIndex
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
            public bool Ma { get; set; }
            [Index(5)]
            public string Url { get; set; }
        }
    }
}
