using BSModManager.Interfaces;
using BSModManager.Static;
using CsvHelper;
using CsvHelper.Configuration;
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
        public void Write(string csvPath, IEnumerable<IModData> modEnum)
        {
            List<ModCsvIndex> modInformationCsvList = new List<ModCsvIndex>();

            foreach (var mod in modEnum)
            {
                var githubModInstance = new ModCsvIndex()
                {
                    Mod = mod.Mod,
                    LocalVersion = mod.Installed.ToString(),
                    LatestVersion = mod.Latest.ToString(),
                    DownloadedFileHash = mod.DownloadedFileHash,
                    Original = mod.Original != "×",
                    Ma = mod.MA != "×",
                    Url = mod.Url
                };
                modInformationCsvList.Add(githubModInstance);
            }

            if (modInformationCsvList.Count == 0) return;

            using (var writer = new StreamWriter(csvPath, false))
            using (var csv = new CsvWriter(writer, new CultureInfo("ja-JP", false)))
            {
                csv.WriteRecords(modInformationCsvList);
                csv.Flush();
            }
        }

        public async Task<List<ModCsvIndex>> Read(string csvPath)
        {
            List<ModCsvIndex> output = null;

            var config = new CsvConfiguration(new CultureInfo("ja-JP", false))
            {
                HeaderValidated = null,
                MissingFieldFound = (e) =>
                {
                    Logger.Instance.Info($"{e.Context}のデータが不足しています。");
                }
            };

            using (var reader = new StreamReader(csvPath))
            using(var csv = new CsvReader(reader, config))
            {
                await Task.Run(() =>
                {
                    output = csv.GetRecords<ModCsvIndex>().ToList();
                });
            }

            // DownloadFileHashのデータがないならstring.Emptyを返す
            return output;
        }

        public class ModCsvIndex
        {
            [Name("Mod")]
            public string Mod { get; set; }
            [Name("LocalVersion")]
            public string LocalVersion { get; set; }
            [Name("LatestVersion")]
            public string LatestVersion { get; set; }
            [Name("DownloadedFileHash")]
            public string DownloadedFileHash { get; set; }
            [Name("Original")]
            public bool Original { get; set; }
            [Name("Ma")]
            public bool Ma { get; set; }
            [Name("Url")]
            public string Url { get; set; }
        }
    }
}
