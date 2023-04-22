using BSModManager.Interfaces;
using BSModManager.Models;
using Prism.Mvvm;
using System.Collections.ObjectModel;
using System.Windows.Data;

namespace BSModManager.ViewModels
{
    public class UpdateTabViewModel : BindableBase
    {
        private readonly ModsContainerAgent modsDataContainerAgent;

        public ObservableCollection<IMod> LocalModsContainer { get; }

        public UpdateTabViewModel(ModsContainerAgent mdca)
        {
            modsDataContainerAgent = mdca;

            LocalModsContainer = modsDataContainerAgent.LocalModsContainer.LocalModsData;

            // https://alfort.online/689
            BindingOperations.EnableCollectionSynchronization(LocalModsContainer, new object());
        }
    }
}
