using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MineFrog;
using MineFrog.Commands;

namespace Commands
{
	public class CommandTest2 : CommandBase
	{
		public override string Name
		{
			get { return "SetRank"; }
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
			get { return new[] { "Setrank" }; }
		}

		public override void Use(Player p, string[] s, string sy)
		{
			p.SendMessage("NYI Ingame, please use from the console for now!");
		}
		public override void Use(string[] s, string sy)
		{
			if (s.Length != 2)
			{
				Help();
				return;
			}
			MineFrog.PreLoader.PDB pdb = PlayerHandler.Find(s[0]);
			MineFrog.PreLoader.GDB gdb = MineFrog.PreLoader.GDB.Find(s[1]);

			if (pdb == null) {Server.Log("Player not found!", LogTypesEnum.Error); return;}
			if (gdb == null) { Server.Log("Group not found!", LogTypesEnum.Error); return; }

			pdb.Group = gdb;
			pdb.GroupID = gdb.GID;
			pdb.sync();

			Server.Log(pdb._username + "'s rank has been set to " + gdb.GroupName, LogTypesEnum.Info);
		}

		public override void Help(Player p)
		{
			p.SendMessage("NYI Ingame, please use through console for now!");
		}
		public override void Help()
		{
			Server.Log("SetRank - Usage: Setrank Merlin33069 owner", LogTypesEnum.Info);
		}
	}
}
