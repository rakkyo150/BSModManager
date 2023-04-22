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
    public class PastModsContainer : BindableBase, IModsContainer
    {
        internal ObservableCollection<IMod> PastModsData = new ObservableCollection<IMod>();

        public PastModsContainer()
        {
            BindingOperations.EnableCollectionSynchronization(PastModsData, new object());
        }

        public void AllCheckedOrUnchecked()
        {
            int i = 0;
            if (PastModsData.Count(x => x.Checked == true) * 2 > PastModsData.Count)
            {
                foreach (IMod _ in PastModsData)
                {
                    PastModsData[i].Checked = false;
                    i++;
                }
                return;
            }

            foreach (IMod _ in PastModsData)
            {
                PastModsData[i].Checked = true;
                i++;
            }
        }

        public void ModRepositoryOpen()
        {
            foreach (IMod a in PastModsData)
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

        public List<IMod> AllCheckedMod()
        {
            return PastModsData.Where(x => x.Checked == true).ToList();
        }

        public void SortByName()
        {
            List<IMod> sorted = this.PastModsData.OrderBy(x => x.Mod).ToList();
            this.PastModsData.Clear();
            foreach (IMod item in sorted) this.PastModsData.Add(item);
        }

        public void Add(IMod modData)
        {
            if (ExistsSameModNameData(modData))
            {
                Logger.Instance.Debug($"{modData.Mod}と被るデータがあるためAddはキャンセルされます");
                return;
            }

            PastModsData.Add(modData);
        }

        public void UpdateInstalled(IMod modData)
        {
            if (!ExistsSameModNameData(modData))
            {
                Add(modData);
                return;
            }
            PastModsData.First(x => x.Mod == modData.Mod).Installed = modData.Installed;
        }

        public void UpdateLatest(IMod modData)
        {
            if (!ExistsSameModNameData(modData))
            {
                Add(modData);
                return;
            }
            PastModsData.First(x => x.Mod == modData.Mod).Latest = modData.Latest;
        }

        public void UpdateDownloadedFileHash(IMod modData)
        {
            if (!ExistsSameModNameData(modData))
            {
                Add(modData);
                return;
            }
            PastModsData.First(x => x.Mod == modData.Mod).DownloadedFileHash = modData.DownloadedFileHash;
        }

        public void UpdateOriginal(IMod modData)
        {
            if (!ExistsSameModNameData(modData))
            {
                Add(modData);
                return;
            }
            PastModsData.First(x => x.Mod == modData.Mod).Original = modData.Original;
        }

        public void UpdateUpdated(IMod modData)
        {
            if (!ExistsSameModNameData(modData))
            {
                Add(modData);
                return;
            }
            PastModsData.First(x => x.Mod == modData.Mod).Updated = modData.Updated;
        }

        public void UpdateMA(IMod modData)
        {
            if (!ExistsSameModNameData(modData))
            {
                Add(modData);
                return;
            }
            PastModsData.First(x => x.Mod == modData.Mod).MA = modData.MA;
        }

        public void UpdateDescription(IMod modData)
        {
            if (!ExistsSameModNameData(modData))
            {
                Add(modData);
                return;
            }
            PastModsData.First(x => x.Mod == modData.Mod).Description = modData.Description;
        }

        public void UpdateURL(IMod modData)
        {
            if (!ExistsSameModNameData(modData))
            {
                Add(modData);
                return;
            }
            PastModsData.First(x => x.Mod == modData.Mod).Url = modData.Url;
        }

        public void Remove(IMod modData)
        {
            if (!ExistsSameModNameData(modData))
            {
                Logger.Instance.Debug($"{modData.Mod}は{PastModsData}に存在しないので削除できません");
                return;
            }

            PastModsData.Remove(PastModsData.First(x => x.Mod == modData.Mod));
        }

        public bool ExistsSameModNameData(IMod modData)
        {
            return PastModsData.Any(x => x.Mod == modData.Mod);
        }

        public IEnumerable<IMod> ReturnCheckedModsData()
        {
            return PastModsData.Where(x => x.Checked == true);
        }
    }
}
