using System;

namespace MCFrog.Database
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
		public void CreateNewTable(string name, DataTypes[] dataTypes)
		{
			database.CreateNewTable(name, dataTypes);
		}

		public override object InitializeLifetimeService()
		{
			// returning null here will prevent the lease manager
			// from deleting the object.
			return null;
		}
	}
}
