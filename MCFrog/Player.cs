using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.IO.Compression;


namespace MCFrog
{
	class Player
	{
		static ASCIIEncoding asen = new ASCIIEncoding();

		TcpClient tcpClient;
		Socket socket;
		NetworkStream stream;
		string ip;
		internal byte UserId = 101; //This is the users ID in the session
		internal int UID = 1337; //This is the users Unique ID for the whole server

		internal Level level = LevelHandler.lobby;
		internal Level oldLevel; //If not null represents the old level that this player was in
		//TODO set this to null when the level listed is unloaded, it may prevent the level from unloading in memory!

		public string username;

		public pos position;
		public pos oldPosition;
		pos delta;

		bool _isDisconnected = false;
		bool isDisconnected
		{
			get
			{
				if (_isDisconnected || Server.shouldShutdown)
					return true;
				else
					return false;
			}
			set
			{
				_isDisconnected = value;
			}
		}
		bool isLoading = false;
		bool isLoggedIn = false;
		bool isAdmin = false;
		bool isInvisible = false;

		bool enableHistoryMode = false;
		bool enableWaterMode = false;
		bool enableLavaMode = false;

		internal bool _isActive
		{
			get
			{
				return (!isDisconnected && !isLoading && isLoggedIn); //All need to be true to be active :D
			}
		}

		const byte PROTOCOL_VERSION = 0x07;

		internal Player(TcpClient client)
		{
			tcpClient = client;
			new Thread(new ThreadStart(Start)).Start();
		}
		internal void Start()
		{
			socket = tcpClient.Client;
			stream = tcpClient.GetStream();
			ip = socket.RemoteEndPoint.ToString().Split(':')[0];
			Server.Log(ip + " connected", LogTypesEnum.normal);
			BeginRead();
		}

		void Disconnect()
		{
			isDisconnected = true;
			
			if (UserId != 101)
				PlayerHandler.Players.Remove(UserId);
			PlayerHandler.Connections.Remove(this);

			KillThisPlayer();

			tcpClient.Close();

			SendGlobalMessage(username + " Left.");
		}

		internal void BeginRead()
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
					PacketType packetType = (PacketType)incomingId;

					switch (packetType)
					{
						case PacketType.Identification:
							//Server.Log("Identification packet Received", LogTypesEnum.debug);
							ReceiveIdentificationPacket();
							break;
						case PacketType.SetBlockIncoming:
							//Server.Log("SetBlockIncoming packet received!", LogTypesEnum.debug);
							ReceiveSetBlockPacket();
							break;
						case PacketType.PositionAndOrientationTeleport:
							//Server.Log("POSandORIENT packet received!", LogTypesEnum.debug);
							ReceivePosAndOrientPacket();
							break;
						case PacketType.Message:
							//Server.Log("Message packet received!", LogTypesEnum.debug);
							ReceiveMessage();
							break;
						default:
							//Server.Log("received packet id: " + incomingId + ", not in switch", LogTypesEnum.debug);
							//TODO Kick here!
							break;
					}
				}
			}
			catch (Exception e)
			{
				if (!isDisconnected) Disconnect();
				//Server.Log(e.Message, LogTypesEnum.error);
			}
		}

		void ReceiveIdentificationPacket()
		{
			byte[] data = new byte[130];
			stream.Read(data, 0, 130);

			ThreadPool.QueueUserWorkItem(HandleLogin, data);
		}
		void HandleLogin(object incoming)
		{
			byte[] data = (byte[])incoming;

			if (data[0] != PROTOCOL_VERSION)
			{
				//TODO Kick dis playa!
				return;
			}

			username = asen.GetString(data, 1, 64).Trim();
			string hash = asen.GetString(data, 65, 64).Trim();
			byte type = data[129];

			Server.Log(username + " Logging in with hash: " + hash + " and TYPE: " + type, LogTypesEnum.info);
			SendGlobalMessage(username + " Logged In.");

			PlayerHandler.Connections.Remove(this);
			UserId = FreeId();
			Server.Log(UserId + "", LogTypesEnum.info);
			PlayerHandler.Players.Add(UserId, this);

			SendMap();

			isLoggedIn = true;

		}

		void ReceiveSetBlockPacket()
		{
			byte[] data = new byte[8];
			stream.Read(data, 0, 8);

			ThreadPool.QueueUserWorkItem(HandleSetBlockPacket, data);
		}
		void HandleSetBlockPacket(object incoming)
		{
			byte[] data = (byte[])incoming;

			ushort x = NTHO(data, 0);
			ushort y = NTHO(data, 2);
			ushort z = NTHO(data, 4);
			bool isPlacing = data[6] == 0x01;
			byte incomingType = data[7];

			int blockPos = level.PosToInt(x, y, z);

			if (enableHistoryMode)
			{
				SendBlockChange(x, y, z, level.blocks[blockPos]);
				History.hisData hD = Server.historyController.GetData(level.name, blockPos);
				SendMessage("Type: " + hD.type);
				SendMessage("UID: " + hD.UID);
				return;
			}
			if (enableWaterMode)
			{
				level.PlayerBlockChange(this, x, y, z, (byte)Blocks.Water);
				return;
			}
			if (enableLavaMode)
			{
				level.PlayerBlockChange(this, x, y, z, (byte)Blocks.Lava);
				return;
			}

			if (isPlacing)
			{
				level.PlayerBlockChange(this, x, y, z, incomingType);
				
				//if (!level.physics.PhysicsUpdates.Contains(blockPos))
				//{
				//    //Server.Log("adding to physics list", LogTypesEnum.debug);
				//    level.physics.PhysicsUpdates.Add(blockPos);
				//}

				//Check the block below too, for grass and whatnot
				//blockPos = level.PosToInt(x, (ushort)(y-1), z);
				//if (!level.physics.PhysicsUpdates.Contains(blockPos))
				//{
				//    //Server.Log("adding to physics list", LogTypesEnum.debug);
				//    level.physics.PhysicsUpdates.Add(blockPos);
				//}
			}
			else
			{
				level.PlayerBlockChange(this, x, y, z, (byte)Blocks.Air);
			}
		}

		void ReceivePosAndOrientPacket()
		{
			byte[] data = new byte[9];
			stream.Read(data, 0, 9);

			ThreadPool.QueueUserWorkItem(HandlePosAndOrientPacket, data);
		}
		void HandlePosAndOrientPacket(object incoming)
		{
			byte[] data = (byte[])incoming;

			byte _id = data[0];
			ushort x = NTHO(data, 1);
			ushort y = NTHO(data, 3);
			ushort z = NTHO(data, 5);
			byte yaw = data[7];
			byte pitch = data[8];
			
			oldPosition = position;
			position.x = x;
			position.y = y;
			position.z = z;
			position.yaw = yaw;
			position.pitch = pitch;

			delta = oldPosition.diff(position);

			UpdatePlayerPos();
		}
		void UpdatePlayerPos()
		{
			//TODO create pos change packet
			Packet packet = new Packet();

			if (delta.IsZero())
			{
				return;
			}
			else
			{
				if (delta.NeedTp())
				{
					packet.id = PacketType.PositionAndOrientationTeleport;
					packet.AddVar(UserId);
					packet.AddVar(position.x);
					packet.AddVar(position.y);
					packet.AddVar(position.z);
					packet.AddVar(position.yaw);
					packet.AddVar(position.pitch);
				}
				else
				{
					if (delta.PosChanged())
					{
						if (delta.RotChanged())
						{
							//Pos AND Rot
							packet.id = PacketType.PositionAndOrientationUpdate;
							packet.AddVar(UserId);
							packet.AddVar((byte)delta.x);
							packet.AddVar((byte)delta.y);
							packet.AddVar((byte)delta.z);
							packet.AddVar(position.yaw);
							packet.AddVar(position.pitch);


						}
						else
						{
							//TODO
							//Just Pos
							packet.id = PacketType.PositionUpdate;
							packet.AddVar(UserId);
							packet.AddVar((byte)delta.x);
							packet.AddVar((byte)delta.y);
							packet.AddVar((byte)delta.z);
						}
					}
					else
					{
						if (delta.RotChanged())
						{
							packet.id = PacketType.OrientationUpdate;
							packet.AddVar(UserId);
							packet.AddVar(position.yaw);
							packet.AddVar(position.pitch);
						}
					}
				}
			}

			foreach (Player p in level.players.ToArray())
			{
				if (p._isActive && p != this)
				{
					p.SendPacket(packet);
				}
			}
		}

		void ReceiveMessage()
		{
			byte[] data = new byte[65];
			stream.Read(data, 0, 65);

			ThreadPool.QueueUserWorkItem(HandleMessage, data);

		}
		void HandleMessage(object incoming)
		{
			byte[] data = (byte[])incoming;
			string message = asen.GetString(data, 1, 64).Trim();

			if (message.Length > 1 && message[0] == '/' && message[1] != '/')
			{
				HandleCommand(message);
				return;
			}

			SendGlobalChat(this, message);
		}
		void HandleCommand(string message)
		{
			string[] command = message.Split(' ');
			command[0] = command[0].Remove(0, 1).ToLower();

			if (command[0] == "historymode")
			{
				enableWaterMode = false;
				enableLavaMode = false;

				enableHistoryMode = !enableHistoryMode;
				SendMessage("History mode is " + enableHistoryMode.ToString());
			}
			else if (command[0] == "water")
			{
				enableHistoryMode = false;
				enableLavaMode = false;

				enableWaterMode = !enableWaterMode;
				SendMessage("Water mode is " + enableWaterMode.ToString());
			}
			else if (command[0] == "lava")
			{
				enableHistoryMode = false;
				enableWaterMode = false;
				
				enableLavaMode = !enableLavaMode;
				SendMessage("Lava mode is " + enableLavaMode.ToString());
			}
			else if (command[0] == "level" || command[0] == "levels")
			{
				if (command.Length == 1)
				{
					SendMessage("You are currently on: " + level.name);
					SendMessage("Possible SubCommands:");
					SendMessage("loaded");
					SendMessage("new <name> (Size x,y,z) (type)");
				}
				else if (command[1].ToLower() == "loaded")
				{
					SendMessage("Loaded Levels:");
					foreach (Level l in LevelHandler.levels.ToArray())
					{
						SendMessage(l.name);
					}
				}
				else if (command[1].ToLower() == "new")
				{
					if (command.Length == 2)
					{
						SendMessage("You at least need to enter a name!");
						return;
					}
					else if (command.Length > 6 || (command.Length > 3 && command.Length != 6))
					{
						SendMessage("Incorrect number of variables!");
						return;
					}
					else if (command.Length == 6)
					{
						try
						{
							string levelName = command[2];

							ushort levelSizeX = Convert.ToUInt16(command[3]);
							ushort levelSizeY = Convert.ToUInt16(command[4]);
							ushort levelSizeZ = Convert.ToUInt16(command[5]);

							Level l = Level.Find(levelName);
							if (l != null)
							{
								SendMessage("The level: " + levelName + " is already loaded!");
								return;
							}
							else
							{
								SendMessage("Generating new level: " + levelName);
								l = new Level(levelName, levelSizeX, levelSizeY, levelSizeZ);
								Player.SendGlobalMessage("NEW LEVEL: " + l.name);
							}
						}
						catch
						{
							SendMessage("New level create failed!");
						}
					}
					else
					{
						Level l = Level.Find(command[2]);
						if (l != null)
						{
							SendMessage("The level: " + command[2] + " is already loaded!");
							return;
						}
						else
						{
							SendMessage("Generating new level: " + command[2]);
							l = new Level(command[2], 64, 64, 64);
							Player.SendGlobalMessage("NEW LEVEL: " + l.name);
						}
					}
				}
				else if (command[1].ToLower() == "load")
				{
					if (command.Length == 2)
					{
						SendMessage("You at least need to enter a name!");
						return;
					}
					else
					{
						Level l = Level.Find(command[2]);
						if (l != null)
						{
							SendMessage("The level: " + command[2] + " is already loaded!");
							return;
						}
						else
						{
							SendMessage("Loading level: " + command[2]);
							try
							{
								l = new Level(command[2]);
							}
							catch(Exception e)
							{
								Console.WriteLine(e.Message);
								SendMessage("Level load failed!");
								return;
							}
							Player.SendGlobalMessage("LOADED LEVEL: " + l.name);
						}
					}
				}
				else if (command[1].ToLower() == "unload")
				{
					if (command.Length == 1)
					{
						SendMessage("You have to enter a name!");
						return;
					}
					else
					{
						Level l = Level.Find(command[2]);

						if (l == null)
						{
							SendMessage("Level not found!");
							return;
						}
						else if (l == LevelHandler.lobby)
						{
							SendMessage("You cannot unload the lobby.");
							return;
						}
						else
						{
							SendMessage("Unloading: " + l.name);
							l.Unload();

							Server.historyController.SaveHistory(l.name);
							Server.historyController.UnloadHistory(l.name);

							SendMessage("Done!");
						}
					}
				}
			}
			else if (command[0] == "goto")
			{
				if (command.Length == 1)
				{
					SendMessage("You have to enter a name to switch levels!");
					return;
				}
				else
				{
					Level l = Level.Find(command[1]);

					if (l == null)
					{
						SendMessage("Level not found!");
						return;
					}
					else
					{
						SwitchMap(l);
					}

					SendMessage("You are now on: " + level.name);
				}
			}
			
			
		}

		void SendPacket(Packet p)
		{
			//Server.Log(p.id + " sending", LogTypesEnum.debug);
			byte[] data = new byte[p.bytes.Count + 1];
			data[0] = (byte)p.id;
			p.bytes.CopyTo(data, 1);

			SendRaw(data);
		}
		void SendRaw(byte id)
		{
			SendRaw(new byte[1] { id });
		}
		void SendRaw(byte[] data)
		{
			if (isDisconnected) return;
			//TODO setup a queue system to allow us to meet the framelimit on all data sending (with a timeout to send if we dont hit the limit
			try
			{
				if (!tcpClient.Connected)
				{
					Disconnect();
					return;
				}
				stream.Write(data, 0, data.Length);
			}
			catch
			{
				//This player has dc'ed
				Disconnect();
				//Console.WriteLine("GAH FAIL");
			}
		}
		void SendRaw(byte id, byte[] data)
		{
			SendRaw(id);
			SendRaw(data);
			//stream.Write(data, 0, data.Length);
		}

		void SendMotd()
		{
			string motd = Configuration.SERVER_MOTD;
			string name = Configuration.SERVER_NAME;

			Packet p = new Packet();
			p.id = PacketType.Identification;
			p.AddVar(PROTOCOL_VERSION);
			p.AddVar(name);
			p.AddVar(motd);
			if (isAdmin) p.AddVar((byte)100);
			else p.AddVar((byte)0);

			Server.Log("Send MOTD", LogTypesEnum.debug);
			SendPacket(p);
			//Console.WriteLine(p.bytes.Count);

			//byte[] buffer = new byte[130];
			//buffer[0] = PROTOCOL_VERSION;
			//StringFormat(Configuration.SERVER_NAME, 64).CopyTo(buffer, 1);
			//StringFormat(Configuration.SERVER_MOTD, 64).CopyTo(buffer, 65);
			//if (isAdmin)
			//    buffer[129] = 100;
			//else
			//    buffer[129] = 0;
			//SendRaw(0, buffer);

		}
		void SendMap()
		{
			if (oldLevel != null)
			{
				oldLevel.players.Remove(this);
				SendAllDeSpawns();
			}

			SendMotd();
			isLoading = true;

			SendRaw(2);

			byte[] buffer = new byte[level.blocks.Length + 4];
			BitConverter.GetBytes(IPAddress.HostToNetworkOrder(level.blocks.Length)).CopyTo(buffer, 0);

			level.blocks.CopyTo(buffer, 4);
			
			buffer = GZip(buffer);
			int number = (int)Math.Ceiling(((double)buffer.Length) / 1024);
			for (int i = 1; buffer.Length > 0; ++i)
			{
				short length = (short)Math.Min(buffer.Length, 1024);
				byte[] send = new byte[1027];
				HTNO(length).CopyTo(send, 0);
				Buffer.BlockCopy(buffer, 0, send, 2, length);
				byte[] tempbuffer = new byte[buffer.Length - length];
				Buffer.BlockCopy(buffer, length, tempbuffer, 0, buffer.Length - length);
				buffer = tempbuffer;
				send[1026] = (byte)(i * 100 / number);
				Server.Log("Sending map part " + i, LogTypesEnum.debug);
				SendRaw(3, send);
				Thread.Sleep(10);
			}

			Server.Log("Map sent, initializing", LogTypesEnum.debug);

			buffer = new byte[6];
			HTNO((short)level.sizeX).CopyTo(buffer, 0);
			HTNO((short)level.sizeY).CopyTo(buffer, 2);
			HTNO((short)level.sizeZ).CopyTo(buffer, 4);
			SendRaw((byte)4, buffer);

			isLoading = false;

			level.players.Add(this);

			SendAllSpawns();

			SendTeleportThisPlayer(level.spawnPos);

			Server.Log("map done", LogTypesEnum.debug);
		}
		internal void SwitchMap(Level l)
		{
			oldLevel = level;
			level = l;

			SendMap();
		}

		internal static void SendGlobalChat(Player player, string message)
		{
			string username = player.username;

			string toSend = "<" + username + ">: " + message;

			Server.Log("!" + toSend, LogTypesEnum.chat);

			Packet packet = new Packet();
			packet.id = PacketType.Message;
			packet.AddVar(player.UserId);
			packet.AddVar(toSend);

			foreach (Player p in PlayerHandler.Players.Values.ToArray())
			{
				if (p._isActive)
				{
					p.SendPacket(packet);
				}
			}
		}
		internal static void SendLevelChat(Player player, string message)
		{
			string username = player.username;

			string toSend = "<" + username + ">: " + message;

			Server.Log("!" + toSend, LogTypesEnum.chat);

			Packet packet = new Packet();
			packet.id = PacketType.Message;
			packet.AddVar(player.UserId);
			packet.AddVar(toSend);

			foreach (Player p in player.level.players.ToArray())
			{
				if (p._isActive)
				{
					p.SendPacket(packet);
				}
			}
		}
		internal static void SendGlobalMessage(string message)
		{
			Server.Log("!" + message, LogTypesEnum.chat);

			Packet packet = new Packet();
			packet.id = PacketType.Message;
			packet.AddVar((byte)0);
			packet.AddVar(message);

			foreach (Player p in PlayerHandler.Players.Values.ToArray())
			{
				if (p._isActive)
				{
					p.SendPacket(packet);
				}
			}
		}
		internal void SendMessage(string message)
		{
			Packet packet = new Packet();
			packet.id = PacketType.Message;
			packet.AddVar(UserId);
			packet.AddVar(message);
			SendPacket(packet);
		}

		void SendAllSpawns()
		{
			foreach (Player p in level.players.ToArray())
			{
				if (p._isActive && p!= this)
				{
					SendSpawn(p);
					p.SendSpawn(this);
				}
			}
		}
		void SendAllDeSpawns()
		{
			foreach (Player p in oldLevel.players.ToArray())
			{
				if (p._isActive && p != this)
				{
					SendDie(p);
					p.SendDie(this);
				}
			}
		}
		void SendSpawn(Player player)
		{
			Server.Log("Spawning " + player.UserId + " for " + UserId, LogTypesEnum.debug);
			Packet p = new Packet();
			p.id = PacketType.SpawnPlayer;
			p.AddVar(player.UserId);
			p.AddVar(player.username);
			p.AddVar(player.position.x);
			p.AddVar(player.position.y);
			p.AddVar(player.position.z);
			p.AddVar(player.position.yaw);
			p.AddVar(player.position.pitch);
			SendPacket(p);
		}
		void SendDie(Player player)
		{
			Packet p = new Packet();
			p.id = PacketType.DespawnPlayer;
			p.AddVar(player.UserId);
			SendPacket(p);
		}
		void KillThisPlayer()
		{
			foreach (Player player in level.players.ToArray())
			{
				player.SendDie(this);
			}
		}

		void SendFullPosPacketTeleport(Player player)
		{
			Packet p = new Packet();
			p.id = PacketType.PositionAndOrientationTeleport;
			p.AddVar(player.UserId);
			p.AddVar(player.position.x);
			p.AddVar(player.position.y);
			p.AddVar(player.position.z);
			p.AddVar(player.position.yaw);
			p.AddVar(player.position.pitch);
			SendPacket(p);
		}
		void SendTeleportThisPlayer(pos newPosition)
		{
			oldPosition = position;
			position = newPosition;

			Packet p = new Packet();
			p.id = PacketType.PositionAndOrientationTeleport;
			p.AddVar((byte)255);
			p.AddVar(position.x);
			p.AddVar(position.y);
			p.AddVar(position.z);
			p.AddVar(position.yaw);
			p.AddVar(position.pitch);
			SendPacket(p);
		}

		internal void SendBlockChange(ushort x, ushort y, ushort z, byte type)
		{
			Packet p = new Packet();
			p.id = PacketType.SetBlockOutgoing;
			p.AddVar(x);
			p.AddVar(y);
			p.AddVar(z);
			p.AddVar(type);
			SendPacket(p);
		}

		internal void SendKick(string message)
		{
			Packet p = new Packet();
			p.id = PacketType.Disconnect;
			p.AddVar(message);
			SendPacket(p);
		}

		public static byte[] GZip(byte[] bytes)
		{
			System.IO.MemoryStream ms = new System.IO.MemoryStream();
			GZipStream gs = new GZipStream(ms, CompressionMode.Compress, true);
			gs.Write(bytes, 0, bytes.Length); gs.Close(); gs.Dispose();
			ms.Position = 0; bytes = new byte[ms.Length];
			ms.Read(bytes, 0, (int)ms.Length); ms.Close(); ms.Dispose();
			return bytes;
		}
		byte[] HTNO(short x)
		{
			byte[] y = BitConverter.GetBytes(x); Array.Reverse(y); return y;
		}
		ushort NTHO(byte[] x, int offset)
		{
			byte[] y = new byte[2];
			Buffer.BlockCopy(x, offset, y, 0, 2); Array.Reverse(y);
			return BitConverter.ToUInt16(y, 0);
		}

		internal static byte FreeId()
		{
			if (PlayerHandler.Players.Count == 0) return (byte)1;
			for (byte i = 1; i < 100; i++)
			{
				if (!PlayerHandler.Players.ContainsKey(i)) return i;
			}
			return 254;
		}
	}

	public class Packet
	{
		internal static ASCIIEncoding encode = new ASCIIEncoding();
		internal List<byte> bytes = new List<byte>();

		internal PacketType id = PacketType.Identification;

		//Variable Adding
		internal void AddVar(string var)
		{
			//TODO proper multiline!!
			//TODO error checks!
			//TODO Color checks!

			if (var.Length > 64) var = "string too long, newline NYI";

			var = var.PadRight(64);
			bytes.AddRange(encode.GetBytes(var));
		}
		internal void AddVar(short var)
		{
			bytes.AddRange(HTNO(var));
		}
		internal void AddVar(ushort var)
		{
			bytes.AddRange(HTNO(var));
		}
		internal void AddVar(byte var)
		{
			bytes.Add(var);
		}

		//Map stuff
		internal void AddInt(int var)
		{
			bytes.AddRange(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(var)));
		}
		internal void AddMapData(Level l)
		{
			for (int i = 0; i < l.blocks.Length; ++i)
			{
				bytes.Add(l.blocks[i]);
			}
		}
		internal void Gzip()
		{
			System.IO.MemoryStream ms = new System.IO.MemoryStream();
			GZipStream gs = new GZipStream(ms, CompressionMode.Compress, true);
			gs.Write(bytes.ToArray(), 0, bytes.Count); gs.Close(); gs.Dispose();
			ms.Position = 0; bytes.Clear(); byte[] buffer = new byte[ms.Length];
			ms.Read(buffer, 0, (int)ms.Length); ms.Close(); ms.Dispose();
			bytes.AddRange(buffer);
		}

		//Misc Methods
		byte[] HTNO(short x)
		{
			byte[] y = BitConverter.GetBytes(x); Array.Reverse(y); return y;
		}
		byte[] HTNO(ushort x)
		{
			byte[] y = BitConverter.GetBytes(x); Array.Reverse(y); return y;
		}
	}
	enum PacketType
	{
		Identification = 0x00,
		Ping = 0x01,
		LevelInitialize = 0x02,
		LevelDataChunk = 0x03,
		LevelFinalize = 0x04,
		SetBlockIncoming = 0x05,
		SetBlockOutgoing = 0x06,
		SpawnPlayer = 0x07,
		PositionAndOrientationTeleport = 0x08, //Teleport
		PositionAndOrientationUpdate = 0x09,
		PositionUpdate = 0x0a,
		OrientationUpdate = 0x0b,
		DespawnPlayer = 0x0c,
		Message = 0x0d,
		Disconnect = 0x0e,
		UpdateUserType = 0x0f,
	}

	public struct pos
	{
		public ushort x, y, z;
		public byte pitch, yaw;

		public pos diff(pos newPos)
		{
			pos delta = new pos();
			delta.x = (ushort)(newPos.x - x);
			delta.y = (ushort)(newPos.y - y);
			delta.z = (ushort)(newPos.z - z);
			delta.yaw = (byte)(newPos.yaw - pitch);
			delta.pitch = (byte)(newPos.yaw - pitch);

			return delta;
		}

		public bool IsZero()
		{
			if (x == 0 && y == 0 && z == 0 && pitch == 0 && yaw == 0)
				return true;
			return false;
		}
		public bool PosChanged()
		{
			if (x == 0 && y == 0 && z == 0)
				return false;
			return true;
		}
		public bool RotChanged()
		{
			if (yaw == 0 && pitch == 0)
				return false;
			return true;
		}
		public bool NeedTp()
		{
			if (Math.Abs(x) >= 32 || Math.Abs(y) >= 32 || Math.Abs(z) >= 32)
				return true;
			return false;
		}
	}
}
