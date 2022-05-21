using BSModManager.Static;
using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace BSModManager.Models
{
    public class LocalModSyncer
    {
        LocalMods localModsDataModel;
        MAMods mAMod;

        public LocalModSyncer(LocalMods mdm,MAMods mam)
        {
            localModsDataModel = mdm;
            mAMod = mam;
        }

        /// <summary>
        /// <para>ローカル情報取得</para>
        /// 第二引数はcsvを基にModのバージョンを取得する(nullならファイルバージョンを利用)
        /// </summary>
        /// <param name="pluginsFolderPath"></param>
        /// <returns></returns>
        public void Sync()
        {
            // Console.WriteLine("Start Getting FileInfo");

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


            Dictionary<string, Version> combinedModNameAndVersion = new Dictionary<string, Version>();

            if (filesName != null)
            {
                foreach (System.IO.FileInfo f in filesName)
                {
                    string pluginPath = Path.Combine(pluginFolderPath, f.Name);

                    System.Diagnostics.FileVersionInfo vi = System.Diagnostics.FileVersionInfo.GetVersionInfo(pluginPath);
                    Version installedModVersion = new Version(vi.FileVersion);

                    combinedModNameAndVersion.Add(f.Name.Replace(".dll", ""), installedModVersion);
                }
            }
            if (pendingFilesName != null)
            {
                foreach (System.IO.FileInfo pendingF in pendingFilesName)
                {
                    string pendingPluginPath = Path.Combine(pendingPluginFolderPath, pendingF.Name);

                    System.Diagnostics.FileVersionInfo pendingVi = System.Diagnostics.FileVersionInfo.GetVersionInfo(pendingPluginPath);
                    Version pendingInstalledModVersion = new Version(pendingVi.FileVersion);

                    if (!combinedModNameAndVersion.ContainsKey(pendingF.Name.Replace(".dll", "")))
                    {
                        combinedModNameAndVersion.Add(pendingF.Name.Replace(".dll", ""), pendingInstalledModVersion);
                    }
                    else
                    {
                        if (pendingInstalledModVersion > combinedModNameAndVersion[pendingF.Name.Replace(".dll", "")])
                        {
                            combinedModNameAndVersion[pendingF.Name.Replace(".dll", "")] = pendingInstalledModVersion;
                        }
                    }
                }
            }

            foreach (KeyValuePair<string, Version> modNameAndVersion in combinedModNameAndVersion)
            {
                // 以前のデータ無し
                if (!localModsDataModel.LocalModsData.Any(x => x.Mod == modNameAndVersion.Key))
                {
                    if (Array.Exists(mAMod.modAssistantAllMods, x => x.name == modNameAndVersion.Key))
                    {
                        var temp = Array.Find(mAMod.modAssistantAllMods, x => x.name == modNameAndVersion.Key);

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

                        localModsDataModel.LocalModsData.Add(new LocalMods.LocalModData(this)
                        {
                            Mod = modNameAndVersion.Key,
                            Installed = modNameAndVersion.Value,
                            Latest = new Version(temp.version),
                            Updated = updated,
                            Original = "〇",
                            MA = "〇",
                            Description = temp.description,
                            Url = temp.link
                        });
                    }
                    else
                    {
                        localModsDataModel.LocalModsData.Add(new LocalMods.LocalModData(this)
                        {
                            Mod = modNameAndVersion.Key,
                            Installed = modNameAndVersion.Value
                        });
                    }
                }
                // 以前のデータある場合
                else
                {
                    localModsDataModel.LocalModsData.First(x => x.Mod == modNameAndVersion.Key).Installed = modNameAndVersion.Value;
                }
            }

            if (localModsDataModel.LocalModsData.Count == 0) return;
            
            // 以前実行時から手動で消したModの情報を消す
            List<LocalMods.LocalModData> removeList = new List<LocalMods.LocalModData>();
            foreach (var data in localModsDataModel.LocalModsData)
            {
                if (!filesName.Any(x => x.Name.Replace(".dll", "") == data.Mod))
                {
                    removeList.Add(data);
                }
            }
            foreach (var removeData in removeList)
            {
                localModsDataModel.LocalModsData.Remove(removeData);
            }
        }
    }
}
