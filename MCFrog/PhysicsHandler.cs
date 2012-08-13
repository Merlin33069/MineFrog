using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace MCFrog
{
	class PhysicsHandler
	{
		public delegate void Del(Level l, int pos);
		public static Dictionary<byte, Del> PhysicsTypes = new Dictionary<byte, Del>();

		public List<int> PhysicsUpdates = new List<int>();
		public Dictionary<int, byte> OtherData = new Dictionary<int, byte>();

		public int checksPerTick = 20;
		public int interval = 10;
		Level level;
		
		internal bool shouldStop = false;
		bool stopped = false;

		internal bool isEnabled = false; //Disabled by default
		
		internal bool realistic = true;
		internal bool finiteLiquids = true;
		internal byte waterCurrent = 7;
		internal byte lavaCurrent = 4;

		internal PhysicsHandler(Level l, bool enabled)
		{
			level = l;
			isEnabled = enabled;

			if (PhysicsTypes.Count == 0) return;

			if (enabled)
			{
				Server.Log("Initializing Physics on " + l.name + ", physics is currently ENABLED", LogTypesEnum.info);
			}
			else
			{
				Server.Log("Initializing Physics on " + l.name + ", physics is currently DISABLED", LogTypesEnum.info);
			}

			new System.Threading.Thread(new System.Threading.ThreadStart(TickTimer)).Start();
		}

		internal void TickTimer()
		{
			while (!Server.shouldShutdown && !level.isUnloaded && !shouldStop)
			{
				Tick();
				System.Threading.Thread.Sleep(interval);
			}
			stopped = true;
		}

		internal static void LoadPhysicsTypes()
		{
			Server.Log("Loading Physics blocks...", LogTypesEnum.system);

			PhysicsTypes.Add(2, Grass);
			PhysicsTypes.Add(3, Dirt);
			PhysicsTypes.Add(8, Water);
			PhysicsTypes.Add(9, StillWater);
			PhysicsTypes.Add(10, Lava);
			PhysicsTypes.Add(11, StillLava);
			PhysicsTypes.Add(12, Sand);
			PhysicsTypes.Add(13, Gravel);
			PhysicsTypes.Add(19, Sponge);
			PhysicsTypes.Add(44, HalfStair);
		}

		internal static void Test(int pos)
		{
			Server.Log("Physics Test", LogTypesEnum.debug);
			System.Threading.Thread.Sleep(1000);
		}

		void Tick()
		{
			//Server.Log("Physics TICK on " + level.name, LogTypesEnum.info);
			
			if (PhysicsUpdates.Count > 10000)
			{
				PhysicsUpdates.Clear();
				Server.Log("Physics was overloaded on " + level.name + ", physics has been reset.", LogTypesEnum.error);
				return;
			}

			for (int iterations = 0; iterations < checksPerTick; ++iterations)
			{
				if (PhysicsUpdates.Count == 0) return;
				
				int blockpos = PhysicsUpdates[0];
				PhysicsUpdates.Remove(blockpos);

				if(blockpos < 0 || blockpos >= level.blocks.Length) continue;
				byte type = level.blocks[blockpos];

				if (PhysicsTypes.ContainsKey(type))
				{
					//Console.WriteLine("rawr");
					PhysicsTypes[type].Invoke(level, blockpos);
				}
				else
				{
					iterations--;
				}
			}
		}

		#region PhysicsMethods
		static void Grass(Level level, int pos)
		{
			BlockPos blockPos = level.IntToPos(pos);
			BlockPos aboveBlockPos = blockPos.diff(0, 1, 0);

			int abovePos = level.PosToInt(aboveBlockPos);
			byte type = level.blocks[abovePos];

			if (Block.lightPass.Contains(type)) return;

			level.PhysicsBlockChange(blockPos, Blocks.Dirt);
		}
		static void Dirt(Level level, int pos)
		{
			BlockPos blockPos = level.IntToPos(pos);

			//This code works, but in order to change dirt into grass we have to check below every block in the same way, so we wont use it for now
			//if (level.physics.realistic)
			//{
			//    for (int y = blockPos.y + 1; y < level.sizeY; ++y)
			//    {
			//        BlockPos currentBlockPos = new BlockPos(blockPos.x, (ushort)y, blockPos.z);
			//        int currentPos = level.PosToInt(currentBlockPos);

			//        byte type = level.blocks[currentPos];

			//        if (!Block.LightPass.Contains(type)) { return; }
			//    }
			//}
			//else
			{
				BlockPos currentBlockPos = blockPos.diff(0, 1, 0);
				int currentPos = level.PosToInt(currentBlockPos);

				byte type = level.blocks[currentPos];

				if (!Block.lightPass.Contains(type)) return;
			}
			if (level.blocks[pos] != (byte)Blocks.Dirt) return;
			level.PhysicsBlockChange(blockPos, Blocks.Grass);
		}
		static void Water(Level level, int pos)
		{
			BlockPos blockPos = level.IntToPos(pos);
			BlockPos belowBlockPos = blockPos.diff(0, -1, 0);

			int belowPos = level.PosToInt(belowBlockPos);
			byte type = level.blocks[belowPos];
			
			if (type == (byte)Blocks.WaterStill)
			{
				level.physics.OtherData.Remove(belowPos);
				level.physics.OtherData.Add(belowPos, level.physics.waterCurrent);
			}
			else if (Block.crushable.Contains(type))
			{
				level.physics.OtherData.Remove(belowPos);
				level.physics.OtherData.Add(belowPos, level.physics.waterCurrent);
				level.PhysicsBlockChange(belowBlockPos, Blocks.WaterStill);
			}
			else
			{
				for (int _X = -1; _X < 2; ++_X)
					for (int _Z = -1; _Z < 2; ++_Z)
					{
						if (Math.Abs(_X) == 1 && Math.Abs(_Z) == 1) continue;
						if (blockPos.x + _X < 0 || blockPos.z + _Z < 0) continue;

						BlockPos aroundPos = blockPos.diff(_X, 0, _Z);
						
						if (level.NotInBounds(aroundPos)) continue;

						int newPos = level.PosToInt(aroundPos);
						
						if (level.blocks[newPos] == (byte)Blocks.WaterStill)
						{
							level.physics.OtherData.Remove(newPos);
							level.physics.OtherData.Add(newPos, level.physics.waterCurrent);
						}
						else if (Block.crushable.Contains(level.blocks[newPos]))
						{
							level.physics.OtherData.Remove(newPos);
							level.physics.OtherData.Add(newPos, level.physics.waterCurrent);
							level.PhysicsBlockChange(aroundPos, Blocks.WaterStill);
							//if (level.physics.PhysicsUpdates.Contains(level.PosToInt(aroundBelowPos))) return;
							//level.physics.PhysicsUpdates.Add(level.PosToInt(aroundBelowPos));
							return;
						}
						else
						{
							continue;
						}
					}
			}
		}
		static void StillWater(Level level, int pos)
		{
			BlockPos blockPos = level.IntToPos(pos);
			BlockPos belowBlockPos = blockPos.diff(0, -1, 0);

			if (!level.physics.OtherData.ContainsKey(pos)) return;

			byte current = (byte)(level.physics.OtherData[pos] - 1);
			if (current == 0) return;

			int belowPos = level.PosToInt(belowBlockPos);
			byte type = level.blocks[belowPos];
			
			if (type == (byte)Blocks.WaterStill)
			{
				if (level.physics.OtherData[belowPos] < current)
				{
					level.physics.OtherData.Remove(belowPos);
					level.physics.OtherData.Add(belowPos, current);
				}
			}
			else if (Block.crushable.Contains(type))
			{
				level.physics.OtherData.Remove(belowPos);
				level.physics.OtherData.Add(belowPos, current);
				level.PhysicsBlockChange(belowBlockPos, Blocks.WaterStill);
			}
			else
			{
				for (int _X = -1; _X < 2; ++_X)
					for (int _Z = -1; _Z < 2; ++_Z)
					{
						if (Math.Abs(_X) == 1 && Math.Abs(_Z) == 1) continue;
						if (blockPos.x + _X < 0 || blockPos.z + _Z < 0) continue;

						BlockPos aroundPos = blockPos.diff(_X, 0, _Z);

						if (level.NotInBounds(aroundPos)) continue;

						int newPos = level.PosToInt(aroundPos);
						byte newType = level.blocks[newPos];
						
						if (newType == (byte)Blocks.WaterStill)
						{
							if (level.physics.OtherData[newPos] >= current) continue;
							level.physics.OtherData.Remove(newPos);
							level.physics.OtherData.Add(newPos, current);
						}
						else if (Block.crushable.Contains(newType))
						{
							level.physics.OtherData.Remove(newPos);
							level.physics.OtherData.Add(newPos, current);
							level.PhysicsBlockChange(aroundPos, Blocks.WaterStill);
							//if (level.physics.PhysicsUpdates.Contains(level.PosToInt(aroundBelowPos))) return;
							//level.physics.PhysicsUpdates.Add(level.PosToInt(aroundBelowPos));
							return;
						}
						else
						{
							continue;
						}
					}
			}
		}
		static void Lava(Level level, int pos)
		{
			BlockPos blockPos = level.IntToPos(pos);
			BlockPos belowBlockPos = blockPos.diff(0, -1, 0);

			int belowPos = level.PosToInt(belowBlockPos);
			byte type = level.blocks[belowPos];
			
			if (type == (byte)Blocks.LavaStill)
			{
				level.physics.OtherData.Remove(belowPos);
				level.physics.OtherData.Add(belowPos, level.physics.lavaCurrent);
			}
			else if (Block.crushable.Contains(type))
			{
				level.physics.OtherData.Remove(belowPos);
				level.physics.OtherData.Add(belowPos, level.physics.lavaCurrent);
				level.PhysicsBlockChange(belowBlockPos, Blocks.LavaStill);
			}
			else
			{
				for (int _X = -1; _X < 2; ++_X)
					for (int _Z = -1; _Z < 2; ++_Z)
					{
						if (Math.Abs(_X) == 1 && Math.Abs(_Z) == 1) continue;
						if (blockPos.x + _X < 0 || blockPos.z + _Z < 0) continue;

						BlockPos aroundPos = blockPos.diff(_X, 0, _Z);

						if (level.NotInBounds(aroundPos)) continue;

						int newPos = level.PosToInt(aroundPos);
						
						if (level.blocks[newPos] == (byte)Blocks.LavaStill)
						{
							level.physics.OtherData.Remove(newPos);
							level.physics.OtherData.Add(newPos, level.physics.lavaCurrent);
						}
						else if (Block.crushable.Contains(level.blocks[newPos]))
						{
							level.physics.OtherData.Remove(newPos);
							level.physics.OtherData.Add(newPos, level.physics.lavaCurrent);
							level.PhysicsBlockChange(aroundPos, Blocks.LavaStill);
							//if (level.physics.PhysicsUpdates.Contains(level.PosToInt(aroundBelowPos))) return;
							//level.physics.PhysicsUpdates.Add(level.PosToInt(aroundBelowPos));
							return;
						}
						else
						{
							continue;
						}
					}
			}
		}
		static void StillLava(Level level, int pos)
		{
			BlockPos blockPos = level.IntToPos(pos);
			BlockPos belowBlockPos = blockPos.diff(0, -1, 0);

			if (!level.physics.OtherData.ContainsKey(pos)) return;

			byte current = (byte)(level.physics.OtherData[pos] - 1);
			if (current == 0) return;

			int belowPos = level.PosToInt(belowBlockPos);
			byte type = level.blocks[belowPos];

			if (type == (byte)Blocks.LavaStill)
			{
				if (level.physics.OtherData[belowPos] < current)
				{
					level.physics.OtherData.Remove(belowPos);
					level.physics.OtherData.Add(belowPos, current);
				}
			}
			else if (Block.crushable.Contains(type))
			{
				level.physics.OtherData.Remove(belowPos);
				level.physics.OtherData.Add(belowPos, current);
				level.PhysicsBlockChange(belowBlockPos, Blocks.LavaStill);
			}
			else
			{
				for (int _X = -1; _X < 2; ++_X)
					for (int _Z = -1; _Z < 2; ++_Z)
					{
						if (Math.Abs(_X) == 1 && Math.Abs(_Z) == 1) continue;
						if (blockPos.x + _X < 0 || blockPos.z + _Z < 0) continue;

						BlockPos aroundPos = blockPos.diff(_X, 0, _Z);

						if (level.NotInBounds(aroundPos)) continue;

						int newPos = level.PosToInt(aroundPos);
						byte newType = level.blocks[newPos];

						if (newType == (byte)Blocks.LavaStill)
						{
							if (level.physics.OtherData[newPos] >= current) continue;
							level.physics.OtherData.Remove(newPos);
							level.physics.OtherData.Add(newPos, current);
						}
						else if (Block.crushable.Contains(newType))
						{
							level.physics.OtherData.Remove(newPos);
							level.physics.OtherData.Add(newPos, current);
							level.PhysicsBlockChange(aroundPos, Blocks.LavaStill);
							//if (level.physics.PhysicsUpdates.Contains(level.PosToInt(aroundBelowPos))) return;
							//level.physics.PhysicsUpdates.Add(level.PosToInt(aroundBelowPos));
							return;
						}
						else
						{
							continue;
						}
					}
			}
		}
		static void Sand(Level level, int pos)
		{
			BlockPos blockPos = level.IntToPos(pos);
			BlockPos belowBlockPos = blockPos.diff(0, -1, 0);
			
			Blocks below = level.GetTile(belowBlockPos);

			if (below == Blocks.Zero)
			{
				return;
			}
			else if (Block.crushable.Contains((byte)below))
			{
				level.PhysicsBlockChange(belowBlockPos, Blocks.Sand);
				level.PhysicsBlockChange(blockPos, Blocks.Air);
				//if(level.physics.PhysicsUpdates.Contains(level.PosToInt(belowPos))) return;
				//level.physics.PhysicsUpdates.Add(level.PosToInt(belowPos));
			}
			else if (level.physics.realistic)
			{
				for(int _X = -1;_X<2;++_X)
					for (int _Z = -1; _Z < 2; ++_Z)
					{
						if (Math.Abs(_X) == 1 && Math.Abs(_Z) == 1) continue;
						if (belowBlockPos.x + _X < 0 || belowBlockPos.z + _Z < 0) continue;
						BlockPos aroundPos = blockPos.diff(_X, 0, _Z);
						BlockPos aroundBelowPos = belowBlockPos.diff(_X, 0, _Z);
						if (level.NotInBounds(aroundBelowPos)) continue;

						int testPos = level.PosToInt(aroundPos);
						int newPos = level.PosToInt(aroundBelowPos);
						if (Block.crushable.Contains(level.blocks[newPos]) && level.blocks[testPos] == 0)
						{
							level.PhysicsBlockChange(aroundBelowPos, Blocks.Sand);
							level.PhysicsBlockChange(blockPos, Blocks.Air);
							//if (level.physics.PhysicsUpdates.Contains(level.PosToInt(aroundBelowPos))) return;
							//level.physics.PhysicsUpdates.Add(level.PosToInt(aroundBelowPos));
							return;
						}
						else
						{
							continue;
						}
					}
			}
		}
		static void Gravel(Level level, int pos)
		{
			BlockPos blockPos = level.IntToPos(pos);
			BlockPos belowPos = blockPos.diff(0, -1, 0);

			Blocks below = level.GetTile(belowPos);

			if (below == Blocks.Zero)
			{
				return;
			}
			else if (Block.crushable.Contains((byte)below))
			{
				level.PhysicsBlockChange(belowPos, Blocks.Gravel);
				level.PhysicsBlockChange(blockPos, Blocks.Air);
				//if(level.physics.PhysicsUpdates.Contains(level.PosToInt(belowPos))) return;
				//level.physics.PhysicsUpdates.Add(level.PosToInt(belowPos));
			}
			else if (level.physics.realistic)
			{
				for (int _X = -1; _X < 2; ++_X)
					for (int _Z = -1; _Z < 2; ++_Z)
					{
						if (Math.Abs(_X) == 1 && Math.Abs(_Z) == 1) continue;
						if (belowPos.x + _X < 0 || belowPos.z + _Z < 0) continue;
						BlockPos aroundPos = blockPos.diff(_X, 0, _Z);
						BlockPos aroundBelowPos = belowPos.diff(_X, 0, _Z);
						if (level.NotInBounds(aroundBelowPos)) continue;

						int testPos = level.PosToInt(aroundPos);
						int newPos = level.PosToInt(aroundBelowPos);
						if (Block.crushable.Contains(level.blocks[newPos]) && level.blocks[testPos] == 0)
						{
							level.PhysicsBlockChange(aroundBelowPos, Blocks.Gravel);
							level.PhysicsBlockChange(blockPos, Blocks.Air);
							//if (level.physics.PhysicsUpdates.Contains(level.PosToInt(aroundBelowPos))) return;
							//level.physics.PhysicsUpdates.Add(level.PosToInt(aroundBelowPos));
							return;
						}
						else
						{
							continue;
						}
					}
			}

		}
		static void Sponge(Level level, int pos)
		{
			BlockPos blockPos = level.IntToPos(pos);

			for (int _x = -1; _x < 2; ++_x)
			{
				for (int _y = -1; _y < 2; ++_y)
				{
					for (int _z = -1; _z < 2; ++_z)
					{
						BlockPos currentBlockPos = blockPos.diff(_x, _y, _z);
						int currentPos = level.PosToInt(currentBlockPos);

						if (level.NotInBounds(currentBlockPos)) continue;

						Blocks type = (Blocks)level.blocks[currentPos];

						if (type == Blocks.Water || type == Blocks.WaterStill || type == Blocks.Lava || type == Blocks.LavaStill)
						{
							level.PhysicsBlockChange(currentBlockPos, Blocks.Air);
						}
					}
				}
			}
		}
		static void HalfStair(Level level, int pos)
		{
			BlockPos blockPos = level.IntToPos(pos);

			BlockPos belowBlockPos = blockPos.diff(0, -1, 0);
			int belowPos = level.PosToInt(belowBlockPos);

			if (level.blocks[belowPos] == (byte)Blocks.Staircasestep)
			{
				level.PhysicsBlockChange(belowBlockPos, Blocks.Staircasefull);
				level.PhysicsBlockChange(blockPos, Blocks.Air);
			}
		}


		#endregion
	}
}
