using BSModManager.Models;
using Prism.Commands;
using Prism.Mvvm;
using System.Collections.ObjectModel;

namespace BSModManager.ViewModels
{
    public class InstallTabViewModel : BindableBase
    {
        public ObservableCollection<PastMods.PastModData> PastModsData { get; }
        public ObservableCollection<RecommendMods.RecommendModData> RecommendModsData { get; }

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

        PastMods pastModsDataModel;
        RecommendMods recommendModsDataModel;

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
