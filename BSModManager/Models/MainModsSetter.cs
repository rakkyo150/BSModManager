using BSModManager.Interfaces;
using BSModManager.Static;
using Prism.Mvvm;
using Prism.Navigation;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading.Tasks;

namespace BSModManager.Models
{
    public class MainModsSetter:BindableBase,IDestructible
    {
        private IMods mainMods;

        CompositeDisposable Disposables { get; } = new CompositeDisposable();

        internal ReactiveProperty<int> InstallTabIndex { get; set; } = new ReactiveProperty<int>(0);
        internal ReactiveProperty<bool> ChangeModInfoButtonEnable { get; set; } = new ReactiveProperty<bool>(true);

        readonly LocalMods localMods;
        readonly PastMods pastMods;
        readonly RecommendMods recommendMods;

        public MainModsSetter(LocalMods lm, PastMods pmdm, RecommendMods rmdm)
        {
            localMods = lm;
            pastMods = pmdm;
            recommendMods = rmdm;

            mainMods = localMods;

            ChangeModInfoButtonEnable.AddTo(Disposables);
            InstallTabIndex.AddTo(Disposables);
        }

        internal IMods MainMods
        {
            get { return mainMods; }
        }

        internal void SetLocalMods()
        {
            mainMods = localMods;
            mainMods.SortByName();
        }

        internal void SetPastMods()
        {
            mainMods = pastMods;
            mainMods.SortByName();
        }

        internal void SetRecommendMods()
        {
            mainMods = recommendMods;
            mainMods.SortByName();
        }

        public void Destroy()
        {
            Disposables.Dispose();
        }
    }
}
