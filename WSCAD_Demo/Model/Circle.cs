using PdfSharp.Drawing;
using System;
using System.Collections.Generic;
using System.Drawing;
using WSCAD_Demo.Utility;

namespace WSCAD_Demo.Model
{
    public class Circle : Shape
    {
        public PointF Center { get; set; }
        public Single Radius { get; set; }

        public override void Intersect(Shape shape, ref List<PointF> inscts)
        {
            if (shape is Line line)
            {
                ShapeUtility.Intersect(line, this, ref inscts);
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
                    ShapeUtility.Intersect(l, this, ref inscts);
                }
            }
            else if (shape is Triangle triangle)
            {
                triangle.ToLines(out List<Line> lines);
                foreach (Line l in lines)
                {
                    ShapeUtility.Intersect(l, this, ref inscts);
                }
            }
            else
            {
            }
        }

        public override void Draw(Graphics graphics)
        {
            Pen pen = null;
            Brush brush = null;
            float viewRadius = Radius * Scale;
            PointF viewCenter = new PointF(Center.X * Scale, Center.Y * Scale);

            try
            {
                //Ver 1.0 -- Simply call the built-in API
                pen = new Pen(Color)
                {
                    DashStyle = DashStyle,
                    Width = IsSelected ? 4 : 2
                };

                graphics.DrawEllipse(pen, viewCenter.X - viewRadius, viewCenter.Y - viewRadius,
                       2 * viewRadius, 2 * viewRadius);
                if (Fill)
                {
                    brush = new SolidBrush(Color);
                    graphics.FillEllipse(brush, viewCenter.X - viewRadius, viewCenter.Y - viewRadius,
                       2 * viewRadius, 2 * viewRadius);
                }

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
                if (brush != null)
                {
                    brush.Dispose();
                }
            }
        }

        public override void Draw(XGraphics graphics)
        {
            float viewRadius = Radius * Scale;
            XPoint viewCenter = new XPoint(Center.X * Scale, Center.Y * Scale);

            try
            {
                //Ver 1.0 -- Simply call the built-in API
                if (Fill)
                {
                    XBrush brush = new XSolidBrush(PDFUtility.ToXColor(Color));
                    graphics.DrawEllipse(brush, viewCenter.X - viewRadius, viewCenter.Y - viewRadius,
                       2 * viewRadius, 2 * viewRadius);
                }
                else
                {
                    XPen pen = new XPen(PDFUtility.ToXColor(Color))
                    {
                        DashStyle = PDFUtility.ToXStyle(DashStyle),
                        Width = 2
                    };
                    graphics.DrawEllipse(pen, viewCenter.X - viewRadius, viewCenter.Y - viewRadius,
                       2 * viewRadius, 2 * viewRadius);
                }

                //Ver 2.0 -- Using SetPixel of Bitmap
            }
            catch (Exception)
            {
            }
            finally
            {
            }
        }

        /// <summary>
        /// Check if the specified point is in this circle
        /// </summary>
        /// <param name="point"></param>
        /// <returns>true on yes, otherwise false</returns>
        public override bool ContainsPoint(PointF point)
        {
            float d = (float)ShapeUtility.Distance(point, Center);
            return Math.Abs(d - Radius) < FloatEpsilon; //Double.Epsilon is too precise
        }

        public override string ToString()
        {
            return string.Format("Circle [Center: {0}, Radius: {1}, {2}]",
                Center, Radius, base.ToString()); 
        }
    }
}
