using System;
using System.Diagnostics;
using System.IO;
using System.Net;

namespace StartConsole
{
    public class Updater
    {
        private const string Filename = "MCFrog_Update.exe";
        private const string Url = "https://dl.dropbox.com/u/9690967/MCFrog/Update/MCFrog_Update.exe";
        private int _current;
        private byte[] _downloadedData;
        private int _total;

        internal void Init()
        {
            Download();
            Save();
            Start();
        }

        private void Download()
        {
            _downloadedData = new byte[0];
            try
            {
                Console.WriteLine("Connecting...");

                WebRequest req = WebRequest.Create(Url);
                WebResponse response = req.GetResponse();
                Stream stream = response.GetResponseStream();

                var buffer = new byte[1024];

                var dataLength = (int) response.ContentLength;
                _total = dataLength;

                Console.WriteLine("Downloading...");

                var memStream = new MemoryStream();
                while (true)
                {
                    if (stream != null)
                    {
                        int bytesRead = stream.Read(buffer, 0, buffer.Length);

                        if (bytesRead == 0)
                        {
                            break;
                        }
                        memStream.Write(buffer, 0, bytesRead);
                        _current += bytesRead;
                        Console.WriteLine(_current + "/" + _total);
                    }
                }
                _downloadedData = memStream.ToArray();

                stream.Close();
                memStream.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine("There was an error accessing the URL." + "\r\n" + e.Message + "\r\n" + e.StackTrace);
            }
        }

        private void Save()
        {
            if (_downloadedData != null && _downloadedData.Length != 0)
            {
                Console.WriteLine("Saving Data...");

                if (File.Exists(Filename))
                {
                    File.Delete(Filename);
                }

                var newFile = new FileStream(Filename, FileMode.Create);
                newFile.Write(_downloadedData, 0, _downloadedData.Length);
                newFile.Close();

                Console.WriteLine("Saved");
            }
            else
                Console.WriteLine("The File was not downloaded correctly, please try again!");
        }

        private void Start()
        {
            var w = new StreamWriter(File.Create("MCFrog_Update_Batch.bat"));
            {
                w.WriteLine("@echo off");
                w.WriteLine("@echo MCFrog Auto Updater");
                w.WriteLine("@echo Uses 7zip SFX Module.");
                w.WriteLine("@call MCFrog_Update.exe -y");
                w.WriteLine("@del MCFrog_Update.exe");
                w.WriteLine("@start StartConsole.exe");
                w.WriteLine("@del MCFrog_Update_Batch.bat");
                w.Flush();
                w.Close();
            }

            Console.WriteLine("Launching Updater...");

            Process.Start(Directory.GetCurrentDirectory() + "\\MCFrog_Update_Batch.bat");
        }
    }
}