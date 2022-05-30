using BSModManager.Static;
using Prism.Mvvm;
using System.Threading.Tasks;

namespace BSModManager.Models
{
    public class SettingsVerifier : BindableBase
    {
        private bool bSFolder = false;
        public bool BSFolder
        {
            get { return bSFolder; }
            set { SetProperty(ref bSFolder, value); }
        }

        private bool gitHubToken = false;
        public bool GitHubToken
        {
            get { return gitHubToken; }
            set { SetProperty(ref gitHubToken, value); }
        }

        private bool mAExe = false;
        public bool MAExe
        {
            get { return mAExe; }
            set { SetProperty(ref mAExe, value); }
        }

        private bool bSFolderAndGitHubToken = false;
        public bool BSFolderAndGitHubToken
        {
            get { return bSFolderAndGitHubToken; }
            set { SetProperty(ref bSFolderAndGitHubToken, value); }
        }

        readonly GitHubApi gitHubApi;

        public SettingsVerifier(GitHubApi gha)
        {
            gitHubApi = gha;

            this.PropertyChanged += (sender, e) =>
            {
                if (BSFolder && GitHubToken) BSFolderAndGitHubToken = true;
                else BSFolderAndGitHubToken = false;
            };

            BSFolder = GameVersion.Version != "---";
            MAExe = FilePath.Instance.MAExePath.Contains("ModAssistant.exe");

            gitHubApi.PropertyChanged += (sender, e) =>
            {
                // https://nryblog.work/call-sync-to-async-method/
                Task.Run(async () => { GitHubToken = await gitHubApi.VerifyGitHubToken(); }).GetAwaiter().GetResult();
            };

            Folder.Instance.PropertyChanged += (sender, e) =>
            {
                BSFolder = GameVersion.Version != "---";
            };

            FilePath.Instance.PropertyChanged += (sender, e) =>
            {
                MAExe = FilePath.Instance.MAExePath.Contains("ModAssistant.exe");
            };
        }
    }
}
