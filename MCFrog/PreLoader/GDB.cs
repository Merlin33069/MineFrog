using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MineFrog.PreLoader
{
	/// <summary>
	/// Class to hold PLAYER DATABASE information
	/// </summary>
	/*
	 * Current DB Structure for USERS table:
	 * 
	 * Name
	 * NickName
	 * IP
	 * WarnLevel
	 * Group
	 * isFrozen
	 * isMuted
	 * 
	 * What Else?
	 */

	class GDB : IComparable<GDB>
	{
		internal static Dictionary<string, GDB> Gdbs = new Dictionary<string, GDB>();

		internal int GID;
		internal string GroupName;
		internal string GroupTag;
		internal string GroupColor;
		internal byte PermissionLevel;
		internal bool isAdmin;
		internal bool canBuild;
		internal bool canChat;
		internal int MaxBlockChange;
		

		public int CompareTo(GDB compareMe)
		{
			return String.Compare(GroupName, compareMe.GroupName, StringComparison.OrdinalIgnoreCase);
		}

		internal static GDB Find(string search)
		{
			foreach (GDB gdb in Gdbs.Values)
			{
				
				if (string.Equals(gdb.GroupName.Trim(), search.Trim(), StringComparison.OrdinalIgnoreCase))
				{
					return gdb;
				}
			}
			return null;
		}

		internal static GDB Find(long search)
		{
			foreach (GDB gdb in Gdbs.Values)
			{
				if (gdb.GID == search)
				{
					return gdb;
				}
			}
			return null;
		}

		internal GDB(int id, object[] data)
		{
			GID = id;
			GroupName = ((string)data[0]).Trim();
			GroupTag = (string)data[1];
			GroupColor = (string)data[2];
			PermissionLevel = (byte)data[3];
			isAdmin = (bool) data[4];
			canBuild = (bool)data[5];
			canChat = (bool)data[6];
			MaxBlockChange = (int)data[7];

			Gdbs.Add(GroupName, this);
		}

		internal void sync()
		{
			//Server.users.UpdateRow(UID, new object[] { Username, "", IP, WarningLevel, GroupID, isFrozen, isMuted });
		}
	}
}
