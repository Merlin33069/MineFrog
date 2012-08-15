using System;
using System.Collections.Generic;
using System.Text;

using System.IO;
namespace MCFrog.Database
{
	class Database
	{
		static readonly ASCIIEncoding Encode = new ASCIIEncoding();
		Table usersTable;
		Table groupsTable;

		enum UserTableColumns
		{
			UID = 0,
			Username = 1,
			Nickname = 2,
			IP = 3,
			Group = 4,
		}

	    private const string keyfilePath = "database/keyfile.DKF";

	    internal void LoadKeyFile()
		{
			Console.WriteLine("Loading DB Keyfile!");
			try
			{
				if(!Directory.Exists(keyfilePath.Split('/')[0].Trim()))
				{
					Directory.CreateDirectory(keyfilePath.Split('/')[0].Trim());
				}
				if (!File.Exists(keyfilePath))
				{
					//TODO Generate Database Keyfile :D
					var fs = new FileStream(keyfilePath, FileMode.Create);

					//TODO write out the format :D

					#region Users Table
					//Column Count
					fs.WriteByte(2);

					//Data Values for table
					fs.WriteByte((byte)DataTypes.Int); //UID
					fs.WriteByte((byte)DataTypes.Name); //Username
					fs.WriteByte((byte)DataTypes.Message); //NickName
					fs.WriteByte((byte)DataTypes.Name); //IP
					fs.WriteByte((byte)DataTypes.Byte); //Group
					#endregion

					fs.Flush();
					fs.Close();

				}

				var file = new FileStream(keyfilePath, FileMode.Open);

				//user table keyfile loading
				int userTableColumnCount = file.ReadByte();
				var userTableDataTypes = new byte[userTableColumnCount];
				file.Read(userTableDataTypes, 0, userTableColumnCount);
				
				file.Flush();
				file.Close();

				DataTypes[] types = GetDataTypes(userTableDataTypes);
				usersTable = new Table("database/users.FDB", types, GetTotalSize(types));

				//TODO group keyfile loading
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				Console.WriteLine(e.StackTrace);
			}
		}

		static DataTypes[] GetDataTypes(byte[] array)
		{
			var types = new DataTypes[array.Length];

			for (int i = 0; i < array.Length; i++)
			{
				types[i] = (DataTypes)array[i];
			}

			return types;
		}
		static int GetTotalSize(IEnumerable<DataTypes> types)
		{
			int i = 0;

			foreach (DataTypes dt in types)
				i += GetSize(dt);

			return i;
		}
		static byte GetSize(DataTypes type)
		{
			switch (type)
			{
				case DataTypes.Bool:
				case DataTypes.Byte:
					return 1;
				case DataTypes.Short:
				case DataTypes.UShort:
					return 2;
				case DataTypes.Int:
				case DataTypes.UInt:
				case DataTypes.Float:
					return 4;
				case DataTypes.DateTime:
				case DataTypes.Long:
				case DataTypes.ULong:
				case DataTypes.Double:
					return 8;
				case DataTypes.Name:
					return 16;
				case DataTypes.Message:
					return 64;
				default:
					return 0;
			}
		}
		internal static byte[] GetBytes(int size, DataTypes[] types, object[] data)
		{
			var bytes = new byte[size];
			int currentPlace = 0;

			for (int i = 0; i < types.Length; ++i)
			{
				switch (types[i])
				{
					case DataTypes.Bool:
						BitConverter.GetBytes((bool)data[i]).CopyTo(bytes, currentPlace);
						break;
					case DataTypes.DateTime:
						BitConverter.GetBytes(((DateTime)data[i]).ToBinary()).CopyTo(bytes, currentPlace);
						break;
					case DataTypes.Byte:
						bytes[currentPlace] = (byte)data[i];
						break;
					case DataTypes.Short:
						BitConverter.GetBytes((short)data[i]).CopyTo(bytes, currentPlace);
						break;
					case DataTypes.UShort:
						BitConverter.GetBytes((ushort)data[i]).CopyTo(bytes, currentPlace);
						break;
					case DataTypes.Int:
						BitConverter.GetBytes((int)data[i]).CopyTo(bytes, currentPlace);
						break;
					case DataTypes.UInt:
						BitConverter.GetBytes((uint)data[i]).CopyTo(bytes, currentPlace);
						break;
					case DataTypes.Long:
						BitConverter.GetBytes((long)data[i]).CopyTo(bytes, currentPlace);
						break;
					case DataTypes.ULong:
						BitConverter.GetBytes((ulong)data[i]).CopyTo(bytes, currentPlace);
						break;
					case DataTypes.Float:
						BitConverter.GetBytes((float)data[i]).CopyTo(bytes, currentPlace);
						break;
					case DataTypes.Double:
						BitConverter.GetBytes((double)data[i]).CopyTo(bytes, currentPlace);
						break;
					case DataTypes.Name:
						string s = ((string)data[i]).PadRight(16);
						Encode.GetBytes(s).CopyTo(bytes, currentPlace);
						break;
					case DataTypes.Message:
						string st = ((string)data[i]).PadRight(64);
						Encode.GetBytes(st).CopyTo(bytes, currentPlace);
						break;
				}

				currentPlace += GetSize(types[i]);
			}

			return bytes;
		}
	}

	enum DataTypes : byte
	{
		Bool = 0,
		DateTime = 1,

		Byte = 10,
		Short = 11,
		UShort = 12,
		Int = 13,
		UInt = 14,
		Long = 15,
		ULong = 16,

		Float = 20,
		Double = 21,

		Name = 30,
		Message = 31,
	}
}
