using System;
using System.Text;
using System.Net;
using System.IO;

namespace MCFrog.HeartBeat
{
	public class HeartBeat : MarshalByRefObject
	{
		static string _hash;
		public static string ServerURL;
		static string _staticVars;

		//static BackgroundWorker worker;
		static HttpWebRequest _request;
		static readonly System.Timers.Timer HeartbeatTimer = new System.Timers.Timer(60000);

	    private bool IsWorkaroundEnabled = false;

	    public HeartBeat()
		{
			Console.WriteLine("Heartbeat Initializing...");
			HeartbeatTimer.Elapsed += delegate
			{
				HeartbeatTimer.Interval = 15000;
				Pumpboth();
			}; HeartbeatTimer.Start();

			Pumpboth();

			if (IsWorkaroundEnabled)
			{
				new PortWorkAround();
			}
		}

		void Stop()
		{
			HeartbeatTimer.Stop();
			HeartbeatTimer.Dispose();
		}

		void Pumpboth()
		{
			_staticVars = "port=" + Configuration.PORT +
				"&max=" + Configuration.MAXPLAYERS +
				"&name=" + UrlEncode(Configuration.ServerName) +
				"&public=" + Configuration.PUBLIC +
				"&version=" + Configuration.VERSION;

			Beat();
		}

	    static void Beat()
		{
			//Console.WriteLine("Beating!");
			
			string postVars = _staticVars;

			const string url = "http://www.minecraft.net/heartbeat.jsp";

			postVars += "&salt=" + Configuration.ServerSalt;
			postVars += "&users=" + 0; //TODO get player count

			_request = (HttpWebRequest)WebRequest.Create(new Uri(url));
			_request.Method = "POST";
			_request.ContentType = "application/x-www-form-urlencoded";
			_request.CachePolicy = new System.Net.Cache.RequestCachePolicy(System.Net.Cache.RequestCacheLevel.NoCacheNoStore);
			byte[] formData = Encoding.ASCII.GetBytes(postVars);
			_request.ContentLength = formData.Length;
			_request.Timeout = 15000;

			try
			{
				using (Stream requestStream = _request.GetRequestStream())
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
				using (WebResponse response = _request.GetResponse())
				{
					using (StreamReader responseReader = new StreamReader(response.GetResponseStream()))
					{
						if (_hash != null)
						{
							string oldhash = _hash;

							string line = responseReader.ReadToEnd().Trim();
							_hash = line.Substring(line.LastIndexOf('=') + 1);
							ServerURL = line;

							if (oldhash != _hash)
							{
								Console.WriteLine("URL found: " + ServerURL);
							}
						}
						else
						{
							string line = responseReader.ReadToEnd().Trim();
							_hash = line.Substring(line.LastIndexOf('=') + 1);
							ServerURL = line;

							Console.WriteLine("URL found: " + ServerURL);
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
			var output = new StringBuilder();
			for (int i = 0; i < input.Length; i++)
			{
				if ((input[i] >= '0' && input[i] <= '9') ||
					(input[i] >= 'a' && input[i] <= 'z') ||
					(input[i] >= 'A' && input[i] <= 'Z') ||
					input[i] == '-' || input[i] == '_' || input[i] == '.' || input[i] == '~')
				{
					output.Append(input[i]);
				}
				else if (Array.IndexOf(_reservedChars, input[i]) != -1)
				{
					output.Append('%').Append(((int)input[i]).ToString("X"));
				}
			}
			return output.ToString();
		}

	    readonly char[] _reservedChars = { ' ', '!', '*', '\'', '(', ')', ';', ':', '@', '&',
                                                 '=', '+', '$', ',', '/', '?', '%', '#', '[', ']' };

		public override object InitializeLifetimeService()
		{
			// returning null here will prevent the lease manager
			// from deleting the object.
			return null;
		}
	}
}
