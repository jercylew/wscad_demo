using PdfSharp.Drawing;
using System;
using System.Collections.Generic;
using System.Drawing;
using WSCAD_Demo.Utility;

namespace WSCAD_Demo.Model
{
    public class Rect : Shape
    {
        public PointF UpperTop { get; set; }
        public Single Width { get; set; }
        public Single Height { get; set; }

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
            PointF viewUpperTop = new PointF(UpperTop.X * Scale, UpperTop.Y * Scale);
            float viewWidth = Width * Scale;
            float viewHeight = Height * Scale;

            try
            {
                //Ver 1.0 -- Simply call the built-in API
                pen = new Pen(Color)
                {
                    DashStyle = DashStyle,
                    Width = IsSelected ? 4 : 2
                };

                graphics.DrawRectangle(pen, viewUpperTop.X, viewUpperTop.Y - viewHeight, viewWidth, viewHeight);
                if (Fill)
                {
                    brush = new SolidBrush(Color);
                    graphics.FillRectangle(brush, viewUpperTop.X, viewUpperTop.Y - viewHeight, viewWidth, viewHeight);
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
                if(brush != null)
                {
                    brush.Dispose();
                }
            }
        }

        public override void Draw(XGraphics graphics)
        {
            XPoint viewUpperTop = new XPoint(UpperTop.X * Scale, UpperTop.Y * Scale);
            float viewWidth = Width * Scale;
            float viewHeight = Height * Scale;

            try
            {
                if (Fill)
                {
                    XBrush brush = new XSolidBrush(PDFUtility.ToXColor(Color));
                    graphics.DrawRectangle(brush, viewUpperTop.X, viewUpperTop.Y - viewHeight, viewWidth, viewHeight);
                }
                else
                {
                    XPen pen = new XPen(PDFUtility.ToXColor(Color))
                    {
                        DashStyle = PDFUtility.ToXStyle(DashStyle),
                        Width = 2
                    };
                    graphics.DrawRectangle(pen, viewUpperTop.X, viewUpperTop.Y - viewHeight, viewWidth, viewHeight);
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
        /// Check if the specified point is in this rectangle
        /// </summary>
        /// <param name="point"></param>
        /// <returns>true on yes, otherwise false</returns>
        public override bool ContainsPoint(PointF point)
        {
            ToLines(out List<Line> lines);
            return lines[0].ContainsPoint(point) ||
                lines[1].ContainsPoint(point) ||
                lines[2].ContainsPoint(point) ||
                lines[3].ContainsPoint(point);
        }
        public override string ToString()
        {
            return string.Format("Rectangle [UpperLeft: {0}, Width: {1}, Height: {2}, {3}]",
                UpperTop, Width, Height, base.ToString());
        }

        /// <summary>
        /// Get the lines enclosing this rectangle
        /// </summary>
        /// <param name="lines"></param>
        public void ToLines(out List<Line> lines)
        {
            lines = new List<Line>
            {
                new Line(UpperTop, new PointF(UpperTop.X + Width, UpperTop.Y)),
                new Line(UpperTop, new PointF(UpperTop.X, UpperTop.Y - Height)),
                new Line(new PointF(UpperTop.X, UpperTop.Y - Height),
                    new PointF(UpperTop.X + Width, UpperTop.Y - Height)),
                new Line(new PointF(UpperTop.X + Width, UpperTop.Y),
                    new PointF(UpperTop.X + Width, UpperTop.Y - Height))
            };
        }
    }
}
