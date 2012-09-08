using System;
using System.Threading;
using MineFrog.Database;
using MineFrog.History;

namespace MineFrog
{
	public class Server : MarshalByRefObject
	{
		public static bool IsAlive = true;
		public static bool ShouldShutdown = false;
		public static InputOutput Input;

		public static HistoryController HistoryController;
		public static DatabaseController DatabaseController;
		public static HeartBeat.HeartBeat HeartBeat;
		public static InputOutput inputOutput;

		public static int Version = 4;
		public DatabaseController DONOTUSEMEDatabaseControllerNS;
		public HistoryController DONOTUSEMEHistoryControllerNS;
		public InputOutput DONOTUSEMEInputOutputNS;
		public HeartBeat.HeartBeat DONOTUSEMEHeartBeatNS;

		public static Table users;
		public static Table groups;

		public void Start()
		{
			inputOutput = DONOTUSEMEInputOutputNS;
			DatabaseController = DONOTUSEMEDatabaseControllerNS;

			HistoryController = DONOTUSEMEHistoryControllerNS;
			HeartBeat = DONOTUSEMEHeartBeatNS;
			InputOutput.InitLogTypes();
			Block.LoadBlocks();

			try
			{
				new Thread(StartConnectionHandler).Start();
				new Thread(StartPlayerHandler).Start();
				new Thread(StartLevelHandler).Start();
				new Thread(StartPerformanceMonitor).Start();
				StartCommandHandler();
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				Console.WriteLine(e.StackTrace);
			}

			int count = 0;
			//while (!DatabaseController.database.IsInitialized)
			//{
			//    Thread.Sleep(1000);
			//    count++;
			//    if (count == 10)
			//    {
			//        //try
			//        //{
			//        //    DatabaseController.database.LoadKeyFile();
			//        //}
			//        //catch
			//        //{
			//            throw new Exception("DATABASE NOT LOADING!");
			//        //}
					
			//    }
			//}
			//DatabaseController.LoadKeyFile();
			//DatabaseController.database.LoadKeyFile();
			CheckDatabaseTables();
			
			StartInput();
		}

		void CheckDatabaseTables()
		{
			Server.Log("Loading Database Tables.", LogTypesEnum.Info);
			CheckGroups();
			CheckUsers();
		}

		void CheckUsers()
		{
			//Server.Log("Chk Usr", LogTypesEnum.Debug);
			if (!DatabaseController.TableExists("users"))
			{
				Server.Log("Table No Exists", LogTypesEnum.Debug);
				DatabaseController.CreateNewTable("users", new[] { DataTypes.Name, DataTypes.Message, DataTypes.Name, DataTypes.Byte, DataTypes.Int, DataTypes.Bool, DataTypes.Bool });
			}
			if (!DatabaseController.TableExists("users"))
			{
				throw new NullReferenceException("Table USERS creation FAILED!");
			}

			users = DatabaseController.FindTable("users");

			for (int i = 0; i < users.RowCount; ++i)
			{
				//Server.Log("Loading PDB #" + i, LogTypesEnum.Debug);
				new PreLoader.PDB(i, users.GetData(i));
			}
		}
		void CheckGroups()
		{
			//Server.Log("Chk Grp", LogTypesEnum.Debug);
			if (!DatabaseController.TableExists("groups"))
			{
				Server.Log("Table No Exists", LogTypesEnum.Debug);
				DatabaseController.CreateNewTable("groups", new[] { DataTypes.Name, DataTypes.Message, DataTypes.Name, DataTypes.Byte, DataTypes.Bool, DataTypes.Bool, DataTypes.Bool, DataTypes.Int});
			}
			if (!DatabaseController.TableExists("groups"))
			{
				throw new NullReferenceException("Table GROUPS creation FAILED!");
			}

			groups = DatabaseController.FindTable("groups");

			if(groups.RowCount == 0)
			{
				InitializeBaseGroups();
			}

			for (int i = 0; i < groups.RowCount; ++i)
			{
				//Server.Log("Loading GDB #" + i, LogTypesEnum.Debug);
				new PreLoader.GDB(i, groups.GetData(i));
			}
		}

		void InitializeBaseGroups()
		{
			var dbData = new object[] { "Guest", "<GUEST>", MCColor.white, (byte)1, false, true, true, 1000 };
			Server.groups.NewRow(dbData);

			dbData = new object[] { "Builder", "<BLDR>", MCColor.lime, (byte)50, false, true, true, 1000 };
			Server.groups.NewRow(dbData);

			dbData = new object[] { "OP", "<OP>", MCColor.teal, (byte)100, true, true, true, 5000 };
			Server.groups.NewRow(dbData);

			dbData = new object[] { "Owner", "<OWNER>", MCColor.gold, (byte)200, true, true, true, 10000 };
			Server.groups.NewRow(dbData);

			dbData = new object[] { "Banned", "<BANNED>", MCColor.black, (byte)0, false, false, false, 0 };
			Server.groups.NewRow(dbData);
		}

		private void StartConnectionHandler()
		{
			Log("Starting ConnectionHandler...", LogTypesEnum.System);
			new ConnectionHandler();
		}

		private void StartPlayerHandler()
		{
			Log("Starting PlayerHandler...", LogTypesEnum.System);
			new PlayerHandler();
		}

		private void StartLevelHandler()
		{
			Log("Starting LevelHandler...", LogTypesEnum.System);
			new LevelHandler();
		}

		private void StartPerformanceMonitor()
		{
			Log("Starting PerformanceMonitor...", LogTypesEnum.System);
			new PerformanceMonitor();
		}

		private void StartCommandHandler()
		{
			Log("Starting CommandHandler...", LogTypesEnum.System);
			new Commands.CommandHandler();
		}

		public static void StartInput()
		{
			Input = new InputOutput();
		}

		public static void Log(string message, LogTypesEnum logTypes)
		{
			InputOutput.Log(message, logTypes);
		}

		public static void Log(Exception e, LogTypesEnum logTypes)
		{
			Log(e.Message, logTypes);
			Log(e.StackTrace, logTypes);
		}

		public override object InitializeLifetimeService()
		{
			// returning null here will prevent the lease manager
			// from deleting the object.
			return null;
		}
	}
}