using BSModManager.Models;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Windows.Data;

namespace BSModManager.ViewModels
{
    public class UpdateTabViewModel : BindableBase
    {
        LocalModsDataModel modsDataModel;
        
        public ObservableCollection<LocalModsDataModel.LocalModData> ModsData { get; }

        public UpdateTabViewModel(LocalModsDataModel mdm)
        {
            modsDataModel = mdm;

            ModsData = modsDataModel.LocalModsData;
            
            // https://alfort.online/689
            BindingOperations.EnableCollectionSynchronization(ModsData, new object());
        }
    }
}
