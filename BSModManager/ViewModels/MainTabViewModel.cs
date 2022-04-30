using Prism.Mvvm;
using System;
using System.Collections.ObjectModel;
using System.Windows.Media;

namespace ModManager.ViewModels
{
    public class MainTabViewModel : BindableBase
    {
        public MainTabViewModel()
        {
            ModsData.Add(new ModData()
            {
                Mod = "TestMod",
                Installed = new Version("1.0.0"),
                Latest = new Version("1.0.0"),
                Original = "〇",
                MA = "×",
                Description = "Test"
            });
            ModsData.Add(new ModData()
            {
                Mod = "TestMod2",
                Installed = new Version("1.0.0"),
                Latest = new Version("1.0.0"),
                Original = "〇",
                MA = "×",
                Description = "Test2"
            });
        }

        public ObservableCollection<ModData> ModsData
        {
            get { return modsData; }
            set { SetProperty(ref modsData, value); }
        }
        
        private ObservableCollection<ModData> modsData = new ObservableCollection<ModData>();

        public class ModData
        {
            public bool Checked { get; set; } = false;
            public string Mod { get; set; } = "";
            public Version Installed { get; set; } = new Version("0.0.0");
            public Version Latest { get; set; } = new Version("0.0.0");
            public string Original { get; set; } = "?";
            public string MA { get; set; } = "False";
            public string Description { get; set; } = "?";
            public Brush InstalledColor { get; set; } = Brushes.Green;
            public Brush LatestColor { get; set; } = Brushes.Red;
        }


    }
}
