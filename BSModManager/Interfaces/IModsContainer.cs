using System.Collections.Generic;

namespace BSModManager.Interfaces
{
    public interface IModsContainer
    {
        void AllCheckedOrUnchecked();
        void ModRepositoryOpen();
        List<IMod> AllCheckedMod();

        void SortByName();

        void Add(IMod modData);
        void Remove(IMod modData);

        void UpdateInstalled(IMod modData);
        void UpdateLatest(IMod modData);
        void UpdateDownloadedFileHash(IMod modData);
        void UpdateOriginal(IMod modData);
        void UpdateUpdated(IMod modData);
        void UpdateMA(IMod modData);
        void UpdateDescription(IMod modData);
        void UpdateURL(IMod modData);

        bool ExistsSameModNameData(IMod modData);

        IEnumerable<IMod> ReturnCheckedModsData();
    }
}
