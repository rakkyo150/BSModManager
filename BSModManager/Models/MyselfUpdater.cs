﻿using BSModManager.Static;
using Octokit;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace BSModManager.Models
{
    public class MyselfUpdater: BindableBase
    {       
        private Version latestMyselfVersion = new Version("0.0.0");
        public Version LatestMyselfVersion
        {
            get { return latestMyselfVersion; }
            set { SetProperty(ref latestMyselfVersion, value); }
        }

        public void UpdateUpdater()
        {
            string downloadPath = Path.Combine(Environment.CurrentDirectory, LatestMyselfVersion.ToString());
            try
            {
                if (Directory.Exists(downloadPath))
                {
                    DirectoryInfo dir = new DirectoryInfo(downloadPath);

                    FileInfo[] files = dir.GetFiles();
                    foreach (FileInfo file in files)
                    {
                        if (file.Name.Contains("Updater") && !file.Name.Contains("BSModManager"))
                        {
                            string tempPath = Path.Combine(Environment.CurrentDirectory, file.Name);
                            file.CopyTo(tempPath, true);
                        }
                    }
                    Console.WriteLine("Updaterのアップデート完了");
                }
                else
                {
                    MessageBox.Show("Updaterのアップデートができませんでした\n最新バージョンのフォルダが生成されているはずなので、手動で中身を上書きコピペしてください",
                        "アップデート失敗", MessageBoxButton.OK, MessageBoxImage.Error);
                    Environment.Exit(0);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                MessageBox.Show("Updaterのアップデートができませんでした\n最新バージョンのフォルダが生成されているはずなので、手動で中身を上書きコピペしてください",
                        "アップデート失敗", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(0);
            }
        }
    }
}