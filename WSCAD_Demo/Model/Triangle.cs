using PdfSharp.Drawing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using WSCAD_Demo.Utility;

namespace WSCAD_Demo.Model
{
    class Triangle : Shape
    {
        public PointF A { get; set; }
        public PointF B { get; set; }
        public PointF C { get; set; }

        /// <summary>
        /// Find the intersect of this triangle and other shape
        /// </summary>
        /// <param name="shape"></param>
        /// <param name="inscts"></param>
        public override void Intersect(Shape shape, ref List<PointF> inscts)
        {
            ToLines(out List<Line> lines);

            foreach (Line l in lines)
            {
                l.Intersect(shape, ref inscts);
            }
        }

        public override void Draw(Graphics graphics)
        {
            Pen pen = null;
            Brush brush = null;
            PointF viewA = new PointF(A.X * Scale, A.Y * Scale);
            PointF viewB = new PointF(B.X * Scale, B.Y * Scale);
            PointF viewC = new PointF(C.X * Scale, C.Y * Scale);

            try
            {
                //Ver 1.0 -- Simply call the built-in API
                pen = new Pen(Color)
                {
                    DashStyle = DashStyle,
                    Width = IsSelected ? 4 : 2
                };
                PointF[] points =
                {
                    viewA,
                    viewB,
                    viewC,
                    viewA
                };

                graphics.DrawLines(pen, points);
                if (Fill)
                {
                    brush = new SolidBrush(Color);
                    graphics.FillPolygon(brush, points, FillMode.Winding);
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
            XPoint viewA = new XPoint(A.X * Scale, A.Y * Scale);
            XPoint viewB = new XPoint(B.X * Scale, B.Y * Scale);
            XPoint viewC = new XPoint(C.X * Scale, C.Y * Scale);

            try
            {
                //Ver 1.0 -- Simply call the built-in API
                XPen pen = new XPen(PDFUtility.ToXColor(Color))
                {
                    DashStyle = PDFUtility.ToXStyle(DashStyle),
                    Width = 2
                };
                XPoint[] points =
                {
                    viewA,
                    viewB,
                    viewC,
                    viewA
                };

                if (Fill)
                {
                    XBrush brush = new XSolidBrush(PDFUtility.ToXColor(Color));
                    graphics.DrawPolygon(brush, points, XFillMode.Winding);
                }
                else
                {
                    graphics.DrawLines(pen, points);
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

        public override string ToString()
        {
            return string.Format("Triangle [A: {0}, B: {1}, C: {2}, {3}]",
                A, B, C, base.ToString());
        }

        /// <summary>
        /// Check if the specified point is in this rectangle
        /// </summary>
        /// <param name="point"></param>
        /// <returns>true on yes, otherwise false</returns>
        public override bool ContainsPoint(PointF point)
        {
            ToLines(out List<Line> lines);
            return lines[0].ContainsPoint(point) ||
                lines[1].ContainsPoint(point) ||
                lines[2].ContainsPoint(point);
        }

        /// <summary>
        /// Get the lines enclosing this rectangle
        /// </summary>
        /// <param name="lines"></param>
        public void ToLines(out List<Line> lines)
        {
            lines = new List<Line>
            {
                new Line(A, B),
                new Line(B, C),
                new Line(A, C)
            };
        }
    }
}
