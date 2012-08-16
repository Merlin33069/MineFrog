using System;
using System.IO;

namespace MCFrog.Database
{
	public class Table
	{
		internal static Database database;

		readonly DataTypes[] _dataTypes; //The types of data that are in each row

		readonly string _name;
		readonly int _rowSize; //Number of bytes in each row
		readonly string _path; //The path to this table file
		long _rowCount; //Number of rows currently in the table

		internal Table(string name, string path, DataTypes[] types, int size)
		{
			_name = name;
			_path = path;
			_dataTypes = types;
			_rowSize = size;

			if (!File.Exists(path))
				File.Create(path);

			GetRowCount();
		}

		void GetRowCount()
		{
			var fi = new FileInfo(_path);
			long size = fi.Length;

			if ((size % _rowSize) != 0)
			{
				Console.WriteLine("OH MA GERDZ THE SIZE IS WRONG, FILE IZ CORRUPTEDZ!");
				throw new IOException("ERROR, TABLE file is CORRUPT! " + _name);
			}
			_rowCount = size / _rowSize;

			Console.WriteLine("Number of rows in " + _name + ": " + _rowCount);

			foreach (var dataTypes in _dataTypes)
			{
				Console.WriteLine(dataTypes + "");
			}

		}

		long NewRow(object[] data)
		{
			if (data.Length != _dataTypes.Length) throw new InvalidDataException("Invalid Column Count!");

			long id = _rowCount;
			++_rowCount;

			byte[] bytes = database.GetBytes(_rowSize, _dataTypes, data);

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
