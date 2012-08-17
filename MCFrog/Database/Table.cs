using System;
using System.IO;

namespace MCFrog.Database
{
	[Serializable]
	public class Table
	{
		internal static Database database;

		readonly DataTypes[] _dataTypes; //The types of data that are in each row
		private readonly int[] _dataPositions; //This is the position in each row of each column
		private readonly FileStream _fileStream;

		readonly string _name;
		readonly int _rowSize; //Number of bytes in each TBrow
		readonly string _path; //The path to this table file
		long _rowCount; //Number of rows currently in the table

		internal Table(string name, string path, DataTypes[] types, int size)
		{
			_name = name;
			_path = path;
			_dataTypes = types;
			_rowSize = size;
			_dataPositions = database.GetPositions(_dataTypes);

			//Open the file but dont close it until the table needs "unloaded"
			_fileStream = File.Open(_path, FileMode.OpenOrCreate);

			GetRowCount();
		}

		void GetRowCount()
		{
			long size = _fileStream.Length;
			
			if ((size % _rowSize) != 0)
			{
				Console.WriteLine("OH MA GERDZ THE SIZE IS WRONG, FILE IZ CORRUPTEDZ!");
				throw new IOException("ERROR, TABLE file is CORRUPT! " + _name);
			}
			_rowCount = size / _rowSize;

			Console.WriteLine("Number of rows in " + _name + ": " + _rowCount);
		}

		public long NewRow(object[] data)
		{
			if (data.Length != _dataTypes.Length) throw new InvalidDataException("Invalid Column Count!");

			var id = _rowCount;
			++_rowCount;

			var bytes = database.GetBytes(_rowSize, _dataTypes, data);

			var fs = new FileStream(_path, FileMode.Append);
			fs.Write(bytes, 0, bytes.Length);
			fs.Close();

			return id;
		}

		public void UpdateRow(long id, object[] data)
		{
			if (data.Length != _dataTypes.Length) throw new InvalidDataException("Invalid Column Count!");

			var position = id*_rowSize;
			var bytes = database.GetBytes(_rowSize, _dataTypes, data);

			if (bytes.Length != _rowSize) throw new InvalidDataException("Invalid Columns Size!");

			_fileStream.Position = position;

			_fileStream.Write(bytes, 0, _rowSize);
		}
		public void UpdateItem(long id, byte columnNumber, object data)
		{
			var position = (id * _rowSize) + _dataPositions[columnNumber];
			var bytes = database.GetBytes(_dataTypes[columnNumber], data);

			var size = database.GetSize(_dataTypes[columnNumber]);

			if (bytes.Length != size) throw new InvalidDataException("Invalid data size!");

			_fileStream.Position = position;

			_fileStream.Write(bytes, 0, database.GetSize(_dataTypes[columnNumber]));
		}

		public object[] GetData(int id)
		{
			if (id > _rowCount) throw new InvalidDataException("ID was too high, not that many rows in table!");
			var position = id * _rowSize;
			var returnData = new object[_dataTypes.Length];

			_fileStream.Position = position;

			for (int i = 0; i < _dataTypes.Length; ++i)
			{
				returnData[i] = database.GetData(_dataTypes[i], _fileStream);
			}

			return returnData;
		}

	}
}
