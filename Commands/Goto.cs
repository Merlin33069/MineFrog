using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MineFrog;
using MineFrog.Commands;

namespace Commands
{
	public class CmdGoto : CommandBase
	{
		public override string Name
		{
			get { return "Goto"; }
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
			get { return new[] { "Goto" }; }
		}

		public override void PlayerUse(Player p, string[] parameters, string fullCommand)
		{
			if (parameters.Length == 0)
			{
				PlayerHelp(p);
				return;
			}
			Level level = Level.Find(parameters[0]);
			if (level == null) p.SendMessage("Not found!");
			else
			p.SwitchMap(level);
		}
		public override void PlayerHelp(Player p)
		{
			p.SendMessage("Invalid command usages!");
		}

	}
}
