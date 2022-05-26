using BSModManager.Static;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace BSModManager.Models
{
    public class ConfigFileHandler
    {
        public Dictionary<string, string> Load()
        {
            Dictionary<string, string> settingDictionary = null;

            if (File.Exists(FilePath.Instance.configFilePath))
            {
                StreamReader re = new StreamReader(FilePath.Instance.configFilePath);
                string _jsonStr = re.ReadToEnd();
                Logger.Instance.Debug(_jsonStr);
                re.Close();
                var _jsonDyn = JsonConvert.DeserializeObject<Dictionary<string, string>>(_jsonStr);

                if (_jsonDyn != null)
                {
                    settingDictionary = _jsonDyn;
                }
            }
            else
            {
                settingDictionary = new Dictionary<string, string>()
                {
                    {"BSFolderPath",null },
                    {"GitHubToken", null},
                    {"MAExePath", null }
                };
            }

            return settingDictionary;
        }

        public void Generate(string bSFolderPath, string gitHubToken, string mAExePath)
        {
            Dictionary<string, string> settingDictionary = new Dictionary<string, string>()
            {
                {"BSFolderPath",bSFolderPath },
                {"GitHubToken", gitHubToken},
                {"MAExePath", mAExePath }
            };

            string _jsonFinish = JsonConvert.SerializeObject(settingDictionary, Formatting.Indented);

            StreamWriter wr = new StreamWriter(new FileStream(FilePath.Instance.configFilePath, FileMode.Create));
            wr.WriteLine(_jsonFinish);
            wr.Close();
        }
    }
}
