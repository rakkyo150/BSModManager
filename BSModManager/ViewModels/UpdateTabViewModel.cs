using BSModManager.Models;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Windows.Data;

namespace BSModManager.ViewModels
{
    public class UpdateTabViewModel : BindableBase
    {
        public ObservableCollection<UpdateTabPropertyModel.ModData> ModsData { get; }

        UpdateTabPropertyModel updateTabPropertyModel;

        public UpdateTabViewModel(UpdateTabPropertyModel mtpm)
        {
            updateTabPropertyModel = mtpm;

            // https://alfort.online/689
            BindingOperations.EnableCollectionSynchronization(updateTabPropertyModel.ModsData, new object());

            this.ModsData = updateTabPropertyModel.ModsData;

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
