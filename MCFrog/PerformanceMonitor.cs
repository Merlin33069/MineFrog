using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCFrog
{
	class PerformanceMonitor
	{
		bool _isEnabled = true;
		bool isEnabled = true;
		System.Diagnostics.Process proc;
		DateTime startTime;

		internal PerformanceMonitor()
		{
			proc = System.Diagnostics.Process.GetCurrentProcess();
			startTime = proc.StartTime;

			StartLoop();
		}

		void StartLoop()
		{
			while (!Server.shouldShutdown && isEnabled)
			{
				System.Threading.Thread.Sleep(10000); //Ten second wait time

				//TimeSpan timeSpan = DateTime.Now - startTime;
				//Server.Log("Uptime: " + timeSpan.ToString(), LogTypesEnum.info);

				//Server.Log("", LogTypesEnum.info);
			}
			isEnabled = false;
		}
	}
}
