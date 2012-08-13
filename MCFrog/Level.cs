using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Compression;

namespace MCFrog
{
	public class Level
	{
		ASCIIEncoding encode = new ASCIIEncoding();
		internal byte[] blocks;

		internal string name;
		internal ushort sizeX, sizeY, sizeZ;
		internal PhysicsHandler physics;

		internal pos spawnPos;

		internal List<Player> players = new List<Player>();
		internal bool isUnloaded = false;

		static string levelFolder = "levels";
		string fileName
		{
			get
			{
				return levelFolder + "/" + name;
			}
		}
		string uncompressedExtension = ".LvU";
		string compressedExtension = ".LvC";
		string backupExtension = ".bak";

		string uncompressedPath
		{
			get
			{
				return fileName + uncompressedExtension;
			}
		}
		string compressedPath
		{
			get
			{
				return fileName + compressedExtension;
			}
		}
		string backupPath
		{
			get
			{
				return fileName + backupExtension;
			}
		}

		int headerSize = 80;

		FileStream fileHandle;

		internal Level(string fileName)
		{
			Load(fileName);

			Server.historyController.LoadHistory(name);
			UncompressAndCreateHandle();
		}
		internal Level(string _name, ushort x, ushort y, ushort z)
		{
			name = _name;
			Create(x, y, z);
			FullSave();

			Server.historyController.LoadHistory(name);
			UncompressAndCreateHandle();
		}

		void UncompressAndCreateHandle()
		{

		}

		void Load(string _name)
		{
			string tempPath = levelFolder + "/" + _name + compressedExtension;
			if (!File.Exists(tempPath)) throw new FileNotFoundException("Could not find file!", tempPath);

			FileStream fileStream = new FileStream(tempPath, FileMode.Open);
			GZipStream gzipStream = new GZipStream(fileStream, CompressionMode.Decompress);

			byte[] header = new byte[headerSize];
			gzipStream.Read(header, 0, headerSize);
			name = encode.GetString(header, 0, 64).Trim();
			sizeX = BitConverter.ToUInt16(header, 64);
			sizeY = BitConverter.ToUInt16(header, 66);
			sizeZ = BitConverter.ToUInt16(header, 68);
			spawnPos = new pos();
			spawnPos.x = BitConverter.ToUInt16(header, 70);
			spawnPos.y = BitConverter.ToUInt16(header, 72);
			spawnPos.z = BitConverter.ToUInt16(header, 74);
			spawnPos.pitch = header[76];
			spawnPos.pitch = header[77];

			bool physics = BitConverter.ToBoolean(header, 78);
			bool realistic = BitConverter.ToBoolean(header, 78);

			this.physics = new PhysicsHandler(this, physics);
			this.physics.realistic = realistic;

			int blockCount = sizeX * sizeY * sizeZ;
			blocks = new byte[blockCount];
			gzipStream.Read(blocks, 0, blockCount);

			gzipStream.Close();
			fileStream.Close();

			LevelHandler.levels.Add(this);
		}
		void Create(ushort inx, ushort iny, ushort inz) //todo add type
		{
			sizeX = inx;
			sizeY = iny;
			sizeZ = inz;

			spawnPos = new pos { x = (ushort)((sizeX * 32) / 2), y = (ushort)(((sizeY * 32) / 2) + 64), z = (ushort)((sizeZ * 32) / 2), pitch = 0, yaw = 0 };

			blocks = new byte[sizeX * sizeY * sizeZ];

			ushort half = (ushort)(sizeY / 2);
			for (ushort x = 0; x < sizeX; ++x)
			{
				for (ushort z = 0; z < sizeZ; ++z)
				{
					for (ushort y = 0; y < sizeY; ++y)
					{
						if (y != half)
						{
							SetTile(x, y, z, (byte)((y >= half) ? Blocks.Air : Blocks.Dirt));
						}
						else
						{
							SetTile(x, y, z, (byte)Blocks.Grass);
						}
					}
				}
			}

			physics = new PhysicsHandler(this, true);

			LevelHandler.levels.Add(this);

		}
		internal void Unload()
		{
			LevelHandler.levels.Remove(this);

			foreach (Player p in PlayerHandler.Players.Values.ToArray())
			{
				if (p.oldLevel == this) p.oldLevel = null;
				if (p.level == this) p.SwitchMap(LevelHandler.lobby);
			}

			foreach (Player pl in players.ToArray())
			{
				try
				{
					pl.SendKick("Internal Error (level has been unloaded)");
				}
				catch { }
			}

			FullSave();

			physics.shouldStop = true;
			physics = null;

			
			blocks = new byte[0];
		}
		internal void FullSave()
		{
			/*
			 * All we need to do here is dump our level blocks to the file
			 * 
			 * we dont need to close the filehandle for the uncompressed version
			 */


			Server.Log("Full Save Directory check...", LogTypesEnum.debug);
			DirectoryCheck();

			Server.Log("Backup old file...", LogTypesEnum.debug);
			if (File.Exists(fileName + compressedExtension))
			{
				if (File.Exists(fileName + compressedExtension + backupExtension)) File.Delete(fileName + compressedExtension + backupExtension);
				File.Copy(fileName + compressedExtension, fileName + compressedExtension + backupExtension);
			}

			Server.Log("Saving new File...", LogTypesEnum.debug);
			FileStream fileStream = new FileStream(compressedPath, FileMode.Create);
			GZipStream gzipStream = new GZipStream(fileStream, CompressionMode.Compress);

			gzipStream.Write(encode.GetBytes(name.PadRight(64)), 0, 64);
			gzipStream.Write(BitConverter.GetBytes(sizeX), 0, 2);
			gzipStream.Write(BitConverter.GetBytes(sizeY), 0, 2);
			gzipStream.Write(BitConverter.GetBytes(sizeZ), 0, 2);
			gzipStream.Write(BitConverter.GetBytes(spawnPos.x), 0, 2);
			gzipStream.Write(BitConverter.GetBytes(spawnPos.y), 0, 2);
			gzipStream.Write(BitConverter.GetBytes(spawnPos.z), 0, 2);
			gzipStream.WriteByte(spawnPos.pitch);
			gzipStream.WriteByte(spawnPos.yaw);
			gzipStream.Write(BitConverter.GetBytes(physics.isEnabled), 0, 1);
			gzipStream.Write(BitConverter.GetBytes(physics.realistic), 0, 1);

			gzipStream.Write(blocks, 0, blocks.Length);

			gzipStream.Flush();
			gzipStream.Close();
			//fileStream.Flush();
			fileStream.Close();

			Console.WriteLine("Done?");

		}
		internal void BlockSave(int pos)
		{
			int filePos = pos + headerSize;

			fileHandle.Position = filePos;
			fileHandle.WriteByte(blocks[pos]);

			//TODO test even if thsi will work?
		}

		void DirectoryCheck()
		{
			if (!Directory.Exists(levelFolder))
				Directory.CreateDirectory(levelFolder);
		}

		internal bool SetTile(ushort x, ushort y, ushort z, byte type)
		{
			if (NotInBounds(x, y, z)) { Server.Log("Out of bounds?", LogTypesEnum.debug); return false; }
			blocks[PosToInt(x, y, z)] = type;

			if (physics != null)
				for (int _X = -1; _X < 2; ++_X)
					for (int _Y = -1; _Y < 2; ++_Y)
						for (int _Z = -1; _Z < 2; ++_Z)
						{
							BlockPos blockPos = new BlockPos(x + _X, y + _Y, z + _Z);
							
							if (NotInBounds(blockPos)) continue;
							int pos = PosToInt(blockPos);
							if (blocks[pos] == 0) continue;
							if (physics.PhysicsUpdates.Contains(pos)) continue;
							physics.PhysicsUpdates.Add(pos);
						}

			return true;
		}
		internal bool BlockChange(ushort x, ushort y, ushort z, byte type)
		{
			if (SetTile(x, y, z, type))
			{
				foreach (Player p in players.ToArray())
				{
					if (p._isActive)
					{
						if (p.level == this)
						{
							p.SendBlockChange(x, y, z, type);
						}
					}
				}
				return true;
			}
			else
			{
				return false;
			}
		}
		internal bool PlayerBlockChange(Player changer, ushort x, ushort y, ushort z, byte type)
		{
			if (BlockChange(x, y, z, type))
			{
				try
				{
					Server.historyController.SetData(name, PosToInt(x, y, z), type, changer.UID);
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
			if (BlockChange(pos.x, pos.y, pos.z, type))
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
		internal bool PhysicsBlockChange(BlockPos pos, Blocks type)
		{
			return PhysicsBlockChange(pos, (byte)type);
		}

		internal byte GetTile(ushort x, ushort y, ushort z)
		{
			if (NotInBounds(x, y, z)) { Server.Log("Out of bounds?", LogTypesEnum.debug); return 255; }
			return blocks[PosToInt(x, y, z)];
		}
		internal Blocks GetTile(BlockPos pos)
		{
			return (Blocks)GetTile(pos.x, pos.y, pos.z);
		}
		internal Blocks GetTile(BlockPos pos, int diffX, int diffY, int diffZ)
		{
			ushort x = (ushort)(pos.x + diffX);
			ushort y = (ushort)(pos.y + diffY);
			ushort z = (ushort)(pos.z + diffZ);

			return (Blocks)GetTile(x, y, z);
		}

		public bool NotInBounds(ushort x, ushort y, ushort z)
		{
			return (x >= sizeX || y >= sizeY || z >= sizeZ);
		}
		public bool NotInBounds(BlockPos pos)
		{
			return NotInBounds(pos.x, pos.y, pos.z);
		}

		public int PosToInt(ushort x, ushort y, ushort z)
		{
			if (x < 0) { return -1; }
			if (x >= sizeX) { return -1; }
			if (y < 0) { return -1; }
			if (y >= sizeY) { return -1; }
			if (z < 0) { return -1; }
			if (z >= sizeZ) { return -1; }
			return x + z * sizeX + y * sizeX * sizeZ;
		}
		public int PosToInt(BlockPos pos)
		{
			return PosToInt(pos.x, pos.y, pos.z);
		}
		public BlockPos IntToPos(int pos)
		{
			ushort x;
			ushort y;
			ushort z;
			y = (ushort)(pos / sizeX / sizeZ); pos -= y * sizeX * sizeZ;
			z = (ushort)(pos / sizeX); pos -= z * sizeX; x = (ushort)pos;

			return new BlockPos(x, y, z);
		}
		public int IntOffset(int pos, int x, int y, int z)
		{
			return pos + x + z * sizeX + y * sizeX * sizeZ;
		}

		internal static Level Find(string name)
		{
			foreach (Level l in LevelHandler.levels.ToArray())
			{
				if (l.name == name) return l;
			}
			return null;
		}
	}
	public struct BlockPos
	{
		public ushort x, y, z;
		public BlockPos(ushort _x, ushort _y, ushort _z)
		{
			x = _x;
			y = _y;
			z = _z;
		}
		public BlockPos(int _x, int _y, int _z)
		{
			x = (ushort)_x;
			y = (ushort)_y;
			z = (ushort)_z;
		}

		public BlockPos diff(int _x, int _y, int _z)
		{
			return new BlockPos((ushort)(x + _x), (ushort)(y + _y), (ushort)(z + _z));
		}
	}
}
