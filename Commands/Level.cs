using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MineFrog;
using MineFrog.Commands;

namespace Commands
{
	public class CmdLevel : CommandBase
	{
		public override string Name
		{
			get { return "Level"; }
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
			get { return new[] { "Level" }; }
		}

		public override void PlayerUse(Player p, string[] parameters, string fullCommand)
		{
			if (parameters.Length == 0)
			{
				PlayerHelp(p);
				return;
			}

			if (parameters[0].Equals("create", StringComparison.OrdinalIgnoreCase) || parameters[0].Equals("new", StringComparison.OrdinalIgnoreCase))
			{
				if (parameters.Length == 2)
				{
					Level lfind = Level.Find(parameters[1]);
					if (lfind != null)
					{
						//Called when a player types /level create <name> or /level new <name> and the name is already used for another level
						p.SendMessage("Level Already Exists!");
					}
					else
					{
						//Called when a player types /level create <name> or /level new <name> and the name isn't already a level
						Level level = new Level(parameters[1], 256, 256, 256);
					}
				}
				else if (parameters.Length == 3)
				{
					//Called when a player types /level create <name> <type> or /level new <name> <type>
					if (parameters[2].Equals("flatgrass", StringComparison.OrdinalIgnoreCase))
					{
						Level level = new Level(parameters[1], 256, 256, 256, parameters[2]);
					}
					else if (parameters[2].Equals("pixel", StringComparison.OrdinalIgnoreCase))
					{
						Level level = new Level(parameters[1], 256, 256, 256, parameters[2]);
					}
					else
					{
						//Called when a player types /level create <name> <type> or /level new <name> <type> and the type is invalid.
						p.SendMessage("Invalid world type! Valid types are flatgrass and pixel.");
					}
				}
				else
				{
					//This is called when the player types /level create or /level new
					Level level = new Level("temporary", 256, 256, 256);
				}
			}
			if (parameters[0].Equals("load", StringComparison.OrdinalIgnoreCase))
			{
				//This is called when the player types /level load
				if (parameters.Length < 2) { PlayerHelp(p); return; }
				Level level = Level.Find(parameters[1]);
				if (level != null) p.SendMessage("Level Already Loaded! found!");
				else
				{
					new Level(parameters[1], false);
				}
			}
			if (parameters[0].Equals("unload", StringComparison.OrdinalIgnoreCase))
			{
				//This is called when the player types /level unload
				if (parameters.Length < 2) { PlayerHelp(p); return; }
				Level level = Level.Find(parameters[1]);
				if (level == null) p.SendMessage("Level not found!");
				else
				{
					level.Unload();
				}
			}
		}
		public override void PlayerHelp(Player p)
		{
			p.SendMessage("Invalid command usages!");
		}

		public override void ConsoleUse(string[] s, string sy)
		{
			if (s.Length == 0)
			{
				ConsoleHelp();
				return;
			}

			if (s[0].Equals("create", StringComparison.OrdinalIgnoreCase) || s[0].Equals("new", StringComparison.OrdinalIgnoreCase))
			{
				//This is called when the player types /level create or /level new

			}
			if (s[0].Equals("load", StringComparison.OrdinalIgnoreCase))
			{
				//This is called when the player types /level load

			}
			if (s[0].Equals("unload", StringComparison.OrdinalIgnoreCase))
			{
				//This is called when the player types /level unload

			}
		}
	}
}
