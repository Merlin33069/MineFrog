using System;
using System.Collections.Generic;

namespace MineFrog.History
{
	public class HistoryController : MarshalByRefObject
	{
		public HistoryController()
		{
			Console.WriteLine("History System Started!");
		}

		readonly Dictionary<string, LevelHistory> _loadedHistories = new Dictionary<string, LevelHistory>();
		readonly HisData _empty = new HisData { UID = int.MaxValue, Empty = true };

		public void LoadHistory(string levelName)
		{
			levelName = levelName.Trim();
			Console.WriteLine("Loading History... " + levelName);
			_loadedHistories.Add(levelName, new LevelHistory(levelName));
		}
		public void SaveHistory(string levelName)
		{
			levelName = levelName.Trim();
			LevelHistory temp = _loadedHistories[levelName];
			temp.Save();
			//TODO save history file
		}
		public void UnloadHistory(string levelName)
		{
			levelName = levelName.Trim();
			//Remove all referances so we have no chance to have *any* memory leaks here
			LevelHistory temp = _loadedHistories[levelName];
			_loadedHistories.Remove(levelName);
			temp.History.Clear();
		}

		public HisData GetData(string levelName, int pos)
		{
			levelName = levelName.Trim();
			if (_loadedHistories[levelName].History.ContainsKey(pos))
			{
				return _loadedHistories[levelName].History[pos];
			}
			return _empty;
		}
		public bool SetData(string levelName, int pos, byte type, int uid)
		{
			levelName = levelName.Trim();
			//Console.WriteLine(AppDomain.CurrentDomain.FriendlyName + ": updating data :D");

			if(!_loadedHistories.ContainsKey(levelName.Trim()))
			{
				//Should not call this
				Console.WriteLine("Level History not loaded?");
				LoadHistory(levelName);
			}

			LevelHistory lh = _loadedHistories[levelName];

			try
			{
				if (lh.History.ContainsKey(pos))
				{
					lh.History[pos].Type = type;
					lh.History[pos].UID = uid;
					return true;
				}
				lh.History.Add(pos, new HisData { Type = type, UID = uid });
				return true;
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
