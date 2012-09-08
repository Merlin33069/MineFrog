using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;

namespace MineFrog.Database
{
	//STATIC IS BAD
[Serializable]
	public class Database
	{
		internal const string KeyfilePath = "database/keyfile.DKF";
		internal const string DatabaseFilesPrePath = "database/";
		public bool IsInitialized = false;

		private FileStream _keyFileStream;

		private readonly ASCIIEncoding Encode = new ASCIIEncoding();

		private readonly Dictionary<string, Table> LoadedTables = new Dictionary<string, Table>();

		internal Database()
		{
			LoadKeyFile();
		}

		public void LoadKeyFile()
		{
			Console.WriteLine("Loading DB Keyfile!");
			try
			{
				//Console.WriteLine("db-1");
				if (!Directory.Exists(KeyfilePath.Split('/')[0].Trim())) //Check for firectory
				{
					//Console.WriteLine("db-2");
					Directory.CreateDirectory(KeyfilePath.Split('/')[0].Trim()); //Create directory if needed
				}
				//Console.WriteLine("db-4");
				if (_keyFileStream == null) _keyFileStream = new FileStream(KeyfilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
				long length = _keyFileStream.Length;
				//Console.WriteLine("db-5");
				if(length == 0) //Making sure that the dater is in da keyfile
				{
					//Console.WriteLine("db-6");
					//_keyFileStream.Close();
					return;
				}
				if (length <= 16) //Here were just checking to make sure there is data in the keyfile. that is worth reading.
				{
					//Console.WriteLine("db-7");
					//_keyFileStream.Close();
					File.Delete(KeyfilePath);
					throw (new FileLoadException("KeyFile INVALID!"));
				}
				//Console.WriteLine("db-8");
				/*
				 * KeyFile Structure:
				 * 
				 * MESSAGE > Name
				 * BYTE > Number of columns
				 * Data[] > Datatypes
				 * 
				 */
				while (_keyFileStream.Position < length)
				{
					//Console.WriteLine("db-9");
					var tableName = (string)GetData(DataTypes.Name, _keyFileStream);

					var numberOfColumns = (byte)_keyFileStream.ReadByte();

					var dataTypesIntermediateBytes = new byte[numberOfColumns];
					//Console.WriteLine("db-10");
					_keyFileStream.Read(dataTypesIntermediateBytes, 0, numberOfColumns);

					DataTypes[] dataTypes = GetDataTypes(dataTypesIntermediateBytes);
					//Console.WriteLine("db-11");
					//Add this table to our list of LOADED tables
					tableName = tableName.Trim();
					LoadedTables.Add(tableName.ToLower(),
									  new Table(this, tableName, DatabaseFilesPrePath + tableName, dataTypes, GetTotalSize(dataTypes)));
				}
				//Console.WriteLine("db-12");
				//_keyFileStream.Close();

				IsInitialized = true;
			}
			catch (Exception e)
			{
				//if (_keyFileStream != null) _keyFileStream.Close();
				Console.WriteLine(e.Message);
				Console.WriteLine(e.StackTrace);
			}
		}

		internal DataTypes[] GetDataTypes(byte[] array)
		{
			var types = new DataTypes[array.Length];

			for (int i = 0; i < array.Length; i++)
			{
				types[i] = (DataTypes) array[i];
			}

			return types;
		}

		internal int GetTotalSize(IEnumerable<DataTypes> types)
		{
			int i = 0;

			foreach (DataTypes dt in types)
				i += GetSize(dt);

			return i;
		}

		internal byte GetSize(DataTypes type)
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

		internal byte[] GetBytes(int size, DataTypes[] types, object[] data)
		{
			if (size == 0) size = GetTotalSize(types);
			var bytes = new byte[size];
			int currentPlace = 0;

			for (int i = 0; i < types.Length; ++i)
			{
				switch (types[i])
				{
					case DataTypes.Bool:
						BitConverter.GetBytes((bool) data[i]).CopyTo(bytes, currentPlace);
						break;
					case DataTypes.DateTime:
						BitConverter.GetBytes(((DateTime) data[i]).ToBinary()).CopyTo(bytes, currentPlace);
						break;
					case DataTypes.Byte:
						bytes[currentPlace] = (byte) data[i];
						break;
					case DataTypes.Short:
						BitConverter.GetBytes((short) data[i]).CopyTo(bytes, currentPlace);
						break;
					case DataTypes.UShort:
						BitConverter.GetBytes((ushort) data[i]).CopyTo(bytes, currentPlace);
						break;
					case DataTypes.Int:
						BitConverter.GetBytes((int) data[i]).CopyTo(bytes, currentPlace);
						break;
					case DataTypes.UInt:
						BitConverter.GetBytes((uint) data[i]).CopyTo(bytes, currentPlace);
						break;
					case DataTypes.Long:
						BitConverter.GetBytes((long) data[i]).CopyTo(bytes, currentPlace);
						break;
					case DataTypes.ULong:
						BitConverter.GetBytes((ulong) data[i]).CopyTo(bytes, currentPlace);
						break;
					case DataTypes.Float:
						BitConverter.GetBytes((float) data[i]).CopyTo(bytes, currentPlace);
						break;
					case DataTypes.Double:
						BitConverter.GetBytes((double) data[i]).CopyTo(bytes, currentPlace);
						break;
					case DataTypes.Name:
						string s = ((string) data[i]).PadRight(16);
						Encode.GetBytes(s).CopyTo(bytes, currentPlace);
						break;
					case DataTypes.Message:
						string st = ((string) data[i]).PadRight(64);
						Encode.GetBytes(st).CopyTo(bytes, currentPlace);
						break;
				}

				currentPlace += GetSize(types[i]);
			}

			return bytes;
		}

		internal byte[] GetBytes(DataTypes dataType, object data)
		{
			var bytes = new byte[GetSize(dataType)];
			switch (dataType)
			{
				case DataTypes.Bool:
					BitConverter.GetBytes((bool) data).CopyTo(bytes, 0);
					break;
				case DataTypes.DateTime:
					BitConverter.GetBytes(((DateTime) data).ToBinary()).CopyTo(bytes, 0);
					break;
				case DataTypes.Byte:
					bytes[0] = (byte) data;
					break;
				case DataTypes.Short:
					BitConverter.GetBytes((short) data).CopyTo(bytes, 0);
					break;
				case DataTypes.UShort:
					BitConverter.GetBytes((ushort) data).CopyTo(bytes, 0);
					break;
				case DataTypes.Int:
					BitConverter.GetBytes((int) data).CopyTo(bytes, 0);
					break;
				case DataTypes.UInt:
					BitConverter.GetBytes((uint) data).CopyTo(bytes, 0);
					break;
				case DataTypes.Long:
					BitConverter.GetBytes((long) data).CopyTo(bytes, 0);
					break;
				case DataTypes.ULong:
					BitConverter.GetBytes((ulong) data).CopyTo(bytes, 0);
					break;
				case DataTypes.Float:
					BitConverter.GetBytes((float) data).CopyTo(bytes, 0);
					break;
				case DataTypes.Double:
					BitConverter.GetBytes((double) data).CopyTo(bytes, 0);
					break;
				case DataTypes.Name:
					string s = ((string) data).PadRight(16);
					Encode.GetBytes(s).CopyTo(bytes, 0);
					break;
				case DataTypes.Message:
					string st = ((string) data).PadRight(64);
					Encode.GetBytes(st).CopyTo(bytes, 0);
					break;
			}
			return bytes;
		}

		internal int[] GetPositions(DataTypes[] types)
		{
			var positions = new int[types.Length];

			for(int i = 0;i<types.Length;i++)
			{
				if (i == 0) positions[i] = 0;
				else
				{
					positions[i] = positions[i - 1] + GetSize(types[i]);
				}
			}

			return positions;
		}

		internal object GetData(DataTypes type, FileStream fileStream)
		{
			//Used to temporarily hold Data we are getting.
			var tempData = new byte[64];

			switch (type)
			{
				case DataTypes.Bool:
					fileStream.Read(tempData, 0, 1);
					return BitConverter.ToBoolean(tempData, 0);
				case DataTypes.DateTime:
					fileStream.Read(tempData, 0, 8);
					return DateTime.FromBinary(BitConverter.ToInt64(tempData, 0));
				case DataTypes.Byte:
					fileStream.Read(tempData, 0, 1);
					return tempData[0];
				case DataTypes.Short:
					fileStream.Read(tempData, 0, 2);
					return BitConverter.ToInt16(tempData, 0);
				case DataTypes.UShort:
					fileStream.Read(tempData, 0, 2);
					return BitConverter.ToUInt16(tempData, 0);
				case DataTypes.Int:
					fileStream.Read(tempData, 0, 4);
					return BitConverter.ToInt32(tempData, 0);
				case DataTypes.UInt:
					fileStream.Read(tempData, 0, 4);
					return BitConverter.ToUInt32(tempData, 0);
				case DataTypes.Long:
					fileStream.Read(tempData, 0, 8);
					return BitConverter.ToInt64(tempData, 0);
				case DataTypes.ULong:
					fileStream.Read(tempData, 0, 8);
					return BitConverter.ToUInt64(tempData, 0);
				case DataTypes.Float:
					fileStream.Read(tempData, 0, 4);
					return BitConverter.ToSingle(tempData, 0);
				case DataTypes.Double:
					fileStream.Read(tempData, 0, 8);
					return BitConverter.ToDouble(tempData, 0);
				case DataTypes.Name:
					fileStream.Read(tempData, 0, 16);
					return Encode.GetString(tempData, 0, 16).Trim();
				case DataTypes.Message:
					fileStream.Read(tempData, 0, 64);
					return Encode.GetString(tempData).Trim();
				default:
					throw (new DataException("KEYFILE DATA VALUE OUT OF RANGE: Unknown DataType in Keyfile!"));
			}
		}

		public void CreateNewTable(string name, DataTypes[] dataTypes)
		{
			if(_keyFileStream == null)
			{
				_keyFileStream = new FileStream(KeyfilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
			}
			try
			{
				_keyFileStream.Position = _keyFileStream.Length;
				name = name.Trim();

				if (name.Length > 16)
				{
					throw (new ArgumentException("Table name must be no longer than 16 characters!"));
				}

				//_keyFileStream = File.Open(KeyfilePath, FileMode.Append);

				var toWrite = Encode.GetBytes(name.PadRight(16));
				_keyFileStream.Write(toWrite, 0, 16);
				_keyFileStream.WriteByte((byte)dataTypes.Length);
				for (int i = 0; i < dataTypes.Length; i++) _keyFileStream.WriteByte((byte)dataTypes[i]);

				_keyFileStream.Flush();
				//_keyFileStream.Close();

				var table = new Table(this, name, DatabaseFilesPrePath + name, dataTypes, GetTotalSize(dataTypes));

				LoadedTables.Add(name.ToLower(), table);
			}
			catch(Exception e)
			{
				//if (_keyFileStream != null) _keyFileStream.Close();
				Console.WriteLine(e.Message);
				Console.WriteLine(e.StackTrace);
			}
		}

		#region Table Exist and Find Methods

		/// <summary>
		/// Method to check whether a table exists in the Loaded Dictionary or not.
		/// </summary>
		/// <param name="name">The name of the table to look for</param>
		/// <returns>A value (bool) representing whether this table exists or not</returns>
		internal bool TableExists(string name)
		{
			return LoadedTables.ContainsKey(name.Trim().ToLower());
		}

		/// <summary>
		/// Method to Find a specified table by name
		/// </summary>
		/// <param name="name">The name of the table to find.</param>
		/// <returns>The table referance or NULL if not found.</returns>
		internal Table TableFind(string name)
		{
			try
			{
				return LoadedTables[name.Trim().ToLower()];
			}
			catch (Exception)
			{
				return null;
			}
		}

		#endregion
	}

	public enum DataTypes : byte
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