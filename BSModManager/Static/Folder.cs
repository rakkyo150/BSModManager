using BSModManager.Models;
using Microsoft.WindowsAPICodePack.Dialogs;
using Prism.Mvvm;
using System;
using System.IO;

namespace BSModManager.Static
{
    // 起動時にも使うのでstaticにした
    internal class Folder : BindableBase
    {
        internal static Folder Instance { get; set; } = new Folder();

        public string backupFolder = Path.Combine(Environment.CurrentDirectory, "Backup");
        public string dataFolder = Path.Combine(Environment.CurrentDirectory, "Data");
        public string tmpFolder = Path.Combine(Path.GetTempPath(), "BSModManager");

        private string bSFolderPath = @"C:\Program Files (x86)\Steam\steamapps\common\Beat Saber";
        public string BSFolderPath
        {
            get { return bSFolderPath; }
            set 
            { 
                SetProperty(ref bSFolderPath, value);
            }
        }

        public string Select(string previouPath)
        {
            var dialog = new CommonOpenFileDialog();

            dialog.IsFolderPicker = true;

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                return dialog.FileName;
            }

            return previouPath;
        }

        public void Open(string path)
        {
            System.Diagnostics.Process.Start("explorer.exe", path);
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
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string tempPath = Path.Combine(destDirName, subdir.Name);
                    Copy(subdir.FullName, tempPath, copySubDirs);
                }
            }
        }

        public void Initialize()
        {
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
    }
}
