using System;
using System.Collections.Generic;
using System.Text;
using MCFrog.History;
using MCFrog.Database;
using System.Threading;

namespace MCFrog
{
	public class Server : MarshalByRefObject
	{
		public static bool isAlive = true;
		public static bool shouldShutdown = false;
		public static InputOutput input;

		public HistoryController historyControllerNS;
		public static HistoryController historyController;

		public DatabaseController databaseControllerNS;
		public static DatabaseController databaseController;

		ConnectionHandler connectionHandler;
		PlayerHandler playerHandler;
		LevelHandler levelHandler;
		PerformanceMonitor performanceMonitor;

		public static int version = 4;

		public void Start()
		{
			historyController = historyControllerNS;
			databaseController = databaseControllerNS;
			InputOutput.InitLogTypes();
			Block.Initialize();

			try
			{
				PhysicsHandler.LoadPhysicsTypes();

				new Thread(new ThreadStart(StartConnectionHandler)).Start();
				new Thread(new ThreadStart(StartPlayerHandler)).Start();
				new Thread(new ThreadStart(StartLevelHandler)).Start();
				new Thread(new ThreadStart(StartPerformanceMonitor)).Start();
			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
				Console.WriteLine(e.StackTrace);
			}
				new TestClass();
		}

		void StartConnectionHandler()
		{
			Server.Log("Starting ConnectionHandler...", LogTypesEnum.system);
			connectionHandler = new ConnectionHandler();
		}
		void StartPlayerHandler()
		{
			Server.Log("Starting PlayerHandler...", LogTypesEnum.system);
			playerHandler = new PlayerHandler();
		}
		void StartLevelHandler()
		{
			Server.Log("Starting LevelHandler...", LogTypesEnum.system);
			levelHandler = new LevelHandler();
		}
		void StartPerformanceMonitor()
		{
			Server.Log("Starting PerformanceMonitor...", LogTypesEnum.system);
			performanceMonitor = new PerformanceMonitor();
		}

		public static void StartInput()
		{
			input = new InputOutput();
		}

		public static void Log(string message, LogTypesEnum logTypes)
		{
			InputOutput.Log(message, logTypes);
		}

		public static void Log(Exception E, LogTypesEnum logTypes)
		{
			Log(E.Message, logTypes);
			Log(E.StackTrace, logTypes);
		}

		public override object InitializeLifetimeService()
		{
			// returning null here will prevent the lease manager
			// from deleting the object.
			return null;
		}
	}
}
