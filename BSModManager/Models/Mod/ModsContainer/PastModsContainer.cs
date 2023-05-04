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
        internal ObservableCollection<IMod> EntityPastModsData = new ObservableCollection<IMod>();
        internal ObservableCollection<IMod> DisplayedPastModsData = new ObservableCollection<IMod>();
        private string searchWords = string.Empty;
        private List<string> Keywords = new List<string>();

        public PastModsContainer()
        {
            BindingOperations.EnableCollectionSynchronization(EntityPastModsData, new object());
            BindingOperations.EnableCollectionSynchronization(DisplayedPastModsData, new object());

            EntityPastModsData.CollectionChanged += (sender, e) =>
            {
                UpdateDisplayedPastModsData();
            };
        }

        public string SearchWords
        {
            get => searchWords;
            set
            {
                SetProperty(ref searchWords, value);
                UpdateDisplayedPastModsData();
            }
        }

        public void UpdateDisplayedPastModsData()
        {
            // searchWordを空白文字ごとに分割してkeywordsリストをクリアしてから追加する
            Keywords.Clear();
            Keywords.AddRange(searchWords.Split(' '));
            Keywords.RemoveAll(x => x == "");
            DisplayedPastModsData.Clear();

            foreach (IMod mod in EntityPastModsData)
            {
                if (Keywords.Count() == 0)
                {
                    DisplayedPastModsData.Add(mod);
                    continue;
                }

                if (ContainKeywords(mod))
                {
                    DisplayedPastModsData.Add(mod);
                }
            }
        }


        private bool ContainKeywords(IMod mod)
        {
            return Keywords.All(x => mod.Mod.ToLower().Contains(x.ToLower())
                                || mod.Url.ToLower().Contains(x.ToLower())
                                || mod.Description.ToLower().Contains(x.ToLower()));
        }

        public void AllCheckedOrUnchecked()
        {
            int i = 0;
            if (DisplayedPastModsData.Count(x => x.Checked == true) * 2 > DisplayedPastModsData.Count)
            {
                foreach (IMod _ in DisplayedPastModsData)
                {
                    DisplayedPastModsData[i].Checked = false;
                    i++;
                }
                return;
            }

            foreach (IMod _ in DisplayedPastModsData)
            {
                DisplayedPastModsData[i].Checked = true;
                i++;
            }
        }

        public void ModRepositoryOpen()
        {
            foreach (IMod a in EntityPastModsData)
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
            return EntityPastModsData.Where(x => x.Checked == true).ToList();
        }

        public void SortByName()
        {
            List<IMod> sorted = this.EntityPastModsData.OrderBy(x => x.Mod).ToList();
            this.EntityPastModsData.Clear();
            foreach (IMod item in sorted) this.EntityPastModsData.Add(item);
        }

        public void Add(IMod modData)
        {
            if (ExistsSameModNameData(modData))
            {
                Logger.Instance.Debug($"{modData.Mod}と被るデータがあるためAddはキャンセルされます");
                return;
            }

            EntityPastModsData.Add(modData);
        }

        public void UpdateInstalled(IMod modData)
        {
            if (!ExistsSameModNameData(modData))
            {
                Add(modData);
                return;
            }
            EntityPastModsData.First(x => x.Mod == modData.Mod).Installed = modData.Installed;
        }

        public void UpdateLatest(IMod modData)
        {
            if (!ExistsSameModNameData(modData))
            {
                Add(modData);
                return;
            }
            EntityPastModsData.First(x => x.Mod == modData.Mod).Latest = modData.Latest;
        }

        public void UpdateDownloadedFileHash(IMod modData)
        {
            if (!ExistsSameModNameData(modData))
            {
                Add(modData);
                return;
            }
            EntityPastModsData.First(x => x.Mod == modData.Mod).DownloadedFileHash = modData.DownloadedFileHash;
        }

        public void UpdateOriginal(IMod modData)
        {
            if (!ExistsSameModNameData(modData))
            {
                Add(modData);
                return;
            }
            EntityPastModsData.First(x => x.Mod == modData.Mod).Original = modData.Original;
        }

        public void UpdateUpdated(IMod modData)
        {
            if (!ExistsSameModNameData(modData))
            {
                Add(modData);
                return;
            }
            EntityPastModsData.First(x => x.Mod == modData.Mod).Updated = modData.Updated;
        }

        public void UpdateMA(IMod modData)
        {
            if (!ExistsSameModNameData(modData))
            {
                Add(modData);
                return;
            }
            EntityPastModsData.First(x => x.Mod == modData.Mod).MA = modData.MA;
        }

        public void UpdateDescription(IMod modData)
        {
            if (!ExistsSameModNameData(modData))
            {
                Add(modData);
                return;
            }
            EntityPastModsData.First(x => x.Mod == modData.Mod).Description = modData.Description;
        }

        public void UpdateURL(IMod modData)
        {
            if (!ExistsSameModNameData(modData))
            {
                Add(modData);
                return;
            }
            EntityPastModsData.First(x => x.Mod == modData.Mod).Url = modData.Url;
        }

        public void Remove(IMod modData)
        {
            if (!ExistsSameModNameData(modData))
            {
                Logger.Instance.Debug($"{modData.Mod}は{EntityPastModsData}に存在しないので削除できません");
                return;
            }

            EntityPastModsData.Remove(EntityPastModsData.First(x => x.Mod == modData.Mod));
        }

        public bool ExistsSameModNameData(IMod modData)
        {
            return EntityPastModsData.Any(x => x.Mod == modData.Mod);
        }

        public IEnumerable<IMod> ReturnCheckedModsData()
        {
            return EntityPastModsData.Where(x => x.Checked == true);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing) { }

            EntityPastModsData.CollectionChanged -= (sender, e) =>
            {
                UpdateDisplayedPastModsData();
            };
        }

        ~PastModsContainer()
        {
            Dispose(false);
        }
    }
}
