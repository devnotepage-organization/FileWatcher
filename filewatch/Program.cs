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
        public static string dstDir = @"C:\FileWatcher";
        public static string watchDir = @"C:\Target";
        static void Main(string[] args)
        {
            var watcher = new FileWatcher();
            watcher.AddWatch(watchDir);
            for (; ; )
            {
                Console.WriteLine("Enter path...");
                string cmd = Console.ReadLine();
                if (cmd == "exit") { break; } else if (cmd == "quit") { break; }
                watcher.AddWatch(cmd);
            }
        }
    }
}

public class FileWatcher
{
    private List<FileSystemWatcher> watchList = new List<FileSystemWatcher>();
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
            watchList.Add(watcher);
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
                string dstDir = Program.dstDir;
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
                        if (retryCount > 999999999)
                        {
                            Console.Error.WriteLine(ex.ToString());
                            break;
                        }
                        Console.WriteLine("retry...");
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
