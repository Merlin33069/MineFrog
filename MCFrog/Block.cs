using System.Collections.Generic;

namespace MineFrog
{
	internal enum Blocks : byte
	{
		Air = 0,
		Stone = 1,
		Grass = 2,
		Dirt = 3,
		Cobblestone = 4,
		Wood = 5,
		Shrub = 6,

		Blackrock = 7,
		Adminium = 7,

		Water = 8,
		WaterStill = 9,
		Lava = 10,
		LavaStill = 11,
		Sand = 12,
		Gravel = 13,
		Goldrock = 14,
		Ironrock = 15,
		Coal = 16,
		Trunk = 17,
		Leaf = 18,
		Sponge = 19,
		Glass = 20,
		Red = 21,
		Orange = 22,
		Yellow = 23,
		Lightgreen = 24,
		Green = 25,
		Aquagreen = 26,
		Cyan = 27,
		Lightblue = 28,
		Blue = 29,
		Purple = 30,
		Lightpurple = 31,
		Pink = 32,
		Darkpink = 33,
		Darkgray = 34,
		Lightgray = 35,
		White = 36,
		Yellowflower = 37,
		Redflower = 38,
		Mushroom = 39,
		Redmushroom = 40,
		Goldsolid = 41,
		Ironsolid = 42,
		Staircasefull = 43,
		Staircasestep = 44,
		Brick = 45,
		Tnt = 46,
		Bookcase = 47,
		Stonevine = 48,
		Obsidian = 49,


		Zero = 255,
	}

	internal static class Block
	{
		internal static List<byte> LightPass = new List<byte>();
		internal static List<byte> Crushable = new List<byte>();

		internal static void Initialize()
		{
			LightPass.Add(0);
			LightPass.Add(6);
			LightPass.Add(18);
			LightPass.Add(20);
			LightPass.Add(37);
			LightPass.Add(38);
			LightPass.Add(39);
			LightPass.Add(40);

			Crushable.Add(0);
			Crushable.Add(6);
			Crushable.Add(37);
			Crushable.Add(38);
			Crushable.Add(39);
			Crushable.Add(40);
		}
	}
}