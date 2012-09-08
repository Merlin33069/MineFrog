using System;
using System.Collections.Generic;
using System.Threading;

namespace MineFrog
{
	public class PhysicsHandler
	{
		public readonly Level Level;

		public int ChecksPerTick = 250;
		public bool FiniteLiquids = true;
		public int Interval = 10;

		public bool IsEnabled = false; //Disabled by default
		public Dictionary<int, byte> OtherData = new Dictionary<int, byte>();
		public Dictionary<int, ushort> Delay = new Dictionary<int, ushort>(); 
		public List<int> PhysicsUpdates = new List<int>();
		List<int> _tempPhysicsList = new List<int>(); 

		public bool Realistic = true;
		public bool ShouldStop = false;
		public byte WaterCurrent = 7;
		public byte LavaCurrent = 4;

		internal PhysicsHandler(Level l, bool enabled)
		{
			Level = l;
			IsEnabled = enabled;

			new Thread(TickTimer).Start();
		}

		internal void TickTimer()
		{
			while (!Server.ShouldShutdown && !Level.IsUnloaded && !ShouldStop)
			{
				Tick();
				Thread.Sleep(Interval);
			}
		}

		private void Tick()
		{
			if (PhysicsUpdates.Count > 100000)
			{
				PhysicsUpdates.Clear();
				Delay.Clear();

				Server.Log("Physics was overloaded on " + Level.Name + ", physics has been reset.", LogTypesEnum.Error);
				return;
			}

			for (int iterations = 0; iterations < ChecksPerTick; ++iterations)
			{
				if (PhysicsUpdates.Count == 0) break;

				int blockpos = PhysicsUpdates[0];
				
				PhysicsUpdates.Remove(blockpos);

				if (Delay.ContainsKey(blockpos))
				{
					if (Delay[blockpos] == 0)
					{
						Delay.Remove(blockpos);
					}
					else
					{
						Delay[blockpos]--;
						//Console.WriteLine(Block.Blocks[Level.BlockData[blockpos]].Name);
						_tempPhysicsList.Add(blockpos);
						iterations--;
						continue;
					}
				}
				if (blockpos < 0 || blockpos >= Level.BlockData.Length) continue;
				byte type = Level.BlockData[blockpos];

				if(Block.Blocks.ContainsKey(type)) Block.Blocks[type].Physics(Level, blockpos);
			}

			PhysicsUpdates.AddRange(_tempPhysicsList);
			_tempPhysicsList.Clear();
		}

		internal void AddCall(int blockpos)
		{
			if (PhysicsUpdates.Contains(blockpos)) return; //Don't add it twice >_>

			var type = Level.BlockData[blockpos];
			ushort delay = 0;
			try
			{
				delay = Block.Blocks[type].PhysicsDelay;
			}
			catch (Exception e)
			{
				Server.Log("Type: " + type + " -> " + e.Message, LogTypesEnum.Error);
			}
			

			Delay.Remove(blockpos);

			if(delay > 0)
			{
				Delay.Add(blockpos, delay);
			}

			PhysicsUpdates.Add((blockpos));
		}
	}
}