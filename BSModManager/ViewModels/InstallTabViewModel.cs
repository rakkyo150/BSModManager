using BSModManager.Models;
using BSModManager.Models.CoreManager;
using BSModManager.Models.Structure;
using BSModManager.Static;
using Octokit;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BSModManager.ViewModels
{
    public class InstallTabViewModel : BindableBase
    {
        public ObservableCollection<PastModsDataModel.PastModData> PastModsData { get; }
        public ObservableCollection<RecommendModsDataModel.RecommendModData> RecommendModsData { get; }

        public DelegateCommand LoadedCommand { get; }

        private int tabIndex=0;
        public int TabIndex 
        {
            get { return tabIndex; }
            set
            {
                SetProperty(ref tabIndex, value);
            }
        }

        PastModsDataModel pastModsDataModel;
        RecommendModsDataModel recommendModsDataModel;

        public InstallTabViewModel(PastModsDataModel pmdm,RecommendModsDataModel rmdm)
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
