using System;

namespace Tiled2Unity
{
	public class Settings
	{
		private static float m_Scale = 1f;

		private const float MinScaleValue = 0.00048828125f;

		public static string ObjectTypeXml = "";

		public static bool PreferConvexPolygons = false;

		public static bool DepthBufferEnabled = false;

		public static readonly float DefaultTexelBias = 0f;

		public static float TexelBias = DefaultTexelBias;

		public static bool IsAutoExporting = false;

		public static bool Verbose = true;

		public static float Scale
		{
			get
			{
				return m_Scale;
			}
			set
			{
				if (m_Scale >= 0.00048828125f)
				{
					m_Scale = value;
				}
			}
		}

		public static event EventHandler PreviewingDisabled;

		public static void DisablePreviewing()
		{
			if (Settings.PreviewingDisabled != null)
			{
				Settings.PreviewingDisabled(null, EventArgs.Empty);
			}
		}
	}
}
