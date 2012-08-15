using System;
using System.Threading;
using MCFrog.Database;
using MCFrog.History;

namespace MCFrog
{
    public class Server : MarshalByRefObject
    {
        public static bool IsAlive = true;
        public static bool ShouldShutdown = false;
        public static InputOutput Input;

        public static HistoryController HistoryController;

        public static DatabaseController DatabaseController;

        public static int Version = 4;
        public DatabaseController DatabaseControllerNS;
        public HistoryController HistoryControllerNS;

        public void Start()
        {
            HistoryController = HistoryControllerNS;
            DatabaseController = DatabaseControllerNS;
            InputOutput.InitLogTypes();
            Block.Initialize();

            try
            {
                PhysicsHandler.LoadPhysicsTypes();

                new Thread(StartConnectionHandler).Start();
                new Thread(StartPlayerHandler).Start();
                new Thread(StartLevelHandler).Start();
                new Thread(StartPerformanceMonitor).Start();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
            }
            new TestClass();
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