using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.IO;

namespace BSModManager.Static
{
    // 起動時にも使うのでstaticにした
    public static class FolderManager
    {
        public static string backupFolder = Path.Combine(Environment.CurrentDirectory, "Backup");
        public static string dataFolder = Path.Combine(Environment.CurrentDirectory, "Data");
        public static string tempFolder = Path.Combine(Path.GetTempPath(), "BSModManager");

        public static string SelectFolderCommand(string previouPath)
        {
            var dialog = new CommonOpenFileDialog();

            dialog.IsFolderPicker = true;

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                return dialog.FileName;
            }

            return previouPath;
        }

        public static void OpenFolderCommand(string path)
        {
            System.Diagnostics.Process.Start("explorer.exe", path);
        }

        public static void FolderInitialize()
        {
            if (!Directory.Exists(backupFolder))
            {
                Directory.CreateDirectory(backupFolder);
            }
            if (!Directory.Exists(dataFolder))
            {
                Directory.CreateDirectory(dataFolder);
            }
            if (!Directory.Exists(tempFolder))
            {
                Directory.CreateDirectory(tempFolder);
            }
        }
    }
}
