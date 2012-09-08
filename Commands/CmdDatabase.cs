using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MineFrog;
using MineFrog.Commands;

namespace Commands
{
	public class CmdDatabase : CommandBase
	{
		public override string Name
		{
			get { return "Database"; }
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
			get { return 200; }
		}
		public override string[] Accessors
		{
			get { return new[] { "Database" }; }
		}

		public override void ConsoleUse(string[] s, string sy)
		{
			if(s[0] == "keyfile")
			{
				Server.DatabaseController.database.LoadKeyFile();
			}
		}

		public override void ConsoleHelp()
		{
			Server.Log("SetRank - Usage: Setrank Merlin33069 owner", LogTypesEnum.Info);
		}
	}
}
