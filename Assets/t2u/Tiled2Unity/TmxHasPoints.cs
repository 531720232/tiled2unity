using System.Collections.Generic;
using System.Drawing;

namespace Tiled2Unity
{
	public interface TmxHasPoints
	{
		List<PointF> Points
		{
			get;
			set;
		}

		bool ArePointsClosed();
	}
}
