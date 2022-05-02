using BSModManager.Models.ViewModelCommonProperty;
using Prism.Commands;
using Prism.Mvvm;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BSModManager.ViewModels
{
    public class UpdateMyselfConfirmViewModel : BindableBase
    {
        UpdateMyselfConfirmPropertyModel updateMyselfConfirmPropertyModel;

        public ReadOnlyReactivePropertySlim<Version> LatestMyselfVersion { get; }

        public UpdateMyselfConfirmViewModel(UpdateMyselfConfirmPropertyModel umcpm)
        {
            updateMyselfConfirmPropertyModel = umcpm;
            LatestMyselfVersion = updateMyselfConfirmPropertyModel.ObserveProperty(x => x.LatestMyselfVersion).ToReadOnlyReactivePropertySlim();
        }
    }
}
