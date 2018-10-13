using System;
using System.Collections.Generic;

namespace Tiled2Unity
{
	internal class SummaryReport
	{
		private delegate void LoggingDelegate(string message, params object[] args);

		private string name = "";

		private List<string> successes = new List<string>();

		private List<string> warnings = new List<string>();

		private List<string> errors = new List<string>();

		public void Capture(string name)
		{
			Listen();
			this.name = name;
			successes.Clear();
			warnings.Clear();
			errors.Clear();
		}

		public void Report()
		{
			Ignore();
			LoggingDelegate loggingDelegate = Logger.WriteSuccess;
			if (warnings.Count > 0)
			{
				loggingDelegate = Logger.WriteWarning;
			}
			if (errors.Count > 0)
			{
				loggingDelegate = Logger.WriteError;
			}
			string line = new string('-', 80);
			Logger.WriteInfo(line);
			loggingDelegate("{0} summary", name);
			Logger.WriteInfo("Succeeded: {0}", successes.Count);
			List<string>.Enumerator enumerator = successes.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					string current = enumerator.Current;
					Logger.WriteSuccess("  {0}", current);
				}
			}
			finally
			{
				((IDisposable)enumerator).Dispose();
			}
			Logger.WriteInfo("Warnings: {0}", warnings.Count);
			enumerator = warnings.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					string current2 = enumerator.Current;
					Logger.WriteWarning("  {0}", current2);
				}
			}
			finally
			{
				((IDisposable)enumerator).Dispose();
			}
			Logger.WriteInfo("Errors: {0}", errors.Count);
			enumerator = errors.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					string current3 = enumerator.Current;
					Logger.WriteError("  {0}", current3);
				}
			}
			finally
			{
				((IDisposable)enumerator).Dispose();
			}
			Logger.WriteInfo(line);
		}

		private void Listen()
		{
			Logger.OnWriteSuccess += Logger_OnWriteSuccess;
			Logger.OnWriteWarning += Logger_OnWriteWarning;
			Logger.OnWriteError += Logger_OnWriteError;
		}

		private void Ignore()
		{
			Logger.OnWriteSuccess -= Logger_OnWriteSuccess;
			Logger.OnWriteWarning -= Logger_OnWriteWarning;
			Logger.OnWriteError -= Logger_OnWriteError;
		}

		private void Logger_OnWriteError(string line)
		{
			line = line.TrimEnd('\r', '\n');
			errors.Add(line);
		}

		private void Logger_OnWriteWarning(string line)
		{
			line = line.TrimEnd('\r', '\n');
			warnings.Add(line);
		}

		private void Logger_OnWriteSuccess(string line)
		{
			line = line.TrimEnd('\r', '\n');
			successes.Add(line);
		}
	}
}
