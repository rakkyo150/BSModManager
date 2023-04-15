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
using static BSModManager.Models.MA;

namespace BSModManager.Models
{
    public class ModDataChanger : BindableBase
    {
        readonly IDialogService dialogService;
        readonly GitHubApi gitHubManager;
        readonly MA mAMod;
        readonly Refresher refresher;
        readonly ModsContainerAgent mainModsChanger;

        private List<IMod> AllCheckedMod = new List<IMod>();
        private int AllCheckedModCount = int.MinValue;

        public ModDataChanger(IDialogService ds, GitHubApi ghm, MA mam, Refresher r, ModsContainerAgent mmc)
        {
            dialogService = ds;
            gitHubManager = ghm;
            mAMod = mam;
            refresher = r;
            mainModsChanger = mmc;
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
                mainModsChanger.ActiveMods.UpdateURL(new LocalMod()
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
                mainModsChanger.ActiveMods.UpdateUpdated(new LocalMod()
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
                    mainModsChanger.ActiveMods.UpdateOriginal(new LocalMod()
                    {
                        Mod = modName,
                        Original = "〇"
                    });
                    SetInfoForMA();
                }
                else
                {
                    mainModsChanger.ActiveMods.UpdateOriginal(new LocalMod()
                    {
                        Mod = modName,
                        Original = "×"
                    });
                    ExistInMA = false;
                    MA = "×";
                }
            }
        }

        private bool isBackButtonEnable = false;
        public bool IsBackButtonEnable
        {
            get { return isBackButtonEnable; }
            set { SetProperty(ref isBackButtonEnable, value); }
        }

        private string nextOrFinishButtonText = "Next";
        public string NextOrFinishButtonText
        {
            get { return nextOrFinishButtonText; }
            set { SetProperty(ref nextOrFinishButtonText, value); }
        }

        private Version latest = new Version("0.0.0");
        public Version Latest
        {
            get { return latest; }
            set
            {
                SetProperty(ref latest, value);
                mainModsChanger.ActiveMods.UpdateLatest(new LocalMod()
                {
                    Mod = modName,
                    Latest = value
                });
            }
        }

        internal void ChangeIsUrlTextBoxReadOnly()
        {
            if (!Original)
            {
                ExistInMA = false;
                return;
            }

            if (!mAMod.ExistsData(modName))
            {
                ExistInMA = false;
                return;
            }

            ExistInMA = true;
        }

        private string mA = "?";
        public string MA
        {
            get { return mA; }
            set
            {
                SetProperty(ref mA, value);
                mainModsChanger.ActiveMods.UpdateMA(new LocalMod()
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
                mainModsChanger.ActiveMods.UpdateDescription(new LocalMod()
                {
                    Mod = modName,
                    Description = value
                });
            }
        }

        private int nowChangingCheckedIndex = 0;
        // ModsDataのうち何個目のCheckedのデータを変更するか
        // Exit時や全情報更新終了時に1に戻す
        public int NowChangingCheckedIndex
        {
            get { return nowChangingCheckedIndex; }
            set
            {
                SetProperty(ref nowChangingCheckedIndex, value);
                if (value >= 1) IsBackButtonEnable = true;
                else IsBackButtonEnable = false;
            }
        }

        public void ShowChangeModInfoPreviousDialog()
        {
            NowChangingCheckedIndex -= 1;

            if (NowChangingCheckedIndex + 1 > AllCheckedModCount)
            {
                Logger.Instance.Error("[バグ]選択されているModの範囲を超えるロジックです");
                return;
            }

            if (NowChangingCheckedIndex < 0)
            {
                Logger.Instance.Error("[バグ]NextCheckedIndexは負の値をとることはできません");
                return;
            }

            SetDialogInitialInfo();
            dialogService.ShowDialog("ChangeModInfo");
        }

        public void ShowChangeModInfoInitialDialog()
        {
            AllCheckedMod = mainModsChanger.ActiveMods.AllCheckedMod();
            AllCheckedModCount = AllCheckedMod.Count();

            if (AllCheckedModCount == 0)
            {
                Logger.Instance.Info("Modが選択されていません");
                return;
            }

            NowChangingCheckedIndex = 0;

            if (NowChangingCheckedIndex + 1 > AllCheckedModCount)
            {
                Logger.Instance.Error("[バグ]選択されているModの範囲を超えるロジックです");
                return;
            }

            if (NowChangingCheckedIndex < 0)
            {
                Logger.Instance.Error("[バグ]NextCheckedIndexは負の値をとることはできません");
                return;
            }

            SetDialogInitialInfo();
            dialogService.ShowDialog("ChangeModInfo");
        }

        public void ShowChangeModInfoNextDialog()
        {
            NowChangingCheckedIndex += 1;

            if (NowChangingCheckedIndex + 1 > AllCheckedModCount)
            {
                Logger.Instance.Error("[バグ]選択されているModの範囲を超えるロジックです");
                return;
            }

            if (NowChangingCheckedIndex < 0)
            {
                Logger.Instance.Error("[バグ]NextCheckedIndexは負の値をとることはできません");
                return;
            }

            SetDialogInitialInfo();
            dialogService.ShowDialog("ChangeModInfo");
        }

        private void SetDialogInitialInfo()
        {
            if (NowChangingCheckedIndex + 1 == AllCheckedModCount) NextOrFinishButtonText = "Finish";
            else NextOrFinishButtonText = "Next";

            IMod checkedMod = AllCheckedMod[NowChangingCheckedIndex];

            modName = checkedMod.Mod;

            ModNameAndProgress = checkedMod.Mod + "(" + (NowChangingCheckedIndex + 1).ToString()
            + "/" + AllCheckedModCount.ToString() + ")";
            Url = checkedMod.Url;
            if (checkedMod.Original == "?" || checkedMod.Original == "〇")
            {
                Original = true;
            }
            else
            {
                Original = false;
            }
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

        public void SetInfoToMods()
        {
            if (ExistInMA) return;

            Release response = null;
            Task.Run(() => { response = gitHubManager.GetLatestReleaseInfoAsync(Url).Result; }).GetAwaiter().GetResult();
            if (response != null)
            {
                string releaseBody = response.Body;
                var releaseCreatedAt = response.CreatedAt;
                DateTimeOffset now = DateTimeOffset.UtcNow;

                Latest = VersionExtractor.DetectVersionFromRawVersion(response.TagName);

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
                Updated = Url == string.Empty ? "?" : "---";
                Description = Url == string.Empty ? "?" : "---";
            }
        }

        private void SetInfoForMA()
        {
            if (!mAMod.ExistsData(modName)) return;

            DateTimeOffset now = DateTimeOffset.UtcNow;

            MAMod[] a = mAMod.ModAssistantAllMods.Where(x => x.name == modName).ToArray();
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
