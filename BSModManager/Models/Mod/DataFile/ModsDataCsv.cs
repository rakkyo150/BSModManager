using BSModManager.Interfaces;
using Csv;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BSModManager.Models
{
    public class ModsDataCsv
    {
        public void Write(string csvPath, IEnumerable<IMod> modEnum)
        {
            List<ModsDataCsvIndex> modInformationCsvList = new List<ModsDataCsvIndex>();

            foreach (IMod mod in modEnum)
            {
                ModsDataCsvIndex githubModInstance = new ModsDataCsvIndex()
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

            // List<ModsDataCsvIndex>をIEnumerable<string[]>に変換する
            string csv = CsvWriter.WriteToText(
                ModsDataCsvIndex.GetPropertyNames(),
                modInformationCsvList.Select(x => new string[] { x.Mod, x.LocalVersion, x.LatestVersion, x.DownloadedFileHash, x.Original.ToString(), x.Ma.ToString(), x.Url }),
                ','
                );

            File.WriteAllText(csvPath, csv);
        }

        public List<ModsDataCsvIndex> Read(string csvPath)
        {
            List<ModsDataCsvIndex> output = new List<ModsDataCsvIndex>();

            string csv = File.ReadAllText(csvPath);
            IEnumerable<ICsvLine> data = CsvReader.ReadFromText(csv);

            foreach (ICsvLine item in data)
            {
                output.Add(new ModsDataCsvIndex()
                {
                    Mod = item["Mod"],
                    LocalVersion = item["LocalVersion"],
                    LatestVersion = item["LatestVersion"],
                    DownloadedFileHash = item["DownloadedFileHash"],
                    Original = System.Convert.ToBoolean(item["Original"]),
                    Ma = System.Convert.ToBoolean(item["Ma"]),
                    Url = item["Url"]
                });
            }

            // DownloadFileHashのデータがないならstring.Emptyを返す
            return output;
        }

        public class ModsDataCsvIndex
        {
            public string Mod { get; set; }
            public string LocalVersion { get; set; }
            public string LatestVersion { get; set; }
            public string DownloadedFileHash { get; set; }
            public bool Original { get; set; }
            public bool Ma { get; set; }
            public string Url { get; set; }

            // ModsDataCsvIndexの全プロパティ名を配列にして出力する
            public static string[] GetPropertyNames()
            {
                return typeof(ModsDataCsvIndex).GetProperties().Select(x => x.Name).ToArray();
            }

        }
    }
}
