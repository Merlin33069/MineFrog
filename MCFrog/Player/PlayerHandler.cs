using System.Collections.Generic;
using MineFrog.PreLoader;

namespace MineFrog
{
	public class PlayerHandler
	{
		internal static List<Player> Connections = new List<Player>();
		internal static Dictionary<byte, Player> Players = new Dictionary<byte, Player>();

		public static PDB Find(string name)
		{
			return PDB.Find(name);
		}
		public static PDB Find(int UID)
		{
			return PDB.Find(UID);
		}
	}
}
