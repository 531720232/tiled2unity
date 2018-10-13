using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Drawing
{
    public struct Size
    {
        public static readonly Size Empty=new Size(0,0);


        //
        // 摘要:
        //     测试是否这 System.Drawing.Size 结构的宽度和高度均为 0。
        //
        // 返回结果:
        //     此属性返回 true 时这 System.Drawing.Size 结构的宽度和高度均为 0; 否则为 false。
      
        public bool IsEmpty
        {
            get
            {

                if (Width == 0 && Height == 0)
                {
                    return true;
                }
                return false;
            }

        }
        //
        // 摘要:
        //     获取或设置此水平组件System.Drawing.Size结构。
        //
        // 返回结果:
        //     此水平组件System.Drawing.Size结构，通常以像素度量。
        public int Width { get; set; }
        //
        // 摘要:
        //     获取或设置的垂直分量 System.Drawing.Size 结构。
        //
        // 返回结果:
        //     垂直分量 System.Drawing.Size 结构，通常以像素为单位进行度量。
        public int Height { get; set; }
        public Size(Point point)
        {
            Width = point.X;
            Height = point.Y;
        }

        public Size(int  w,int h)
        {
            Width = w;
            Height = h;
        }
        public static Size Add(Size sz1, Size sz2)
        {
            var size = default(Size);
            size.Width =sz1.Width+ sz2.Width;
            size.Height = sz1.Height + sz2.Height;
            return size;
        }
        //public static Size Ceiling(Size sz1, Size sz2)
        //{

        //}
        //public static Size Add(Size sz1, Size sz2)
        //{

        //}
        //public static Size Add(Size sz1, Size sz2)
        //{

        //}
        public void Offset(Size f)
        {
            Width += f.Width;
            Height += f.Height;
        }
    }

    public struct SizeF
    {
        public static readonly SizeF Empty = new SizeF(0, 0);


        //
        // 摘要:
        //     测试是否这 System.Drawing.Size 结构的宽度和高度均为 0。
        //
        // 返回结果:
        //     此属性返回 true 时这 System.Drawing.Size 结构的宽度和高度均为 0; 否则为 false。

        public bool IsEmpty
        {
            get
            {

                if (Width == 0 && Height == 0)
                {
                    return true;
                }
                return false;
            }

        }
        //
        // 摘要:
        //     获取或设置此水平组件System.Drawing.Size结构。
        //
        // 返回结果:
        //     此水平组件System.Drawing.Size结构，通常以像素度量。
        public float Width { get; set; }
        //
        // 摘要:
        //     获取或设置的垂直分量 System.Drawing.Size 结构。
        //
        // 返回结果:
        //     垂直分量 System.Drawing.Size 结构，通常以像素为单位进行度量。
        public float Height { get; set; }
        public SizeF(PointF point)
        {
            Width = point.X;
            Height = point.Y;
        }

        public SizeF(float w, float h)
        {
            Width = w;
            Height = h;
        }
        public static Size Add(Size sz1, Size sz2)
        {
            var size = default(Size);
            size.Width = sz1.Width + sz2.Width;
            size.Height = sz1.Height + sz2.Height;
            return size;
        }
        public void Offset(SizeF f)
        {
            Width += f.Width;
            Height += f.Height;
        }
        //public static Size Ceiling(Size sz1, Size sz2)
        //{

        //}
        //public static Size Add(Size sz1, Size sz2)
        //{

        //}
        //public static Size Add(Size sz1, Size sz2)
        //{

        //}
    }

    public struct Point
    {
        public static readonly Point Empty=new Point(0,0);
        public Point(Size size)
        {
            X = size.Width;
            Y = size.Height;
        }
        public Point(int dw)
        {
            X =dw;
            Y = dw;
        }
        public Point(int x,int y)
        {
            X = x;
            Y = y;
        }
        public bool IsEmpty
        {
            get {
                if(X==0&&Y==0)
                {
                    return true;
                }
                return false;


            }
        }
       
public int X { get; set; }
        public int Y { get; set; }
        public static Point Add(Point pt,Size size)
        {
            var p = default(Point);
            p.X = pt.X + size.Width;
            p.Y = pt.Y + size.Height;
            return
                p;
        }
        //
        // 摘要:
        //     将指定 System.Drawing.Point 结构 System.Drawing.PointF 结构。
        //
        // 参数:
        //   p:
        //     要转换的 System.Drawing.Point。
        //
        // 返回结果:
        //     System.Drawing.PointF 中时得到的转换。
        public static implicit operator PointF(Point p)
        {
            return new PointF(p.X, p.Y);
        }
        public void Offset(Point f)
        {
            X += f.X;
            Y += f.Y;
        }

    }
    public struct PointF
    {
        public static readonly PointF Empty=new PointF(0,0);
        public PointF(float x, float y)
        {
            X = x;
            Y = y;
        }
        public bool IsEmpty
        {
            get
            {
                if (X == 0 && Y == 0)
                {
                    return true;
                }
                return false;


            }
        }
        public float X { get; set; }
        public float Y { get; set; }

        public void Offset(PointF f)
        {
            X += f.X;
            Y += f.Y;
        }
        public static PointF Add(PointF pt, Size size)
        {
            var p = default(PointF);
            p.X = pt.X + size.Width;
            p.Y = pt.Y + size.Height;
            return
                p;
        }
        public static PointF Add(PointF pt, SizeF size)
        {
            var p = default(PointF);
            p.X = pt.X + size.Width;
            p.Y = pt.Y + size.Height;
            return
                p;
        }
        public static bool operator ==(PointF left, PointF right)
        {
            return left.X == right.X&& left.Y == right.Y;
        }
        public static bool operator !=(PointF left, PointF right)
        {
            return left.X!= right.X && left.Y != right.Y;
        }
    }

    public static class RectEx
    {
        public static  UnityEngine.Rect Union(UnityEngine.Rect a,UnityEngine.Rect b)
        {
           var startX = a.xMin<b.xMin ? a.xMin : b.xMin;
         var   endX = a.xMax > b.xMax ? a.xMax : b.xMax;
        var    startY = a.yMin < b.yMin ? a.yMin : b.yMin;
        var    endY = a.yMax > b.yMax ? a.yMax : b.yMax;
            return UnityEngine.Rect.MinMaxRect(startX, startY, endX, endY);
        }

    }
}
