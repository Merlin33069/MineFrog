using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCFrog.PreLoader
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
		internal byte Group;
		internal bool isFrozen;
		internal bool isMuted;

		public int CompareTo(PDB compareMe)
		{
			return String.Compare(Username, compareMe.Username, StringComparison.Ordinal);
		}

		internal static PDB Find(string search)
		{
			foreach (PDB pdb in Pdbs.Values)
			{
				if (pdb.Username == search.Trim().ToLower() || pdb.IP == search)
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
		
		internal PDB(int ID, object[] data)
		{
			UID = ID;
			Username = ((string)data[0]).Trim().ToLower();
			Nickname = (string) data[1];
			IP = (string)data[2];
			WarningLevel = (byte) data[3];
			Group = (byte) data[4];
			isFrozen = (bool) data[5];
			isMuted = (bool) data[6];

			Pdbs.Add(Username, this);
		}
	}
}
