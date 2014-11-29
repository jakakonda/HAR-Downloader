using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using Newtonsoft.Json.Linq;

namespace HARDownloader
{
    static class Program
    {
        private static DirectoryInfo _dir = null;
        private static int _fileCount = 0;
        private static int _downloaded = 0;

        static void Main(string[] args)
        {
            if (args.Length == 0 || !File.Exists(args[0]))
            {
                Console.WriteLine("Usage: ");
                Console.WriteLine("hardl.exe file.har [dir]");

                return;
            }

            var dirName = args.Length >= 2 ? args[1] : args[0] + ".dl";

            if (args.Length >= 2 && !Directory.Exists(dirName))
                _dir = Directory.CreateDirectory(dirName);
            else
                _dir = new DirectoryInfo(dirName);

            DownloadHARFile(args[0]);
        }


        private static void EnsureDirectory(string path)
        {
            if (Directory.Exists(path))
                return;

            Directory.CreateDirectory(path);
        }


        private static void DownloadHARFile(string f)
        {
            var data = JObject.Parse(File.ReadAllText(f));

            var files = new Dictionary<string, ManualResetEvent>();

            foreach (var entry in data["log"]["entries"])
            {
                var url = new Uri(entry["request"]["url"].ToString());
                var file = Path.GetFileName(url.AbsolutePath);
                var dir =
                    (_dir.FullName + "/" + url.Authority + '/' +
                     url.AbsolutePath.Substring(0, url.AbsolutePath.Length - (file.Length > 0 ? file.Length : 0)))
                        .Replace("//", "/");

                if (string.IsNullOrEmpty(file))
                    file = "index.html";

                var filePath = dir + file;

                if (files.ContainsKey(filePath))
                    continue;

                var we = new WebClient();
                var waiter = new ManualResetEvent(false);
                files.Add(filePath, waiter);

                EnsureDirectory(dir);

                if (File.Exists(filePath))
                    File.Delete(filePath);

                try
                {
                    we.DownloadFileAsync(url, filePath);
                    we.DownloadFileCompleted += (sender, args) =>
                    {
                        waiter.Set();
                        Console.WriteLine(++_downloaded + "/" + _fileCount);
                    };

                    _fileCount++;
                    Console.WriteLine("Downloading: " + url);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: " + ex);
                }
            }

            foreach (var waiter in files)
                waiter.Value.WaitOne();
        }
    }
}
