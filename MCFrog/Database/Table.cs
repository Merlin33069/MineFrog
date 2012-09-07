using System;
using System.IO;

namespace MineFrog.Database
{
	[Serializable]
	public class Table
	{
		internal Database database;

		readonly DataTypes[] _dataTypes; //The types of data that are in each row
		private readonly int[] _dataPositions; //This is the position in each row of each column
		private readonly FileStream _fileStream;

		readonly string _name;
		readonly int _rowSize; //Number of bytes in each row
		readonly string _path; //The path to this table file
		public int RowCount; //Number of rows currently in the table

		internal Table(Database _database, string name, string path, DataTypes[] types, int size)
		{
			database = _database;
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
			RowCount = (int)(size / _rowSize);

			Console.WriteLine("Number of rows in " + _name + ": " + RowCount);
		}

		public int NewRow(object[] data)
		{
			if (data.Length != _dataTypes.Length) throw new InvalidDataException("Invalid Column Count!");

			var id = RowCount;
			++RowCount;

			var bytes = database.GetBytes(_rowSize, _dataTypes, data);

			//var fs = new FileStream(_path, FileMode.Append);
			_fileStream.Position = _fileStream.Length;
			_fileStream.Write(bytes, 0, bytes.Length);
			_fileStream.Flush();
			//var test = GetData(id);
			//Console.WriteLine("username in db is: " + (string)test[0]);

			return id;
		}

		public void UpdateRow(long id, object[] data)
		{
			try
			{
				if (data.Length != _dataTypes.Length) throw new InvalidDataException("Invalid Column Count!");

				var position = id*_rowSize;
				var bytes = database.GetBytes(_rowSize, _dataTypes, data);

				if (bytes.Length != _rowSize) throw new InvalidDataException("Invalid Columns Size!");

				_fileStream.Position = position;

				_fileStream.Write(bytes, 0, _rowSize);

				_fileStream.Flush();
			}
			catch(Exception e)
			{
				Console.WriteLine(e.Message);
				Console.WriteLine(e.StackTrace);
			}
		}
		public void UpdateItem(long id, byte columnNumber, object data)
		{
			var position = (id * _rowSize) + _dataPositions[columnNumber];
			var bytes = database.GetBytes(_dataTypes[columnNumber], data);

			var size = database.GetSize(_dataTypes[columnNumber]);

			if (bytes.Length != size) throw new InvalidDataException("Invalid data size!");

			_fileStream.Position = position;

			_fileStream.Write(bytes, 0, database.GetSize(_dataTypes[columnNumber]));

			_fileStream.Flush();
		}

		public object[] GetData(int id)
		{
			if (id > RowCount) throw new InvalidDataException("ID was too high, not that many rows in table!");
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
