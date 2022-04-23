using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace BSModManager.Models
{
    public class ConfigFileManager
    {
        public string configFilePath = Path.Combine(Environment.CurrentDirectory, "config.json");

        public Dictionary<string, string> LoadConfigFile()
        {
            Dictionary<string, string> settingDictionary = null;

            if (File.Exists(configFilePath))
            {
                StreamReader re = new StreamReader(configFilePath);
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
                    {"GitHubToken", null}
                };
            }

            return settingDictionary;
        }

        public void MakeConfigFile(string bSFolderPath, string gitHubToken)
        {
            Dictionary<string, string> settingDictionary = new Dictionary<string, string>()
            {
                {"BSFolderPath",bSFolderPath },
                {"GitHubToken", gitHubToken}
            };

            string _jsonFinish = JsonConvert.SerializeObject(settingDictionary, Formatting.Indented);

            StreamWriter wr = new StreamWriter(new FileStream(configFilePath, FileMode.Create));
            wr.WriteLine(_jsonFinish);
            wr.Close();
        }
    }
}
