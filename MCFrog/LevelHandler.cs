using System.Collections.Generic;

namespace MCFrog
{
	public class LevelHandler
	{
		internal static Level Lobby;

		internal static List<Level> Levels = new List<Level>();

		public LevelHandler()
		{
			Lobby = new Level("Lobby", true);
			//Lobby = new Level("test", 128, 64, 128);
		}
	}
}
