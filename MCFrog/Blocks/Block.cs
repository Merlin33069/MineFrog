using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace MineFrog
{
	public enum MCBlocks : byte
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

	public abstract class Block
	{
		internal static List<byte> LightPass = new List<byte>();
		internal static List<byte> Crushable = new List<byte>();
		internal static List<byte> Admin = new List<byte>(); 

		internal static Dictionary<byte, Block> Blocks = new Dictionary<byte, Block>();  

		public abstract string Name { get; }

		public virtual bool IsCrushable { get { return false; } }
		public virtual bool CanLightPass { get { return false; } }
		public virtual bool OnlyAdmin { get { return false; } }
		public virtual ushort PhysicsDelay { get { return 0; } }
		
		public abstract byte ThisID { get; }
		public virtual byte BaseType { get { return ThisID; } }

		public virtual void OnPlace(Player p, Level level, int pos) { }
		public virtual void OnBreak(Player p, Level level, int pos) { }
		public virtual void Physics(Level level, int pos) { }

		/// <summary>
		/// This method adds a byte to the Otherdata dictionary for physics
		/// This method will clear out any existing otherdata for the block position
		/// </summary>
		/// <param name="level">The level your working with</param>
		/// <param name="pos">The Block position index in the level block data array</param>
		/// <param name="value">The "OtherData" value to set for this block.</param>
		public void AddOtherData(Level level, int pos, byte value)
		{
			RemoveOtherData(level, pos);
			level.Physics.OtherData.Add(pos, value);
		}
		/// <summary>
		/// This will remove any "OtherData" for the specified block index
		/// </summary>
		/// <param name="level">The level your working with</param>
		/// <param name="pos">The index to remove from the list</param>
		public void RemoveOtherData(Level level, int pos)
		{
			level.Physics.OtherData.Remove(pos);
		}

		public bool PhysicsBlockChange(BlockPos pos, byte type)
		{
			return pos.Level.PhysicsBlockChange(pos, type);
		}
		public bool PhysicsBlockChange(BlockPos pos, MCBlocks type)
		{
			return pos.Level.PhysicsBlockChange(pos, type);
		}

		internal void Initialize()
		{
			//Server.Log(Name + " block added as " + ThisID, LogTypesEnum.Debug);

			if(IsCrushable) Crushable.Add(ThisID);
			if(CanLightPass) LightPass.Add(ThisID);
			if(OnlyAdmin) Admin.Add(ThisID);

			Blocks.Add(ThisID, this);
		}

		internal static void LoadBlocks()
		{
			foreach (string fileOn in Directory.GetFiles(Directory.GetCurrentDirectory()))
			{
				FileInfo file = new FileInfo(fileOn);

				//Console.WriteLine("Found File in Plugin folder:");
				//Console.WriteLine(file.Name);

				//Preliminary check, must be .dll
				if (file.Extension.Equals(".dll") || file.Extension.Equals(".exe"))
				{
					//Create a new assembly from the plugin file we're adding..
					Assembly pluginAssembly = Assembly.LoadFrom(file.Name);

					//Next we'll loop through all the Types found in the assembly
					foreach (Type pluginType in pluginAssembly.GetTypes())
					{
						if (pluginType.IsSubclassOf(typeof(Block)) && pluginType.IsPublic && !pluginType.IsAbstract)
						{
							var block = (Block)Activator.CreateInstance(pluginAssembly.GetType(pluginType.ToString()));
							block.Initialize();
						}
					}
				}
			}
		}
	}
}