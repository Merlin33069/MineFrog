using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

namespace StartConsole
{
	class Program
	{
		static void Main(string[] args)
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

		static void Update()
		{
			new Updater().Init();
		}
		static bool UpdateCheck()
		{
			string line;

			try
			{
				WebRequest req = WebRequest.Create("https://dl.dropbox.com/u/9690967/MCFrog/Update/version.html");
				WebResponse response = req.GetResponse();
				StreamReader responseReader = new StreamReader(response.GetResponseStream());
				line = responseReader.ReadLine().Trim();
			}
			catch
			{
				Console.WriteLine("There was an error connecting to the update server. This may impact your ability to play the game.", "Connection Error.");
				return false;
			}

			if (line == null)
			{
				Console.WriteLine("Error retreiving data, try again later, if this persists please email me at 'StormCom@live.com'.", "Retreival Error.");
				return false;
			}

			try
			{
				if (Convert.ToInt32(line.ToLower().Trim()) > LillyPad.LillyPad.version)
				{
					return true;
				}
				else
				{
					return false;
				}
			}
			catch
			{
				Console.WriteLine("Internal Error while attempting to check for updates, if this persists, please contact me at 'StormCom@Live.com'.", "Internal Error.");
				return false;
			}
		}
	}
}
