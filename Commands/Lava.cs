using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MineFrog;
using MineFrog.Commands;

namespace Commands
{
	public class CmdLava : CommandBase
	{
		public override string Name
		{
			get { return "Lava"; }
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
			get { return new[] { "Lava" }; }
		}

		public override void PlayerUse(Player p, string[] parameters, string fullCommand)
		{
			p._enableLavaMode = !p._enableLavaMode;
			p.SendMessage("Lava mode is now: " + p._enableLavaMode);
		}
	}
}
