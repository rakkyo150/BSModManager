using BSModManager.Interfaces;
using BSModManager.Static;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows.Data;

namespace BSModManager.Models
{
    public class RecommendMods : BindableBase, IMods
    {
        public ObservableCollection<IModData> RecommendModsData = new ObservableCollection<IModData>();

        public RecommendMods()
        {
            BindingOperations.EnableCollectionSynchronization(RecommendModsData, new object());
        }

        public void AllCheckedOrUnchecked()
        {
            int i = 0;
            if (RecommendModsData.Count(x => x.Checked == true) * 2 > RecommendModsData.Count)
            {
                foreach (var _ in RecommendModsData)
                {
                    RecommendModsData[i].Checked = false;
                    i++;
                }
                return;
            }

            foreach (var _ in RecommendModsData)
            {
                RecommendModsData[i].Checked = true;
                i++;
            }
        }

        public void ModRepositoryOpen()
        {
            foreach (var a in RecommendModsData)
            {
                if (!a.Checked) continue;

                try
                {
                    string searchUrl = a.Url;
                    ProcessStartInfo pi = new ProcessStartInfo()
                    {
                        FileName = searchUrl,
                        UseShellExecute = true,
                    };
                    Process.Start(pi);
                }
                catch (Exception ex)
                {
                    Logger.Instance.Error($"{ex.Message}\n{a.Mod}のURL : \"{a.Url}\"を開けませんでした");
                }
            }
        }

        public List<IModData> AllCheckedMod()
        {
            return RecommendModsData.Where(x => x.Checked == true).ToList();
        }

        public void SortByName()
        {
            var sorted = this.RecommendModsData.OrderBy(x => x.Mod).ToList();
            this.RecommendModsData.Clear();
            foreach (var item in sorted) this.RecommendModsData.Add(item);
        }

        public void Add(IModData modData)
        {
            if (ExistsSameModNameData(modData))
            {
                Logger.Instance.Debug($"{modData.Mod}と被るデータがあるためAddはキャンセルされます");
                return;
            }

            RecommendModsData.Add(modData);
        }

        public void UpdateInstalled(IModData modData)
        {
            if (!ExistsSameModNameData(modData))
            {
                Add(modData);
                return;
            }
            RecommendModsData.First(x => x.Mod == modData.Mod).Installed = modData.Installed;
        }

        public void UpdateLatest(IModData modData)
        {
            if (!ExistsSameModNameData(modData))
            {
                Add(modData);
                return;
            }
            RecommendModsData.First(x => x.Mod == modData.Mod).Latest = modData.Latest;
        }

        public void UpdateDownloadedFileHash(IModData modData)
        {
            if (!ExistsSameModNameData(modData))
            {
                Add(modData);
                return;
            }
            RecommendModsData.First(x => x.Mod == modData.Mod).DownloadedFileHash = modData.DownloadedFileHash;
        }

        public void UpdateOriginal(IModData modData)
        {
            if (!ExistsSameModNameData(modData))
            {
                Add(modData);
                return;
            }
            RecommendModsData.First(x => x.Mod == modData.Mod).Original = modData.Original;
        }

        public void UpdateUpdated(IModData modData)
        {
            if (!ExistsSameModNameData(modData))
            {
                Add(modData);
                return;
            }
            RecommendModsData.First(x => x.Mod == modData.Mod).Updated = modData.Updated;
        }

        public void UpdateMA(IModData modData)
        {
            if (!ExistsSameModNameData(modData))
            {
                Add(modData);
                return;
            }
            RecommendModsData.First(x => x.Mod == modData.Mod).MA = modData.MA;
        }

        public void UpdateDescription(IModData modData)
        {
            if (!ExistsSameModNameData(modData))
            {
                Add(modData);
                return;
            }
            RecommendModsData.First(x => x.Mod == modData.Mod).Description = modData.Description;
        }

        public void UpdateURL(IModData modData)
        {
            if (!ExistsSameModNameData(modData))
            {
                Add(modData);
                return;
            }
            RecommendModsData.First(x => x.Mod == modData.Mod).Url = modData.Url;
        }

        public void Remove(IModData modData)
        {
            if (!ExistsSameModNameData(modData))
            {
                Logger.Instance.Debug($"{modData.Mod}は{RecommendModsData}に存在しないので削除できません");
                return;
            }

            RecommendModsData.Remove(RecommendModsData.First(x => x.Mod == modData.Mod));
        }

        public bool ExistsSameModNameData(IModData modData)
        {
            return RecommendModsData.Any(x => x.Mod == modData.Mod);
        }

        public IEnumerable<IModData> ReturnCheckedModsData()
        {
            return RecommendModsData.Where(x => x.Checked == true);
        }
    }
}
