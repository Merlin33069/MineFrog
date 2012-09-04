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
	
	class PDB : IComparable<PDB>
	{
		internal static Dictionary<string, PDB> Pdbs = new Dictionary<string, PDB>();  

		internal int UID;
		internal string Username;
		internal string Nickname;
		internal string IP;
		internal byte WarningLevel;

		internal int GroupID;
		internal bool isFrozen;
		internal bool isMuted;

		public int CompareTo(PDB compareMe)
		{
			return String.Compare(Username, compareMe.Username, StringComparison.OrdinalIgnoreCase);
		}

		internal static PDB Find(string search)
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

		internal static PDB Find(long search)
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
			isFrozen = (bool) data[5];
			isMuted = (bool) data[6];

			Pdbs.Add(Username, this);
		}

		internal void sync()
		{
			Server.users.UpdateRow(UID, new object[] {Username, "", IP, WarningLevel, GroupID, isFrozen, isMuted});
		}
	}
}
