using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Threading;

using MCFrog;
using MCFrog.History;
using MCFrog.Database;
using MCFrog.HeartBeat;

namespace LillyPad
{
	public class LillyPad
	{
		ConnectionHandler connectionHandler;
		PlayerHandler playerHandler;
		LevelHandler levelHandler;

		public static int version
		{
			get
			{
				return MCFrog.Server.version;
			}
		}

		static AppDomain HistoryAppDomain;
		HistoryController historyController;

		static AppDomain DatabaseAppDomain;
		DatabaseController databaseController; 

		static AppDomain HeartBeatDomain;
		HeartBeat heartBeater;

		static AppDomain ServerDomain;
		MCFrog.Server server;

		//Database database;
		
		//Server

		public LillyPad()
		{
			InputOutput.InitLogTypes();
			/*
			 * The LillyPad System simply takes each subsystem and starts it
			 * in its own thread, doing this allows us to desync the entire
			 * system and restart subsystems without restarting the whole
			 * server.
			 * 
			 */

			HistoryAppDomain = AppDomain.CreateDomain("History_AppDomain");
			Type t = typeof(MCFrog.History.HistoryController);
			historyController = (MCFrog.History.HistoryController)HistoryAppDomain.CreateInstanceAndUnwrap("MCFrog", t.FullName);

			DatabaseAppDomain = AppDomain.CreateDomain("Database_AppDomain");
			t = typeof(MCFrog.Database.DatabaseController);
			databaseController = (MCFrog.Database.DatabaseController)DatabaseAppDomain.CreateInstanceAndUnwrap("MCFrog", t.FullName);

			HeartBeatDomain = AppDomain.CreateDomain("HeartBeat_AppDomain");
			t = typeof(MCFrog.HeartBeat.HeartBeat);
			heartBeater = (MCFrog.HeartBeat.HeartBeat)HistoryAppDomain.CreateInstanceAndUnwrap("MCFrog", t.FullName);

			Server.Log("Starting Server SybSystems...", LogTypesEnum.system);

			ServerDomain = AppDomain.CreateDomain("Server_AppDomain");
			t = typeof(MCFrog.Server);
			server = (MCFrog.Server)ServerDomain.CreateInstanceAndUnwrap("MCFrog", t.FullName);

			server.historyControllerNS = historyController;
			server.databaseControllerNS = databaseController;
			server.Start();

			//new Thread(new ThreadStart(StartServer)).Start();
			//new Thread(new ThreadStart(StartConnectionHandler)).Start();
			//new Thread(new ThreadStart(StartPlayerHandler)).Start();
			//new Thread(new ThreadStart(StartLevelHandler)).Start();

			Server.StartInput();
		}

		void StartServer()
		{
			Server.Log("Starting Server...", LogTypesEnum.system);
			//server = new Server(historyController);
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
	}
}
