using BSModManager.Models;
using Prism.Mvvm;
using Prism.Navigation;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Reactive.Disposables;

namespace BSModManager.ViewModels
{
    public class UpdateMyselfConfirmViewModel : BindableBase, IDestructible
    {
        readonly MyselfUpdater updater;

        CompositeDisposable Disposables { get; } = new CompositeDisposable();

        public ReadOnlyReactivePropertySlim<Version> LatestMyselfVersion { get; }

        public UpdateMyselfConfirmViewModel(MyselfUpdater u)
        {
            updater = u;
            LatestMyselfVersion = updater.ObserveProperty(x => x.LatestMyselfVersion).ToReadOnlyReactivePropertySlim().AddTo(Disposables);
        }

        public void Destroy()
        {
            Disposables.Dispose();
        }
    }
}
