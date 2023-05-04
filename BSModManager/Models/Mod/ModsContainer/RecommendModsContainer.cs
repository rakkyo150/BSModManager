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
    public class RecommendModsContainer : BindableBase, IModsContainer
    {
        public ObservableCollection<IMod> EntityRecommendModsData = new ObservableCollection<IMod>();
        internal ObservableCollection<IMod> DisplayedRecommendModsData = new ObservableCollection<IMod>();
        private string searchWords = string.Empty;
        private List<string> Keywords = new List<string>();

        public RecommendModsContainer()
        {
            BindingOperations.EnableCollectionSynchronization(EntityRecommendModsData, new object());
            EntityRecommendModsData.CollectionChanged += (sender, e) =>
            {
                UpdateDisplayedRecommendModsData();
            };
        }

        public string SearchWords
        {
            get => searchWords;
            set
            {
                SetProperty(ref searchWords, value);
                UpdateDisplayedRecommendModsData();
            }
        }

        public void UpdateDisplayedRecommendModsData()
        {
            // searchWordを空白文字ごとに分割してkeywordsリストをクリアしてから追加する
            Keywords.Clear();
            Keywords.AddRange(searchWords.Split(' '));
            Keywords.RemoveAll(x => x == "");
            DisplayedRecommendModsData.Clear();

            foreach (IMod mod in EntityRecommendModsData)
            {
                if (Keywords.Count() == 0)
                {
                    DisplayedRecommendModsData.Add(mod);
                    continue;
                }

                if (ContainKeywords(mod))
                {
                    DisplayedRecommendModsData.Add(mod);
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
            if (DisplayedRecommendModsData.Count(x => x.Checked == true) * 2 > DisplayedRecommendModsData.Count)
            {
                foreach (IMod _ in DisplayedRecommendModsData)
                {
                    DisplayedRecommendModsData[i].Checked = false;
                    i++;
                }
                return;
            }

            foreach (IMod _ in DisplayedRecommendModsData)
            {
                DisplayedRecommendModsData[i].Checked = true;
                i++;
            }
        }

        public void ModRepositoryOpen()
        {
            foreach (IMod a in EntityRecommendModsData)
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
            return EntityRecommendModsData.Where(x => x.Checked == true).ToList();
        }

        public void SortByName()
        {
            List<IMod> sorted = this.EntityRecommendModsData.OrderBy(x => x.Mod).ToList();
            this.EntityRecommendModsData.Clear();
            foreach (IMod item in sorted) this.EntityRecommendModsData.Add(item);
        }

        public void Add(IMod modData)
        {
            if (ExistsSameModNameData(modData))
            {
                Logger.Instance.Debug($"{modData.Mod}と被るデータがあるためAddはキャンセルされます");
                return;
            }

            EntityRecommendModsData.Add(modData);
        }

        public void UpdateInstalled(IMod modData)
        {
            if (!ExistsSameModNameData(modData))
            {
                Add(modData);
                return;
            }
            EntityRecommendModsData.First(x => x.Mod == modData.Mod).Installed = modData.Installed;
        }

        public void UpdateLatest(IMod modData)
        {
            if (!ExistsSameModNameData(modData))
            {
                Add(modData);
                return;
            }
            EntityRecommendModsData.First(x => x.Mod == modData.Mod).Latest = modData.Latest;
        }

        public void UpdateDownloadedFileHash(IMod modData)
        {
            if (!ExistsSameModNameData(modData))
            {
                Add(modData);
                return;
            }
            EntityRecommendModsData.First(x => x.Mod == modData.Mod).DownloadedFileHash = modData.DownloadedFileHash;
        }

        public void UpdateOriginal(IMod modData)
        {
            if (!ExistsSameModNameData(modData))
            {
                Add(modData);
                return;
            }
            EntityRecommendModsData.First(x => x.Mod == modData.Mod).Original = modData.Original;
        }

        public void UpdateUpdated(IMod modData)
        {
            if (!ExistsSameModNameData(modData))
            {
                Add(modData);
                return;
            }
            EntityRecommendModsData.First(x => x.Mod == modData.Mod).Updated = modData.Updated;
        }

        public void UpdateMA(IMod modData)
        {
            if (!ExistsSameModNameData(modData))
            {
                Add(modData);
                return;
            }
            EntityRecommendModsData.First(x => x.Mod == modData.Mod).MA = modData.MA;
        }

        public void UpdateDescription(IMod modData)
        {
            if (!ExistsSameModNameData(modData))
            {
                Add(modData);
                return;
            }
            EntityRecommendModsData.First(x => x.Mod == modData.Mod).Description = modData.Description;
        }

        public void UpdateURL(IMod modData)
        {
            if (!ExistsSameModNameData(modData))
            {
                Add(modData);
                return;
            }
            EntityRecommendModsData.First(x => x.Mod == modData.Mod).Url = modData.Url;
        }

        public void Remove(IMod modData)
        {
            if (!ExistsSameModNameData(modData))
            {
                Logger.Instance.Debug($"{modData.Mod}は{EntityRecommendModsData}に存在しないので削除できません");
                return;
            }

            EntityRecommendModsData.Remove(EntityRecommendModsData.First(x => x.Mod == modData.Mod));
        }

        public bool ExistsSameModNameData(IMod modData)
        {
            return EntityRecommendModsData.Any(x => x.Mod == modData.Mod);
        }

        public IEnumerable<IMod> ReturnCheckedModsData()
        {
            return EntityRecommendModsData.Where(x => x.Checked == true);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing) { }

            EntityRecommendModsData.CollectionChanged -= (sender, e) =>
            {
                UpdateDisplayedRecommendModsData();
            };
        }

        ~RecommendModsContainer()
        {
            Dispose(false);
        }
    }
}
