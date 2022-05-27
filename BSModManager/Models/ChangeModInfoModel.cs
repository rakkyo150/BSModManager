using BSModManager.Interfaces;
using BSModManager.Models.Mods.Structures;
using BSModManager.Static;
using Octokit;
using Prism.Mvvm;
using Prism.Services.Dialogs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using static BSModManager.Models.MAMods;

namespace BSModManager.Models
{
    public class ChangeModInfoModel : BindableBase
    {
        readonly IDialogService dialogService;
        readonly GitHubApi gitHubManager;
        readonly MAMods mAMod;
        readonly Refresher refresher;

        private IMods iMods;

        public ChangeModInfoModel(IDialogService ds, LocalMods im, GitHubApi ghm, MAMods mam, Refresher r)
        {
            dialogService = ds;
            iMods = im;
            gitHubManager = ghm;
            mAMod = mam;
            refresher = r;
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
                iMods.UpdateURL(new LocalModData(refresher)
                {
                    Mod = modName,
                    Url = value
                });
            }
        }

        private string updated;
        public string Updated
        {
            get { return updated; }
            set
            {
                SetProperty(ref updated, value);
                iMods.UpdateUpdated(new LocalModData(refresher)
                {
                    Mod = modName,
                    Updated = value
                });
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
                    iMods.UpdateOriginal(new LocalModData(refresher)
                    {
                        Mod = modName,
                        Original = "〇"
                    });
                    SetInfoForMA();
                }
                else
                {
                    iMods.UpdateOriginal(new LocalModData(refresher)
                    {
                        Mod = modName,
                        Original = "×"
                    });
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
                iMods.UpdateLatest(new LocalModData(refresher)
                {
                    Mod = modName,
                    Latest = value
                });
            }
        }

        private string mA = "?";
        public string MA
        {
            get { return mA; }
            set
            {
                SetProperty(ref mA, value);
                iMods.UpdateMA(new LocalModData(refresher)
                {
                    Mod = modName,
                    MA = value
                });
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
                iMods.UpdateDescription(new LocalModData(refresher)
                {
                    Mod = modName,
                    Description = value
                });
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

        public void ChangeIMod(IMods mods)
        {
            iMods = mods;
        }
        
        public void ChangeInfo()
        {
            // 何個目のCheckedか
            int count = 0;
            List<IModData> AllCheckedMod = iMods.AllCheckedMod();
            int AllChekedModCount = AllCheckedMod.Count();

            foreach (var checkedMod in AllCheckedMod)
            {
                // Finishボタン押したとき
                if (Position > AllChekedModCount)
                {
                    break;
                }

                count++;

                if (count == AllChekedModCount)
                {
                    NextOrFinish = "Finish";
                }

                if (count == Position)
                {
                    modName = checkedMod.Mod;

                    ModNameAndProgress = checkedMod.Mod + "(" + Position.ToString()
                    + "/" + AllChekedModCount.ToString() + ")";
                    Url = checkedMod.Url;
                    if (checkedMod.Original == "?" || checkedMod.Original == "〇")
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

            // 全情報更新終了したので
            Position = 1;
            NextOrFinish = "Next";
        }

        public void Search()
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
                Logger.Instance.Error(ex.Message + "\nGoogle検索できませんでした");
            }
        }

        public void GetInfo()
        {
            if (ExistInMA)  return;

            Release response = null;
            Task.Run(() => { response = gitHubManager.GetLatestReleaseAsync(Url).Result; }).GetAwaiter().GetResult();
            if (response != null)
            {
                string releaseBody = response.Body;
                var releaseCreatedAt = response.CreatedAt;
                DateTimeOffset now = DateTimeOffset.UtcNow;

                Latest = gitHubManager.DetectVersionFromTagName(response.TagName);

                if ((now - releaseCreatedAt).Days >= 1)
                {
                    Updated = (now - releaseCreatedAt).Days + "D ago";
                }
                else
                {
                    Updated = (now - releaseCreatedAt).Hours + "H" + (now - releaseCreatedAt).Minutes + "m ago";
                }
                Description = releaseBody;
            }
            else
            {
                Latest = new Version("0.0.0");
                Updated = Url == "" ? "?" : "---";
                Description = Url == "" ? "?" : "---";
            }
        }

        private void SetInfoForMA()
        {
            if (!mAMod.ExistsData(new MAModData() { name=modName})) return;

            DateTimeOffset now = DateTimeOffset.UtcNow;

            MAModData[] a = mAMod.ModAssistantAllMods.Where(x => x.name == modName).ToArray();
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
}
