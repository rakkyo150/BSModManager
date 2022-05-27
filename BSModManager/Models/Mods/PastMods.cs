using BSModManager.Interfaces;
using BSModManager.Static;
using Prism.Mvvm;
using Prism.Navigation;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace BSModManager.Models
{
    public class PastMods : BindableBase, IMods
    {
        internal ObservableCollection<IModData> PastModsData = new ObservableCollection<IModData>();
        
        public PastMods()
        {
            BindingOperations.EnableCollectionSynchronization(PastModsData, new object());
        }

        public void AllCheckedOrUnchecked()
        {
            int i = 0;
            if (PastModsData.Count(x => x.Checked == true) * 2 > PastModsData.Count)
            {
                foreach (var _ in PastModsData)
                {
                    PastModsData[i].Checked = false;
                    i++;
                }
                return;
            }

            foreach (var _ in PastModsData)
            {
                PastModsData[i].Checked = true;
                i++;
            }
        }

        public void ModRepositoryOpen()
        {
            foreach (var a in PastModsData)
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
            return PastModsData.Where(x => x.Checked == true).ToList();
        }

        public void SortByName()
        {
            var sorted = this.PastModsData.OrderBy(x => x.Mod).ToList();
            this.PastModsData.Clear();
            foreach (var item in sorted) this.PastModsData.Add(item);
        }

        public void Add(IModData modData)
        {
            if (ExistsSameModData(modData))
            {
                Logger.Instance.Debug($"{modData.Mod}と被るデータがあるためAddはキャンセルされます");
                return;
            }

            PastModsData.Add(modData);
        }

        public void UpdateInstalled(IModData modData)
        {
            if (!ExistsSameModData(modData))
            {
                Add(modData);
                return;
            }
            PastModsData.First(x => x.Mod == modData.Mod).Installed = modData.Installed;
        }

        public void UpdateLatest(IModData modData)
        {
            if (!ExistsSameModData(modData))
            {
                Add(modData);
                return;
            }
            PastModsData.First(x => x.Mod == modData.Mod).Latest = modData.Latest;
        }

        public void UpdateOriginal(IModData modData)
        {
            if (!ExistsSameModData(modData))
            {
                Add(modData);
                return;
            }
            PastModsData.First(x => x.Mod == modData.Mod).Original = modData.Original;
        }

        public void UpdateUpdated(IModData modData)
        {
            if (!ExistsSameModData(modData))
            {
                Add(modData);
                return;
            }
            PastModsData.First(x => x.Mod == modData.Mod).Updated = modData.Updated;
        }

        public void UpdateMA(IModData modData)
        {
            if (!ExistsSameModData(modData))
            {
                Add(modData);
                return;
            }
            PastModsData.First(x => x.Mod == modData.Mod).MA = modData.MA;
        }

        public void UpdateDescription(IModData modData)
        {
            if (!ExistsSameModData(modData))
            {
                Add(modData);
                return;
            }
            PastModsData.First(x => x.Mod == modData.Mod).Description = modData.Description;
        }

        public void UpdateURL(IModData modData)
        {
            if (!ExistsSameModData(modData))
            {
                Add(modData);
                return;
            }
            PastModsData.First(x => x.Mod == modData.Mod).Url = modData.Url;
        }

        public void Remove(IModData modData)
        {
            if (!ExistsSameModData(modData))
            {
                Logger.Instance.Debug($"{modData.Mod}は{PastModsData}に存在しないので削除できません");
                return;
            }

            PastModsData.Remove(PastModsData.First(x => x.Mod == modData.Mod));
        }

        internal bool ExistsSameModData(IModData modData)
        {
            return PastModsData.Any(x => x.Mod == modData.Mod);
        }

        public IEnumerable<IModData> ReturnCheckedModsData()
        {
            return PastModsData.Where(x => x.Checked == true);
        }
    }
}
