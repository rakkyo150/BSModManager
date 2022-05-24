using System.Collections.Generic;

namespace BSModManager.Interfaces
{
    public interface IMods
    {
        void AllCheckedOrUnchecked();
        void ModRepositoryOpen();

        void Update(IModData modData);
        void Add(IModData modData);
        void Remove(IModData modData);

        IEnumerable<IModData> ReturnCheckedModsData();
    }
}
