using System.Collections.Generic;

namespace MCFrog
{
	public class PlayerHandler
	{
		internal static List<Player> Connections = new List<Player>();
		internal static Dictionary<byte, Player> Players = new Dictionary<byte, Player>();
	}
}
