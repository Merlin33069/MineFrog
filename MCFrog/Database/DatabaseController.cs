using System;

namespace MineFrog.Database
{
	public class DatabaseController : MarshalByRefObject
	{
		public Database database;

		public DatabaseController()
		{
			Console.WriteLine("Starting DB Controller...");

			database = new Database();
		}

		public bool TableExists(string tableName)
		{
			return database.TableExists(tableName);
		}
		public Table FindTable(string tableName)
		{
			return database.TableFind(tableName);
		}
		public void CreateNewTable(string name, DataTypes[] dataTypes)
		{
			database.CreateNewTable(name, dataTypes);
		}

		public void LoadKeyFile()
		{
			database.LoadKeyFile();
		}

		public override object InitializeLifetimeService()
		{
			// returning null here will prevent the lease manager
			// from deleting the object.
			return null;
		}
	}
}
