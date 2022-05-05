using BSModManager.Models.ViewModelCommonProperty;
using Prism.Mvvm;
using Prism.Navigation;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Reactive.Disposables;

namespace BSModManager.ViewModels
{
    public class UpdateMyselfConfirmViewModel : BindableBase,IDestructible
    {
        UpdateMyselfConfirmPropertyModel updateMyselfConfirmPropertyModel;

        CompositeDisposable disposables { get; } = new CompositeDisposable();
        
        public ReadOnlyReactivePropertySlim<Version> LatestMyselfVersion { get; }

        public UpdateMyselfConfirmViewModel(UpdateMyselfConfirmPropertyModel umcpm)
        {
            updateMyselfConfirmPropertyModel = umcpm;
            LatestMyselfVersion = updateMyselfConfirmPropertyModel.ObserveProperty(x => x.LatestMyselfVersion).ToReadOnlyReactivePropertySlim().AddTo(disposables);
        }

        public void Destroy()
        {
            disposables.Dispose();
        }
    }
}
