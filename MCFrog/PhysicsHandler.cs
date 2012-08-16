using System;
using System.Collections.Generic;
using System.Threading;

namespace MCFrog
{
	internal class PhysicsHandler
	{
		#region Delegates

		public delegate void Del(Level l, int pos);

		#endregion

		public static Dictionary<byte, Del> PhysicsTypes = new Dictionary<byte, Del>();
		private readonly Level _level;

		public int ChecksPerTick = 20;
		internal bool FiniteLiquids = true;
		public int Interval = 10;

		internal bool IsEnabled = false; //Disabled by default
		internal byte LavaCurrent = 4;
		public Dictionary<int, byte> OtherData = new Dictionary<int, byte>();
		public List<int> PhysicsUpdates = new List<int>();

		internal bool Realistic = true;
		internal bool ShouldStop = false;
		internal byte WaterCurrent = 7;

		internal PhysicsHandler(Level l, bool enabled)
		{
			_level = l;
			IsEnabled = enabled;

			if (PhysicsTypes.Count == 0) return;

			if (enabled)
			{
				Server.Log("Initializing Physics on " + l.Name + ", physics is currently ENABLED", LogTypesEnum.Info);
			}
			else
			{
				Server.Log("Initializing Physics on " + l.Name + ", physics is currently DISABLED", LogTypesEnum.Info);
			}

			new Thread(TickTimer).Start();
		}

		internal void TickTimer()
		{
			while (!Server.ShouldShutdown && !_level.IsUnloaded && !ShouldStop)
			{
				Tick();
				Thread.Sleep(Interval);
			}
		}

		internal static void LoadPhysicsTypes()
		{
			Server.Log("Loading Physics blocks...", LogTypesEnum.System);

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
			Server.Log("Physics Test", LogTypesEnum.Debug);
			Thread.Sleep(1000);
		}

		private void Tick()
		{
			//Server.Log("Physics TICK on " + level.name, LogTypesEnum.info);

			if (PhysicsUpdates.Count > 10000)
			{
				PhysicsUpdates.Clear();
				Server.Log("Physics was overloaded on " + _level.Name + ", physics has been reset.", LogTypesEnum.Error);
				return;
			}

			for (int iterations = 0; iterations < ChecksPerTick; ++iterations)
			{
				if (PhysicsUpdates.Count == 0) return;

				int blockpos = PhysicsUpdates[0];
				PhysicsUpdates.Remove(blockpos);

				if (blockpos < 0 || blockpos >= _level.BlockData.Length) continue;
				byte type = _level.BlockData[blockpos];

				if (PhysicsTypes.ContainsKey(type))
				{
					//Console.WriteLine("rawr");
					PhysicsTypes[type].Invoke(_level, blockpos);
				}
				else
				{
					iterations--;
				}
			}
		}

		#region PhysicsMethods

		private static void Grass(Level level, int pos)
		{
			BlockPos blockPos = level.IntToPos(pos);
			BlockPos aboveBlockPos = blockPos.Diff(0, 1, 0);

			int abovePos = level.PosToInt(aboveBlockPos);
			byte type = level.BlockData[abovePos];

			if (Block.LightPass.Contains(type)) return;

			level.PhysicsBlockChange(blockPos, Blocks.Dirt);
		}

		private static void Dirt(Level level, int pos)
		{
			BlockPos blockPos = level.IntToPos(pos);

			//This code works, but in order to change dirt into grass we have to check below every block in the same way, so we wont use it for now
			//if (level.physics.realistic)
			//{
			//	for (int y = blockPos.y + 1; y < level.sizeY; ++y)
			//	{
			//		BlockPos currentBlockPos = new BlockPos(blockPos.x, (ushort)y, blockPos.z);
			//		int currentPos = level.PosToInt(currentBlockPos);

			//		byte type = level.blocks[currentPos];

			//		if (!Block.LightPass.Contains(type)) { return; }
			//	}
			//}
			//else
			{
				BlockPos currentBlockPos = blockPos.Diff(0, 1, 0);
				int currentPos = level.PosToInt(currentBlockPos);

				byte type = level.BlockData[currentPos];

				if (!Block.LightPass.Contains(type)) return;
			}
			if (level.BlockData[pos] != (byte) Blocks.Dirt) return;
			level.PhysicsBlockChange(blockPos, Blocks.Grass);
		}

		private static void Water(Level level, int pos)
		{
			BlockPos blockPos = level.IntToPos(pos);
			BlockPos belowBlockPos = blockPos.Diff(0, -1, 0);

			int belowPos = level.PosToInt(belowBlockPos);
			byte type = level.BlockData[belowPos];

			if (type == (byte) Blocks.WaterStill)
			{
				level.Physics.OtherData.Remove(belowPos);
				level.Physics.OtherData.Add(belowPos, level.Physics.WaterCurrent);
			}
			else if (Block.Crushable.Contains(type))
			{
				level.Physics.OtherData.Remove(belowPos);
				level.Physics.OtherData.Add(belowPos, level.Physics.WaterCurrent);
				level.PhysicsBlockChange(belowBlockPos, Blocks.WaterStill);
			}
			else
			{
				for (int x = -1; x < 2; ++x)
					for (int z = -1; z < 2; ++z)
					{
						if (Math.Abs(x) == 1 && Math.Abs(z) == 1) continue;
						if (blockPos.X + x < 0 || blockPos.Z + z < 0) continue;

						BlockPos aroundPos = blockPos.Diff(x, 0, z);

						if (level.NotInBounds(aroundPos)) continue;

						int newPos = level.PosToInt(aroundPos);

						if (level.BlockData[newPos] == (byte) Blocks.WaterStill)
						{
							level.Physics.OtherData.Remove(newPos);
							level.Physics.OtherData.Add(newPos, level.Physics.WaterCurrent);
						}
						else if (Block.Crushable.Contains(level.BlockData[newPos]))
						{
							level.Physics.OtherData.Remove(newPos);
							level.Physics.OtherData.Add(newPos, level.Physics.WaterCurrent);
							level.PhysicsBlockChange(aroundPos, Blocks.WaterStill);
							//if (level.physics.PhysicsUpdates.Contains(level.PosToInt(aroundBelowPos))) return;
							//level.physics.PhysicsUpdates.Add(level.PosToInt(aroundBelowPos));
							return;
						}
					}
			}
		}

		private static void StillWater(Level level, int pos)
		{
			BlockPos blockPos = level.IntToPos(pos);
			BlockPos belowBlockPos = blockPos.Diff(0, -1, 0);

			if (!level.Physics.OtherData.ContainsKey(pos)) return;

			var current = (byte) (level.Physics.OtherData[pos] - 1);
			if (current == 0) return;

			int belowPos = level.PosToInt(belowBlockPos);
			byte type = level.BlockData[belowPos];

			if (type == (byte) Blocks.WaterStill)
			{
				if (level.Physics.OtherData[belowPos] < current)
				{
					level.Physics.OtherData.Remove(belowPos);
					level.Physics.OtherData.Add(belowPos, current);
				}
			}
			else if (Block.Crushable.Contains(type))
			{
				level.Physics.OtherData.Remove(belowPos);
				level.Physics.OtherData.Add(belowPos, current);
				level.PhysicsBlockChange(belowBlockPos, Blocks.WaterStill);
			}
			else
			{
				for (int x = -1; x < 2; ++x)
					for (int z = -1; z < 2; ++z)
					{
						if (Math.Abs(x) == 1 && Math.Abs(z) == 1) continue;
						if (blockPos.X + x < 0 || blockPos.Z + z < 0) continue;

						BlockPos aroundPos = blockPos.Diff(x, 0, z);

						if (level.NotInBounds(aroundPos)) continue;

						int newPos = level.PosToInt(aroundPos);
						byte newType = level.BlockData[newPos];

						if (newType == (byte) Blocks.WaterStill)
						{
							if (level.Physics.OtherData[newPos] >= current) continue;
							level.Physics.OtherData.Remove(newPos);
							level.Physics.OtherData.Add(newPos, current);
						}
						else if (Block.Crushable.Contains(newType))
						{
							level.Physics.OtherData.Remove(newPos);
							level.Physics.OtherData.Add(newPos, current);
							level.PhysicsBlockChange(aroundPos, Blocks.WaterStill);
							//if (level.physics.PhysicsUpdates.Contains(level.PosToInt(aroundBelowPos))) return;
							//level.physics.PhysicsUpdates.Add(level.PosToInt(aroundBelowPos));
							return;
						}
					}
			}
		}

		private static void Lava(Level level, int pos)
		{
			BlockPos blockPos = level.IntToPos(pos);
			BlockPos belowBlockPos = blockPos.Diff(0, -1, 0);

			int belowPos = level.PosToInt(belowBlockPos);
			byte type = level.BlockData[belowPos];

			if (type == (byte) Blocks.LavaStill)
			{
				level.Physics.OtherData.Remove(belowPos);
				level.Physics.OtherData.Add(belowPos, level.Physics.LavaCurrent);
			}
			else if (Block.Crushable.Contains(type))
			{
				level.Physics.OtherData.Remove(belowPos);
				level.Physics.OtherData.Add(belowPos, level.Physics.LavaCurrent);
				level.PhysicsBlockChange(belowBlockPos, Blocks.LavaStill);
			}
			else
			{
				for (int x = -1; x < 2; ++x)
					for (int z = -1; z < 2; ++z)
					{
						if (Math.Abs(x) == 1 && Math.Abs(z) == 1) continue;
						if (blockPos.X + x < 0 || blockPos.Z + z < 0) continue;

						BlockPos aroundPos = blockPos.Diff(x, 0, z);

						if (level.NotInBounds(aroundPos)) continue;

						int newPos = level.PosToInt(aroundPos);

						if (level.BlockData[newPos] == (byte) Blocks.LavaStill)
						{
							level.Physics.OtherData.Remove(newPos);
							level.Physics.OtherData.Add(newPos, level.Physics.LavaCurrent);
						}
						else if (Block.Crushable.Contains(level.BlockData[newPos]))
						{
							level.Physics.OtherData.Remove(newPos);
							level.Physics.OtherData.Add(newPos, level.Physics.LavaCurrent);
							level.PhysicsBlockChange(aroundPos, Blocks.LavaStill);
							//if (level.physics.PhysicsUpdates.Contains(level.PosToInt(aroundBelowPos))) return;
							//level.physics.PhysicsUpdates.Add(level.PosToInt(aroundBelowPos));
							return;
						}
					}
			}
		}

		private static void StillLava(Level level, int pos)
		{
			BlockPos blockPos = level.IntToPos(pos);
			BlockPos belowBlockPos = blockPos.Diff(0, -1, 0);

			if (!level.Physics.OtherData.ContainsKey(pos)) return;

			var current = (byte) (level.Physics.OtherData[pos] - 1);
			if (current == 0) return;

			int belowPos = level.PosToInt(belowBlockPos);
			byte type = level.BlockData[belowPos];

			if (type == (byte) Blocks.LavaStill)
			{
				if (level.Physics.OtherData[belowPos] < current)
				{
					level.Physics.OtherData.Remove(belowPos);
					level.Physics.OtherData.Add(belowPos, current);
				}
			}
			else if (Block.Crushable.Contains(type))
			{
				level.Physics.OtherData.Remove(belowPos);
				level.Physics.OtherData.Add(belowPos, current);
				level.PhysicsBlockChange(belowBlockPos, Blocks.LavaStill);
			}
			else
			{
				for (int x = -1; x < 2; ++x)
					for (int z = -1; z < 2; ++z)
					{
						if (Math.Abs(x) == 1 && Math.Abs(z) == 1) continue;
						if (blockPos.X + x < 0 || blockPos.Z + z < 0) continue;

						BlockPos aroundPos = blockPos.Diff(x, 0, z);

						if (level.NotInBounds(aroundPos)) continue;

						int newPos = level.PosToInt(aroundPos);
						byte newType = level.BlockData[newPos];

						if (newType == (byte) Blocks.LavaStill)
						{
							if (level.Physics.OtherData[newPos] >= current) continue;
							level.Physics.OtherData.Remove(newPos);
							level.Physics.OtherData.Add(newPos, current);
						}
						else if (Block.Crushable.Contains(newType))
						{
							level.Physics.OtherData.Remove(newPos);
							level.Physics.OtherData.Add(newPos, current);
							level.PhysicsBlockChange(aroundPos, Blocks.LavaStill);
							//if (level.physics.PhysicsUpdates.Contains(level.PosToInt(aroundBelowPos))) return;
							//level.physics.PhysicsUpdates.Add(level.PosToInt(aroundBelowPos));
							return;
						}
					}
			}
		}

		private static void Sand(Level level, int pos)
		{
			BlockPos blockPos = level.IntToPos(pos);
			BlockPos belowBlockPos = blockPos.Diff(0, -1, 0);

			Blocks below = level.GetTile(belowBlockPos);

			if (below == Blocks.Zero)
			{
			}
			else if (Block.Crushable.Contains((byte) below))
			{
				level.PhysicsBlockChange(belowBlockPos, Blocks.Sand);
				level.PhysicsBlockChange(blockPos, Blocks.Air);
				//if(level.physics.PhysicsUpdates.Contains(level.PosToInt(belowPos))) return;
				//level.physics.PhysicsUpdates.Add(level.PosToInt(belowPos));
			}
			else if (level.Physics.Realistic)
			{
				for (int x = -1; x < 2; ++x)
					for (int z = -1; z < 2; ++z)
					{
						if (Math.Abs(x) == 1 && Math.Abs(z) == 1) continue;
						if (belowBlockPos.X + x < 0 || belowBlockPos.Z + z < 0) continue;
						BlockPos aroundPos = blockPos.Diff(x, 0, z);
						BlockPos aroundBelowPos = belowBlockPos.Diff(x, 0, z);
						if (level.NotInBounds(aroundBelowPos)) continue;

						int testPos = level.PosToInt(aroundPos);
						int newPos = level.PosToInt(aroundBelowPos);
						if (Block.Crushable.Contains(level.BlockData[newPos]) && level.BlockData[testPos] == 0)
						{
							level.PhysicsBlockChange(aroundBelowPos, Blocks.Sand);
							level.PhysicsBlockChange(blockPos, Blocks.Air);
							//if (level.physics.PhysicsUpdates.Contains(level.PosToInt(aroundBelowPos))) return;
							//level.physics.PhysicsUpdates.Add(level.PosToInt(aroundBelowPos));
							return;
						}
					}
			}
		}

		private static void Gravel(Level level, int pos)
		{
			BlockPos blockPos = level.IntToPos(pos);
			BlockPos belowPos = blockPos.Diff(0, -1, 0);

			Blocks below = level.GetTile(belowPos);

			if (below == Blocks.Zero)
			{
			}
			else if (Block.Crushable.Contains((byte) below))
			{
				level.PhysicsBlockChange(belowPos, Blocks.Gravel);
				level.PhysicsBlockChange(blockPos, Blocks.Air);
				//if(level.physics.PhysicsUpdates.Contains(level.PosToInt(belowPos))) return;
				//level.physics.PhysicsUpdates.Add(level.PosToInt(belowPos));
			}
			else if (level.Physics.Realistic)
			{
				for (int x = -1; x < 2; ++x)
					for (int z = -1; z < 2; ++z)
					{
						if (Math.Abs(x) == 1 && Math.Abs(z) == 1) continue;
						if (belowPos.X + x < 0 || belowPos.Z + z < 0) continue;
						BlockPos aroundPos = blockPos.Diff(x, 0, z);
						BlockPos aroundBelowPos = belowPos.Diff(x, 0, z);
						if (level.NotInBounds(aroundBelowPos)) continue;

						int testPos = level.PosToInt(aroundPos);
						int newPos = level.PosToInt(aroundBelowPos);
						if (Block.Crushable.Contains(level.BlockData[newPos]) && level.BlockData[testPos] == 0)
						{
							level.PhysicsBlockChange(aroundBelowPos, Blocks.Gravel);
							level.PhysicsBlockChange(blockPos, Blocks.Air);
							//if (level.physics.PhysicsUpdates.Contains(level.PosToInt(aroundBelowPos))) return;
							//level.physics.PhysicsUpdates.Add(level.PosToInt(aroundBelowPos));
							return;
						}
					}
			}
		}

		private static void Sponge(Level level, int pos)
		{
			BlockPos blockPos = level.IntToPos(pos);

			for (int x = -1; x < 2; ++x)
			{
				for (int y = -1; y < 2; ++y)
				{
					for (int z = -1; z < 2; ++z)
					{
						BlockPos currentBlockPos = blockPos.Diff(x, y, z);
						int currentPos = level.PosToInt(currentBlockPos);

						if (level.NotInBounds(currentBlockPos)) continue;

						var type = (Blocks) level.BlockData[currentPos];

						if (type == Blocks.Water || type == Blocks.WaterStill || type == Blocks.Lava ||
							type == Blocks.LavaStill)
						{
							level.PhysicsBlockChange(currentBlockPos, Blocks.Air);
						}
					}
				}
			}
		}

		private static void HalfStair(Level level, int pos)
		{
			BlockPos blockPos = level.IntToPos(pos);

			BlockPos belowBlockPos = blockPos.Diff(0, -1, 0);
			int belowPos = level.PosToInt(belowBlockPos);

			if (level.BlockData[belowPos] == (byte) Blocks.Staircasestep)
			{
				level.PhysicsBlockChange(belowBlockPos, Blocks.Staircasefull);
				level.PhysicsBlockChange(blockPos, Blocks.Air);
			}
		}

		#endregion
	}
}