using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;

namespace MCFrog
{
	public class ConnectionHandler
	{
		static TcpListener listener;

		public ConnectionHandler()
		{
			StartListening();
			Console.WriteLine("test");
		}

		static void StartListening()
		{
			while (!Server.shouldShutdown)
			{
				try
				{
					listener = new TcpListener(System.Net.IPAddress.Any, Configuration.PORT);
					listener.Start();
					IAsyncResult ar = listener.BeginAcceptTcpClient(new AsyncCallback(AcceptCallback), listener);
					break;
				}
				catch (SocketException E)
				{
					Console.WriteLine("e1");
					Server.Log(E, LogTypesEnum.error);
					break;
				}
				catch (Exception E)
				{
					Console.WriteLine("e2");
					Server.Log(E, LogTypesEnum.error);
					continue;
				}
			}
		}
		private static void AcceptCallback(IAsyncResult ar)
		{
			TcpListener listener2 = (TcpListener)ar.AsyncState;
			try
			{
				TcpClient clientSocket = listener2.EndAcceptTcpClient(ar);
				PlayerHandler.Connections.Add(new Player(clientSocket));
			}
			catch (Exception e)
			{
				Server.Log(e.Message, LogTypesEnum.error);
			}

			if (!Server.shouldShutdown)
			{
				listener.BeginAcceptTcpClient(new AsyncCallback(AcceptCallback), listener);
			}
		}

	}
}
