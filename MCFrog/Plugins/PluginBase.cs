using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MineFrog.Plugins
{
	public abstract class PluginBase
	{
		public abstract string Name { get; }
		public abstract Version Version { get; }
		public abstract string Author { get; }

		public virtual bool OnPlayerPlaceBlock()
		{
			return false;
		}

		public virtual bool OnPlayerBreakBlock()
		{
			return false;
		}

		public virtual void OnServeStart()
		{

		}

		public virtual void OnServerStop()
		{

		}

		public virtual void OnLevelLoad()
		{

		}

		public virtual void OnLevelUnload()
		{

		}

		public virtual void OnLevelSave()
		{

		}

		public virtual bool OnPlayerChat()
		{
			return false;
		}

		public virtual bool OnPlayerJoin()
		{
			return false;
		}

		public virtual void OnPlayerLeave()
		{

		}


	}
}
