using System.Drawing;

namespace Tiled2Unity.Geometry
{
	internal class Math
	{
		public static float Cross(PointF A, PointF B, PointF C)
		{
			PointF pointF = new PointF(B.X - A.X, B.Y - A.Y);
			PointF pointF2 = new PointF(C.X - B.X, C.Y - B.Y);
			return pointF.X * pointF2.Y - pointF.Y * pointF2.X;
		}
	}
}
