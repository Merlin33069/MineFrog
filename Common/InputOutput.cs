using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Common
{
	public class InputOutput
	{
		public static InputOutput instance;

		public static Dictionary<LogTypesEnum, LogTypeClass> LogTypeList = new Dictionary<LogTypesEnum, LogTypeClass>();
		public static string messagepart = "";

		public bool shutdown = false;

		public InputOutput()
		{
			while (!shutdown)
			{
				ConsoleKeyInfo keyinfo = Console.ReadKey(true);
				char keyChar = keyinfo.KeyChar;
				if (keyChar == '\r')
				{
					messagepart = messagepart.Trim();

					if (messagepart != "")
					{
						HandleCommand(messagepart);
					}

					string overwrite = "";
					for (int i = 0; i < messagepart.Length; i++)
					{
						overwrite += " ";
					}

					Console.SetCursorPosition(0, Console.CursorTop);
					Console.Write(overwrite);
					Console.SetCursorPosition(0, Console.CursorTop);

					messagepart = "";
				}
				else
				{
					messagepart += keyChar.ToString();
					Console.Write(keyChar.ToString());
				}
				//Server.Log(key, LogTypesEnum.debug);
			}
		}

		public static void Log(string message, ConsoleColor TextColor, ConsoleColor BackgroundColor)
		{
			if (messagepart.Trim() != "")
			{
				if (Console.CursorTop > 0)
					Console.SetCursorPosition(0, Console.CursorTop);
				else
				{

				}
			}

			Console.ForegroundColor = TextColor;
			Console.BackgroundColor = BackgroundColor;
			Console.WriteLine(message.PadRight(Console.WindowWidth - 1));
			Console.ResetColor();

			if (messagepart.Trim() != "")
			{
				Console.Write(messagepart);
				//Console.SetCursorPosition(messagepart.Length, Console.CursorTop);
			}
		}
		public static void Log(string message, LogTypesEnum logTypes)
		{
			LogTypeClass logType = LogTypeList[logTypes];
			//Console.WriteLine("ME ME: " + logType.BackgroundColor + " " + logType.TextColor + " " + logType.Prefix);
			InputOutput.Log(logType.Prefix + message, logType.TextColor, logType.BackgroundColor);
		}

		static void HandleCommand(string command)
		{
			//TODO ?
		}

		public static void InitLogTypes()
		{
			LogTypeList.Add(LogTypesEnum.normal, new LogTypeClass(ConsoleColor.Gray, ConsoleColor.Black, ""));
			LogTypeList.Add(LogTypesEnum.info, new LogTypeClass(ConsoleColor.White, ConsoleColor.Black, "[INFO]:"));
			LogTypeList.Add(LogTypesEnum.system, new LogTypeClass(ConsoleColor.Green, ConsoleColor.Black, "[SYSTEM]:"));

			LogTypeList.Add(LogTypesEnum.warning, new LogTypeClass(ConsoleColor.Red, ConsoleColor.Black, "[WARNING]:"));
			LogTypeList.Add(LogTypesEnum.error, new LogTypeClass(ConsoleColor.Yellow, ConsoleColor.Black, "[ERROR]:"));
			LogTypeList.Add(LogTypesEnum.critical, new LogTypeClass(ConsoleColor.White, ConsoleColor.Red, "[CRITICAL]:"));

			LogTypeList.Add(LogTypesEnum.chat, new LogTypeClass(ConsoleColor.Magenta, ConsoleColor.Black, ""));
			LogTypeList.Add(LogTypesEnum.debug, new LogTypeClass(ConsoleColor.DarkGreen, ConsoleColor.Black, "[DBG]:"));
		}
		public static void LogSamples()
		{
			Log("Normal Logging Message", LogTypesEnum.normal);
			Log("Informational Logging Message", LogTypesEnum.info);
			Log("System Information Logging", LogTypesEnum.system);
			Log("WARNING Logging message", LogTypesEnum.warning);
			Log("OMFG AN ERROR =(", LogTypesEnum.error);
			Log("WE ARE ALL GONNA DIE O_o", LogTypesEnum.critical);
			Log("Hi, i'm bob, what's your name?", LogTypesEnum.chat);
			Log("Debug-g-g-g-g-g-g-g-g-g.... Hello? anyone there? :*(", LogTypesEnum.debug);
		}

	}
	public enum LogTypesEnum
	{
		normal, //normal is for just run of the mill stuff (player connections for example)
		info, //info is more in depth but not required 
		system, //Server info (startup shutdown loaded etc)
		warning, //Server warnings (high traffic etc)
		error, //Server errors
		critical, //Errors that cause sessions to crash

		chat, //Chat messages
		debug, //Debug messages

	}
	public class LogTypeClass
	{
		internal ConsoleColor TextColor;
		internal ConsoleColor BackgroundColor;
		internal string Prefix;
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
