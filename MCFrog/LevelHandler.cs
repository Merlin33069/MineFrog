using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCFrog
{
	public class LevelHandler
	{
		internal static Level lobby;

		internal static List<Level> levels = new List<Level>();

		public LevelHandler()
		{
			lobby = new Level("test", 128, 64, 128);
		}
	}
}
