﻿using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace BSModManager.Interfaces
{
    public interface IMods
    {
        void AllCheckedOrUnchecked();
        void ModRepositoryOpen();
        List<IModData> AllCheckedMod();

        void SortByName();

        void Add(IModData modData);
        void Remove(IModData modData);

        void UpdateInstalled(IModData modData);
        void UpdateLatest(IModData modData);
        void UpdateOriginal(IModData modData);
        void UpdateUpdated(IModData modData);
        void UpdateMA(IModData modData);
        void UpdateDescription(IModData modData);
        void UpdateURL(IModData modData);

        IEnumerable<IModData> ReturnCheckedModsData();
    }
}
