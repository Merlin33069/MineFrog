using System;
using System.Net;
using System.Net.Sockets;

namespace MineFrog
{
	public class ConnectionHandler
	{
		private static TcpListener _listener;

		public ConnectionHandler()
		{
			StartListening();
			Console.WriteLine("test");
		}

		private static void StartListening()
		{
			while (!Server.ShouldShutdown)
			{
				try
				{
					_listener = new TcpListener(IPAddress.Any, Configuration.PORT);
					_listener.Start();
					_listener.BeginAcceptTcpClient(AcceptCallback, _listener);
					break;
				}
				catch (SocketException e)
				{
					Console.WriteLine("e1");
					Server.Log(e, LogTypesEnum.Error);
					break;
				}
				catch (Exception e)
				{
					Console.WriteLine("e2");
					Server.Log(e, LogTypesEnum.Error);
				}
			}
		}

		private static void AcceptCallback(IAsyncResult ar)
		{
			var listener2 = (TcpListener) ar.AsyncState;
			try
			{
				TcpClient clientSocket = listener2.EndAcceptTcpClient(ar);
				PlayerHandler.Connections.Add(new Player(clientSocket));
			}
			catch (Exception e)
			{
				Server.Log(e.Message, LogTypesEnum.Error);
			}

			if (!Server.ShouldShutdown)
			{
				_listener.BeginAcceptTcpClient(AcceptCallback, _listener);
			}
		}
	}
}