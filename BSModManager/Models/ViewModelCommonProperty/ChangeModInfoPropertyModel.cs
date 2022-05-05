using BSModManager.Models.CoreManager;
using BSModManager.Models.Structure;
using Octokit;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace BSModManager.Models.ViewModelCommonProperty
{
    public class ChangeModInfoPropertyModel : BindableBase
    {
        IDialogService dialogService;
        MainTabPropertyModel mainTabPropertyModel;
        MainWindowPropertyModel MainWindowPropertyModel;
        GitHubManager gitHubManager;
        InnerData innerData;

        public ChangeModInfoPropertyModel(IDialogService ds, MainTabPropertyModel mtpm,MainWindowPropertyModel mwpm,GitHubManager ghm,InnerData id)
        {
            dialogService = ds;
            mainTabPropertyModel = mtpm;
            MainWindowPropertyModel = mwpm;
            gitHubManager = ghm;
            innerData = id;
        }

        private string modName;

        private string modNameAndProgress;
        public string ModNameAndProgress
        {
            get { return modNameAndProgress; }
            set { SetProperty(ref modNameAndProgress, value); }
        }

        private string url;
        // SetterでModsDataにデータセットされます
        public string Url
        {
            get { return url; }
            set
            {
                SetProperty(ref url, value);
                mainTabPropertyModel.ModsData.First(x => x.Mod == modName).Url = Url;
            }
        }

        private string updated;
        public string Updated
        {
            get { return updated; }
            set 
            { 
                SetProperty(ref updated, value);
                mainTabPropertyModel.ModsData.First(x => x.Mod == modName).Updated = Updated;
            }
        }

        private bool original;
        // SetterでModsDataにデータセットされます
        public bool Original
        {
            get { return original; }
            set
            {
                SetProperty(ref original, value);
                if (Original)
                {
                    Console.WriteLine("test");
                    mainTabPropertyModel.ModsData.First(x => x.Mod == modName).Original = "〇";
                    if (Array.Exists(innerData.modAssistantAllMods, x=>x.name==modName))
                    {
                        DateTimeOffset now = DateTimeOffset.UtcNow;

                        ModAssistantModInformation[] a = innerData.modAssistantAllMods.Where(x => x.name == modName).ToArray();
                        ExistInMA = true;
                        Latest = new Version(a[0].version);
                        Url = a[0].link;
                        MA = "〇";
                        Description = a[0].description;

                        DateTime mAUpdatedAt = DateTime.Parse(a[0].updatedDate);
                        if ((now - mAUpdatedAt).Days >= 1)
                        {
                            Updated = (now - mAUpdatedAt).Days + "D ago";
                        }
                        else
                        {
                            Updated = (now - mAUpdatedAt).Hours + "H" + (now - mAUpdatedAt).Minutes + "m ago";
                        }
                    }
                }
                else
                {
                    mainTabPropertyModel.ModsData.First(x => x.Mod == modName).Original = "×";
                    ExistInMA = false;
                    MA = "×";
                }
            }
        }

        private string nextOrFinish = "Next";
        public string NextOrFinish
        {
            get { return nextOrFinish; }
            set { SetProperty(ref nextOrFinish, value); }
        }

        private Version latest = new Version("0.0.0");
        public Version Latest
        {
            get { return latest; }
            set 
            { 
                SetProperty(ref latest, value);
                mainTabPropertyModel.ModsData.First(x => x.Mod == modName).Latest = Latest;
            }
        }

        private string mA = "?";
        public string MA
        {
            get { return mA; }
            set
            {
                SetProperty(ref mA, value);
                mainTabPropertyModel.ModsData.First(x => x.Mod == modName).MA = MA;
            }
        }

        private bool existInMA = false;
        public bool ExistInMA
        {
            get { return existInMA; }
            set
            {
                SetProperty(ref existInMA, value);
            }
        }

        private string description = "?";
        public string Description
        {
            get { return description; }
            set 
            { 
                SetProperty(ref description, value);
                mainTabPropertyModel.ModsData.First(x => x.Mod == modName).Description = Description;
            }
        }

        private int position = 1;
        // ModsDataのうち何個目のCheckedのデータを変更するか
        // Exit時や全情報更新終了時に1に戻す
        public int Position
        {
            get { return position; }
            set { SetProperty(ref position, value); }
        }
        public void ChangeModInfo()
        {
            // 何個目のCheckedか
            int count = 0;
            int AllCheckedMod = mainTabPropertyModel.ModsData.Count(x => x.Checked == true);

            foreach (var a in mainTabPropertyModel.ModsData)
            {
                // Finishボタン押したとき
                if (Position > AllCheckedMod)
                {
                    break;
                }

                if (a.Checked)
                {
                    count++;

                    if (count == AllCheckedMod)
                    {
                        NextOrFinish = "Finish";
                    }

                    if (count == Position)
                    {
                        modName = a.Mod;

                        ModNameAndProgress = a.Mod + "(" + Position.ToString()
                        + "/" + AllCheckedMod.ToString() + ")";
                        Url = a.Url;
                        if (a.Original == "?" || a.Original == "〇")
                        {
                            Original = true;
                        }
                        else
                        {
                            Original = false;
                        }

                        Position++;

                        // ここで表示されるViewでNext/Finishボタンを押すとChangeModInfoが再帰的に呼び出される
                        // Exitの場合は再帰的な呼び出しはない
                        dialogService.ShowDialog("ChangeModInfo");
                        break;
                    }
                }
            }

            // 全情報更新終了したので
            Position = 1;
            NextOrFinish = "Next";
        }

        public void SearchMod()
        {
            try
            {
                string searchUrl = $"https://www.google.com/search?q={modName}";
                ProcessStartInfo pi = new ProcessStartInfo()
                {
                    FileName = searchUrl,
                    UseShellExecute = true,
                };
                Process.Start(pi);
            }
            catch (Exception ex)
            {
                MainWindowPropertyModel.Console = "Google検索できませんでした";
            }
        }

        public void GetModInfo()
        {
            if (ExistInMA)
            {
                return;
            }
            
            Release response  =null;
            Task.Run(() => { response = gitHubManager.GetGitHubModLatestVersionAsync(Url).Result; }).GetAwaiter().GetResult();
            if (response != null)
            {
                string releaseBody = response.Body;
                var releaseCreatedAt = response.CreatedAt;
                DateTimeOffset now = DateTimeOffset.UtcNow;

                Latest = gitHubManager.DetectVersion(response.TagName);

                if ((now - releaseCreatedAt).Days >= 1)
                {
                    Updated=(now - releaseCreatedAt).Days + "D ago";
                }
                else
                {
                    Updated=(now - releaseCreatedAt).Hours + "H" + (now - releaseCreatedAt).Minutes + "m ago";
                }
                Console.WriteLine("リリースの説明");
                Description=releaseBody;
            }
            else
            {
                Latest = new Version("0.0.0");
                Updated = "?";
                Description = "?";
            }
        }
    }
}
