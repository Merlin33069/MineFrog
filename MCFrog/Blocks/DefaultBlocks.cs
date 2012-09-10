using System;
using System.Collections.Generic;

namespace MineFrog.Blocks
{
	public class Air : Block
	{
		public override string Name
		{
			get { return "Air"; }
		}

		public override bool IsCrushable
		{
			get { return true; }
		}
		public override bool CanLightPass
		{
			get { return true; }
		}
		
		public override byte ThisID
		{
			get { return 0; }
		}
	}
	public class Stone : Block
	{
		public override string Name
		{
			get { return "Stone"; }
		}

		public override byte ThisID
		{
			get { return 1; }
		}
	}
	public class Grass : Block
	{
		public override string Name
		{
			get { return "Grass"; }
		}

		public override byte ThisID
		{
			get { return 2; }
		}

		public override void Physics(Level level, int pos)
		{
			BlockPos blockPos = level.IntToBlockPos(pos);
			BlockPos aboveBlockPos = blockPos.Diff(0, 1, 0);

			int abovePos = level.PosToInt(aboveBlockPos);
			byte type = level.BlockData[abovePos];

			if (LightPass.Contains(type)) return;

			level.PhysicsBlockChange(blockPos, MCBlocks.Dirt);
		}
	}
	public class Dirt : Block
	{
		public override string Name
		{
			get { return "Dirt"; }
		}

		public override ushort PhysicsDelay
		{
			get { return 100; }
		}

		public override byte ThisID
		{
			get { return 3; }
		}

		public override void Physics(Level level, int pos)
		{
			BlockPos blockPos = level.IntToBlockPos(pos);

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

				if (!LightPass.Contains(type)) return;
			}
			if (level.BlockData[pos] != (byte)MCBlocks.Dirt) return;
			level.PhysicsBlockChange(blockPos, MCBlocks.Grass);
		}
	}
	public class Cobblestone : Block
	{
		public override string Name
		{
			get { return "Cobblestone"; }
		}

		public override byte ThisID
		{
			get { return 4; }
		}
	}
	public class Wood : Block
	{
		public override string Name
		{
			get { return "Wood"; }
		}

		public override byte ThisID
		{
			get { return 5; }
		}
	}
	public class Shrub : Block
	{
		public override string Name
		{
			get { return "Shrub"; }
		}

		public override bool IsCrushable
		{
			get { return true; }
		}
		public override bool CanLightPass
		{
			get { return true; }
		}

		public override byte ThisID
		{
			get { return 6; }
		}
	}
	public class Adminium : Block
	{
		public override string Name
		{
			get { return "Adminium"; }
		}

		public override bool OnlyAdmin
		{
			get { return true; }
		}

		public override byte ThisID
		{
			get { return 7; }
		}
	}
	public class Water : Block
	{
		public override string Name
		{
			get { return "Water"; }
		}

		public override ushort PhysicsDelay
		{
			get { return 5; }
		}

		public override byte ThisID
		{
			get { return 8; }
		}

		public override void Physics(Level level, int pos)
		{
			var blockPos = level.IntToBlockPos(pos);
			var belowPos = blockPos.Below;

			if (!BlockCheck(belowPos))
				blockPos.Around(BlockCheck);
		}

		bool BlockCheck(BlockPos block)
		{
			if (block.BlockMCType == MCBlocks.WaterStill)
			{
				AddOtherData(block.Level, block.Index, block.Level.Physics.WaterCurrent);
				return true;
			}
			if (Crushable.Contains(block.BlockType))
			{
				AddOtherData(block.Level, block.Index, block.Level.Physics.WaterCurrent);
				PhysicsBlockChange(block, MCBlocks.WaterStill);
				return true;
			}

			return false;
		}
	}
	public class WaterStill : Block
	{
		public override string Name
		{
			get { return "WaterStill"; }
		}

		public override ushort PhysicsDelay
		{
			get { return 5; }
		}

		public override byte ThisID
		{
			get { return 9; }
		}

		public override void Physics(Level level, int pos)
		{
			BlockPos blockPos = level.IntToBlockPos(pos);
			BlockPos belowBlockPos = blockPos.Diff(0, -1, 0);

			if (!level.Physics.OtherData.ContainsKey(pos)) return;

			var current = (byte)(level.Physics.OtherData[pos] - 1);
			if (current == 0) return;

			int belowPos = level.PosToInt(belowBlockPos);
			byte type = level.BlockData[belowPos];

			if (type == (byte)MCBlocks.WaterStill)
			{
				if (level.Physics.OtherData[belowPos] < current)
				{
					level.Physics.OtherData.Remove(belowPos);
					level.Physics.OtherData.Add(belowPos, current);
				}
			}
			else if (Crushable.Contains(type))
			{
				level.Physics.OtherData.Remove(belowPos);
				level.Physics.OtherData.Add(belowPos, current);
				level.PhysicsBlockChange(belowBlockPos, MCBlocks.WaterStill);
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

						if (newType == (byte)MCBlocks.WaterStill)
						{
							if (level.Physics.OtherData.ContainsKey(newPos))
							{
								if (level.Physics.OtherData[newPos] >= current) continue;
								level.Physics.OtherData[newPos] = current;
							}
							else
							{
								level.Physics.OtherData.Add(newPos, current);
							}
						}
						else if (Crushable.Contains(newType))
						{
							level.Physics.OtherData.Remove(newPos);
							level.Physics.OtherData.Add(newPos, current);
							level.PhysicsBlockChange(aroundPos, MCBlocks.WaterStill);
							//if (level.physics.PhysicsUpdates.Contains(level.PosToInt(aroundBelowPos))) return;
							//level.physics.PhysicsUpdates.Add(level.PosToInt(aroundBelowPos));
							return;
						}
					}
			}
		}
	}
	public class Lava : Block
	{
		public override string Name
		{
			get { return "Lava"; }
		}

		public override ushort PhysicsDelay
		{
			get { return 100; }
		}

		public override byte ThisID
		{
			get { return 10; }
		}

		public override void Physics(Level level, int pos)
		{
			BlockPos blockPos = level.IntToBlockPos(pos);
			BlockPos belowBlockPos = blockPos.Diff(0, -1, 0);

			int belowPos = level.PosToInt(belowBlockPos);
			byte type = level.BlockData[belowPos];

			if (type == (byte)MCBlocks.LavaStill)
			{
				level.Physics.OtherData.Remove(belowPos);
				level.Physics.OtherData.Add(belowPos, level.Physics.LavaCurrent);
			}
			else if (Crushable.Contains(type))
			{
				level.Physics.OtherData.Remove(belowPos);
				level.Physics.OtherData.Add(belowPos, level.Physics.LavaCurrent);
				level.PhysicsBlockChange(belowBlockPos, MCBlocks.LavaStill);
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

						if (level.BlockData[newPos] == (byte)MCBlocks.LavaStill)
						{
							level.Physics.OtherData.Remove(newPos);
							level.Physics.OtherData.Add(newPos, level.Physics.LavaCurrent);
						}
						else if (Crushable.Contains(level.BlockData[newPos]))
						{
							level.Physics.OtherData.Remove(newPos);
							level.Physics.OtherData.Add(newPos, level.Physics.LavaCurrent);
							level.PhysicsBlockChange(aroundPos, MCBlocks.LavaStill);
							//if (level.physics.PhysicsUpdates.Contains(level.PosToInt(aroundBelowPos))) return;
							//level.physics.PhysicsUpdates.Add(level.PosToInt(aroundBelowPos));
							return;
						}
					}
			}
		}
	}
	public class LavaStill : Block
	{
		public override string Name
		{
			get { return "LavaStill"; }
		}

		public override ushort PhysicsDelay
		{
			get { return 100; }
		}

		public override byte ThisID
		{
			get { return 11; }
		}

		public override void Physics(Level level, int pos)
		{
			BlockPos blockPos = level.IntToBlockPos(pos);
			BlockPos belowBlockPos = blockPos.Diff(0, -1, 0);

			if (!level.Physics.OtherData.ContainsKey(pos)) return;

			var current = (byte)(level.Physics.OtherData[pos] - 1);
			if (current == 0) return;

			int belowPos = level.PosToInt(belowBlockPos);
			byte type = level.BlockData[belowPos];

			if (type == (byte)MCBlocks.LavaStill)
			{
				if (level.Physics.OtherData[belowPos] < current)
				{
					level.Physics.OtherData.Remove(belowPos);
					level.Physics.OtherData.Add(belowPos, current);
				}
			}
			else if (Crushable.Contains(type))
			{
				level.Physics.OtherData.Remove(belowPos);
				level.Physics.OtherData.Add(belowPos, current);
				level.PhysicsBlockChange(belowBlockPos, MCBlocks.LavaStill);
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

						if (newType == (byte)MCBlocks.LavaStill)
						{
							if (level.Physics.OtherData.ContainsKey(newPos))
							{
								if (level.Physics.OtherData[newPos] >= current) continue;
								level.Physics.OtherData[newPos] = current;
							}
							else
							{
								level.Physics.OtherData.Add(newPos, current);
							}
						}
						else if (Crushable.Contains(newType))
						{
							level.Physics.OtherData.Remove(newPos);
							level.Physics.OtherData.Add(newPos, current);
							level.PhysicsBlockChange(aroundPos, MCBlocks.LavaStill);
							//if (level.physics.PhysicsUpdates.Contains(level.PosToInt(aroundBelowPos))) return;
							//level.physics.PhysicsUpdates.Add(level.PosToInt(aroundBelowPos));
							return;
						}
					}
			}
		}
	}
	public class Sand : Block
	{
		public override string Name
		{
			get { return "Sand"; }
		}

		public override ushort PhysicsDelay
		{
			get { return 10; }
		}

		public override byte ThisID
		{
			get { return 12; }
		}

		public override void Physics(Level level, int pos)
		{
			BlockPos blockPos = level.IntToBlockPos(pos);
			BlockPos belowBlockPos = blockPos.Diff(0, -1, 0);

			MCBlocks below = level.GetTile(belowBlockPos);

			if (below == MCBlocks.Zero)
			{
			}
			else if (Crushable.Contains((byte)below))
			{
				level.PhysicsBlockChange(belowBlockPos, MCBlocks.Sand);
				level.PhysicsBlockChange(blockPos, MCBlocks.Air);
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
						if (Crushable.Contains(level.BlockData[newPos]) && level.BlockData[testPos] == 0)
						{
							level.PhysicsBlockChange(aroundBelowPos, MCBlocks.Sand);
							level.PhysicsBlockChange(blockPos, MCBlocks.Air);
							//if (level.physics.PhysicsUpdates.Contains(level.PosToInt(aroundBelowPos))) return;
							//level.physics.PhysicsUpdates.Add(level.PosToInt(aroundBelowPos));
							return;
						}
					}
			}
		}
	}
	public class Gravel : Block
	{
		public override string Name
		{
			get { return "Gravel"; }
		}

		public override ushort PhysicsDelay
		{
			get { return 5; }
		}

		public override byte ThisID
		{
			get { return 13; }
		}

		public override void Physics(Level level, int pos)
		{
			BlockPos blockPos = level.IntToBlockPos(pos);
			BlockPos belowPos = blockPos.Diff(0, -1, 0);

			MCBlocks below = level.GetTile(belowPos);

			if (below == MCBlocks.Zero)
			{
			}
			else if (Crushable.Contains((byte)below))
			{
				level.PhysicsBlockChange(belowPos, MCBlocks.Gravel);
				level.PhysicsBlockChange(blockPos, MCBlocks.Air);
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
						if (Crushable.Contains(level.BlockData[newPos]) && level.BlockData[testPos] == 0)
						{
							level.PhysicsBlockChange(aroundBelowPos, MCBlocks.Gravel);
							level.PhysicsBlockChange(blockPos, MCBlocks.Air);
							//if (level.physics.PhysicsUpdates.Contains(level.PosToInt(aroundBelowPos))) return;
							//level.physics.PhysicsUpdates.Add(level.PosToInt(aroundBelowPos));
							return;
						}
					}
			}
		}
	}
	public class Goldrock : Block
	{
		public override string Name
		{
			get { return "Goldrock"; }
		}

		public override byte ThisID
		{
			get { return 14; }
		}
	}
	public class Ironrock : Block
	{
		public override string Name
		{
			get { return "Ironrock"; }
		}

		public override byte ThisID
		{
			get { return 15; }
		}
	}
	public class Coal : Block
	{
		public override string Name
		{
			get { return "Coal"; }
		}

		public override byte ThisID
		{
			get { return 16; }
		}
	}
	public class Trunk : Block
	{
		public override string Name
		{
			get { return "Trunk"; }
		}

		public override byte ThisID
		{
			get { return 17; }
		}
	}
	public class Leaf : Block
	{
		public override string Name
		{
			get { return "Leaf"; }
		}

		public override bool IsCrushable
		{
			get { return true; }
		}
		public override bool CanLightPass
		{
			get { return true; }
		}

		public override byte ThisID
		{
			get { return 18; }
		}


	}
	public class Sponge : Block
	{
		public override string Name
		{
			get { return "Sponge"; }
		}

		public override byte ThisID
		{
			get { return 19; }
		}

		public override void Physics(Level level, int pos)
		{
			BlockPos blockPos = level.IntToBlockPos(pos);

			for (int x = -1; x < 2; ++x)
			{
				for (int y = -1; y < 2; ++y)
				{
					for (int z = -1; z < 2; ++z)
					{
						BlockPos currentBlockPos = blockPos.Diff(x, y, z);
						int currentPos = level.PosToInt(currentBlockPos);

						if (level.NotInBounds(currentBlockPos)) continue;

						var type = (MCBlocks)level.BlockData[currentPos];

						if (type == MCBlocks.Water || type == MCBlocks.WaterStill || type == MCBlocks.Lava ||
							type == MCBlocks.LavaStill)
						{
							level.PhysicsBlockChange(currentBlockPos, MCBlocks.Air);
						}
					}
				}
			}
		}
	}
	public class Glass : Block
	{
		public override string Name
		{
			get { return "Glass"; }
		}

		public override bool CanLightPass
		{
			get { return true; }
		}

		public override byte ThisID
		{
			get { return 20; }
		}
	}
	public class Red : Block
	{
		public override string Name
		{
			get { return "Red"; }
		}

		public override byte ThisID
		{
			get { return 21; }
		}
	}
	public class Orange : Block
	{
		public override string Name
		{
			get { return "Orange"; }
		}

		public override byte ThisID
		{
			get { return 22; }
		}
	}
	public class Yellow : Block
	{
		public override string Name
		{
			get { return "Yellow"; }
		}

		public override byte ThisID
		{
			get { return 23; }
		}
	}
	public class Lightgreen : Block
	{
		public override string Name
		{
			get { return "Lightgreen"; }
		}

		public override byte ThisID
		{
			get { return 24; }
		}
	}
	public class Green : Block
	{
		public override string Name
		{
			get { return "Green"; }
		}

		public override byte ThisID
		{
			get { return 25; }
		}
	}
	public class Aquagreen : Block
	{
		public override string Name
		{
			get { return "Aquagreen"; }
		}

		public override byte ThisID
		{
			get { return 26; }
		}
	}
	public class Cyan : Block
	{
		public override string Name
		{
			get { return "Cyan"; }
		}

		public override byte ThisID
		{
			get { return 27; }
		}
	}
	public class Lightblue : Block
	{
		public override string Name
		{
			get { return "Lightblue"; }
		}

		public override byte ThisID
		{
			get { return 28; }
		}
	}
	public class Blue : Block
	{
		public override string Name
		{
			get { return "Blue"; }
		}

		public override byte ThisID
		{
			get { return 29; }
		}
	}
	public class Purple : Block
	{
		public override string Name
		{
			get { return "Purple"; }
		}

		public override byte ThisID
		{
			get { return 30; }
		}
	}
	public class Lightpurple : Block
	{
		public override string Name
		{
			get { return "Lightpurple"; }
		}

		public override byte ThisID
		{
			get { return 31; }
		}
	}
	public class Pink : Block
	{
		public override string Name
		{
			get { return "Pink"; }
		}

		public override byte ThisID
		{
			get { return 32; }
		}
	}
	public class Darkpink : Block
	{
		public override string Name
		{
			get { return "Darkpink"; }
		}

		public override byte ThisID
		{
			get { return 33; }
		}
	}
	public class Darkgray : Block
	{
		public override string Name
		{
			get { return "Darkgray"; }
		}

		public override byte ThisID
		{
			get { return 34; }
		}
	}
	public class Lightgray : Block
	{
		public override string Name
		{
			get { return "Lightgray"; }
		}

		public override byte ThisID
		{
			get { return 35; }
		}
	}
	public class White : Block
	{
		public override string Name
		{
			get { return "White"; }
		}

		public override byte ThisID
		{
			get { return 36; }
		}
	}
	public class Yellowflower : Block
	{
		public override string Name
		{
			get { return "Yellowflower"; }
		}

		public override bool IsCrushable
		{
			get { return true; }
		}
		public override bool CanLightPass
		{
			get { return true; }
		}

		public override byte ThisID
		{
			get { return 37; }
		}
	}
	public class Redflower : Block
	{
		public override string Name
		{
			get { return "Redflower"; }
		}

		public override bool IsCrushable
		{
			get { return true; }
		}
		public override bool CanLightPass
		{
			get { return true; }
		}

		public override byte ThisID
		{
			get { return 38; }
		}
	}
	public class Mushroom : Block
	{
		public override string Name
		{
			get { return "Mushroom"; }
		}

		public override bool IsCrushable
		{
			get { return true; }
		}
		public override bool CanLightPass
		{
			get { return true; }
		}

		public override byte ThisID
		{
			get { return 39; }
		}
	}
	public class Redmushroom : Block
	{
		public override string Name
		{
			get { return "Redmushroom"; }
		}

		public override bool IsCrushable
		{
			get { return true; }
		}
		public override bool CanLightPass
		{
			get { return true; }
		}

		public override byte ThisID
		{
			get { return 40; }
		}
	}
	public class Goldsolid : Block
	{
		public override string Name
		{
			get { return "Goldsolid"; }
		}

		public override byte ThisID
		{
			get { return 41; }
		}
	}
	public class Ironsolid : Block
	{
		public override string Name
		{
			get { return "Ironsolid"; }
		}

		public override byte ThisID
		{
			get { return 42; }
		}
	}
	public class Staricasefull : Block
	{
		public override string Name
		{
			get { return "Staircasefull"; }
		}

		public override byte ThisID
		{
			get { return 43; }
		}
	}
	public class StaircaseStep : Block
	{
		public override string Name
		{
			get { return "Staircasestep"; }
		}

		public override byte ThisID
		{
			get { return 44; }
		}

		public override void Physics(Level level, int pos)
		{
			BlockPos blockPos = level.IntToBlockPos(pos);

			BlockPos belowBlockPos = blockPos.Diff(0, -1, 0);
			int belowPos = level.PosToInt(belowBlockPos);

			if (level.BlockData[belowPos] == (byte)MCBlocks.Staircasestep)
			{
				level.PhysicsBlockChange(belowBlockPos, MCBlocks.Staircasefull);
				level.PhysicsBlockChange(blockPos, MCBlocks.Air);
			}
		}
	}
	public class Brick : Block
	{
		public override string Name
		{
			get { return "Brick"; }
		}

		public override byte ThisID
		{
			get { return 45; }
		}
	}
	public class Tnt : Block
	{
		public override string Name
		{
			get { return "Tnt"; }
		}

		public override byte ThisID
		{
			get { return 46; }
		}
	}
	public class Bookcase : Block
	{
		public override string Name
		{
			get { return "Bookcase"; }
		}

		public override byte ThisID
		{
			get { return 47; }
		}
	}
	public class Stonevine : Block
	{
		public override string Name
		{
			get { return "Stonevine"; }
		}

		public override byte ThisID
		{
			get { return 48; }
		}
	}
	public class Obsidian : Block
	{
		public override string Name
		{
			get { return "Obsidian"; }
		}

		public override byte ThisID
		{
			get { return 49; }
		}
	}

	public class Zero : Block
	{
		public override string Name
		{
			get { return "Zero"; }
		}
		public override byte ThisID
		{
			get { return 255; }
		}
		public override byte BaseType
		{
			get { return 0; }
		}
	}
}
