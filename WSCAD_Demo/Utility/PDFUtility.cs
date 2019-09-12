using PdfSharp.Drawing;
using PdfSharp.Pdf;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WSCAD_Demo.Model;

namespace WSCAD_Demo.Utility
{
    class PDFUtility
    {
        /// <summary>
        /// Paint the cartesian system in the current form
        /// </summary>
        /// <param name="gh">The graphics device used to draw the cartesian system</param>
        /// <param name="color">The color of the lines</param>
        /// <param name="width">The width of the window</param>
        /// <param name="height">The height of the window</param>
        /// <returns></returns>
        public static bool DrawCartesian(XGraphics graphics, float width, float height,
            float unit, float scale)
        {
            bool bRet = true;
            try
            {
                XPen pen = new XPen(XColor.FromArgb(255, 0, 0, 0));
                double penWidth = pen.Width;

                //The X and Y aix
                pen.Width = 2 * penWidth;
                graphics.DrawLine(pen, 0, 15 - (float)(height / 2.0), 0, (float)(height / 2.0) - 15);
                graphics.DrawLine(pen, 0, 15 - (float)(height / 2.0), -6, 25 - (float)(height / 2.0));
                graphics.DrawLine(pen, 0, 15 - (float)(height / 2.0), 6, 25 - (float)(height / 2.0));
                graphics.DrawLine(pen, 15 - (float)(width / 2.0), 0, (float)(width / 2.0) - 15, 0);
                graphics.DrawLine(pen, (float)(width / 2.0) - 15, 0, (float)(width / 2.0 - 25), 6);
                graphics.DrawLine(pen, (float)(width / 2.0) - 15, 0, (float)(width / 2.0 - 25), -6);

                // Create font and brush.
                XFont drawFont = new XFont("Arial", 12);
                XSolidBrush drawBrush = new XSolidBrush(XColors.Black);
                graphics.DrawString("x", drawFont, drawBrush, (float)(width / 2.0 - 20), -10);
                graphics.DrawString("y", drawFont, drawBrush, -20, -(float)(height / 2.0) + 20);


                //The vertical grid lines
                pen.Color = XColor.FromArgb(255, 200, 200, 200);
                pen.Width = penWidth;
                drawFont = new XFont("Arial", 8);
                int count = 1;
                while (count * unit < ((float)(width / 2.0) - 15))
                {
                    graphics.DrawLine(pen, (count * unit), 15 - (Single)(height / 2.0),
                        (count * unit), (float)(height / 2.0) - 15);
                    graphics.DrawLine(pen, -(count * unit), 15 - (Single)(height / 2.0),
                        -(count * unit), (Single)(height / 2.0) - 15);

                    graphics.DrawString(string.Format("{0:.0}", (count * unit) / scale), drawFont,
                        drawBrush, (count * unit) - 10, 10);
                    graphics.DrawString(string.Format("-{0:.0}", (count * unit) / scale), drawFont,
                        drawBrush, -(count * unit) - 12, 10);

                    count++;
                }

                //The horizontal grid lines
                count = 1;
                while (count * unit < ((float)(height / 2.0) - 15))
                {
                    graphics.DrawLine(pen, 15 - (float)(width / 2.0), (count * unit),
                        (float)(width / 2.0) - 15, (count * unit));
                    graphics.DrawLine(pen, 15 - (float)(width / 2.0), -(count * unit),
                        (float)(width / 2.0) - 15, -(count * unit));

                    graphics.DrawString(string.Format("-{0:.0}", (count * unit) / scale), drawFont,
                        drawBrush, 3, (count * unit));
                    graphics.DrawString(string.Format("{0:.0}", (count * unit) / scale), drawFont,
                        drawBrush, 3, -(count * unit));

                    count++;
                }
            }
            catch (Exception)
            {
                bRet = false;
            }
            finally
            {
            }

            return bRet;
        }

        public static void DrawIntersectPoints(XGraphics graphics, List<PointF> points, float scale)
        {
            XPen pen = null;
            float viewX;
            float viewY;

            try
            {
                pen = new XPen(XColor.FromArgb(255, 255, 0, 0));
                double penWidth = pen.Width;

                //The X and Y aix
                pen.Width = 2 * penWidth;
                foreach (PointF point in points)
                {
                    viewX = point.X * scale;
                    viewY = point.Y * scale;

                    graphics.DrawLine(pen, viewX - 3, viewY + 3, viewX + 3, viewY - 3);
                    graphics.DrawLine(pen, viewX + 3, viewY + 3, viewX - 3, viewY - 3);
                }
            }
            catch (Exception)
            {
            }
            finally
            {
            }
        }

        public static bool DrawShapes(PdfPage page, GraphDoc graphDoc,
            float width, float height, float scale)
        {
            bool bRet = true;
            XGraphics graphics = null;

            try
            {
                graphics = XGraphics.FromPdfPage(page);
                //Translate origin to the center of the window
                graphics.TranslateTransform((float)(page.Width / 2.0),
                    (float)(page.Height / 2.0)); //15 for margin
                DrawCartesian(graphics, ((float)(page.Width - 30)),
                    ((float)(page.Height - 30)), 20f, scale);

                //Reverse the Y aix direction and transform with scale unit
                graphics.ScaleTransform(1, (float)(-1));
                foreach (Shape shape in graphDoc.Graphs)
                {
                    shape.Scale = scale;
                    shape.Draw(graphics);
                }

                //Draw the intersect points
                DrawIntersectPoints(graphics, graphDoc.IntersectPoints, scale);
            }
            catch (Exception)
            {
                bRet = false;
            }
            finally
            {
                graphics.Dispose();
            }

            return bRet;
        }

        public static XDashStyle ToXStyle(DashStyle style)
        {
            XDashStyle xstyle;
            switch (style)
            {
                case DashStyle.Dash:
                    xstyle = XDashStyle.Dash;
                    break;
                case DashStyle.Dot:
                    xstyle = XDashStyle.Dot;
                    break;
                case DashStyle.Solid:
                    xstyle = XDashStyle.Solid;
                    break;
                case DashStyle.DashDot:
                    xstyle = XDashStyle.DashDot;
                    break;
                case DashStyle.DashDotDot:
                    xstyle = XDashStyle.DashDotDot;
                    break;
                case DashStyle.Custom:
                    xstyle = XDashStyle.Custom;
                    break;
                default:
                    xstyle = XDashStyle.Solid;
                    break;
            }

            return xstyle;
        }

        public static XColor ToXColor(Color color)
        {
            return XColor.FromArgb(color.A, color.R, color.G, color.B);
        }
    }
}
