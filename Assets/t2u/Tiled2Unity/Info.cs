using System.Reflection;

namespace Tiled2Unity
{
	public class Info
	{
		public static string GetLibraryName()
		{
			return "Tiled2Unity";
		}

		public static string GetVersion()
		{
			return new AssemblyName(Assembly.GetExecutingAssembly().FullName).Version.ToString();
		}

		public static string GetPlatform()
		{
			Assembly.GetExecutingAssembly().ManifestModule.GetPEKind(out PortableExecutableKinds peKind, out ImageFileMachine _);
			if (peKind.HasFlag(PortableExecutableKinds.PE32Plus))
			{
				return "Win64";
			}
			return "Win32";
		}
	}
}
