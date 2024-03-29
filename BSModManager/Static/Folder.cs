﻿using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.IO;

namespace BSModManager.Static
{
    // 起動時にも使うのでstaticにした
    internal class Folder
    {
        internal static Folder Instance { get; set; } = new Folder();

        public string logFolder = Path.Combine(Environment.CurrentDirectory, "Log");
        public string backupFolder = Path.Combine(Environment.CurrentDirectory, "Backup");
        public string dataFolder = Path.Combine(Environment.CurrentDirectory, "Data");
        public string tmpFolder = Path.Combine(Path.GetTempPath(), "BSModManager");

        public Folder()
        {
            if (!Directory.Exists(logFolder))
            {
                Directory.CreateDirectory(logFolder);
            }
            if (!Directory.Exists(backupFolder))
            {
                Directory.CreateDirectory(backupFolder);
            }
            if (!Directory.Exists(dataFolder))
            {
                Directory.CreateDirectory(dataFolder);
            }
            if (!Directory.Exists(tmpFolder))
            {
                Directory.CreateDirectory(tmpFolder);
            }
        }

        public string Select(string previouPath)
        {
            CommonOpenFileDialog dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                return dialog.FileName;
            }

            return previouPath;
        }

        public void Open(string path)
        {
            try
            {
                System.Diagnostics.Process.Start("explorer.exe", path);
            }
            catch (Exception ex)
            {
                Logger.Instance.Error($"{ex.Message}\nフォルダを開くのに失敗しました");
            }
        }

        public void Copy(string sourceDirName, string destDirName, bool copySubDirs)
        {
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            DirectoryInfo[] dirs = dir.GetDirectories();

            // If the destination directory doesn't exist, create it.       
            Directory.CreateDirectory(destDirName);

            // Get the files in the directory and copy them to the new location.
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                string tempPath = Path.Combine(destDirName, file.Name);
                file.CopyTo(tempPath, true);
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (!copySubDirs) return;

            foreach (DirectoryInfo subdir in dirs)
            {
                string tempPath = Path.Combine(destDirName, subdir.Name);
                Copy(subdir.FullName, tempPath, copySubDirs);
            }
        }
    }
}
