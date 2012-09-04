using System;
using System.Collections.Generic;

namespace MineFrog.History
{
	class LevelHistory
	{
		//Store history data for each block
		internal Dictionary<int, HisData> History = new Dictionary<int, HisData>();
		internal string Name;

		internal LevelHistory(string levelName)
		{
			//TODO load l's history
			Name = levelName;
		}

		internal void Save()
		{
			//TODO gzip the list, as it stands, and dump it to a file
		}
	}

	[Serializable]
	public class HisData
	{
		internal bool Empty = false;
		internal int UID; //User id that modified the block
		internal byte Type; //Old TYPE of the block
	}
}
