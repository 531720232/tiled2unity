using System;

namespace Tiled2Unity
{
	public class Logger
	{
		public delegate void WriteVerboseDelegate(string line);

		public delegate void WriteInfoDelegate(string line);

		public delegate void WriteSuccessDelegate(string line);

		public delegate void WriteWarningDelegate(string line);

		public delegate void WriteErrorDelegate(string line);

		public static event WriteVerboseDelegate OnWriteVerbose;

		public static event WriteInfoDelegate OnWriteInfo;

		public static event WriteSuccessDelegate OnWriteSuccess;

		public static event WriteWarningDelegate OnWriteWarning;

		public static event WriteErrorDelegate OnWriteError;

		public static void WriteVerbose()
		{
			WriteVerbose("");
		}

		public static void WriteVerbose(string line)
		{
			if (Settings.Verbose)
			{
				line += "\n";
				if (Logger.OnWriteVerbose != null)
				{
					Logger.OnWriteVerbose(line);
				}
			}
		}

		public static void WriteVerbose(string fmt, params object[] args)
		{
			if (Settings.Verbose)
			{
				WriteVerbose(string.Format(fmt, args));
			}
		}

		public static void WriteInfo()
		{
			WriteInfo("");
		}

		public static void WriteInfo(string line)
		{
			line += "\n";
			if (Logger.OnWriteInfo != null)
			{
				Logger.OnWriteInfo(line);
			}
			UnityEngine.Debug.Log(line);
		}

		public static void WriteInfo(string fmt, params object[] args)
		{
			WriteInfo(string.Format(fmt, args));
		}

		public static void WriteSuccess(string success)
		{
			success += "\n";
			if (Logger.OnWriteSuccess != null)
			{
				Logger.OnWriteSuccess(success);
			}
            UnityEngine.Debug.Log(success);
		}

		public static void WriteSuccess(string fmt, params object[] args)
		{
			WriteSuccess(string.Format(fmt, args));
		}

		public static void WriteWarning(string warning)
		{
			warning += "\n";
			if (Logger.OnWriteWarning != null)
			{
				Logger.OnWriteWarning(warning);
			}
            UnityEngine.Debug.LogWarning(warning);
		}

		public static void WriteWarning(string fmt, params object[] args)
		{
			WriteWarning(string.Format(fmt, args));
		}

		public static void WriteError(string error)
		{
			error += "\n";
			if (Logger.OnWriteError != null)
			{
				Logger.OnWriteError(error);
			}
            UnityEngine.Debug.LogError(error);
		}

		public static void WriteError(string fmt, params object[] args)
		{
			WriteError(string.Format(fmt, args));
		}
	}
}
