using System;
using System.IO;
using System.Net;

namespace StartConsole
{
	internal class Program
	{
		private static void Main()
		{
			/*
			 * Just so you can follow my train of thought
			 * 
			 * This is just a wrapper to start the server, all it does is
			 * call the LillyPad system which starts the server
			 * 
			 * The GUI is a little more involved and we will get to that later.
			 */
			Console.Title = "MCFrog";
			
			if (UpdateCheck())
			{
				Update();
				return;
			}

			Console.WriteLine("Starting Main System...");
			new LillyPad.LillyPad();
		}

		private static void Update()
		{
			new Updater().Init();
		}

		private static bool UpdateCheck()
		{
			string line = null;

			try
			{
				WebRequest req = WebRequest.Create("https://dl.dropbox.com/u/9690967/MCFrog/Update/version.html");
				WebResponse response = req.GetResponse();
				var responseReader = new StreamReader(response.GetResponseStream());
				string readLine = responseReader.ReadLine();
				if (readLine != null) line = readLine.Trim();
			}
			catch
			{
				Console.WriteLine(
					"There was an error connecting to the update server. This may impact your ability to play the game.");
				return false;
			}

			try
			{
				return line != null && Convert.ToInt32(line.ToLower().Trim()) > LillyPad.LillyPad.Version;
			}
			catch
			{
				Console.WriteLine(
					"Internal Error while attempting to check for updates, if this persists, please contact me at 'StormCom@Live.com'.");
				return false;
			}
		}
	}
}