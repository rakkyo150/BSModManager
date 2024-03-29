﻿using System;
using System.Windows.Media;

namespace BSModManager.Interfaces
{
    public interface IMod
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
        string DownloadedFileHash
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
