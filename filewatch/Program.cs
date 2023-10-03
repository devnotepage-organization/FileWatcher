using filewatch;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace filewatch
{
    internal class Program
    {
        public static string DstDir = @"C:\FileWatcher";
        public static string WatchDir = @"C:\Target";
        static void Main(string[] args)
        {
            var watcher = new FileWatcher();
            var creator = new FileCreator();
            watcher.AddWatch(WatchDir);
            for (; ; )
            {
                Console.WriteLine("Enter path...");
                string cmd = Console.ReadLine();
                if (cmd == "exit") { break; }
                else if (cmd == "quit") { break; }
                else if (cmd == "test") { /*creator.Start();*/ continue; }
                watcher.AddWatch(cmd);
            }
            creator.Stop();
        }
    }
    // ファイル監視機能
    public class FileWatcher
    {
        private List<FileSystemWatcher> _watchList = new List<FileSystemWatcher>();
        public void AddWatch(string watchPath)
        {
            try
            {
                FileSystemWatcher watcher = new FileSystemWatcher();
                watcher.Path = watchPath;
                watcher.Filter = "*.*";
                watcher.IncludeSubdirectories = true;
                watcher.NotifyFilter = NotifyFilters.FileName;
                watcher.Created += new FileSystemEventHandler(DoWork);
                watcher.Deleted += new FileSystemEventHandler(DoWork);
                watcher.EnableRaisingEvents = true;
                _watchList.Add(watcher);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
            }
        }
        private void DoWork(object source, FileSystemEventArgs e)
        {
            try
            {
                if (e.ChangeType == WatcherChangeTypes.Created
                 || e.ChangeType == WatcherChangeTypes.Changed)
                {
                    Console.WriteLine("[" + e.ChangeType.ToString() + "]" + e.FullPath);
                    string dstDir = Program.DstDir;
                    string dstPath = Path.Combine(dstDir, Path.GetFileName(e.FullPath));
                    if (!Directory.Exists(dstDir))
                    {
                        Directory.CreateDirectory(dstDir);
                    }
                    // コピーリトライ
                    for (int retryCount = 0; ; retryCount++)
                    {
                        try
                        {
                            File.Copy(e.FullPath, dstPath, true);
                            break;
                        }
                        catch (IOException ex)
                        {
                            if (retryCount > 99)
                            {
                                Console.Error.WriteLine(ex.ToString());
                                break;
                            }
                            Console.WriteLine("retry...{0}", retryCount);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.ToString());
            }
        }
    }
    // ファイル作成機能
    public class FileCreator
    {
        private System.Timers.Timer _timer = null;
        public void Start()
        {
            if (_timer == null)
            {
                _timer = new System.Timers.Timer();
                _timer.Interval = (5 * 1000);
                _timer.Elapsed += (s, e) =>
                {
                    // ファイル作成削除テスト
                    string fileName = "test_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".txt";
                    string filePath = Path.Combine(Program.WatchDir, fileName);
                    using (var writer = new StreamWriter(filePath))
                    {
                        writer.WriteLine("aaa");
                    }
                    File.Delete(filePath);
                };
            }
            if (_timer != null)
            {
                Console.WriteLine("Timer start.");
                _timer.Start();
            }
        }
        public void Stop()
        {
            if (_timer != null)
            {
                _timer.Stop();
                Console.WriteLine("Timer stop.");
            }
        }
    }
}
