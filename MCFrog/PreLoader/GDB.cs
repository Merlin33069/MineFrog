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
	 * GroupTag
	 * GroupColor
	 * PermissionLevel
	 * isAdmin
	 * canBuild
	 * canChat
	 * MaxBlockChange
	 * 
	 * What Else?
	 */

	public class GDB : IComparable<GDB>
	{
		internal static Dictionary<string, GDB> Gdbs = new Dictionary<string, GDB>();

		public int GID;
		public string GroupName;
		public string GroupTag;
		public string GroupColor;
		public byte PermissionLevel;
		public bool isAdmin;
		public bool canBuild;
		public bool canChat;
		public int MaxBlockChange;
		

		public int CompareTo(GDB compareMe)
		{
			return String.Compare(GroupName, compareMe.GroupName, StringComparison.OrdinalIgnoreCase);
		}

		public static GDB Find(string search)
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

		public static GDB Find(long search)
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

		public void sync()
		{
			Server.groups.UpdateRow(GID, new object[] { GroupName, GroupTag, GroupColor, PermissionLevel, isAdmin, canBuild, canChat, MaxBlockChange });
		}
	}
}
