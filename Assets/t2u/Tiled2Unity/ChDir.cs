using System;
using System.IO;

namespace Tiled2Unity
{
	public class ChDir : IDisposable
	{
		private string directoryOld = "";

		private string directoryNow = "";

		public ChDir(string path)
		{
			directoryOld = Directory.GetCurrentDirectory();
			if (Directory.Exists(path))
			{
				directoryNow = path;
			}
			else
			{
				if (!File.Exists(path))
				{
					throw new DirectoryNotFoundException($"Cannot set current directory. Does not exist: {path}");
				}
				directoryNow = Path.GetDirectoryName(path);
			}
			Directory.SetCurrentDirectory(directoryNow);
		}

		public void Dispose()
		{
			Directory.SetCurrentDirectory(directoryOld);
		}
	}
}
