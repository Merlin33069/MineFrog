using System;
using MineFrog;
using MineFrog.Database;
using MineFrog.HeartBeat;
using MineFrog.History;

namespace LillyPad
{
	public class LillyPad
	{
		private static AppDomain _historyAppDomain;

		private static AppDomain _databaseAppDomain;

		private static AppDomain _heartBeatDomain;

		private static AppDomain _serverDomain;
		private readonly DatabaseController _databaseController;
		private readonly HistoryController _historyController;
		private readonly Server _server;
		private HeartBeat _heartBeater; //TODO pass heartbeater and pass back current users

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

			_historyAppDomain = AppDomain.CreateDomain("History_AppDomain");
			Type t = typeof (HistoryController);
			if (t.FullName != null)
				_historyController = (HistoryController) _historyAppDomain.CreateInstanceAndUnwrap("MineFrog", t.FullName);

			_databaseAppDomain = AppDomain.CreateDomain("Database_AppDomain");
			t = typeof (DatabaseController);
			if (t.FullName != null)
				_databaseController =
					(DatabaseController)_databaseAppDomain.CreateInstanceAndUnwrap("MineFrog", t.FullName);

			_heartBeatDomain = AppDomain.CreateDomain("HeartBeat_AppDomain");
			t = typeof (HeartBeat);
			if (t.FullName != null)
				_heartBeater = (HeartBeat)_historyAppDomain.CreateInstanceAndUnwrap("MineFrog", t.FullName);

			Server.Log("Starting Server SybSystems...", LogTypesEnum.System);

			#region TODO Move server to auto-restarter!

			_serverDomain = AppDomain.CreateDomain("Server_AppDomain");
			t = typeof (Server);
			if (t.FullName != null)
				_server = (Server) _serverDomain.CreateInstanceAndUnwrap("MineFrog", t.FullName);

			_server.DONOTUSEMEHistoryControllerNS = _historyController;
			_server.DONOTUSEMEDatabaseControllerNS = _databaseController;
			_server.DONOTUSEMEHeartBeatNS = _heartBeater;
			_server.Start();

			//Server.StartInput();

			
			#endregion

		}

		public static int Version
		{
			get { return Server.Version; }
		}
	}
}