using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MineFrog;
using MineFrog.Commands;

namespace Commands
{
	public class CommandTest : CommandBase
	{
		public override string Name
		{
			get { return "test"; }
		}
		public override CommandTypes Type
		{
			get { return CommandTypes.Mod; }
		}
		public override string Author
		{
			get { return "Merlin33069-Debug"; }
		}
		public override Version Version
		{
			get { return new Version(1, 0, 0); }
		}
		public override byte Permission
		{
			get { return 0; }
		}
		public override string[] Accessors
		{
			get { return new[] { "Test" }; }
		}

		public override void Use(Player p, string[] s, string sy)
		{
			p.SendMessage("You just used the command :D");
		}
		new public void Use(string[] s, string sy)
		{
			Server.Log("Overriding NON Abstract method seems to have worked!", LogTypesEnum.Info);
		}

		public override void Help(Player p)
		{
			p.SendMessage("Your being helped alright....");
		}
	}
}
