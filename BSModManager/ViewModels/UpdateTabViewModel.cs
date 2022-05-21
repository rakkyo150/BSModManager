﻿using BSModManager.Models;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Windows.Data;

namespace BSModManager.ViewModels
{
    public class UpdateTabViewModel : BindableBase
    {
        LocalMods modsDataModel;

        public ObservableCollection<LocalMods.LocalModData> ModsData { get; }

        public UpdateTabViewModel(LocalMods mdm)
        {
            modsDataModel = mdm;

            ModsData = modsDataModel.LocalModsData;

            // https://alfort.online/689
            BindingOperations.EnableCollectionSynchronization(ModsData, new object());
        }
    }
}
