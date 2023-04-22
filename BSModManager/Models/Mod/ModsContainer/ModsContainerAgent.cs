using BSModManager.Interfaces;
using Prism.Mvvm;
using Prism.Navigation;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System.Reactive.Disposables;
using System.Security.Cryptography.X509Certificates;

namespace BSModManager.Models
{
    public class ModsContainerAgent : BindableBase, IDestructible
    {
        private IModsContainer activeMods;
        private readonly LocalModsContainer localMods;
        private readonly PastModsContainer pastMods;
        private readonly RecommendModsContainer recommendMods;

        private CompositeDisposable Disposables { get; } = new CompositeDisposable();

        internal ReactiveProperty<int> InstallTabIndex { get; set; } = new ReactiveProperty<int>(0);
        internal ReactiveProperty<bool> ChangeModInfoButtonEnable { get; set; } = new ReactiveProperty<bool>(true);

        public ModsContainerAgent(LocalModsContainer lm, PastModsContainer pmdm, RecommendModsContainer rmdm)
        {
            localMods = lm;
            pastMods = pmdm;
            recommendMods = rmdm;

            activeMods = localMods;
        }

        internal IModsContainer ActiveMods
        {
            get { return activeMods; }
        }

        internal LocalModsContainer LocalModsContainer { get { return localMods; } }
        internal PastModsContainer PastModsContainer { get { return pastMods; } }
        internal RecommendModsContainer RecommendModsContainer { get { return recommendMods; } }

        internal void ActivateLocalModsContainer()
        {
            activeMods = localMods;
            activeMods.SortByName();
        }

        internal void ActivatePastModsContainer()
        {
            activeMods = pastMods;
            activeMods.SortByName();
        }

        internal void ActivateRecommendModsContainer()
        {
            activeMods = recommendMods;
            activeMods.SortByName();
        }

        public void Destroy()
        {
            Disposables.Dispose();
        }
    }
}
