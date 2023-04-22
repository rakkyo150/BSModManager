using BSModManager.Interfaces;
using BSModManager.Models;
using Prism.Commands;
using Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.Collections.ObjectModel;

namespace BSModManager.ViewModels
{
    public class InstallTabViewModel : BindableBase
    {
        public ObservableCollection<IMod> PastModsContainer { get; }
        public ObservableCollection<IMod> RecommendModsContainer { get; }

        internal ReactiveProperty<int> InstallTabIndex { get; set; } = new ReactiveProperty<int>(0);
        internal ReactiveProperty<bool> ChangeModInfoButtonEnable { get; set; } = new ReactiveProperty<bool>(true);

        public DelegateCommand LoadedCommand { get; }

        private readonly ModsContainerAgent modsDataContainerAgent;

        public InstallTabViewModel(ModsContainerAgent mdca)
        {
            modsDataContainerAgent = mdca;

            PastModsContainer = modsDataContainerAgent.PastModsContainer.PastModsData;
            RecommendModsContainer = modsDataContainerAgent.RecommendModsContainer.RecommendModsData;

            InstallTabIndex = this.ObserveProperty(x => x.TabIndex).ToReactiveProperty();
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
                    modsDataContainerAgent.ActivatePastModsContainer();
                    ChangeModInfoButtonEnable.Value = true;
                }
                else
                {
                    modsDataContainerAgent.ActivateRecommendModsContainer();
                    ChangeModInfoButtonEnable.Value = false;
                }
            }
        }
    }
}
