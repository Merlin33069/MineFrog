using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using MineFrog.History;
using MineFrog.PreLoader;
using System.Security.Cryptography;

namespace MineFrog
{
	public class Player
	{
		private const byte ProtocolVersion = 0x07;
		private static readonly ASCIIEncoding Asen = new ASCIIEncoding();
		static MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();

		private readonly TcpClient _tcpClient;
		public int UID = 0; //This is the users Unique ID for the whole server
		public byte UserId = 101; //This is the users ID in the session
		public bool _isDisconnected;
		public Pos _delta;
		public bool _enableHistoryMode;
		public bool _enableLavaMode;
		public bool _enableWaterMode;
		public string _ip;

		public bool IsAdmin
		{
			get { return gdb.isAdmin; }
		}
		public string Nickname
		{
			get
			{
				if (string.IsNullOrWhiteSpace(pdb.Nickname)) return Username;
				else return pdb.Nickname;
			}
			set { pdb.Nickname = value; pdb.sync();}
		}
		public byte WarningLevel
		{
			get { return pdb.WarningLevel; }
			set { pdb.WarningLevel = value; pdb.sync(); }
		}
		public bool IsFrozen
		{
			get { return pdb.isFrozen; }
			set { pdb.isFrozen = value; }
		}
		public bool IsMuted
		{
			get { return pdb.isMuted || gdb.canChat; }
			set { pdb.isMuted = value; }
		}

		//TODO BlockChange System for Commands!

		public byte PermissionLevel
		{
			get
			{
				//Server.Log(gdb.PermissionLevel + " wtf?", LogTypesEnum.Debug);
				return gdb.PermissionLevel;
			}
		}
		public bool canBuild
		{
			get { return gdb.canBuild; }
		}
		public int MaxBlockChange
		{
			get { return gdb.MaxBlockChange; }
		}

		public string Tag
		{
			get
			{
				return gdb.GroupColor + "" + gdb.GroupTag + MCColor.white + "" + Nickname;
			}
		}

		private bool _isInvisible = false;

		public bool _isLoading;
		public bool _isLoggedIn;

		public PDB pdb;
		public GDB gdb
		{
			get { return pdb.Group; }
			set
			{
				pdb.Group = value;
				pdb.GroupID = gdb.GID;
				pdb.sync();
			}
		}

		public Level Level = LevelHandler.Lobby;
		public Level OldLevel; //If not null represents the old level that this player was in

		public Pos OldPosition;
		public Pos Position;
		private Socket _socket;
		private NetworkStream _stream;
		public string Username;

		internal Player(TcpClient client)
		{
			_tcpClient = client;
			new Thread(Start).Start();
		}

		private bool IsDisconnected
		{
			get
			{
				if (_isDisconnected || Server.ShouldShutdown)
					return true;
				return false;
			}
			set { _isDisconnected = value; }
		}

		internal bool IsActive
		{
			get { return (!IsDisconnected && !_isLoading && _isLoggedIn); //All need to be true to be active :D
			}
		}

		internal void Start()
		{
			_socket = _tcpClient.Client;
			_stream = _tcpClient.GetStream();
			_ip = _socket.RemoteEndPoint.ToString().Split(':')[0];
			Server.Log(_ip + " connected", LogTypesEnum.Normal);
			BeginRead();
		}

		private void Disconnect()
		{
			IsDisconnected = true;

			if (UserId != 101)
				PlayerHandler.Players.Remove(UserId);
			PlayerHandler.Connections.Remove(this);

			KillThisPlayer();

			_tcpClient.Close();

			SendGlobalMessage(Username + " Left.");
		}

		internal void BeginRead()
		{
			try
			{
				while (!IsDisconnected)
				{
					while (!_stream.DataAvailable)
					{
						if (IsDisconnected) return;
						Thread.Sleep(100);
					}

					var incomingId = (byte) _stream.ReadByte();
					var packetType = (PacketType) incomingId;

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
							SendKick("Invalid Packet ID " + incomingId);
							Server.Log("received packet id: " + incomingId + ", not in switch", LogTypesEnum.Debug);
							break;
					}
				}
			}
			catch
			{
				if (!IsDisconnected) Disconnect();
				//Server.Log(e.Message, LogTypesEnum.error);
			}
		}

		private void ReceiveIdentificationPacket()
		{
			var data = new byte[130];
			_stream.Read(data, 0, 130);

			ThreadPool.QueueUserWorkItem(HandleLogin, data);
		}

		private void HandleLogin(object incoming)
		{
			var data = (byte[]) incoming;

			if (data[0] != ProtocolVersion)
			{
				SendKick("Invalid Protocol version!");
				return;
			}

			Username = Asen.GetString(data, 1, 64).Trim();
			string hash = Asen.GetString(data, 65, 32).Trim();
			byte type = data[129];

			if (string.IsNullOrWhiteSpace(hash) || hash == "--" || hash != BitConverter.ToString(md5.ComputeHash(Asen.GetBytes(Configuration.ServerSalt + Username))).Replace("-", "").ToLower().TrimStart('0'))
			{
				SendKick("Account could not be verified, try again.");
				Server.Log(Server.HeartBeat._hash + "", LogTypesEnum.Debug);
				Server.Log("'" + hash + "' != '" + BitConverter.ToString(md5.ComputeHash(Asen.GetBytes(Configuration.ServerSalt + Username))).Replace("-", "").ToLower().TrimStart('0') + "'", LogTypesEnum.Debug);
				return;
			}

			pdb = PDB.Find(Username.Trim().ToLower());
			if(pdb == null)
			{
				var dbData = new object[] {Username, "", _ip, (byte) 0, 0, false, false};
				try
				{
					UID = Server.users.NewRow(dbData);
				}
				catch (Exception e)
				{
					throw new Exception("FUCK");
				}

				//UID = Server.users.NewRow(dbData);
				pdb = new PDB(UID, dbData);
			}
			else
			{
				UID = pdb.UID;
			}

			Server.Log(Username + " Logging in with hash: " + hash + " and TYPE: " + type, LogTypesEnum.Info);
			SendGlobalMessage(Username + " Logged In.");

			PlayerHandler.Connections.Remove(this);
			UserId = FreeId();
			Server.Log(UserId + "", LogTypesEnum.Info);
			PlayerHandler.Players.Add(UserId, this);

			SendMap();

			_isLoggedIn = true;
		}

		private void ReceiveSetBlockPacket()
		{
			var data = new byte[8];
			_stream.Read(data, 0, 8);

			ThreadPool.QueueUserWorkItem(HandleSetBlockPacket, data);
		}

		private void HandleSetBlockPacket(object incoming)
		{
			var data = (byte[]) incoming;

			ushort x = NTHO(data, 0);
			ushort y = NTHO(data, 2);
			ushort z = NTHO(data, 4);
			bool isPlacing = data[6] == 0x01;
			byte incomingType = data[7];

			if (Level.NotInBounds(x, y, z)) return;

			int blockPos = Level.PosToInt(x, y, z);

			if (blockPos >= Level.BlockData.Length || blockPos < 0)
			{
				Server.Log(blockPos + " was OUT OF BOUNDS!", LogTypesEnum.Error);
				return;
			}

			byte oldBlock = Level.BlockData[blockPos];

			if (_enableHistoryMode)
			{
				SendBlockChange(x, y, z, oldBlock);
				var hD = Server.HistoryController.GetData(Level.Name, blockPos);
				var playername = "Unknown";
				var oldName = "Unknown";
				var currentName = "Unknown";

				if (Block.Blocks.ContainsKey(oldBlock)) currentName = Block.Blocks[oldBlock].Name;
				if (Block.Blocks.ContainsKey(hD.Type)) oldName = Block.Blocks[hD.Type].Name;
				if (hD.UID != int.MaxValue) playername = PDB.Find(hD.UID).Username;
				
				SendMessage("CurrentType: " + currentName);
				SendMessage("OldType: " + oldName);
				SendMessage("Editor: " + playername);
				return;
			}
			if(PermissionLevel < Level.BuildPermissions)
			{
				SendBlockChange(x, y, z, oldBlock);
				return;
			}
			if (_enableWaterMode)
			{
				Level.PlayerBlockChange(this, x, y, z, (byte) MCBlocks.Water);
				return;
			}
			if (_enableLavaMode)
			{
				Level.PlayerBlockChange(this, x, y, z, (byte) MCBlocks.Lava);
				return;
			}

			if (isPlacing)
			{
				Level.PlayerBlockChange(this, x, y, z, incomingType);
				Block.Blocks[incomingType].OnPlace(this, Level, blockPos);
			}
			else
			{
				Level.PlayerBlockChange(this, x, y, z, (byte) MCBlocks.Air);
				Block.Blocks[oldBlock].OnBreak(this, Level, blockPos);
			}
		}

		private void ReceivePosAndOrientPacket()
		{
			var data = new byte[9];
			_stream.Read(data, 0, 9);

			ThreadPool.QueueUserWorkItem(HandlePosAndOrientPacket, data);
		}

		private void HandlePosAndOrientPacket(object incoming)
		{
			var data = (byte[]) incoming;

			ushort x = NTHO(data, 1);
			ushort y = NTHO(data, 3);
			ushort z = NTHO(data, 5);
			byte yaw = data[7];
			byte pitch = data[8];

			OldPosition = Position;
			Position.X = x;
			Position.Y = y;
			Position.Z = z;
			Position.Yaw = yaw;
			Position.Pitch = pitch;

			_delta = OldPosition.Diff(Position);

			UpdatePlayerPos();
		}

		private void UpdatePlayerPos()
		{
			if (_isInvisible) return;

			var packet = new Packet();

			if (_delta.IsZero())
			{
				return;
			}
			if (_delta.NeedTp())
			{
				packet.Id = PacketType.PositionAndOrientationTeleport;
				packet.AddVar(UserId);
				packet.AddVar(Position.X);
				packet.AddVar(Position.Y);
				packet.AddVar(Position.Z);
				packet.AddVar(Position.Yaw);
				packet.AddVar(Position.Pitch);
			}
			else
			{
				if (_delta.PosChanged())
				{
					if (_delta.RotChanged())
					{
						//Pos AND Rot
						packet.Id = PacketType.PositionAndOrientationUpdate;
						packet.AddVar(UserId);
						packet.AddVar((byte) _delta.X);
						packet.AddVar((byte) _delta.Y);
						packet.AddVar((byte) _delta.Z);
						packet.AddVar(Position.Yaw);
						packet.AddVar(Position.Pitch);
					}
					else
					{
						//Just Pos
						packet.Id = PacketType.PositionUpdate;
						packet.AddVar(UserId);
						packet.AddVar((byte) _delta.X);
						packet.AddVar((byte) _delta.Y);
						packet.AddVar((byte) _delta.Z);
					}
				}
				else
				{
					if (_delta.RotChanged())
					{
						packet.Id = PacketType.OrientationUpdate;
						packet.AddVar(UserId);
						packet.AddVar(Position.Yaw);
						packet.AddVar(Position.Pitch);
					}
				}
			}

			foreach (Player p in Level.Players.ToArray())
			{
				if (p.IsActive && p != this)
				{
					p.SendPacket(packet);
				}
			}
		}

		private void ReceiveMessage()
		{
			var data = new byte[65];
			_stream.Read(data, 0, 65);

			ThreadPool.QueueUserWorkItem(HandleMessage, data);
		}

		private void HandleMessage(object incoming)
		{
			var data = (byte[]) incoming;
			string message = Asen.GetString(data, 1, 64).Trim();

			if (message.Length > 1 && message[0] == '/' && message[1] != '/')
			{
				HandleCommand(message);
				return;
			}

			SendGlobalChat(this, message);
		}

		private void HandleCommand(string message)
		{
			string[] command = message.Split(' ');
			command[0] = command[0].Remove(0, 1).ToLower();

			string messageSend = "";
			string[] parameters = new string[0];

			string accessor = command[0].ToLower();
			if(command.Length > 1)
			{
				message = message.Substring(accessor.Length + 2);
				parameters = message.Split(' ');
			}

			if (Commands.CommandHandler.Commands.ContainsKey(accessor))
			{
				Commands.CommandBase commandBase = Commands.CommandHandler.Commands[accessor];
				if (PermissionLevel < commandBase.Permission)
				{
					SendMessage("You do not have permission to use that command!");
				}
				else
				{
					ThreadPool.QueueUserWorkItem(delegate
					{
						commandBase.PlayerUse(this, parameters, messageSend);
					});
				}
			}
				
			else
				SendMessage("Command " + accessor + " does not exist!");


			//if (command[0] == "historymode")
			//{
			//    _enableWaterMode = false;
			//    _enableLavaMode = false;

			//    _enableHistoryMode = !_enableHistoryMode;
			//    SendMessage("History mode is " + _enableHistoryMode.ToString());
			//}
			//else if (command[0] == "water")
			//{
			//    _enableHistoryMode = false;
			//    _enableLavaMode = false;

			//    _enableWaterMode = !_enableWaterMode;
			//    SendMessage("Water mode is " + _enableWaterMode.ToString());
			//}
			//else if (command[0] == "lava")
			//{
			//    _enableHistoryMode = false;
			//    _enableWaterMode = false;

			//    _enableLavaMode = !_enableLavaMode;
			//    SendMessage("Lava mode is " + _enableLavaMode.ToString());
			//}
			//else if (command[0] == "level" || command[0] == "levels")
			//{
			//    if (command.Length == 1)
			//    {
			//        SendMessage("You are currently on: " + Level.Name);
			//        SendMessage("Possible SubCommands:");
			//        SendMessage("loaded");
			//        SendMessage("new <name> (Size x,y,z) (type)");
			//    }
			//    else
			//        switch (command[1].ToLower())
			//        {
			//            case "loaded":
			//                SendMessage("Loaded Levels:");
			//                foreach (Level l in LevelHandler.Levels.ToArray())
			//                {
			//                    SendMessage(l.Name);
			//                }
			//                break;
			//            case "new":
			//                if (command.Length == 2)
			//                {
			//                    SendMessage("You at least need to enter a name!");
			//                    return;
			//                }
			//                if (command.Length > 6 || (command.Length > 3 && command.Length != 6))
			//                {
			//                    SendMessage("Incorrect number of variables!");
			//                    return;
			//                }
			//                if (command.Length == 6)
			//                {
			//                    try
			//                    {
			//                        string levelName = command[2];

			//                        ushort levelSizeX = Convert.ToUInt16(command[3]);
			//                        ushort levelSizeY = Convert.ToUInt16(command[4]);
			//                        ushort levelSizeZ = Convert.ToUInt16(command[5]);

			//                        Level l = Level.Find(levelName);
			//                        if (l != null)
			//                        {
			//                            SendMessage("The level: " + levelName + " is already loaded!");
			//                            return;
			//                        }
			//                        SendMessage("Generating new level: " + levelName);
			//                        l = new Level(levelName, levelSizeX, levelSizeY, levelSizeZ);
			//                        SendGlobalMessage("NEW LEVEL: " + l.Name);
			//                    }
			//                    catch
			//                    {
			//                        SendMessage("New level create failed!");
			//                    }
			//                }
			//                else
			//                {
			//                    Level l = Level.Find(command[2]);
			//                    if (l != null)
			//                    {
			//                        SendMessage("The level: " + command[2] + " is already loaded!");
			//                        return;
			//                    }
			//                    SendMessage("Generating new level: " + command[2]);
			//                    l = new Level(command[2], 64, 64, 64);

			//                    SendGlobalMessage("NEW LEVEL: " + l.Name);
			//                }
			//                break;
			//            case "load":
			//                if (command.Length == 2)
			//                {
			//                    SendMessage("You at least need to enter a name!");
			//                }
			//                else
			//                {
			//                    Level l = Level.Find(command[2]);
			//                    if (l != null)
			//                    {
			//                        SendMessage("The level: " + command[2] + " is already loaded!");
			//                        return;
			//                    }
			//                    SendMessage("Loading level: " + command[2]);
			//                    try
			//                    {
			//                        l = new Level(command[2], false);
			//                    }
			//                    catch (Exception e)
			//                    {
			//                        Console.WriteLine(e.Message);
			//                        SendMessage("Level load failed!");
			//                        return;
			//                    }
			//                    SendGlobalMessage("LOADED LEVEL: " + l.Name);
			//                }
			//                break;
			//            case "unload":
			//                if (command.Length == 1)
			//                {
			//                    SendMessage("You have to enter a name!");
			//                }
			//                else
			//                {
			//                    Level l = Level.Find(command[2]);

			//                    if (l == null)
			//                    {
			//                        SendMessage("Level not found!");
			//                        return;
			//                    }
			//                    if (l == LevelHandler.Lobby)
			//                    {
			//                        SendMessage("You cannot unload the lobby.");
			//                        return;
			//                    }
			//                    SendMessage("Unloading: " + l.Name);
			//                    l.Unload();

			//                    Server.HistoryController.SaveHistory(l.Name);
			//                    Server.HistoryController.UnloadHistory(l.Name);

			//                    SendMessage("Done!");
			//                }
			//                break;
			//        }
			//}
			//else if (command[0] == "goto")
			//{
			//    if (command.Length == 1)
			//    {
			//        SendMessage("You have to enter a name to switch levels!");
			//        return;
			//    }
			//    if (command.Length == 4)
			//    {
			//        try
			//        {
			//            ushort x = Convert.ToUInt16(command[1]);
			//            ushort y = Convert.ToUInt16(command[1]);
			//            ushort z = Convert.ToUInt16(command[1]);

			//            var newPosition = new Pos {X = x, Y = y, Z = z, Yaw = 0, Pitch = 0};

			//            SendTeleportThisPlayer(newPosition);

			//            return;
			//        }
			//        catch
			//        {
			//            SendMessage("Goto Failed!");
			//            return;
			//        }
			//    }
			//    Level l = Level.Find(command[1]);

			//    if (l == null)
			//    {
			//        SendMessage("Level not found!");
			//        return;
			//    }
			//    SwitchMap(l);

			//    SendMessage("You are now on: " + Level.Name);
			//}
		}

		private void SendPacket(Packet p)
		{
			//Server.Log(p.id + " sending", LogTypesEnum.debug);
			var data = new byte[p.Bytes.Count + 1];
			data[0] = (byte) p.Id;
			p.Bytes.CopyTo(data, 1);

			SendRaw(data);
		}

		private void SendRaw(byte id)
		{
			SendRaw(new[] {id});
		}

		private void SendRaw(byte[] data)
		{
			if (IsDisconnected) return;
			//TODO setup a queue system to allow us to meet the framelimit on all data sending (with a timeout to send if we dont hit the limit)
			try
			{
				if (!_tcpClient.Connected)
				{
					Disconnect();
					return;
				}
				_stream.Write(data, 0, data.Length);
			}
			catch
			{
				//This player has dc'ed
				Disconnect();
				//Console.WriteLine("GAH FAIL");
			}
		}

		private void SendRaw(byte id, byte[] data)
		{
			SendRaw(id);
			SendRaw(data);
			//stream.Write(data, 0, data.Length);
		}

		private void SendMotd()
		{
			string motd = Configuration.ServerMotd;
			string name = Configuration.ServerName;

			var p = new Packet {Id = PacketType.Identification};
			p.AddVar(ProtocolVersion);
			p.AddVar(name);
			p.AddVar(motd);
			p.AddVar((byte) (IsAdmin ? 100 : 0));

			Server.Log("Send MOTD", LogTypesEnum.Debug);
			SendPacket(p);
			//Console.WriteLine(p.bytes.Count);

			//byte[] buffer = new byte[130];
			//buffer[0] = PROTOCOL_VERSION;
			//StringFormat(Configuration.SERVER_NAME, 64).CopyTo(buffer, 1);
			//StringFormat(Configuration.SERVER_MOTD, 64).CopyTo(buffer, 65);
			//if (isAdmin)
			//	buffer[129] = 100;
			//else
			//	buffer[129] = 0;
			//SendRaw(0, buffer);
		}

		private void SendMap()
		{
			if (OldLevel != null)
			{
				OldLevel.Players.Remove(this);
				SendAllDeSpawns();
			}

			SendMotd();
			_isLoading = true;

			SendRaw(2);

			var mapSize = new byte[4];
			BitConverter.GetBytes(IPAddress.HostToNetworkOrder(Level.BlockData.Length)).CopyTo(mapSize, 0);

			//level.blocks.CopyTo(buffer, 4);
			//int bufferLength = level.blocks.Length + 4;

			byte[] buffer = GZip(mapSize, Level.BlockData);
			var number = (int) Math.Ceiling(((double) buffer.Length)/1024);
			for (int i = 1; buffer.Length > 0; ++i)
			{
				var length = (short) Math.Min(buffer.Length, 1024);
				var send = new byte[1027];
				HTNO(length).CopyTo(send, 0);
				Buffer.BlockCopy(buffer, 0, send, 2, length);
				var tempbuffer = new byte[buffer.Length - length];
				Buffer.BlockCopy(buffer, length, tempbuffer, 0, buffer.Length - length);
				buffer = tempbuffer;
				send[1026] = (byte) (i*100/number);
				Server.Log("Sending map part " + i, LogTypesEnum.Debug);
				SendRaw(3, send);
				Thread.Sleep(10);
			}

			Server.Log("Map sent, initializing", LogTypesEnum.Debug);

			buffer = new byte[6];
			HTNO((short) Level.SizeX).CopyTo(buffer, 0);
			HTNO((short) Level.SizeY).CopyTo(buffer, 2);
			HTNO((short) Level.SizeZ).CopyTo(buffer, 4);
			SendRaw(4, buffer);

			_isLoading = false;

			Level.Players.Add(this);

			SendAllSpawns();

			SendTeleportThisPlayer(Level.SpawnPos);

			Server.Log("map done", LogTypesEnum.Debug);
		}

		internal void SwitchMap(Level l)
		{
			OldLevel = Level;
			Level = l;

			SendMap();
		}

		internal static void SendGlobalChat(Player player, string message)
		{
			string toSend = player.Tag + ": " + message;

			Server.Log("!" + toSend, LogTypesEnum.Chat);

			var packet = new Packet {Id = PacketType.Message};
			packet.AddVar(player.UserId);
			packet.AddVar(toSend);

			foreach (Player p in PlayerHandler.Players.Values.ToArray())
			{
				if (p.IsActive)
				{
					p.SendPacket(packet);
				}
			}
		}

		internal static void SendLevelChat(Player player, string message)
		{
			string toSend = player.Tag + ": " + message;

			Server.Log("!" + toSend, LogTypesEnum.Chat);

			var packet = new Packet {Id = PacketType.Message};
			packet.AddVar(player.UserId);
			packet.AddVar(toSend);

			foreach (Player p in player.Level.Players.ToArray())
			{
				if (p.IsActive)
				{
					p.SendPacket(packet);
				}
			}
		}

		internal static void SendGlobalMessage(string message)
		{
			Server.Log("!" + message, LogTypesEnum.Chat);

			var packet = new Packet {Id = PacketType.Message};
			packet.AddVar(0);
			packet.AddVar(message);

			foreach (Player p in PlayerHandler.Players.Values.ToArray())
			{
				if (p.IsActive)
				{
					p.SendPacket(packet);
				}
			}
		}

		public void SendMessage(string message)
		{
			var packet = new Packet {Id = PacketType.Message};
			packet.AddVar(UserId);
			packet.AddVar(message);
			SendPacket(packet);
		}

		private void SendAllSpawns()
		{
			foreach (Player p in Level.Players.ToArray())
			{
				if (p.IsActive && p != this)
				{
					SendSpawn(p);
					p.SendSpawn(this);
				}
			}
		}

		private void SendAllDeSpawns()
		{
			foreach (Player p in OldLevel.Players.ToArray())
			{
				if (p.IsActive && p != this)
				{
					SendDie(p);
					p.SendDie(this);
				}
			}
		}

		private void SendSpawn(Player player)
		{
			Server.Log("Spawning " + player.UserId + " for " + UserId, LogTypesEnum.Debug);
			var p = new Packet {Id = PacketType.SpawnPlayer};
			p.AddVar(player.UserId);
			p.AddVar(player.Username);
			p.AddVar(player.Position.X);
			p.AddVar(player.Position.Y);
			p.AddVar(player.Position.Z);
			p.AddVar(player.Position.Yaw);
			p.AddVar(player.Position.Pitch);
			SendPacket(p);
		}

		private void SendDie(Player player)
		{
			var p = new Packet {Id = PacketType.DespawnPlayer};
			p.AddVar(player.UserId);
			SendPacket(p);
		}

		private void KillThisPlayer()
		{
			foreach (Player player in Level.Players.ToArray())
			{
				player.SendDie(this);
			}
		}

/*
		private void SendFullPosPacketTeleport(Player player)
		{
			var p = new Packet {id = PacketType.PositionAndOrientationTeleport};
			p.AddVar(player.UserId);
			p.AddVar(player.Position.x);
			p.AddVar(player.Position.y);
			p.AddVar(player.Position.z);
			p.AddVar(player.Position.yaw);
			p.AddVar(player.Position.pitch);
			SendPacket(p);
		}
*/

		private void SendTeleportThisPlayer(Pos newPosition)
		{
			OldPosition = Position;
			Position = newPosition;

			newPosition.X = 64;
			newPosition.Y = 64;
			newPosition.Z = 64;

			var p = new Packet {Id = PacketType.PositionAndOrientationTeleport};
			p.AddVar(255);
			p.AddVar(Position.X);
			p.AddVar(Position.Y);
			p.AddVar(Position.Z);
			p.AddVar(Position.Yaw);
			p.AddVar(Position.Pitch);
			SendPacket(p);
		}

		internal void SendBlockChange(ushort x, ushort y, ushort z, byte type)
		{
			var p = new Packet {Id = PacketType.SetBlockOutgoing};
			p.AddVar(x);
			p.AddVar(y);
			p.AddVar(z);
			p.AddVar((Block.Blocks.ContainsKey(type) ? Block.Blocks[type].BaseType : (byte)0));
			SendPacket(p);
		}

		internal void SendKick(string message)
		{
			var p = new Packet {Id = PacketType.Disconnect};
			p.AddVar(message);
			SendPacket(p);
		}

		public static byte[] GZip(byte[] mapSize, byte[] levelData)
		{
			var ms = new MemoryStream();
			var gs = new GZipStream(ms, CompressionMode.Compress, true);
			gs.Write(mapSize, 0, mapSize.Length);
			//gs.Write(levelData, 0, levelData.Length);

			int currentstart = 0;

			for (int i = 0; i < levelData.Length; i++ )
			{
				byte block = levelData[i];
				if(block>49)
				{
					if(i>0) gs.Write(levelData, currentstart, (i-currentstart));
					currentstart = i + 1;

					gs.WriteByte((Block.Blocks.ContainsKey(block) ? Block.Blocks[block].BaseType : (byte)0));
				}
				
			}

			if (currentstart != levelData.Length)
			{
				gs.Write(levelData, currentstart, (levelData.Length - currentstart));
			}

			gs.Flush();
			gs.Dispose();

			ms.Position = 0;
			var bytes = new byte[ms.Length];
			ms.Read(bytes, 0, (int) ms.Length);
			ms.Close();
			ms.Dispose();
			return bytes;
		}

		private byte[] HTNO(short x)
		{
			byte[] y = BitConverter.GetBytes(x);
			Array.Reverse(y);
			return y;
		}

		private ushort NTHO(byte[] x, int offset)
		{
			var y = new byte[2];
			Buffer.BlockCopy(x, offset, y, 0, 2);
			Array.Reverse(y);
			return BitConverter.ToUInt16(y, 0);
		}

		internal static byte FreeId()
		{
			if (PlayerHandler.Players.Count == 0) return 1;
			for (byte i = 1; i < 100; i++)
			{
				if (!PlayerHandler.Players.ContainsKey(i)) return i;
			}
			return 254;
		}

		
	}

	public class Packet
	{
		internal static ASCIIEncoding Encode = new ASCIIEncoding();
		internal List<byte> Bytes = new List<byte>();

		internal PacketType Id = PacketType.Identification;

		//Variable Adding
		internal void AddVar(string var)
		{
			//TODO proper multiline!!
			//TODO error checks!
			//TODO Color checks!

			if (var.Length > 64) var = "string too long, newline NYI";

			var = var.PadRight(64);
			Bytes.AddRange(Encode.GetBytes(var));
		}

		internal void AddVar(short var)
		{
			Bytes.AddRange(HTNO(var));
		}

		internal void AddVar(ushort var)
		{
			Bytes.AddRange(HTNO(var));
		}

		internal void AddVar(byte var)
		{
			Bytes.Add(var);
		}

		//Map stuff
		internal void AddInt(int var)
		{
			Bytes.AddRange(BitConverter.GetBytes(IPAddress.HostToNetworkOrder(var)));
		}

		internal void AddMapData(Level l)
		{
			for (int index = 0; index < l.BlockData.Length; index++)
			{
				byte t = l.BlockData[index];
				Bytes.Add(t);
			}
		}

		//internal void Gzip()
		//{
		//    var ms = new MemoryStream();
		//    var gs = new GZipStream(ms, CompressionMode.Compress, true);
		//    gs.Write(Bytes.ToArray(), 0, Bytes.Count);
		//    gs.Close();
		//    gs.Dispose();
		//    ms.Position = 0;
		//    Bytes.Clear();
		//    var buffer = new byte[ms.Length];
		//    ms.Read(buffer, 0, (int) ms.Length);
		//    ms.Close();
		//    ms.Dispose();
		//    Bytes.AddRange(buffer);
		//}

		//Misc Methods
		private IEnumerable<byte> HTNO(short x)
		{
			byte[] y = BitConverter.GetBytes(x);
			Array.Reverse(y);
			return y;
		}

		private IEnumerable<byte> HTNO(ushort x)
		{
			byte[] y = BitConverter.GetBytes(x);
			Array.Reverse(y);
			return y;
		}
	}

	internal enum PacketType
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

	public struct Pos
	{
		public byte Pitch;
		public ushort X;
		public ushort Y;
		public byte Yaw;
		public ushort Z;

		public Pos Diff(Pos newPos)
		{
			var delta = new Pos
							{
								X = (ushort) (newPos.X - X),
								Y = (ushort) (newPos.Y - Y),
								Z = (ushort) (newPos.Z - Z),
								Yaw = (byte) (newPos.Yaw - Pitch),
								Pitch = (byte) (newPos.Yaw - Pitch)
							};

			return delta;
		}

		public bool IsZero()
		{
			if (X == 0 && Y == 0 && Z == 0 && Pitch == 0 && Yaw == 0)
				return true;
			return false;
		}

		public bool PosChanged()
		{
			if (X == 0 && Y == 0 && Z == 0)
				return false;
			return true;
		}

		public bool RotChanged()
		{
			if (Yaw == 0 && Pitch == 0)
				return false;
			return true;
		}

		public bool NeedTp()
		{
			if (Math.Abs(X) >= 32 || Math.Abs(Y) >= 32 || Math.Abs(Z) >= 32)
				return true;
			return false;
		}
	}
}