using Prism.Mvvm;
using System;

namespace BSModManager.Models.ViewModelCommonProperty
{
    public class UpdateMyselfConfirmPropertyModel : BindableBase
    {
        private Version latestMyselfVersion = new Version("0.0.0");
        public Version LatestMyselfVersion
        {
            get { return latestMyselfVersion; }
            set { SetProperty(ref latestMyselfVersion, value); }
        }
    }
}
