using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace MCFrog.Database
{
	class Table
	{
		DataTypes[] dataTypes; //The types of data that are in each row

		int rowSize; //Number of bytes in each row
		long rowCount; //Number of rows currently in the table
		string path; //The path to this table file

		internal Table(string _path, DataTypes[] types, int size)
		{
			path = _path;
			dataTypes = types;
			rowSize = size;

			if (!File.Exists(path))
				File.Create(path);

			GetRowCount();
		}

		void GetRowCount()
		{
			FileInfo FI = new FileInfo(path);
			long size = FI.Length;

			if ((size % rowSize) != 0)
			{
				Console.WriteLine("OH MA GERDZ THE SIZE IS WRONG, FILE IZ CORRUPTEDZ!");
				throw new IOException("ERROR, TABLE file is CORRUPT!");
			}
			else
			{
				rowCount = size / rowSize;
			}

			Console.WriteLine("Number of rows: " + rowCount);

			NewRow(new object[2] { 5, "test" });
		}

		long NewRow(object[] data)
		{
			long id = rowCount;
			++rowCount;

			if (data.Length != dataTypes.Length) throw new InvalidDataException("Invalid Column Count!");

			byte[] bytes = Database.GetBytes(rowSize, dataTypes, data);

			FileStream fs = new FileStream(path, FileMode.Append);
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
