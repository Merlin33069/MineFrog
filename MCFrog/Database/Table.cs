using System;
using System.IO;

namespace MCFrog.Database
{
	class Table
	{
	    DataTypes[] _dataTypes; //The types of data that are in each row

		int _rowSize; //Number of bytes in each row
		long _rowCount; //Number of rows currently in the table
		string _path; //The path to this table file

		internal Table(string path, DataTypes[] types, int size)
		{
			_path = path;
			_dataTypes = types;
			_rowSize = size;

			if (!File.Exists(path))
				File.Create(path);

			GetRowCount();
		}

		void GetRowCount()
		{
			FileInfo fi = new FileInfo(_path);
			long size = fi.Length;

			if ((size % _rowSize) != 0)
			{
				Console.WriteLine("OH MA GERDZ THE SIZE IS WRONG, FILE IZ CORRUPTEDZ!");
				throw new IOException("ERROR, TABLE file is CORRUPT!");
			}
		    _rowCount = size / _rowSize;

		    Console.WriteLine("Number of rows: " + _rowCount);

			NewRow(new object[] { 5, "test" });
		}

		long NewRow(object[] data)
		{
			long id = _rowCount;
			++_rowCount;

			if (data.Length != _dataTypes.Length) throw new InvalidDataException("Invalid Column Count!");

			byte[] bytes = Database.GetBytes(_rowSize, _dataTypes, data);

			FileStream fs = new FileStream(_path, FileMode.Append);
			fs.Write(bytes, 0, bytes.Length);
			fs.Close();

			return id;
		}

		void UpdateRow(long id, object[] data)
		{

		}
		void UpdateItem(long id, byte columnNumber, object data)
		{

		}

		object GetData(int id, byte column)
		{
			return new object();
			//TODO id*rowsize + columnsize_before_this_column
			//thn we can get the data at that point :D
		}

	}
}
