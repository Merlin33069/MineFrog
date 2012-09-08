using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MineFrog.Commands
{
	public abstract class CommandBase
	{
		/// <summary>
		/// This is the Name of your command, this is only used for informational purposes (inside of /help for example)
		/// </summary>
		public abstract string Name { get; }
		/// <summary>
		/// This is the type of your command, what is your command used for? moderation? just for fun? let us know!
		/// </summary>
		public abstract CommandTypes Type { get; }
		/// <summary>
		/// What is YOUR name? this can be your real name if you want but we reccomend that you use either your minecraft name or a new name just for your commands!
		/// </summary>
		public abstract string Author { get; }
		/// <summary>
		/// The version of your command, be sure to keep this up to date!
		/// </summary>
		public abstract Version Version { get; }
		/// <summary>
		/// The permission level of your command, what level of permission should players have to use your command? this can be changed by server owners ofc.
		/// </summary>
		public abstract byte Permission { get; }
		/// <summary>
		/// Here is where you list off the ways that users use your command, normally you would just add your commands name here.
		/// 
		/// Note:
		/// This is NOT case sensative
		/// If a command that uses the same accessor as yours is added before your command, that accessor for your command will not be added! This may mean that your command is unusable!
		/// </summary>
		public abstract string[] Accessors { get; }

		/// <summary>
		/// This method will be called when a player uses your command
		/// </summary>
		/// <param name="p">The player that called your command.</param>
		/// <param name="parameters">The parameters (as strings) that the player entered for your command, for example if a player entered "/say hello there" than this string[] would contain two strings, "hello" and "there", if you need chat messages you can more easily get them by using the fullcommand parameter</param>
		/// <param name="fullCommand">This is the full command (without the actual command accessor) that the player used, you can use this to get chat messages if you need to, to get parameters it is suggester you use the parameter parameter (lolwut)</param>
		public virtual void PlayerUse(Player p, string[] parameters, string fullCommand)
		{
			p.SendMessage(Name + " is a console only command!.");
		}
		/// <summary>
		/// This method is called when your command is NOT called by a player (for exampe if the Console uses a command)
		/// </summary>
		/// <param name="parameters">The parameters (as strings) that the player entered for your command, for example if a player entered "/say hello there" than this string[] would contain two strings, "hello" and "there", if you need chat messages you can more easily get them by using the fullcommand parameter</param>
		/// <param name="fullCommand">This is the full command (without the actual command accessor) that the player used, you can use this to get chat messages if you need to, to get parameters it is suggester you use the parameter parameter (lolwut)</param>
		public virtual void ConsoleUse(string[] parameters, string fullCommand)
		{
			Server.Log(Name + " Is not available as a console command!", LogTypesEnum.Info);
		}

		/// <summary>
		/// This method is called when a player uses /Help *Command*
		/// </summary>
		/// <param name="p">The player that called help on your command</param>
		public virtual void PlayerHelp(Player p)
		{
			p.SendMessage(Name + " is a console only command!.");
		}
		/// <summary>
		/// This method is called when /help is used on your command by the console.
		/// </summary>
		public virtual void ConsoleHelp()
		{
			Server.Log(Name + " Is not available as a console command!", LogTypesEnum.Info);
		}

		public void Initialize()
		{
			foreach (var accessor in Accessors)
			{
				if (CommandHandler.Commands.ContainsKey(accessor.ToLower()))
				{
					continue;
				}

				CommandHandler.Commands.Add(accessor.ToLower(), this);

				//Server.Log(accessor + " added for command " + Name, LogTypesEnum.Debug);
			}
		}
	}
	public enum CommandTypes
	{
		Mod,
	}
}
