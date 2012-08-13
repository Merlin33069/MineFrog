using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCFrog.History
{
	class LevelHistory
	{
		//Store history data for each block
		internal Dictionary<int, hisData> History = new Dictionary<int, hisData>();
		internal string name;

		internal LevelHistory(string levelName)
		{
			//TODO load l's history
			name = levelName;
		}

		internal void Save()
		{
			//TODO gzip the list, as it stands, and dump it to a file
		}
	}

	[Serializable]
	public class hisData
	{
		internal bool empty = false;
		internal int UID; //User id that modified the block
		internal byte type; //Old TYPE of the block
	}
}
