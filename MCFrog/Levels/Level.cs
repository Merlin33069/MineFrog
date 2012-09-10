using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace MineFrog
{
	public class Level
	{
		private const string LevelFolder = "levels";
		private const string CompressedExtension = ".LvC";
		private const string BackupExtension = ".bak";
		private const int HeaderSize = 82;
		private readonly ASCIIEncoding _encode= new ASCIIEncoding();
		//private readonly FileStream _fileHandle;
		public byte[] BlockData;
		public bool IsUnloaded = false;

		public string Name;
		public PhysicsHandler Physics;
		public List<Player> Players = new List<Player>();
		public ushort SizeX;
		public ushort SizeY;
		public ushort SizeZ;

		internal int BlockChangeCount;
		internal int UpdateCountToSave = 0; //This counts up to the frequency, so we know when to save :D
		internal int UpdateCountSaveFrequency = 30; //Each update check is 10 seconds, so this is 5 mins

		internal byte VisitPermissions = 1;
		internal byte BuildPermissions = 1;

		public byte WaterCurrent
		{
			get { return Physics.WaterCurrent; }
			set { Physics.WaterCurrent = value; }
		}
		public byte LavaCurrent
		{
			get { return Physics.LavaCurrent; }
			set { Physics.LavaCurrent = value; }
		}

		public Pos SpawnPos;

		public Level(string fileName, bool shouldCreateIfNotExist)
		{
			try
			{
				Load(fileName);
			}
			catch
			{
				try
				{
					Name = fileName;
					Create(128, 64, 128);
					FullSave();
				}
				catch (Exception e)
				{
					Server.Log(e, LogTypesEnum.Error);
				}
			}
			

			Server.HistoryController.LoadHistory(Name);
			UncompressAndCreateHandle();
		}

		public Level(string name, ushort x, ushort y, ushort z)
		{
			try
			{
				Name = name;
				Create(x, y, z);
				FullSave();

				Server.HistoryController.LoadHistory(Name);
				UncompressAndCreateHandle();
			}
			catch (Exception e)
			{
				Server.Log(e, LogTypesEnum.Error);
			}
		}

		private string FileName
		{
			get { return LevelFolder + "/" + Name; }
		}

		private string CompressedPath
		{
			get { return FileName + CompressedExtension; }
		}

		private void UncompressAndCreateHandle()
		{
		}

		public void Load(string name)
		{
			string tempPath = LevelFolder + "/" + name + CompressedExtension;
			if (!File.Exists(tempPath)) throw new FileNotFoundException("Could not find file!", tempPath);

			var fileStream = new FileStream(tempPath, FileMode.Open);
			var gzipStream = new GZipStream(fileStream, CompressionMode.Decompress);

			var header = new byte[HeaderSize];
			gzipStream.Read(header, 0, HeaderSize);
			Name = _encode.GetString(header, 0, 64).Trim();
			SizeX = BitConverter.ToUInt16(header, 64);
			SizeY = BitConverter.ToUInt16(header, 66);
			SizeZ = BitConverter.ToUInt16(header, 68);
			Server.Log("Loading level " + Name + " which is " + SizeX + "x" + SizeY + "x" + SizeZ, LogTypesEnum.Debug);
			SpawnPos = new Pos
						   {
							   X = BitConverter.ToUInt16(header, 70),
							   Y = BitConverter.ToUInt16(header, 72),
							   Z = BitConverter.ToUInt16(header, 74),
							   Pitch = header[76]
						   };
			SpawnPos.Pitch = header[77];

			bool physics = BitConverter.ToBoolean(header, 78);
			bool realistic = BitConverter.ToBoolean(header, 79);

			BuildPermissions = header[80];
			VisitPermissions = header[81];

			Physics = new PhysicsHandler(this, physics) {Realistic = realistic};

			int blockCount = SizeX*SizeY*SizeZ;
			BlockData = new byte[blockCount];
			gzipStream.Read(BlockData, 0, blockCount);

			gzipStream.Close();
			fileStream.Close();

			LevelHandler.Levels.Add(this);
		}

		public void Create(ushort inx, ushort iny, ushort inz) //todo add type
		{
			SizeX = inx;
			SizeY = iny;
			SizeZ = inz;

			SpawnPos = new Pos
						   {
							   X = (ushort) ((SizeX*32)/2),
							   Y = (ushort) (((SizeY*32)/2) + 64),
							   Z = (ushort) ((SizeZ*32)/2),
							   Pitch = 0,
							   Yaw = 0
						   };

			BlockData = new byte[SizeX*SizeY*SizeZ];

			var half = (ushort) (SizeY/2);
			for (ushort x = 0; x < SizeX; ++x)
			{
				for (ushort z = 0; z < SizeZ; ++z)
				{
					for (ushort y = 0; y < SizeY; ++y)
					{
						if (y != half)
						{
							SetTile(x, y, z, (byte) ((y >= half) ? MCBlocks.Air : MCBlocks.Dirt));
						}
						else
						{
							SetTile(x, y, z, (byte) MCBlocks.Grass);
						}
					}
				}
			}

			Physics = new PhysicsHandler(this, true);

			LevelHandler.Levels.Add(this);
		}

		public void Unload()
		{
			LevelHandler.Levels.Remove(this);

			foreach (Player p in PlayerHandler.Players.Values.ToArray())
			{
				if (p.OldLevel == this) p.OldLevel = null;
				if (p.Level == this) p.SwitchMap(LevelHandler.Lobby);
			}

			foreach (Player pl in Players.ToArray())
			{
				pl.SendKick("Internal Error (level has been unloaded)");
			}

			FullSave();

			Physics.ShouldStop = true;
			Physics = null;


			BlockData = new byte[0];
		}

		public void FullSave()
		{
			/*
			 * All we need to do here is dump our level blocks to the file
			 * 
			 * we dont need to close the filehandle for the uncompressed version
			 */


			Server.Log("Full Save Directory check...", LogTypesEnum.Debug);
			DirectoryCheck();

			Server.Log("Backup old file...", LogTypesEnum.Debug);
			if (File.Exists(FileName + CompressedExtension))
			{
				if (File.Exists(FileName + CompressedExtension + BackupExtension))
					File.Delete(FileName + CompressedExtension + BackupExtension);
				File.Copy(FileName + CompressedExtension, FileName + CompressedExtension + BackupExtension);
			}

			Server.Log("Saving new File...", LogTypesEnum.Debug);
			var fileStream = new FileStream(CompressedPath, FileMode.Create);
			var gzipStream = new GZipStream(fileStream, CompressionMode.Compress);

			gzipStream.Write(_encode.GetBytes(Name.PadRight(64)), 0, 64);
			gzipStream.Write(BitConverter.GetBytes(SizeX), 0, 2);
			gzipStream.Write(BitConverter.GetBytes(SizeY), 0, 2);
			gzipStream.Write(BitConverter.GetBytes(SizeZ), 0, 2);
			gzipStream.Write(BitConverter.GetBytes(SpawnPos.X), 0, 2);
			gzipStream.Write(BitConverter.GetBytes(SpawnPos.Y), 0, 2);
			gzipStream.Write(BitConverter.GetBytes(SpawnPos.Z), 0, 2);
			gzipStream.WriteByte(SpawnPos.Pitch);
			gzipStream.WriteByte(SpawnPos.Yaw);
			gzipStream.Write(BitConverter.GetBytes(Physics.IsEnabled), 0, 1);
			gzipStream.Write(BitConverter.GetBytes(Physics.Realistic), 0, 1);
			gzipStream.WriteByte(BuildPermissions);
			gzipStream.WriteByte(VisitPermissions);

			gzipStream.Write(BlockData, 0, BlockData.Length);

			gzipStream.Flush();
			gzipStream.Close();
			//fileStream.Flush();
			fileStream.Close();

			Console.WriteLine("Done?");
		}

		//internal void BlockSave(int pos)
		//{
		//	int filePos = pos + HeaderSize;

		//	_fileHandle.Position = filePos;
		//	_fileHandle.WriteByte(BlockData[pos]);

		//	//TODO test even if thsi will work?
		//}

		private void DirectoryCheck()
		{
			if (!Directory.Exists(LevelFolder))
				Directory.CreateDirectory(LevelFolder);
		}

		public bool SetTile(ushort x, ushort y, ushort z, byte type)
		{
			if (NotInBounds(x, y, z))
			{
				Server.Log("Out of bounds?", LogTypesEnum.Debug);
				return false;
			}
			BlockData[PosToInt(x, y, z)] = type;

			BlockChangeCount++;

			if (Physics != null)
				PhysicsCheck(x, y, z);

			return true;
		}

		public void PhysicsCheck(ushort inX, ushort inY, ushort inZ)
		{
			for (int x = -1; x < 2; ++x)
				for (int y = -1; y < 2; ++y)
					for (int z = -1; z < 2; ++z)
					{
						var blockPos = new BlockPos(inX + x, inY + y, inZ + z, this);

						if (NotInBounds(blockPos)) continue;
						int pos = PosToInt(blockPos);
						if (BlockData[pos] == 0) continue;
						if (Physics.PhysicsUpdates.Contains(pos)) continue;
						Physics.AddCall(pos);
					}
		}

		public bool BlockChange(ushort x, ushort y, ushort z, byte type)
		{
			if (SetTile(x, y, z, type))
			{
				foreach (Player p in Players.ToArray())
				{
					if (p.IsActive)
					{
						if (p.Level == this)
						{
							p.SendBlockChange(x, y, z, type);
						}
					}
				}
				return true;
			}
			return false;
		}

		internal bool PlayerBlockChange(Player changer, ushort x, ushort y, ushort z, byte type)
		{
			if (BlockChange(x, y, z, type))
			{
				try
				{
					Server.HistoryController.SetData(Name, PosToInt(x, y, z), type, changer.UID);
				}
				catch (Exception e)
				{
					Console.WriteLine(e.Message);
					Console.WriteLine(e.StackTrace);
				}
				return true;
			}
			return false;
		}

		internal bool PhysicsBlockChange(BlockPos pos, byte type)
		{
			if (BlockChange(pos.X, pos.Y, pos.Z, type))
			{
				try
				{
					//Server.historyController.SetData(name, PosToInt(pos.x, pos.y, pos.z), type, 0);
				}
				catch (Exception e)
				{
					Console.WriteLine(e.Message);
					Console.WriteLine(e.StackTrace);
				}
				return true;
			}
			return false;
		}

		public bool PhysicsBlockChange(BlockPos pos, MCBlocks type)
		{
			return PhysicsBlockChange(pos, (byte) type);
		}

		public byte GetTile(int pos)
		{
			return BlockData[pos];
		}

		public byte GetTile(ushort x, ushort y, ushort z)
		{
			if (NotInBounds(x, y, z))
			{
				Server.Log("Out of bounds?", LogTypesEnum.Debug);
				return 255;
			}
			return BlockData[PosToInt(x, y, z)];
		}

		public MCBlocks GetTile(BlockPos pos)
		{
			return (MCBlocks) GetTile(pos.X, pos.Y, pos.Z);
		}

		public MCBlocks GetTile(BlockPos pos, int diffX, int diffY, int diffZ)
		{
			var x = (ushort) (pos.X + diffX);
			var y = (ushort) (pos.Y + diffY);
			var z = (ushort) (pos.Z + diffZ);

			return (MCBlocks) GetTile(x, y, z);
		}

		public bool NotInBounds(ushort x, ushort y, ushort z)
		{
			return (x >= SizeX || y >= SizeY || z >= SizeZ);
		}

		public bool NotInBounds(BlockPos pos)
		{
			return NotInBounds(pos.X, pos.Y, pos.Z);
		}

		public int PosToInt(ushort x, ushort y, ushort z)
		{
			if (x >= SizeX)
			{
				return -1;
			}
			if (y >= SizeY)
			{
				return -1;
			}
			if (z >= SizeZ)
			{
				return -1;
			}
			return x + z*SizeX + y*SizeX*SizeZ;
		}

		public int PosToInt(BlockPos pos)
		{
			return PosToInt(pos.X, pos.Y, pos.Z);
		}

		public BlockPos IntToBlockPos(int pos)
		{
			//TODO make int to block pos more efficient (if possible)
			int index = pos;
			var y = (ushort) (pos/SizeX/SizeZ);
			pos -= y*SizeX*SizeZ;
			var z = (ushort) (pos/SizeX);
			pos -= z*SizeX;
			var x = (ushort) pos;

			return new BlockPos(x, y, z, index, this);
		}

		public int IntOffset(int pos, int x, int y, int z)
		{
			return pos + x + z*SizeX + y*SizeX*SizeZ;
		}

		public static Level Find(string name)
		{
			return LevelHandler.Levels.ToArray().FirstOrDefault(l => l.Name == name);
		}

		internal void SaveCheck()
		{
			//Server.Log("Checking if level should save " + Name, LogTypesEnum.Debug);
			UpdateCountToSave++;

			if (UpdateCountToSave >= UpdateCountSaveFrequency || BlockChangeCount >= 5000)
			{
				Player.SendGlobalMessage("Saving Level: " + Name);
				FullSave();

				UpdateCountToSave = 0;
				BlockChangeCount = 0;
			}
		}

	}

	//Declaring this outside the scope of a class so that we can just use it.S
	public delegate bool BlockCheck(BlockPos pos);

	public struct BlockPos
	{
		public ushort X;
		public ushort Y;
		public ushort Z;
		public int Index;
		public readonly Level Level;
		public bool InBounds;
		public byte BlockType;
		public MCBlocks BlockMCType;

		public BlockPos Below
		{
			get { return Diff(0, -1, 0); }
		}
		public BlockPos Above
		{
			get { return Diff(0, 1, 0); }
		}

		public BlockPos(ushort x, ushort y, ushort z, int pos, Level l)
		{
			X = x;
			Y = y;
			Z = z;
			Level = l;
			Index = pos;
			InBounds = !l.NotInBounds(X, Y, Z);
			BlockType = !InBounds ? (byte)255 : l.GetTile(Index);
			BlockMCType = (MCBlocks) BlockType;
		}
		public BlockPos(int x, int y, int z, int pos, Level l)
		{
			X = (ushort) x;
			Y = (ushort) y;
			Z = (ushort)z;
			Level = l;
			Index = pos;
			InBounds = !l.NotInBounds(X, Y, Z);
			BlockType = !InBounds ? (byte)255 : l.GetTile(Index);
			BlockMCType = (MCBlocks)BlockType;
		}

		public BlockPos(ushort x, ushort y, ushort z, Level l)
		{
			X = x;
			Y = y;
			Z = z;
			Level = l;
			Index = l.PosToInt(X, Y, Z);
			InBounds = !l.NotInBounds(X, Y, Z);
			BlockType = !InBounds ? (byte)255 : l.GetTile(Index);
			BlockMCType = (MCBlocks)BlockType;
		}
		public BlockPos(int x, int y, int z, Level l)
		{
			X = (ushort)x;
			Y = (ushort)y;
			Z = (ushort)z;
			Level = l;
			Index = l.PosToInt(X, Y, Z);
			InBounds = !l.NotInBounds(X, Y, Z);
			BlockType = !InBounds ? (byte)255 : l.GetTile(Index);
			BlockMCType = (MCBlocks)BlockType;
		}
		
		public void Around(BlockCheck check)
		{
			for(var inx = -1; inx < 2; inx++)
			{
				for (var inz = -1; inz < 2; inz++)
				{
					//This skips checking of the center block
					if (inx == 0 && inz == 0) continue;

					//This checks to make sure were not checking corners
					if (Math.Abs(inx) == 1 && Math.Abs(inz) == 1) continue;

					//Create a new BlockPos struct
					BlockPos blockPos = Diff(inx, 0, inz);

					//Check if were inbounds, if not we don't check this one
					if (!blockPos.InBounds) continue;

					//Perform the delegate call!
					check(blockPos);
				}
			}
		}

		public BlockPos Diff(int x, int y, int z)
		{
			return new BlockPos((ushort) (X + x), (ushort) (Y + y), (ushort) (Z + z), Level);
		}
	}
}