using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCFrog
{
	enum Blocks : byte
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

	static class Block
	{
		internal static List<byte> lightPass = new List<byte>();
		internal static List<byte> crushable = new List<byte>();

		internal static void Initialize()
		{
			//TODO make list of LightPass blocks
			lightPass.Add(0);
			lightPass.Add(6);
			lightPass.Add(18);
			lightPass.Add(20);
			lightPass.Add(37);
			lightPass.Add(38);
			lightPass.Add(39);
			lightPass.Add(40);

			//TODO make a list of Crushable blocks
			crushable.Add(0);
			crushable.Add(6);
			crushable.Add(37);
			crushable.Add(38);
			crushable.Add(39);
			crushable.Add(40);
		}
	}

}
