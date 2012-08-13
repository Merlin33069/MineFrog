using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;

namespace MCFrog.HeartBeat
{
	public class HeartBeat : MarshalByRefObject
	{
		static string hash;
		public static string serverURL;
		static string staticVars;

		//static BackgroundWorker worker;
		static HttpWebRequest request;
		static System.Timers.Timer heartbeatTimer = new System.Timers.Timer(60000);

		static bool isWorkaroundEnabled = false;
		
		public HeartBeat()
		{
			Console.WriteLine("Heartbeat Initializing...");
			heartbeatTimer.Elapsed += delegate
			{
				heartbeatTimer.Interval = 15000;
				pumpboth();
			}; heartbeatTimer.Start();

			pumpboth();

			if (isWorkaroundEnabled)
			{
				new PortWorkAround();
			}
		}

		void Stop()
		{
			heartbeatTimer.Stop();
			heartbeatTimer.Dispose();
		}

		void pumpboth()
		{
			staticVars = "port=" + Configuration.PORT +
				"&max=" + Configuration.MAXPLAYERS +
				"&name=" + UrlEncode(Configuration.SERVER_NAME) +
				"&public=" + Configuration.PUBLIC +
				"&version=" + Configuration.VERSION;

			Beat();

			return;
		}

		void Beat()
		{
			//Console.WriteLine("Beating!");
			
			string postVars = staticVars;

			string url = "http://www.minecraft.net/heartbeat.jsp";

			postVars += "&salt=" + Configuration.SERVER_SALT;
			postVars += "&users=" + 0; //TODO get player count

			request = (HttpWebRequest)WebRequest.Create(new Uri(url));
			request.Method = "POST";
			request.ContentType = "application/x-www-form-urlencoded";
			request.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
			byte[] formData = Encoding.ASCII.GetBytes(postVars);
			request.ContentLength = formData.Length;
			request.Timeout = 15000;

			try
			{
				using (Stream requestStream = request.GetRequestStream())
				{
					requestStream.Write(formData, 0, formData.Length);
					requestStream.Close();
				}
			}
			catch
			{
				//TODO exception handling
			}
			try
			{
				using (WebResponse response = request.GetResponse())
				{
					using (StreamReader responseReader = new StreamReader(response.GetResponseStream()))
					{
						if (hash != null)
						{
							string oldhash = hash;

							string line = responseReader.ReadToEnd().Trim();
							hash = line.Substring(line.LastIndexOf('=') + 1);
							serverURL = line;

							if (oldhash != hash)
							{
								Console.WriteLine("URL found: " + serverURL);
							}
						}
						else
						{
							string line = responseReader.ReadToEnd().Trim();
							hash = line.Substring(line.LastIndexOf('=') + 1);
							serverURL = line;

							Console.WriteLine("URL found: " + serverURL);
						}
					}
				}
			}
			catch (Exception e)
			{

			}
		}

		string UrlEncode(string input)
		{
			StringBuilder output = new StringBuilder();
			for (int i = 0; i < input.Length; i++)
			{
				if ((input[i] >= '0' && input[i] <= '9') ||
					(input[i] >= 'a' && input[i] <= 'z') ||
					(input[i] >= 'A' && input[i] <= 'Z') ||
					input[i] == '-' || input[i] == '_' || input[i] == '.' || input[i] == '~')
				{
					output.Append(input[i]);
				}
				else if (Array.IndexOf<char>(reservedChars, input[i]) != -1)
				{
					output.Append('%').Append(((int)input[i]).ToString("X"));
				}
			}
			return output.ToString();
		}
		char[] reservedChars = { ' ', '!', '*', '\'', '(', ')', ';', ':', '@', '&',
                                                 '=', '+', '$', ',', '/', '?', '%', '#', '[', ']' };

		public override object InitializeLifetimeService()
		{
			// returning null here will prevent the lease manager
			// from deleting the object.
			return null;
		}
	}
}
