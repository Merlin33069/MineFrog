using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

namespace Updater
{
	public class Updater
	{
		private byte[] downloadedData;
		private string filename = "MCFrog_Update.exe";
		private string url = "https://dl.dropbox.com/u/9690967/MCFrog/Update/MCFrog_Update.exe";
		private string completesize;
		private int current = 0;
		private int total = 0;

		private void Init()
		{
			Download();
			Save();
			Start();
		}

		private void Download()
		{
			downloadedData = new byte[0];
			try
			{
				Console.WriteLine("Connecting...");

				WebRequest req = WebRequest.Create(url);
				WebResponse response = req.GetResponse();
				Stream stream = response.GetResponseStream();

				byte[] buffer = new byte[1024];

				int dataLength = (int)response.ContentLength;
				total = dataLength;

				Console.WriteLine("Downloading...");

				MemoryStream memStream = new MemoryStream();
				while (true)
				{
					int bytesRead = stream.Read(buffer, 0, buffer.Length);

					if (bytesRead == 0)
					{
						break;
					}
					else
					{
						memStream.Write(buffer, 0, bytesRead);
						current += bytesRead;
						Console.WriteLine(current + "/" + total);
					}
				}
				downloadedData = memStream.ToArray();

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
			if (downloadedData != null && downloadedData.Length != 0)
			{
				Console.WriteLine("Saving Data...");

				if (File.Exists(filename))
				{
					File.Delete(filename);
				}

				FileStream newFile = new FileStream(filename, FileMode.Create);
				newFile.Write(downloadedData, 0, downloadedData.Length);
				newFile.Close();

				Console.WriteLine("Saved");
			}
			else
				Console.WriteLine("The File was not downloaded correctly, please try again!");
		}
		private void Start()
		{
			StreamWriter w = new StreamWriter(File.Create("Alesha_Update_Batch.bat"));
			{
				w.WriteLine("@echo off");
				w.WriteLine("@echo StormCom Auto Updater");
				w.WriteLine("@echo Uses 7zip SFX Module.");
				w.WriteLine("@call Alesha_Update.exe -y");
				w.WriteLine("@del Alesha_Update.exe");
				w.WriteLine("@start Alesha.exe");
				w.WriteLine("@del Alesha_Update_Batch.bat");
				w.Flush();
				w.Close();
			}

			Console.WriteLine("Launching Updater...");;
			System.Diagnostics.Process.Start(Directory.GetCurrentDirectory() + "\\Alesha_Update_Batch.bat");
		}
	}
}
