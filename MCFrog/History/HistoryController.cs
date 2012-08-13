using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCFrog.History
{
	public class HistoryController : MarshalByRefObject
	{
		public HistoryController()
		{
			Console.WriteLine("History System Started!");
		}

		Dictionary<string, LevelHistory> loadedHistories = new Dictionary<string, LevelHistory>();
		hisData empty = new hisData { empty = true };

		public void LoadHistory(string levelName)
		{
			levelName = levelName.Trim();
			Console.WriteLine("Loading History... " + levelName);
			loadedHistories.Add(levelName, new LevelHistory(levelName));
		}
		public void SaveHistory(string levelName)
		{
			levelName = levelName.Trim();
			LevelHistory temp = loadedHistories[levelName];
			temp.Save();
			//TODO save history file
		}
		public void UnloadHistory(string levelName)
		{
			levelName = levelName.Trim();
			//Remove all referances so we have no chance to have *any* memory leaks here
			LevelHistory temp = loadedHistories[levelName];
			loadedHistories.Remove(levelName);
			temp.History.Clear();
		}

		public hisData GetData(string levelName, int pos)
		{
			levelName = levelName.Trim();
			if (loadedHistories[levelName].History.ContainsKey(pos))
			{
				return loadedHistories[levelName].History[pos];
			}
			else
			{
				return empty;
			}
		}
		public bool SetData(string levelName, int pos, byte type, int UID)
		{
			levelName = levelName.Trim();
			//Console.WriteLine(AppDomain.CurrentDomain.FriendlyName + ": updating data :D");

			if(!loadedHistories.ContainsKey(levelName.Trim()))
			{
				//Should not call this
				Console.WriteLine("Level History not loaded?");
				LoadHistory(levelName);
			}

			LevelHistory LH = loadedHistories[levelName];

			try
			{
				if (LH.History.ContainsKey(pos))
				{
					LH.History[pos].type = type;
					LH.History[pos].UID = UID;
					return true;
				}
				else
				{
					LH.History.Add(pos, new hisData { type = type, UID = UID });
					return true;
				}
			}
			catch
			{
				return false;
			}
		}

		public override object InitializeLifetimeService()
		{
			// returning null here will prevent the lease manager
			// from deleting the object.
			return null;
		}
	}
}
