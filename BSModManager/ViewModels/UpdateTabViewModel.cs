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

            /*
            mainTabPropertyModel.ModsData.Add(new MainTabPropertyModel.ModData()
            {
                Mod = "TestMod",
                Installed = new Version("1.0.0"),
                Latest = new Version("1.0.0"),
                Original = "〇",
                MA = "×",
                Description = "Test"
            });
            mainTabPropertyModel.ModsData.Add(new MainTabPropertyModel.ModData()
            {
                Mod = "TestMod2",
                Installed = new Version("1.0.0"),
                Latest = new Version("1.0.0"),
                Original = "〇",
                MA = "×",
                Description = "Test2"
            });
            */
        }
    }
}
