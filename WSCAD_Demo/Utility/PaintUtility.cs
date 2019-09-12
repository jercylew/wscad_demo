using System;
using System.Collections.Generic;
using System.Drawing;
using WSCAD_Demo.Model;

namespace WSCAD_Demo.Utility
{
    class PaintUtility
    {
        /// <summary>
        /// Paint the cartesian system in the current form
        /// </summary>
        /// <param name="gh">The graphics device used to draw the cartesian system</param>
        /// <param name="color">The color of the lines</param>
        /// <param name="width">The width of the window</param>
        /// <param name="height">The height of the window</param>
        /// <returns></returns>
        public static bool DrawCartesian(Graphics graphics, Single width, Single height,
            Single unit, Single scale)
        {
            bool bRet = true;
            Pen pen = null;
            Font drawFont = null;
            SolidBrush drawBrush = null;

            try
            {
                pen = new Pen(Color.FromArgb(255, 0, 0, 0));
                Single penWidth = pen.Width;

                //The X and Y aix
                pen.Width = 2 * penWidth;
                graphics.DrawLine(pen, 0, -(Single)(height / 2.0), 0, (Single)(height / 2.0));
                graphics.DrawLine(pen, 0, -(Single)(height / 2.0), -6, 10 - (Single)(height / 2.0));
                graphics.DrawLine(pen, 0, -(Single)(height / 2.0), 6, 10 - (Single)(height / 2.0));
                graphics.DrawLine(pen, -(Single)(width / 2.0), 0, (Single)(width / 2.0), 0);
                graphics.DrawLine(pen, (Single)(width / 2.0), 0, (Single)(width / 2.0 - 10), 6);
                graphics.DrawLine(pen, (Single)(width / 2.0), 0, (Single)(width / 2.0 - 10), -6);

                // Create font and brush.
                drawFont = new Font("Arial", 12);
                drawBrush = new SolidBrush(Color.Black);
                graphics.DrawString("x", drawFont, drawBrush, (Single)(width / 2.0 - 20), -20);
                graphics.DrawString("y", drawFont, drawBrush, -20, -(Single)(height / 2.0));


                //The vertical grid lines
                pen.Color = Color.FromArgb(255, 200, 200, 200);
                pen.Width = penWidth;
                drawFont.Dispose();
                drawFont = new Font("Arial", 8);
                int count = 1;
                while (count*unit < (Single)(width / 2.0))
                {
                    graphics.DrawLine(pen, (count * unit), -(Single)(height / 2.0),
                        (count * unit), (Single)(height / 2.0));
                    graphics.DrawLine(pen, -(count * unit), -(Single)(height / 2.0),
                        -(count * unit), (Single)(height / 2.0));

                    graphics.DrawString(string.Format("{0:.0}", (count * unit)/scale), drawFont,
                        drawBrush, (count * unit)-10, 3);
                    graphics.DrawString(string.Format("-{0:.0}", (count * unit)/scale), drawFont,
                        drawBrush, -(count * unit)-12, 3);

                    count++;
                }

                //The horizontal grid lines
                count = 1;
                while (count * unit < (Single)(height / 2.0))
                {
                    graphics.DrawLine(pen, -(Single)(width / 2.0), (count * unit),
                        (Single)(width / 2.0), (count * unit));
                    graphics.DrawLine(pen, -(Single)(width / 2.0), -(count * unit),
                        (Single)(width / 2.0), -(count * unit));

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
                if (pen != null)
                {
                    pen.Dispose();
                }
                if (drawFont != null)
                {
                    drawFont.Dispose();
                }
                if (drawBrush != null)
                {
                    drawBrush.Dispose();
                }
            }

            return bRet;
        }

        public static void DrawIntersectPoints(Graphics graphics, List<PointF> points, float scale)
        {
            Pen pen = null;
            float viewX;
            float viewY;

            try
            {
                pen = new Pen(Color.FromArgb(255, 255, 0, 0));
                Single penWidth = pen.Width;

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
                if (pen != null)
                {
                    pen.Dispose();
                }
            }
        }

        public static bool DrawShapes(Graphics graphics, GraphDoc graphDoc,
            Single width, Single height, Single scale)
        {
            bool bRet = true;

            try
            {
                //Translate origin to the center of the window
                graphics.TranslateTransform((Single)(width / 2.0), (Single)(height / 2.0));
                DrawCartesian(graphics, width, height, 40f, scale);

                //Reverse the Y aix direction and transform with scale unit
                graphics.ScaleTransform(1, (Single)(-1));
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

            return bRet;
        }
    }
}
