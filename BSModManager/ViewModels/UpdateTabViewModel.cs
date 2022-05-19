using BSModManager.Models;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Windows.Data;

namespace BSModManager.ViewModels
{
    public class UpdateTabViewModel : BindableBase
    {
        ModsDataModel modsDataModel;
        
        public ObservableCollection<ModsDataModel.ModData> ModsData { get; }

        public UpdateTabViewModel(ModsDataModel mdm)
        {
            modsDataModel = mdm;

            ModsData = modsDataModel.ModsData;
            
            // https://alfort.online/689
            BindingOperations.EnableCollectionSynchronization(ModsData, new object());
        }
    }
}
