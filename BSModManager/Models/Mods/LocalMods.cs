using BSModManager.Interfaces;
using BSModManager.Static;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;

namespace BSModManager.Models
{
    public class LocalMods : BindableBase, IMods
    {
        public ObservableCollection<IModData> LocalModsData = new ObservableCollection<IModData>();

        public void AllCheckedOrUnchecked()
        {
            int i = 0;
            if (LocalModsData.Count(x => x.Checked == true) * 2 > LocalModsData.Count)
            {
                foreach (var _ in LocalModsData)
                {
                    LocalModsData[i].Checked = false;
                    i++;
                }
                return;
            }

            foreach (var _ in LocalModsData)
            {
                LocalModsData[i].Checked = true;
                i++;
            }
        }

        public void ModRepositoryOpen()
        {
            foreach (var a in LocalModsData)
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
            return LocalModsData.Where(x => x.Checked == true).ToList();
        }

        public void SortByName()
        {
            var sorted = this.LocalModsData.OrderBy(x => x.Mod).ToList();
            this.LocalModsData.Clear();
            foreach (var item in sorted) this.LocalModsData.Add(item);
        }

        public void Add(IModData modData)
        {
            if (ExistsSameModNameData(modData))
            {
                Logger.Instance.Debug($"{modData.Mod}と被るデータがあるためAddはキャンセルされます");
                return;
            }

            LocalModsData.Add(modData);
        }

        public void UpdateInstalled(IModData modData)
        {
            if (!ExistsSameModNameData(modData))
            {
                Add(modData);
                return;
            }
            LocalModsData.First(x => x.Mod == modData.Mod).Installed = modData.Installed;
        }

        public void UpdateLatest(IModData modData)
        {
            if (!ExistsSameModNameData(modData))
            {
                Add(modData);
                return;
            }
            LocalModsData.First(x => x.Mod == modData.Mod).Latest = modData.Latest;
        }

        public void UpdateDownloadedFileHash(IModData modData)
        {
            if (!ExistsSameModNameData(modData))
            {
                Add(modData);
                return;
            }
            LocalModsData.First(x => x.Mod == modData.Mod).DownloadedFileHash = modData.DownloadedFileHash;
        }

        public void UpdateOriginal(IModData modData)
        {
            if (!ExistsSameModNameData(modData))
            {
                Add(modData);
                return;
            }
            LocalModsData.First(x => x.Mod == modData.Mod).Original = modData.Original;
        }

        public void UpdateUpdated(IModData modData)
        {
            if (!ExistsSameModNameData(modData))
            {
                Add(modData);
                return;
            }
            LocalModsData.First(x => x.Mod == modData.Mod).Updated = modData.Updated;
        }

        public void UpdateMA(IModData modData)
        {
            if (!ExistsSameModNameData(modData))
            {
                Add(modData);
                return;
            }
            LocalModsData.First(x => x.Mod == modData.Mod).MA = modData.MA;
        }

        public void UpdateDescription(IModData modData)
        {
            if (!ExistsSameModNameData(modData))
            {
                Add(modData);
                return;
            }
            LocalModsData.First(x => x.Mod == modData.Mod).Description = modData.Description;
        }

        public void UpdateURL(IModData modData)
        {
            if (!ExistsSameModNameData(modData))
            {
                Add(modData);
                return;
            }
            LocalModsData.First(x => x.Mod == modData.Mod).Url = modData.Url;
        }

        public void Remove(IModData modData)
        {
            if (!ExistsSameModNameData(modData))
            {
                Logger.Instance.Debug($"{modData.Mod}は{LocalModsData}に存在しないので削除できません");
                return;
            }

            LocalModsData.Remove(LocalModsData.First(x => x.Mod == modData.Mod));
        }

        public bool ExistsSameModNameData(IModData modData)
        {
            return LocalModsData.Any(x => x.Mod == modData.Mod);
        }

        public bool PermissionToChangeInstalledVersion(IModData modData)
        {
            if (!LocalModsData.Any(x => x.Mod == modData.Mod)) return false;

            // 初期化が必要なので
            if(LocalModsData.First(x => x.Mod == modData.Mod).DownloadedFileHash == string.Empty) return true;
            
            return !LocalModsData.Any(x => x.DownloadedFileHash == modData.DownloadedFileHash);
        }

        public IEnumerable<IModData> ReturnCheckedModsData()
        {
            return LocalModsData.Where(x => x.Checked == true);
        }
    }
}
