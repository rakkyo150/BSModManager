using BSModManager.Models;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using System.Windows.Data;

namespace BSModManager.ViewModels
{
    public class UpdateTabViewModel : BindableBase
    {
        public ObservableCollection<MainTabPropertyModel.ModData> ModsData { get; }

        MainTabPropertyModel mainTabPropertyModel;

        public UpdateTabViewModel(MainTabPropertyModel mtpm)
        {
            mainTabPropertyModel = mtpm;

            // https://alfort.online/689
            BindingOperations.EnableCollectionSynchronization(mainTabPropertyModel.ModsData, new object());

            this.ModsData = mainTabPropertyModel.ModsData;

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
