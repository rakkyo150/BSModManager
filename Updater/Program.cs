﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Updater
{
    class Program
    {
        static void Main(string[] args)
        {

            String[] a = Environment.GetCommandLineArgs();

            string downloadPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, a[1]);
            Console.WriteLine(downloadPath);

            try
            {
                if (Directory.Exists(downloadPath))
                {
                    DirectoryInfo dir = new DirectoryInfo(downloadPath);

                    FileInfo[] files = dir.GetFiles();
                    foreach (FileInfo file in files)
                    {
                        if (!file.Name.Contains("Updater") || file.Name.Contains("BSModManager"))
                        {
                            string tempPath = Path.Combine(Environment.CurrentDirectory, file.Name);
                            file.CopyTo(tempPath, true);
                        }
                    }
                    Console.WriteLine("本体のアップデート完了");
                    Console.WriteLine("Enterで本体を再起動します");
                    Console.ReadLine();
                    ProcessStartInfo processStartInfo = new ProcessStartInfo();
                    processStartInfo.FileName = Path.Combine(Environment.CurrentDirectory, "BSModManager.exe");
                    Process process = Process.Start(processStartInfo);
                    Environment.Exit(0);
                }
                else
                {
                    Console.WriteLine("本体のアップデートができませんでした");
                    Console.WriteLine("最新バージョンのフォルダが生成されているはずなので、手動で中身を上書きコピペしてください");
                    Console.ReadLine();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("正常にアップデートができませんでした");
                Console.WriteLine("最新バージョンのフォルダが生成されているはずなので、手動で中身を上書きコピペしてください");
                Console.ReadLine();
            }
        }
    }
}