using System;
using System.Diagnostics;
using System.Threading;

namespace MineFrog
{
	internal class PerformanceMonitor
	{
		private readonly Process _proc;
		private bool _isEnabled = true;
		private DateTime _startTime;

		internal PerformanceMonitor()
		{
			_proc = Process.GetCurrentProcess();
			_startTime = _proc.StartTime;

			StartLoop();
		}

		private void StartLoop()
		{
			while (!Server.ShouldShutdown && _isEnabled)
			{
				Thread.Sleep(10000); //Ten second wait time

				//TimeSpan timeSpan = DateTime.Now - startTime;
				//Server.Log("Uptime: " + timeSpan.ToString(), LogTypesEnum.info);

				//Server.Log("", LogTypesEnum.info);
			}
			_isEnabled = false;
		}
	}

	public struct Averager
	{
		private long _sum;
		private int _count;

		public void Add(long number)
		{
			_sum += number;
			_count++;
		}
		public float Average()
		{
			if (_count == 0) return 0;
			return (float)_sum/_count;
		}

		public void Clear()
		{
			_sum = 0;
			_count = 0;
		}
	}
}