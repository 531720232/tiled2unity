using System;
using System.Drawing;

namespace Tiled2Unity
{
	internal class TmxRotationMatrix
	{
		private float[,] m;

		public float this[int i, int j]
		{
			get
			{
				return m[i, j];
			}
			set
			{
				m[i, j] = value;
			}
		}

		public TmxRotationMatrix()
		{
			float[,] obj = new float[2, 2];
			obj[0, 0] = 1f;
			obj[1, 1] = 1f;
			m = obj;
			
		}

		public TmxRotationMatrix(float degrees)
		{
			float[,] obj = new float[2, 2];
			obj[0, 0] = 1f;
			obj[1, 1] = 1f;
			m = obj;
		
			double num = (double)degrees * 3.1415926535897931 / 180.0;
			float num2 = (float)Math.Cos(num);
			float num3 = (float)Math.Sin(num);
			m[0, 0] = num2;
			m[0, 1] = 0f - num3;
			m[1, 0] = num3;
			m[1, 1] = num2;
		}

		public TmxRotationMatrix(float m00, float m01, float m10, float m11)
		{
			float[,] obj = new float[2, 2];
			obj[0, 0] = 1f;
			obj[1, 1] = 1f;
			m = obj;
		
			m[0, 0] = m00;
			m[0, 1] = m01;
			m[1, 0] = m10;
			m[1, 1] = m11;
		}

		public static TmxRotationMatrix Multiply(TmxRotationMatrix M1, TmxRotationMatrix M2)
		{
			float m = M1[0, 0] * M2[0, 0] + M1[0, 1] * M2[1, 0];
			float m2 = M1[0, 0] * M2[0, 1] + M1[0, 1] * M2[1, 1];
			float m3 = M1[1, 0] * M2[0, 0] + M1[1, 1] * M2[1, 0];
			float m4 = M1[1, 0] * M2[0, 1] + M1[1, 1] * M2[1, 1];
			return new TmxRotationMatrix(m, m2, m3, m4);
		}

		public void TransformPoint(ref PointF pt)
		{
			float x = pt.X * m[0, 0] + pt.Y * m[1, 0];
			float y = pt.X * m[0, 1] + pt.Y * m[1, 1];
			pt.X = x;
			pt.Y = y;
		}

		public void TransformPoints(PointF[] points)
		{
			for (int i = 0; i < points.Length; i++)
			{
				TransformPoint(ref points[i]);
			}
		}
	}
}
