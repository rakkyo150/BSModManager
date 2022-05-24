using BSModManager.Interfaces;
using BSModManager.Static;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BSModManager.Models
{
    public class Syncer
    {
        PastMods pastMods;
        RecommendMods recommendMods;
        LocalMods localMods;
        MAMods mAMod;

        public Syncer(LocalMods mdm, MAMods mam, PastMods pm, RecommendMods rm)
        {
            localMods = mdm;
            mAMod = mam;
            pastMods = pm;
            recommendMods = rm;
        }

        // インストールタブの動機も行う
        public void Sync()
        {
            string pluginFolderPath = Path.Combine(Folder.Instance.BSFolderPath, "Plugins");
            string pendingPluginFolderPath = Path.Combine(Folder.Instance.BSFolderPath, "IPA", "Pending", "Plugins");

            System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(pluginFolderPath);
            System.IO.DirectoryInfo pendingDi = new System.IO.DirectoryInfo(pendingPluginFolderPath);

            IEnumerable<System.IO.FileInfo> filesName = null;
            IEnumerable<System.IO.FileInfo> pendingFilesName = null;

            if (Directory.Exists(pluginFolderPath))
            {
                filesName = di.EnumerateFiles("*.dll", System.IO.SearchOption.TopDirectoryOnly);
            }
            if (Directory.Exists(pendingDi.FullName))
            {
                pendingFilesName = pendingDi.EnumerateFiles("*.dll", System.IO.SearchOption.TopDirectoryOnly);
            }


            Dictionary<string, Version> localModNameAndVersionDic = new Dictionary<string, Version>();

            if (filesName != null)
            {
                foreach (System.IO.FileInfo f in filesName)
                {
                    string pluginPath = Path.Combine(pluginFolderPath, f.Name);

                    System.Diagnostics.FileVersionInfo vi = System.Diagnostics.FileVersionInfo.GetVersionInfo(pluginPath);
                    Version installedModVersion = new Version(vi.FileVersion);

                    localModNameAndVersionDic.Add(f.Name.Replace(".dll", ""), installedModVersion);
                }
            }

            if (pendingFilesName != null)
            {
                foreach (System.IO.FileInfo pendingF in pendingFilesName)
                {
                    string pendingPluginPath = Path.Combine(pendingPluginFolderPath, pendingF.Name);

                    System.Diagnostics.FileVersionInfo pendingVi = System.Diagnostics.FileVersionInfo.GetVersionInfo(pendingPluginPath);
                    Version pendingInstalledModVersion = new Version(pendingVi.FileVersion);

                    if (!localModNameAndVersionDic.ContainsKey(pendingF.Name.Replace(".dll", "")))
                    {
                        localModNameAndVersionDic.Add(pendingF.Name.Replace(".dll", ""), pendingInstalledModVersion);
                        continue;
                    }

                    if (pendingInstalledModVersion > localModNameAndVersionDic[pendingF.Name.Replace(".dll", "")])
                    {
                        localModNameAndVersionDic[pendingF.Name.Replace(".dll", "")] = pendingInstalledModVersion;
                    }
                }
            }

            foreach (KeyValuePair<string, Version> localModNameAndVersion in localModNameAndVersionDic)
            {
                if (!ExistsDataInLocalMods(localModNameAndVersion))
                {
                    localMods.Add(new LocalMods.LocalModData(this)
                    {
                        Mod = localModNameAndVersion.Key,
                        Installed = localModNameAndVersion.Value
                    });

                    continue;
                }

                if (!ExistsDataInMAMod(localModNameAndVersion))
                {
                    localMods.Add(new LocalMods.LocalModData(this)
                    {
                        Mod = localModNameAndVersion.Key,
                        Installed = localModNameAndVersion.Value
                    });

                    continue;
                }

                var temp = Array.Find(mAMod.modAssistantAllMods, x => x.name == localModNameAndVersion.Key);

                DateTime now = DateTime.Now;
                DateTime mAUpdatedAt = DateTime.Parse(temp.updatedDate);
                string updated = null;
                if ((now - mAUpdatedAt).Days >= 1)
                {
                    updated = (now - mAUpdatedAt).Days + "D ago";
                }
                else
                {
                    updated = (now - mAUpdatedAt).Hours + "H" + (now - mAUpdatedAt).Minutes + "m ago";
                }

                localMods.Add(new LocalMods.LocalModData(this)
                {
                    Mod = localModNameAndVersion.Key,
                    Installed = localModNameAndVersion.Value,
                    Latest = new Version(temp.version),
                    Updated = updated,
                    Original = "〇",
                    MA = "〇",
                    Description = temp.description,
                    Url = temp.link
                });
            }

            if (NoLocalModsData()) return;

            List<IModData> removeList = new List<IModData>();
            foreach (var data in localMods.LocalModsData)
            {
                if (RemovedFromLocal(filesName, data))
                {
                    removeList.Add(data);
                }
            }

            RemoveFromLocalMods(removeList);
        }

        private bool NoLocalModsData()
        {
            return localMods.LocalModsData.Count == 0;
        }

        private void RemoveFromLocalMods(List<IModData> removeList)
        {
            foreach (var removeData in removeList)
            {
                localMods.Remove(removeData);
            }
        }

        private static bool RemovedFromLocal(IEnumerable<FileInfo> filesName, IModData data)
        {
            return !filesName.Any(x => x.Name.Replace(".dll", "") == data.Mod);
        }

        private bool ExistsDataInMAMod(KeyValuePair<string, Version> modNameAndVersion)
        {
            return Array.Exists(mAMod.modAssistantAllMods, x => x.name == modNameAndVersion.Key);
        }

        private bool ExistsDataInLocalMods(KeyValuePair<string, Version> modNameAndVersion)
        {
            return localMods.LocalModsData.Any(x => x.Mod == modNameAndVersion.Key);
        }
    }
}
