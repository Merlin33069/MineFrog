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
	
	public class PDB : IComparable<PDB>
	{
		internal static Dictionary<string, PDB> Pdbs = new Dictionary<string, PDB>();  

		public int UID;
		public string Username;
		public string _username
		{
			get { return Username; }
		}
		public string Nickname;
		public string IP;
		public byte WarningLevel;
		public GDB Group;
		public int GroupID;
		public bool isFrozen;
		public bool isMuted;

		public int CompareTo(PDB compareMe)
		{
			return String.Compare(Username, compareMe.Username, StringComparison.OrdinalIgnoreCase);
		}

		public static PDB Find(string search)
		{
			foreach (PDB pdb in Pdbs.Values)
			{
				if (string.Equals(pdb.Username.Trim(), search.Trim(), StringComparison.OrdinalIgnoreCase))
				{
					return pdb;
				}
			}
			return null;
		}

		public static PDB Find(long search)
		{
			foreach (PDB pdb in Pdbs.Values)
			{
				if (pdb.UID == search)
				{
					return pdb;
				}
			}
			return null;
		}
		
		internal PDB(int id, object[] data)
		{
			UID = id;
			Username = ((string)data[0]).Trim().ToLower();
			Nickname = (string) data[1];
			IP = (string)data[2];
			WarningLevel = (byte) data[3];
			GroupID = (int)data[4];
			Group = GDB.Find(GroupID);
			isFrozen = (bool) data[5];
			isMuted = (bool) data[6];

			if(Pdbs.ContainsKey(Username))
			{
				Server.Log("Duplicate user '" + Username + "' in database!", LogTypesEnum.Error);
				Server.Log("This CANNOT be fixed!", LogTypesEnum.Error);
				return;
			}

			Pdbs.Add(Username, this);

			if (Group == null)
			{
				Server.Log("Group with ID " + GroupID + " NOT FOUND, user " + Username + " being reverted to GUEST Status", LogTypesEnum.Error);
				GroupID = 0;
				Group = GDB.Find(0);

				sync();
			}
		}

		public void sync()
		{
			Server.Log("Saving user data to user table!", LogTypesEnum.Debug);
			Server.users.UpdateRow(UID, new object[] {Username, "", IP, WarningLevel, GroupID, isFrozen, isMuted});
		}
	}
}
