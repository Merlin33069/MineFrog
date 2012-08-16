using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;

namespace MCFrog.Database
{
	//STATIC IS BAD

	public class Database
	{
		internal const string KeyfilePath = "database/keyfile.DKF";
		internal const string DatabaseFilesPrePath = "database/";
		public bool IsInitialized = false;
		
		private readonly ASCIIEncoding Encode = new ASCIIEncoding();

		private readonly Dictionary<string, Table> LoadedTables = new Dictionary<string, Table>();

		internal Database()
		{
			Table.database = this;
			LoadKeyFile();
		}

		internal void LoadKeyFile()
		{
			Console.WriteLine("Loading DB Keyfile!");
			try
			{
				if (!Directory.Exists(KeyfilePath.Split('/')[0].Trim())) //Check for firectory
				{
					Directory.CreateDirectory(KeyfilePath.Split('/')[0].Trim()); //Create directory if needed
				}
				if (!File.Exists(KeyfilePath)) //Check for keyfile
				{
					File.Create(KeyfilePath); //Create keyfile if needed!
					return;
				}

				FileStream fileStream = File.OpenRead(KeyfilePath);
				long length = fileStream.Length;

				if(length == 0) //Making sure that the dater is in da keyfile
				{
					//Nothing to see here ^_^
					return;
				}
				if (length <= 16) //Here were just checking to make sure there is data in the keyfile. that is worth reading.
				{
					fileStream.Close();
					File.Delete(KeyfilePath);
					throw (new FileLoadException("KeyFile INVALID!"));
				}

				/*
				 * KeyFile Structure:
				 * 
				 * MESSAGE > Name
				 * BYTE > Number of columns
				 * Data[] > Datatypes
				 * 
				 */
				while (fileStream.Position < length)
				{
					Console.WriteLine("Loading tabel!");
					var tableName = (string) GetData(DataTypes.Name, fileStream);

					var numberOfColumns = (byte) fileStream.ReadByte();

					var dataTypesIntermediateBytes = new byte[numberOfColumns];

					fileStream.Read(dataTypesIntermediateBytes, 0, numberOfColumns);

					DataTypes[] dataTypes = GetDataTypes(dataTypesIntermediateBytes);
					
					//Add this table to our list of LOADED tables
					tableName = tableName.Trim();
					Console.WriteLine("'" + DatabaseFilesPrePath + tableName+"'");
					LoadedTables.Add(tableName.ToLower(),
									  new Table(tableName, DatabaseFilesPrePath + tableName, dataTypes, GetTotalSize(dataTypes)));
				}

				fileStream.Close();

				IsInitialized = true;
			}
			catch (Exception e)
			{
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
					return BitConverter.ToInt64(tempData, 0);
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

		internal void CreateNewTable(string name, DataTypes[] dataTypes)
		{
			name = name.Trim();

			if(name.Length > 16)
			{
				throw (new ArgumentException("Table name must be no longer than 16 characters!"));
			}

			var fileStream = File.Open(KeyfilePath, FileMode.Append);

			var toWrite = Encode.GetBytes(name.PadRight(16));
			fileStream.Write(toWrite, 0, 16);
			fileStream.WriteByte((byte)dataTypes.Length);
			for (int i = 0; i < dataTypes.Length;i++ ) fileStream.WriteByte((byte)dataTypes[i]);

			fileStream.Flush();
			fileStream.Close();

			LoadedTables.Add(name.ToLower(),
									  new Table(name, DatabaseFilesPrePath + name, dataTypes, GetTotalSize(dataTypes)));
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