using BSModManager.Interfaces;
using BSModManager.Models;
using Prism.Commands;
using Prism.Mvvm;
using System.Collections.ObjectModel;

namespace BSModManager.ViewModels
{
    public class InstallTabViewModel : BindableBase
    {
        public ObservableCollection<IModData> PastModsData { get; }
        public ObservableCollection<IModData> RecommendModsData { get; }

        public DelegateCommand LoadedCommand { get; }

        private int tabIndex = 0;
        public int TabIndex
        {
            get { return tabIndex; }
            set
            {
                SetProperty(ref tabIndex, value);
            }
        }

        readonly PastMods pastModsDataModel;
        readonly RecommendMods recommendModsDataModel;

        public InstallTabViewModel(PastMods pmdm, RecommendMods rmdm)
        {
            pastModsDataModel = pmdm;
            recommendModsDataModel = rmdm;

            PastModsData = pastModsDataModel.PastModsData;
            RecommendModsData = recommendModsDataModel.RecommendModsData;
        }

        public void Install()
        {

        }
    }
}
