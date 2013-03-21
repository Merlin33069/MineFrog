using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MineFrog;
using MineFrog.Commands;

namespace Commands
{
	public class CmdBenchMarking : CommandBase
	{
		public override string Name
		{
			get { return "BenchMarker"; }
		}
		public override CommandTypes Type
		{
			get { return CommandTypes.Mod; }
		}
		public override string Author
		{
			get { return "Merlin33069"; }
		}
		public override Version Version
		{
			get { return new Version(1, 0, 0); }
		}
		public override byte Permission
		{
			get { return 100; }
		}
		public override string[] Accessors
		{
			get { return new[] { "Benchmark" }; }
		}

		public override void ConsoleUse(string[] s, string sy)
		{
			if(s.Length == 1) if(s[0].Equals("clear", StringComparison.OrdinalIgnoreCase))
			{
				Player.Average1.Clear();
				Player.Average2.Clear();
				Player.Average3.Clear();
				Player.Average4.Clear();
				Console.WriteLine("Benchmarks cleared");
				return;
			}
			Console.WriteLine("Variable Initializing: " + TimeSpan.FromTicks((long)Player.Average1.Average()));
			Console.WriteLine("Authentication: " + TimeSpan.FromTicks((long)Player.Average2.Average()));
			Console.WriteLine("Database Initializing: " + TimeSpan.FromTicks((long)Player.Average3.Average()));
			Console.WriteLine("Player Initializing: " + TimeSpan.FromTicks((long)Player.Average4.Average()));
			Console.WriteLine("BlockPos Creation: " + TimeSpan.FromTicks((long)Level.BlockPosCreationAverager.Average()));
		}
	}
}
