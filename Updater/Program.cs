using System;
using System.Diagnostics;
using System.IO;

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
                if (!Directory.Exists(downloadPath))
                {
                    Console.WriteLine("本体のアップデートができませんでした");
                    Console.WriteLine("最新バージョンのフォルダが生成されているはずなので、手動で中身を上書きコピペしてください");
                    Console.ReadLine();
                    return;
                }

                Process[] ps = Process.GetProcessesByName("BSModManager");

                if (ps.Length != 0)
                {
                    Console.WriteLine("BSModManager.exeが終了するまで待機中");
                    foreach (Process p in ps)
                    {
                        p.WaitForExit();
                    }
                    Console.WriteLine("BSModManager.exeが終了しました");
                }

                // Setup.msiを起動する
                ProcessStartInfo processStartInfo = new ProcessStartInfo
                {
                    FileName = Path.Combine(downloadPath, "Setup.msi"),
                };
                Process process = Process.Start(processStartInfo);
                process.WaitForExit();
                Console.WriteLine("ExitCode : " + process.ExitCode);

                Console.WriteLine("本体のアップデート完了");
                Console.WriteLine("Enterで本体を再起動します");
                Console.ReadLine();
                processStartInfo = new ProcessStartInfo
                {
                    FileName = Path.Combine(Environment.CurrentDirectory, "BSModManager.exe")
                };
                Process.Start(processStartInfo);
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Console.WriteLine("正常にアップデートができませんでした");
                Console.WriteLine("最新バージョンのフォルダが生成されているはずなので、手動で中身を上書きコピペしてください");
                Console.ReadLine();
            }
        }
    }
}
