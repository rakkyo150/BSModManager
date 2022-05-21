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
        MyselfUpdater updater;

        CompositeDisposable disposables { get; } = new CompositeDisposable();

        public ReadOnlyReactivePropertySlim<Version> LatestMyselfVersion { get; }

        public UpdateMyselfConfirmViewModel(MyselfUpdater u)
        {
            updater = u;
            LatestMyselfVersion = updater.ObserveProperty(x => x.LatestMyselfVersion).ToReadOnlyReactivePropertySlim().AddTo(disposables);
        }

        public void Destroy()
        {
            disposables.Dispose();
        }
    }
}
