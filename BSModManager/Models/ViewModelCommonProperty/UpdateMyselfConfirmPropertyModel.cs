using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BSModManager.Models.ViewModelCommonProperty
{
    public class UpdateMyselfConfirmPropertyModel: BindableBase
    {
        private Version latestMyselfVersion = new Version("0.0.0");
        public Version LatestMyselfVersion
        {
            get { return latestMyselfVersion; }
            set { SetProperty(ref latestMyselfVersion, value); }
        }
    }
}
