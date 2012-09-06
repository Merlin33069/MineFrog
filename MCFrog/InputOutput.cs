using System;
using System.Collections.Generic;
using System.Globalization;


//TODO Setup the TYPES of logging to be static, so that I can use them in other subsystems.

namespace MineFrog
{
	public class InputOutput
	{
		public static InputOutput Instance;

		private Server server;

		public static Dictionary<LogTypesEnum, LogTypeClass> LogTypeList = new Dictionary<LogTypesEnum, LogTypeClass>();
		public static string Messagepart = "";

		public bool Shutdown = false;

		public InputOutput()
		{
			while (!Shutdown)
			{
				ConsoleKeyInfo keyinfo = Console.ReadKey(true);
				char keyChar = keyinfo.KeyChar;
				if (keyChar == '\r')
				{
					Messagepart = Messagepart.Trim();

					if (Messagepart != "")
					{
						HandleCommand(Messagepart);
					}

					string overwrite = "";
					for (int i = 0; i < Messagepart.Length; i++)
					{
						overwrite += " ";
					}

					Console.SetCursorPosition(0, Console.CursorTop);
					Console.Write(overwrite);
					Console.SetCursorPosition(0, Console.CursorTop);

					Messagepart = "";
				}
				else
				{
					Messagepart += keyChar.ToString(CultureInfo.InvariantCulture);
					Console.Write(keyChar.ToString(CultureInfo.InvariantCulture));
				}
				//Server.Log(key, LogTypesEnum.debug);
			}
		}

		public static void Log(string message, ConsoleColor textColor, ConsoleColor backgroundColor)
		{
			if (Messagepart.Trim() != "")
			{
				if (Console.CursorTop > 0)
					Console.SetCursorPosition(0, Console.CursorTop);
			}

			Console.ForegroundColor = textColor;
			Console.BackgroundColor = backgroundColor;
			Console.WriteLine(message.PadRight(Console.WindowWidth - 1));
			Console.ResetColor();

			if (Messagepart.Trim() != "")
			{
				Console.Write(Messagepart);
				//Console.SetCursorPosition(messagepart.Length, Console.CursorTop);
			}
		}

		public static void Log(string message, LogTypesEnum logTypes)
		{
			//if (logTypes == LogTypesEnum.debug) return;
			LogTypeClass logType = LogTypeList[logTypes];
			Log(logType.Prefix + message, logType.TextColor, logType.BackgroundColor);
		}

		private static void HandleCommand(string message)
		{
			string[] command = message.Split(' ');
			
			string messageSend = "";
			string[] parameters = new string[0];

			string accessor = command[0].ToLower().Trim();
			if(command.Length > 1)
			{
				message = message.Substring(accessor.Length + 1);
				parameters = message.Split(' ');
			}

			if (Commands.CommandHandler.Commands.ContainsKey(accessor))
				Commands.CommandHandler.Commands[accessor].ConsoleUse(parameters, messageSend);
			else
			{
				Server.Log("Command Not Found!", LogTypesEnum.Error);
			}
		}

		public static void InitLogTypes()
		{
			LogTypeList.Add(LogTypesEnum.Normal, new LogTypeClass(ConsoleColor.Gray, ConsoleColor.Black, ""));
			LogTypeList.Add(LogTypesEnum.Info, new LogTypeClass(ConsoleColor.White, ConsoleColor.Black, "[INFO]:"));
			LogTypeList.Add(LogTypesEnum.System, new LogTypeClass(ConsoleColor.Green, ConsoleColor.Black, "[SYSTEM]:"));

			LogTypeList.Add(LogTypesEnum.Warning, new LogTypeClass(ConsoleColor.Red, ConsoleColor.Black, "[WARNING]:"));
			LogTypeList.Add(LogTypesEnum.Error, new LogTypeClass(ConsoleColor.Yellow, ConsoleColor.Black, "[ERROR]:"));
			LogTypeList.Add(LogTypesEnum.Critical, new LogTypeClass(ConsoleColor.White, ConsoleColor.Red, "[CRITICAL]:"));

			LogTypeList.Add(LogTypesEnum.Chat, new LogTypeClass(ConsoleColor.Magenta, ConsoleColor.Black, ""));
			LogTypeList.Add(LogTypesEnum.Debug, new LogTypeClass(ConsoleColor.DarkGreen, ConsoleColor.Black, "[DBG]:"));
		}

		public static void LogSamples()
		{
			Log("Normal Logging Message", LogTypesEnum.Normal);
			Log("Informational Logging Message", LogTypesEnum.Info);
			Log("System Information Logging", LogTypesEnum.System);
			Log("WARNING Logging message", LogTypesEnum.Warning);
			Log("OMFG AN ERROR =(", LogTypesEnum.Error);
			Log("WE ARE ALL GONNA DIE O_o", LogTypesEnum.Critical);
			Log("Hi, i'm bob, what's your name?", LogTypesEnum.Chat);
			Log("Debug-g-g-g-g-g-g-g-g-g.... Hello? anyone there? :*(", LogTypesEnum.Debug);
		}
	}

	public enum LogTypesEnum
	{
		Normal, //normal is for just run of the mill stuff (player connections for example)
		Info, //info is more in depth but not required 
		System, //Server info (startup shutdown loaded etc)
		Warning, //Server warnings (high traffic etc)
		Error, //Server errors
		Critical, //Errors that cause sessions to crash

		Chat, //Chat messages
		Debug, //Debug messages
	}

	public class LogTypeClass
	{
		internal ConsoleColor BackgroundColor;
		internal string Prefix;
		internal ConsoleColor TextColor;
		//TODO add an event to allow for doing things on errors and whatnot

		internal LogTypeClass(ConsoleColor textColor, ConsoleColor backgroundColor, string prefix)
		{
			TextColor = textColor;
			BackgroundColor = backgroundColor;
			Prefix = prefix;
			//TODO add event thing here... maybe
		}
	}
}