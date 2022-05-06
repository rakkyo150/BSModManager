using BSModManager.Static;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace BSModManager.Models
{
    public class ConfigFileManager
    {
        public Dictionary<string, string> LoadConfigFile()
        {
            Dictionary<string, string> settingDictionary = null;

            if (File.Exists(FilePath.configFilePath))
            {
                StreamReader re = new StreamReader(FilePath.configFilePath);
                string _jsonStr = re.ReadToEnd();
                Console.WriteLine(_jsonStr);
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

        public void MakeConfigFile(string bSFolderPath, string gitHubToken, string mAExePath)
        {
            Dictionary<string, string> settingDictionary = new Dictionary<string, string>()
            {
                {"BSFolderPath",bSFolderPath },
                {"GitHubToken", gitHubToken},
                {"MAExePath", mAExePath }
            };

            string _jsonFinish = JsonConvert.SerializeObject(settingDictionary, Formatting.Indented);

            StreamWriter wr = new StreamWriter(new FileStream(FilePath.configFilePath, FileMode.Create));
            wr.WriteLine(_jsonFinish);
            wr.Close();
        }
    }
}
