using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace BSModManager.Interfaces
{
    public interface IModData
    {
        bool Checked
        {
            get;
            set;
        }
        string Mod
        {
            get;
            set;
        }
        Version Installed
        {
            get;
            set;
        }
        Version Latest
        {
            get;
            set;
        }
        string Original
        {
            get;
            set;
        }
        string Updated
        {
            get;
            set;
        }
        string MA
        {
            get;
            set;
        }
        string Description
        {
            get;
            set;
        }
        string Url
        {
            get;
            set;
        }
        Brush InstalledColor
        {
            get;
            set;
        }
    }
}
