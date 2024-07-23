using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace YA4KRGDLTool
{
    class Program
    {
        static async Task Main(string[] args)
        {
            OnlineDLState app = new OnlineDLState();
            await app.Run();
        }
    }

    class OnlineDLState
    {
        private List<string> files;
        private int curSelected = 0;
        private string selectedFile;
        private List<string> fileTexts;

        public async Task Run()
        {
            Console.Title = "Yet Another 4K Song Downloader";
            Console.WriteLine("Choose a song to download:");

            await FetchDirectoryListing("https://raw.githubusercontent.com/yophlox/YA4kRG-OnlineMaps/main/maps.json");

            while (true)
            {
                Update();
                await Task.Delay(100);
            }
        }

        public void Update()
        {
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true).Key;
                if (key == ConsoleKey.UpArrow || key == ConsoleKey.DownArrow)
                {
                    ChangeSelection(key == ConsoleKey.UpArrow ? -1 : 1);
                }
                if (key == ConsoleKey.Enter && selectedFile != null)
                {
                    DownloadFile(selectedFile);
                }
            }
        }

        public async Task FetchDirectoryListing(string url)
        {
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    var response = await client.GetStringAsync(url);
                    Console.WriteLine("Data received: " + response);
                    ParseDirectoryListing(response);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to fetch directory listing: " + e.Message);
                }
            }
        }

        public void ParseDirectoryListing(string data)
        {
            var maps = JArray.Parse(data);
            files = new List<string>();
            fileTexts = new List<string>();

            foreach (var map in maps)
            {
                var name = map["name"].ToString();
                var downloadUrl = map["download"].ToString();
                files.Add(downloadUrl);
                fileTexts.Add(name);
                Console.WriteLine(name);
            }

            if (files.Count > 0)
            {
                selectedFile = files[0];
                ChangeSelection(0);
            }
        }

        public void ChangeSelection(int change)
        {
            curSelected += change;

            if (curSelected < 0) curSelected = 0;
            if (curSelected >= files.Count) curSelected = files.Count - 1;

            ClearConsole();

            for (int i = 0; i < fileTexts.Count; i++)
            {
                Console.ForegroundColor = i == curSelected ? ConsoleColor.Red : ConsoleColor.White;
                Console.WriteLine(fileTexts[i]);
            }

            selectedFile = files[curSelected];
        }

        public void ClearConsole()
        {
            Console.Clear();
            Console.WriteLine("Choose a song to download:");
        }

        public async void DownloadFile(string fileUrl)
        {
            using (HttpClient client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/58.0.3029.110 Safari/537.3");
                client.DefaultRequestHeaders.Add("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8");
                client.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.5");
                client.DefaultRequestHeaders.Add("Connection", "keep-alive");

                try
                {
                    var data = await client.GetByteArrayAsync(fileUrl);
                    Console.WriteLine("Download complete: " + fileUrl);
                    SaveFile(fileUrl, data);
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to download file: " + e.Message);
                }
            }
        }

        public void SaveFile(string fileUrl, byte[] data)
        {
            var fileName = fileUrl.Substring(fileUrl.LastIndexOf("/") + 1);
            var directoryPath = "assets/downloads/";

            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            var filePath = Path.Combine(directoryPath, fileName);

            try
            {
                File.WriteAllBytes(filePath, data);
                Console.WriteLine("File saved to: " + filePath);
                ExtractZipFile(filePath, "assets/charts/");
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to save file: " + filePath + " - " + e.Message);
            }
        }

        public void ExtractZipFile(string zipFilePath, string extractPath)
        {
            try
            {
                if (!Directory.Exists(extractPath))
                {
                    Directory.CreateDirectory(extractPath);
                }

                ZipFile.ExtractToDirectory(zipFilePath, extractPath);
                Console.WriteLine($"File extracted to: {extractPath}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to extract file: {e.Message}");
            }
        }
    }
}
