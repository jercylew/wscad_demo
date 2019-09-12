using PdfSharp.Drawing;
using System;
using System.Collections.Generic;
using System.Drawing;
using WSCAD_Demo.Utility;

namespace WSCAD_Demo.Model
{
    public class Line : Shape
    {
        public PointF Start { get; set; }
        public PointF End { get; set; }

        public Line()
        {
        }

        public Line(PointF p1, PointF p2)
        {
            Start = p1;
            End = p2;
        }

        /// <summary>
        /// Convert to the form of y = ax + b
        /// </summary>
        /// <param name="slope">The outputed slope of the line</param>
        /// <param name="intercept">The intercept of the line</param>
        public void ToSlopeIntercept(out Single slope, out Single intercept)
        {
            Line.ToSlopeIntercept(Start, End, out slope, out intercept);
        }

        /// <summary>
        /// Convert to the form of y = ax + b
        /// </summary>
        /// <param name="p1">The first point</param>
        /// <param name="p2">The second point</param>
        /// <param name="slope">The outputed slope of the line</param>
        /// <param name="intercept">The intercept of the line</param>
        public static void ToSlopeIntercept(PointF p1, PointF p2,
            out float slope, out float intercept)
        {
            if (Math.Abs(p1.X - p2.X) < float.Epsilon) //Vertical line does not have a slope
            {
                slope = float.PositiveInfinity;
                intercept = float.PositiveInfinity;

                return;
            }

            slope = (p1.Y - p2.Y) / (p1.X - p2.X);
            intercept = p1.Y - slope * p1.X;
        }

        public override void Intersect(Shape shape, ref List<PointF> inscts)
        {
            if (shape is Line line)
            {
                ShapeUtility.Intersect(this, line, ref inscts);
            }
            else if (shape is Circle circle)
            {
                ShapeUtility.Intersect(this, circle, ref inscts);
            }
            else if (shape is Rect rect)
            {
                rect.ToLines(out List<Line> lines);
                foreach (Line l in lines)
                {
                    ShapeUtility.Intersect(this, l, ref inscts);
                }
            }
            else if (shape is Triangle triangle)
            {
                triangle.ToLines(out List<Line> lines);
                foreach (Line l in lines)
                {
                    ShapeUtility.Intersect(this, l, ref inscts);
                }
            }
            else
            {
            }
        }

        /// <summary>
        /// Check if the specified point is in this line
        /// </summary>
        /// <param name="point"></param>
        /// <returns>true on yes, otherwise false</returns>
        public override bool ContainsPoint(PointF point)
        {
            if (IsVerticalLine())
            {
                return (Math.Abs(point.X - Start.X) < FloatEpsilon) &&
                    ShapeUtility.IsValueInRange(point.Y, Start.Y, End.Y);
            }

            if (IsHorizontalLine())
            {
                return (Math.Abs(point.Y - Start.Y) < FloatEpsilon) &&
                    ShapeUtility.IsValueInRange(point.X, Start.X, End.X);
            }

            if (!ShapeUtility.IsPointWithinRange(point, Start.X, End.X, Start.Y, End.Y))
            {
                return false;
            }

            float d = (float)ShapeUtility.Distance(point, this);
            return d < FloatEpsilon; //Double.Epsilon
        }

        public override void Draw(Graphics graphics)
        {
            Pen pen = null;
            PointF viewStart = new PointF(Start.X * Scale, Start.Y * Scale);
            PointF viewEnd = new PointF(End.X * Scale, End.Y * Scale);

            try
            {
                //Ver 1.0 -- Simply call the built-in API
                pen = new Pen(Color)
                {
                    DashStyle = DashStyle,
                    Width = IsSelected ? 4 : 2
                };
                graphics.DrawLine(pen, viewStart, viewEnd);

                //Ver 2.0 -- Using SetPixel of Bitmap
            }
            catch (Exception)
            {
            }
            finally
            {
                if (pen != null)
                {
                    pen.Dispose();
                }
            }
        }

        public override void Draw(XGraphics graphics)
        {
            try
            {
                XPoint viewStart = new XPoint(Start.X * Scale, Start.Y * Scale);
                XPoint viewEnd = new XPoint(End.X * Scale, End.Y * Scale);

                //Ver 1.0 -- Simply call the built-in API
                XPen pen = new XPen(PDFUtility.ToXColor(Color))
                {
                    DashStyle = PDFUtility.ToXStyle(DashStyle),
                    Width = 2
                };
                graphics.DrawLine(pen, viewStart, viewEnd);

                //Ver 2.0 -- Using SetPixel of Bitmap
            }
            catch (Exception)
            {
            }
            finally
            {
            }
        }

        public bool IsVerticalLine()
        {
            return (Math.Abs(Start.X - End.X) < float.Epsilon);
        }

        public bool IsHorizontalLine()
        {
            return (Math.Abs(Start.Y - End.Y) < float.Epsilon);
        }

        public override string ToString()
        {
            return string.Format("Line [Start: {0}, End: {1}, {2}]",
                Start.ToString(), End.ToString(), base.ToString());
        }
    }
}
