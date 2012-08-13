using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCFrog
{
	public class PlayerHandler
	{
		internal static List<Player> Connections = new List<Player>();
		internal static Dictionary<byte, Player> Players = new Dictionary<byte, Player>();

		public PlayerHandler()
		{

		}
	}
}
