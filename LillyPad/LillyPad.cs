using System;
using MCFrog;
using MCFrog.Database;
using MCFrog.HeartBeat;
using MCFrog.History;

namespace LillyPad
{
    public class LillyPad
    {
        //ConnectionHandler _connectionHandler;
        //PlayerHandler _playerHandler;
        //LevelHandler _levelHandler;

        private static AppDomain _historyAppDomain;

        private static AppDomain _databaseAppDomain;

        private static AppDomain _heartBeatDomain;

        private static AppDomain _serverDomain;
        private readonly DatabaseController _databaseController;
        private readonly HistoryController _historyController;
        private readonly Server _server;
        private HeartBeat _heartBeater;

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

            _historyAppDomain = AppDomain.CreateDomain("History_AppDomain");
            Type t = typeof (HistoryController);
            if (t.FullName != null)
                _historyController = (HistoryController) _historyAppDomain.CreateInstanceAndUnwrap("MCFrog", t.FullName);

            _databaseAppDomain = AppDomain.CreateDomain("Database_AppDomain");
            t = typeof (DatabaseController);
            if (t.FullName != null)
                _databaseController =
                    (DatabaseController) _databaseAppDomain.CreateInstanceAndUnwrap("MCFrog", t.FullName);

            _heartBeatDomain = AppDomain.CreateDomain("HeartBeat_AppDomain");
            t = typeof (HeartBeat);
            if (t.FullName != null)
                _heartBeater = (HeartBeat) _historyAppDomain.CreateInstanceAndUnwrap("MCFrog", t.FullName);

            Server.Log("Starting Server SybSystems...", LogTypesEnum.System);

            _serverDomain = AppDomain.CreateDomain("Server_AppDomain");
            t = typeof (Server);
            if (t.FullName != null)
                _server = (Server) _serverDomain.CreateInstanceAndUnwrap("MCFrog", t.FullName);

            _server.HistoryControllerNS = _historyController;
            _server.DatabaseControllerNS = _databaseController;
            _server.Start();

            //new Thread(new ThreadStart(StartServer)).Start();
            //new Thread(new ThreadStart(StartConnectionHandler)).Start();
            //new Thread(new ThreadStart(StartPlayerHandler)).Start();
            //new Thread(new ThreadStart(StartLevelHandler)).Start();

            Server.StartInput();
        }

        public static int Version
        {
            get { return Server.Version; }
        }

/*
		void StartServer()
		{
			Server.Log("Starting Server...", LogTypesEnum.system);
			//server = new Server(historyController);
		}

		void StartConnectionHandler()
		{
			Server.Log("Starting ConnectionHandler...", LogTypesEnum.system);
			_connectionHandler = new ConnectionHandler();
		}
		void StartPlayerHandler()
		{
			Server.Log("Starting PlayerHandler...", LogTypesEnum.system);
			_playerHandler = new PlayerHandler();
		}
		void StartLevelHandler()
		{
			Server.Log("Starting LevelHandler...", LogTypesEnum.system);
			_levelHandler = new LevelHandler();
		}
 */
    }
}