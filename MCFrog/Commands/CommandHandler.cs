using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.IO;

namespace MineFrog.Commands
{
	internal class CommandHandler
	{
		internal static Dictionary<string, CommandBase> Commands = new Dictionary<string, CommandBase>();

		internal CommandHandler()
		{
			LoadCommands();
		}

		internal void LoadCommands()
		{
			foreach (string fileOn in Directory.GetFiles(Directory.GetCurrentDirectory()))
			{
				FileInfo file = new FileInfo(fileOn);

				//Console.WriteLine("Found File in Plugin folder:");
				//Console.WriteLine(file.Name);

				//Preliminary check, must be .dll
				if (file.Extension.Equals(".dll") || file.Extension.Equals(".exe"))
				{
					//Create a new assembly from the plugin file we're adding..
					Assembly pluginAssembly = Assembly.LoadFrom(file.Name);

					//Next we'll loop through all the Types found in the assembly
					foreach (Type pluginType in pluginAssembly.GetTypes())
					{
						if (pluginType.IsSubclassOf(typeof(CommandBase)) && pluginType.IsPublic && !pluginType.IsAbstract)
						{
							var command = (CommandBase)Activator.CreateInstance(pluginAssembly.GetType(pluginType.ToString()));
							command.Initialize();
							//Server.Log("Found one :D", LogTypesEnum.Debug);
						}
					}
				}
			}
		}
	}
}

