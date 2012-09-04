using System.Collections.Generic;

namespace MineFrog
{
	public class LevelHandler
	{
		internal static Level Lobby;

		internal static List<Level> Levels = new List<Level>();
		static readonly System.Timers.Timer LevelSaveCheckTimer = new System.Timers.Timer(10000);

		public LevelHandler()
		{
			Lobby = new Level("Lobby", true);

			LevelSaveCheckTimer.Elapsed += delegate
			{
				LevelSaveCheckTimer.Interval = 15000;
				LevelSaveUpdateCheck();
			};
			LevelSaveCheckTimer.Start();
		}
		public void LevelSaveUpdateCheck()
		{
			//Server.Log("Level save Update Check Timer called", LogTypesEnum.Debug);
			foreach (var level in Levels.ToArray())
			{
				level.SaveCheck();
			}
		}
	}
}
