using BSModManager.Interfaces;
using BSModManager.Models;
using BSModManager.Static;
using Prism.Commands;
using Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.Collections.ObjectModel;

namespace BSModManager.ViewModels
{
    public class InstallTabViewModel : BindableBase
    {
        public ObservableCollection<IModData> PastModsData { get; }
        public ObservableCollection<IModData> RecommendModsData { get; }

        public DelegateCommand LoadedCommand { get; }

        readonly MainModsSetter mainModsChanger;

        public InstallTabViewModel(MainModsSetter mmc, PastMods pmdm, RecommendMods rmdm)
        {
            pastModsDataModel = pmdm;
            recommendModsDataModel = rmdm;
            mainModsChanger = mmc;

            PastModsData = pastModsDataModel.PastModsData;
            RecommendModsData = recommendModsDataModel.RecommendModsData;

            mainModsChanger.InstallTabIndex = this.ObserveProperty(x => x.TabIndex).ToReactiveProperty();
        }

        private int tabIndex = 0;
        public int TabIndex
        {
            get { return tabIndex; }
            set
            {
                SetProperty(ref tabIndex, value);
                if (value == 0)
                {
                    mainModsChanger.SetPastMods();
                    mainModsChanger.ChangeModInfoButtonEnable.Value = true;
                }
                else
                {
                    mainModsChanger.SetRecommendMods();
                    mainModsChanger.ChangeModInfoButtonEnable.Value = false;
                }
            }
        }

        readonly PastMods pastModsDataModel;
        readonly RecommendMods recommendModsDataModel;

        public void Install()
        {

        }
    }
}
