using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MineFrog;
using MineFrog.Commands;

namespace Commands
{
	public class CmdWater : CommandBase
	{
		public override string Name
		{
			get { return "Water"; }
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
			get { return new[] { "Water" }; }
		}

		public override void PlayerUse(Player p, string[] parameters, string fullCommand)
		{
			p._enableWaterMode = !p._enableWaterMode;
			p.SendMessage("Water mode is now: " + p._enableWaterMode);
		}
	}
}
