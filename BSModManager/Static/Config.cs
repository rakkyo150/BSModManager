using BSModManager.Static;
using Newtonsoft.Json;
using Octokit;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Media;
using FileMode = System.IO.FileMode;

namespace BSModManager.Models
{
    public class Config : BindableBase
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged == null) return;
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        public static Config Instance { get; set; } = new Config();

        private string bSFolderPath = @"C:\Program Files (x86)\Steam\steamapps\common\Beat Saber";
        public string BSFolderPath
        {
            get => bSFolderPath;
            set
            {
                bSFolderPath = value;
                Update();
                VeryfyConfig();
                NotifyPropertyChanged();
            }
        }

        private string gitHubToken = string.Empty;
        public string GitHubToken
        {
            get => gitHubToken;
            set
            {
                gitHubToken = value;
                Update();
                VeryfyConfig();
                NotifyPropertyChanged();
            }
        }

        private string mAExePath = string.Empty;
        public string MAExePath
        {
            get => mAExePath;
            set
            {
                mAExePath = value;
                Update();
                VeryfyConfig();
                NotifyPropertyChanged();
            }
        }

        private bool bSFolderVerification = false;
        public bool BSFolderVerification => bSFolderVerification;

        private bool gitHubTokenVerification = false;
        public bool GitHubTokenVerification => gitHubTokenVerification;

        private bool mAExeVerification = false;
        public bool MAExeVerification => mAExeVerification;

        public string BSFolderVerificationString => bSFolderVerification ? "〇" : "×";
        public Brush BSFolderVerificationColor => bSFolderVerification ? Brushes.Green : Brushes.Red;

        public string GitHubTokenVerificationString => gitHubTokenVerification ? "〇" : "×";
        public Brush GitHubTokenVerificationColor => gitHubTokenVerification ? Brushes.Green : Brushes.Red;

        public string MAExeVerificationString => mAExeVerification ? "〇" : "×";
        public Brush MAExeVerificationColor => mAExeVerification ? Brushes.Green : Brushes.Red;

        public bool BSFolderAndGitHubTokenVerification
        {
            get
            {
                if (BSFolderVerification && GitHubTokenVerification) return true;
                else return false;
            }
        }

        internal void VeryfyConfig()
        {
            bSFolderVerification = VersionExtractor.GameVersion != "---";
            mAExeVerification = MAExePath.Contains("ModAssistant.exe");
            gitHubTokenVerification = Task.Run(() => VerifyGitHubToken()).GetAwaiter().GetResult();
        }

        public async Task<bool> VerifyGitHubToken()
        {
            try
            {
                var credential = new Credentials(GitHubToken);
                GitHubClient gitHub = new GitHubClient(new ProductHeaderValue("BSModManager"))
                {
                    Credentials = credential
                };

                string owner = "rakkyo150";
                string name = "BSModManager";

                var response = await gitHub.Repository.Release.GetLatest(owner, name);
                return true;
            }
            catch (Exception ex)
            {
                Logger.Instance.Info(ex.Message);
                return false;
            }
        }

        public Dictionary<string, string> Load()
        {
            Dictionary<string, string> settingDictionary = null;

            if (File.Exists(FilePath.Instance.configFilePath))
            {
                StreamReader re = new StreamReader(FilePath.Instance.configFilePath);
                string _jsonStr = re.ReadToEnd();
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
                    {"BSFolderPath",string.Empty },
                    {"GitHubToken", string.Empty},
                    {"MAExePath", string.Empty }
                };
            }

            return settingDictionary;
        }

        public void Update()
        {
            Dictionary<string, string> settingDictionary = new Dictionary<string, string>()
            {
                {"BSFolderPath", BSFolderPath },
                {"GitHubToken", GitHubToken},
                {"MAExePath", MAExePath }
            };

            string _jsonFinish = JsonConvert.SerializeObject(settingDictionary, Formatting.Indented);

            StreamWriter wr = new StreamWriter(new FileStream(FilePath.Instance.configFilePath, FileMode.Create));
            wr.WriteLine(_jsonFinish);
            wr.Flush();
            wr.Close();
        }
    }
}
