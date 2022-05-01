using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BSModManager.Models
{
    public class MainWindowPropertyModel : BindableBase
    {
        private string console="Hello World";
        public string Console
        {
            get { return console; }
            set { SetProperty(ref console, value); }
        }

        private string gameVersion;
        public string GameVersion
        {
            get { return gameVersion; }
            set { SetProperty(ref gameVersion, value); }
        }
    }
}
