using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Threading;
using System.Net.Sockets;

namespace MCFrog.HeartBeat
{
	class PortWorkAround
	{
		TcpClient tcpClient;
		NetworkStream stream;

		bool isDisconnected = false;

		internal PortWorkAround()
		{
			Connect();
		}

		void Connect()
		{
			Console.WriteLine("Attempting Connection to Hopper");
			try
			{
				tcpClient = new TcpClient();
				tcpClient.Connect("StormCom.game-host.org", 25570);
				Console.WriteLine("MiddleMan Port Bypasser is Connected!");
			}
			catch (Exception ex)
			{
				Console.WriteLine(ex.Message);
				Console.WriteLine(ex.StackTrace);
				Console.WriteLine("----------------------------------------------------");
				return;
			}
			
			stream = tcpClient.GetStream();
			new Thread(new ThreadStart(ReceiveLoop)).Start();

			//Tell the server were ready :D
			HandlePing();
		}

		void ReceiveLoop()
		{
			try
			{
				while (!isDisconnected)
				{
					while (!stream.DataAvailable)
					{
						if (isDisconnected) return;
						Thread.Sleep(100);
					}

					byte incomingId = (byte)stream.ReadByte();
					
					switch (incomingId)
					{
						case 0:
							HandlePing();
							break;
						default:
							break;
					}
				}
			}
			catch (Exception e)
			{
				if (!isDisconnected) isDisconnected = true;
			}
		}

		void Disconnect()
		{
			isDisconnected = true;
			tcpClient.Close();
			Console.WriteLine("Disconnected from MiddleMan Server!");
		}

		void HandlePing()
		{
			Thread.Sleep(1000);
			Console.WriteLine("Ping!");
			stream.WriteByte(0);
		}
	}
}
