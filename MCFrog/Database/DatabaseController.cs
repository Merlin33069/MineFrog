using System;

namespace MCFrog.Database
{
	public class DatabaseController : MarshalByRefObject
	{
		Database database;

		public DatabaseController()
		{
			Console.WriteLine("Starting DB Controller...");

			database = new Database();

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
