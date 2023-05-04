using BSModManager.Interfaces;
using BSModManager.Models;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Navigation;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;

namespace BSModManager.ViewModels
{
    public class InstallTabViewModel : BindableBase, IDestructible
    {
        public ObservableCollection<IMod> PastModsContainer { get; }
        public ObservableCollection<IMod> RecommendModsContainer { get; }

        private CompositeDisposable disposables { get; } = new CompositeDisposable();

        internal ReactiveProperty<int> InstallTabIndex { get; set; } = new ReactiveProperty<int>(0);
        internal ReactiveProperty<bool> ChangeModInfoButtonEnable { get; set; } = new ReactiveProperty<bool>(true);
        public ReactiveProperty<string> PastSearchWords { get; } = new ReactiveProperty<string>("");
        public ReactiveProperty<string> RecommendSearchWords { get; } = new ReactiveProperty<string>("");

        public DelegateCommand LoadedCommand { get; }

        private readonly ModsContainerAgent modsDataContainerAgent;

        public InstallTabViewModel(ModsContainerAgent mdca)
        {
            modsDataContainerAgent = mdca;

            PastModsContainer = modsDataContainerAgent.PastModsContainer.DisplayedPastModsData;
            RecommendModsContainer = modsDataContainerAgent.RecommendModsContainer.DisplayedRecommendModsData;

            PastSearchWords = modsDataContainerAgent.PastModsContainer.ToReactivePropertyAsSynchronized(x => x.SearchWords).AddTo(disposables);
            RecommendSearchWords = modsDataContainerAgent.RecommendModsContainer.ToReactivePropertyAsSynchronized(x => x.SearchWords).AddTo(disposables);

            InstallTabIndex = this.ObserveProperty(x => x.TabIndex).ToReactiveProperty().AddTo(disposables);
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

        public void Destroy()
        {
            disposables.Dispose();
        }
    }
}
