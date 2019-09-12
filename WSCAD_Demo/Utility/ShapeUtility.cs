using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WSCAD_Demo.Model;

namespace WSCAD_Demo.Utility
{
    class ShapeUtility
    {
        /// <summary>
        /// Distance between two points
        /// </summary>
        /// <param name="p1">The first point</param>
        /// <param name="p2">The second point</param>
        /// <returns></returns>
        public static double Distance(PointF p1, PointF p2)
        {
            return Math.Sqrt(Math.Pow((p1.X - p2.X), 2) +
                Math.Pow((p1.Y - p2.Y), 2));
        }

        /// <summary>
        /// Distance between line and point: (Ax + By + C) / Sqrt(A^2 + B^2)
        /// </summary>
        /// <param name="p">The point</param>
        /// <param name="l">The line</param>
        /// <returns></returns>
        public static double Distance(PointF p, Line l)
        {
            //Vertical line has no slope
            if (l.IsVerticalLine())
            {
                return Math.Abs(p.X - l.Start.X);
            }

            //kx + (-1)y + b = 0
            l.ToSlopeIntercept(out float k, out float b);
            return Math.Abs(k * p.X - p.Y + b) / Math.Sqrt(k * k + 1);
        }

        /// <summary>
        /// Find the intersection point of two lines
        /// Ref: https://en.wikipedia.org/wiki/Line–line_intersection
        /// </summary>
        /// <param name="l1">The first line</param>
        /// <param name="l2">The second line</param>
        /// <param name="intsctPoints"></param>
        public static void Intersect(Line l1, Line l2, ref List<PointF> intsctPoints)
        {
            PointF point;
            float x, y;

            if (l1.IsVerticalLine() && l2.IsVerticalLine())
            {
                return;
            }

            if (!l1.IsVerticalLine() && !l2.IsVerticalLine())
            {
                l1.ToSlopeIntercept(out float k1, out float b1);
                l2.ToSlopeIntercept(out float k2, out float b2);

                if (Math.Abs(k1 - k2) < Single.Epsilon)
                {
                    return;
                }

                x = (b2 - b1) / (k1 - k2);
                y = (k1 * b2 - k2 * b1) / (k1 - k2);
            }
            else
            {
                Line l = l1.IsVerticalLine() ? l2 : l1;
                x = l1.IsVerticalLine() ? l1.Start.X : l2.Start.X;
                l.ToSlopeIntercept(out float k, out float b);
                y = k * x + b;
            }
            
            point = new PointF(x, y);
            if (IsPointWithinRange(point, l1.Start.X, l1.End.X, l1.Start.Y, l1.End.Y) &&
                IsPointWithinRange(point, l2.Start.X, l2.End.X, l2.Start.Y, l2.End.Y))
            {
                intsctPoints.Add(point);
            }
        }

        /// <summary>
        /// Find the intersection of a line and a circle
        /// Ref: http://www.ambrsoft.com/TrigoCalc/Circles2/circlrLine_.htm
        /// </summary>
        /// <param name="l1">The line</param>
        /// <param name="l2">The circle</param>
        /// <param name="intsctPoints"></param>
        public static void Intersect(Line l, Circle c, ref List<PointF> intsctPoints)
        {
            if (Distance(c.Center, l) > c.Radius)
            {
                return;
            }

            PointF point1;
            PointF point2;

            if (l.IsVerticalLine())
            {
                float x = l.Start.X;
                float y = (float)Math.Sqrt(c.Radius * c.Radius - x * x);

                point1 = new PointF(x + c.Center.X, y + c.Center.Y);
                point2 = new PointF(x + c.Center.X, c.Center.Y - y);
            }
            else
            {
                //y = kx + b
                l.ToSlopeIntercept(out float m, out float d);
                float a = c.Center.X;
                float b = c.Center.Y;
                float r = c.Radius;
                float del = (r * r) * (1 + m * m) -
                    (b - m * a - d) * (b - m * a - d);

                float x1, y1;
                float x2, y2;

                x1 = (float)(a + b * m - d * m + Math.Sqrt(del)) / (1 + m * m);
                y1 = (float)(d + a * m + b * m * m + m * Math.Sqrt(del)) / (1 + m * m);

                x2 = (float)(a + b * m - d * m - Math.Sqrt(del)) / (1 + m * m);
                y2 = (float)(d + a * m + b * m * m - m * Math.Sqrt(del)) / (1 + m * m);

                point1 = new PointF(x1, y1);
                point2 = new PointF(x2, y2);
            }

            if (IsPointWithinRange(point1, l.Start.X, l.End.X, l.Start.Y, l.End.Y))
            {
                intsctPoints.Add(point1);
            }

            if (!point1.Equals(point2) &&
                IsPointWithinRange(point2, l.Start.X, l.End.X, l.Start.Y, l.End.Y))
            {
                intsctPoints.Add(point2);
            }
        }

        /// <summary>
        /// Find the intersection points of two circles
        /// Ref: https://www.xarg.org
        /// </summary>
        /// <param name="c1">The first circle</param>
        /// <param name="c2">The second circle</param>
        /// <param name="intsctPoints"></param>
        public static void Intersect(Circle c1, Circle c2, ref List<PointF> intsctPoints)
        {
            float d = (float)Distance(c1.Center, c2.Center);

            if (d <= (c1.Radius + c2.Radius) &&
                d >= Math.Abs(c1.Radius - c2.Radius))
            {
                float ex = (c2.Center.X - c1.Center.X) / d;
                float ey = (c2.Center.Y - c1.Center.Y) / d;

                float x = (c1.Radius * c1.Radius - c2.Radius * c2.Radius + d * d) / (2 * d);
                float y = (float)Math.Sqrt(c1.Radius * c1.Radius - x * x);

                PointF p1 = new PointF {
                    X = c1.Center.X + x * ex - y * ey,
                    Y = c1.Center.Y + x * ey + y * ex
                  };

                PointF p2 = new PointF
                {
                    X = c1.Center.X + x * ex + y * ey,
                    Y = c1.Center.Y + x * ey - y * ex
                };

                intsctPoints.Add(p1);
                intsctPoints.Add(p2);
            }
            else
            {
                //No intersection
            }
        }

        /// <summary>
        /// Check if the specified point is within the range defined by x and y ranges
        /// </summary>
        /// <param name="point"></param>
        /// <param name="x1"></param>
        /// <param name="x2"></param>
        /// <param name="y1"></param>
        /// <param name="y2"></param>
        /// <returns></returns>
        public static bool IsPointWithinRange(PointF point, float x1, float x2,
            float y1, float y2)
        {
            return (IsValueInRange(point.X, x1, x2)) &&
                (IsValueInRange(point.Y, y1, y2));
        }

        public static bool IsValueInRange(float value, float a, float b)
        {
            return ((value - a) * (value - b)) <= 0;
        }
    }
}
