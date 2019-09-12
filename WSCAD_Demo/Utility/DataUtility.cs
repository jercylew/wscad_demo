using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Text.RegularExpressions;
using System.Xml;
using WSCAD_Demo.Model;

namespace WSCAD_Demo.Utility
{
    class DataUtility
    {
        private static readonly string PATTERN_POINT = @"^([+-]?(\d*(\.\d+)?|\d+));([+-]?(\d*(\.\d+)?|\d+))$";
        private static readonly string PATTERN_COLOR = @"^(\d+);(\d+);(\d+);(\d+)$";

        public static void LoadGraphsFromJson(string fileName, ref GraphDoc graphDoc)
        {
            try
            {
                string textJson = System.IO.File.ReadAllText(fileName);
                string type;
                string shapeName = "";
                JArray jar = JArray.Parse(textJson);
                List<PointF> limitPoints = new List<PointF>();

                foreach (JObject o in jar)
                {
                    Shape shape = null;
                    limitPoints.Clear();
                    type = o.Value<string>("type").ToLower().Trim();

                    if (type == "line")
                    {
                        shape = new Line()
                        {
                            Start = ToPoint(o.Value<string>("a")),
                            End = ToPoint(o.Value<string>("b"))
                        };

                        limitPoints.Add(((Line)(shape)).Start);
                        limitPoints.Add(((Line)(shape)).End);
                        shapeName = string.Format("Line-{0}", graphDoc.lineCount++);
                    }
                    else if (type == "circle")
                    {
                        shape = new Circle()
                        {
                            Center = ToPoint(o.Value<string>("center")),
                            Radius = o.Value<float>("radius")
                        };

                        limitPoints.Add(new PointF(((Circle)(shape)).Center.X + ((Circle)(shape)).Radius,
                            ((Circle)(shape)).Center.Y + ((Circle)(shape)).Radius));
                        limitPoints.Add(new PointF(((Circle)(shape)).Center.X - ((Circle)(shape)).Radius,
                            ((Circle)(shape)).Center.Y - ((Circle)(shape)).Radius));
                        shapeName = string.Format("Circle-{0}", graphDoc.circleCount++);
                    }
                    else if (type == "triangle")
                    {
                        shape = new Triangle()
                        {
                            A = ToPoint(o.Value<string>("a")),
                            B = ToPoint(o.Value<string>("b")),
                            C = ToPoint(o.Value<string>("c"))
                        };

                        limitPoints.Add(((Triangle)(shape)).A);
                        limitPoints.Add(((Triangle)(shape)).B);
                        limitPoints.Add(((Triangle)(shape)).C);
                        shapeName = string.Format("Triangle-{0}", graphDoc.trigleCount++);
                    }
                    else if (type == "rectangle")
                    {
                        shape = new Rect()
                        {
                            UpperTop = ToPoint(o.Value<string>("a")),
                            Width = o.Value<Single>("width"),
                            Height = o.Value<Single>("height")
                        };

                        limitPoints.Add(((Rect)(shape)).UpperTop);
                        limitPoints.Add(new PointF( ((Rect)(shape)).UpperTop.X - ((Rect)(shape)).Width,
                            ((Rect)(shape)).UpperTop.Y - ((Rect)(shape)).Height));
                        shapeName = string.Format("Rect-{0}", graphDoc.rectgleCount++);
                    }
                    else
                    {
                    }

                    if (shape != null)
                    {
                        shape.Color = ToColor(o.Value<string>("color"));
                        shape.DashStyle = ToDashStyle(o.Value<string>("lineType"));
                        shape.Fill = o.Value<bool>("filled");
                        shape.Name = shapeName;
                        List<PointF> insctPoints = graphDoc.IntersectPoints;
                        //Find intersect points
                        foreach (Shape sh in graphDoc.Graphs)
                        {
                            shape.Intersect(sh, ref insctPoints);
                        }
                        graphDoc.Graphs.Add(shape);
                        UpdateAxesLimit(limitPoints, ref graphDoc);
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        public static void LoadGraphsFromXml(string fileName, ref GraphDoc graphDoc)
        {
            string type;
            string shapeName = "";
            List<PointF> limitPoints = new List<PointF>();

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.PreserveWhitespace = true;
                doc.Load(fileName);

                XmlNodeList nodeList;
                XmlNode root = doc.DocumentElement;

                nodeList = root.SelectNodes("descendant::shape");

                //Change the price on the books.
                foreach (XmlNode node in nodeList)
                {
                    Shape shape = null;
                    limitPoints.Clear();

                    type = node.SelectSingleNode("descendant::type").InnerText.ToLower().Trim();

                    if (type == "line")
                    {
                        shape = new Line()
                        {
                            Start = ToPoint(node.SelectSingleNode("descendant::a").InnerText),
                            End = ToPoint(node.SelectSingleNode("descendant::b").InnerText)
                        };

                        limitPoints.Add(((Line)(shape)).Start);
                        limitPoints.Add(((Line)(shape)).End);
                        shapeName = string.Format("Line-{0}", graphDoc.lineCount++);
                    }
                    else if (type == "circle")
                    {
                        shape = new Circle()
                        {
                            Center = ToPoint(node.SelectSingleNode("descendant::center").InnerText),
                            Radius = XmlConvert.ToSingle(node.SelectSingleNode("descendant::radius").InnerText)
                        };

                        limitPoints.Add(new PointF(((Circle)(shape)).Center.X + ((Circle)(shape)).Radius,
                            ((Circle)(shape)).Center.Y + ((Circle)(shape)).Radius));
                        limitPoints.Add(new PointF(((Circle)(shape)).Center.X - ((Circle)(shape)).Radius,
                            ((Circle)(shape)).Center.Y - ((Circle)(shape)).Radius));
                        shapeName = string.Format("Circle-{0}", graphDoc.circleCount++);
                    }
                    else if (type == "triangle")
                    {
                        shape = new Triangle()
                        {
                            A = ToPoint(node.SelectSingleNode("descendant::a").InnerText),
                            B = ToPoint(node.SelectSingleNode("descendant::b").InnerText),
                            C = ToPoint(node.SelectSingleNode("descendant::c").InnerText)
                        };

                        limitPoints.Add(((Triangle)(shape)).A);
                        limitPoints.Add(((Triangle)(shape)).B);
                        limitPoints.Add(((Triangle)(shape)).C);
                        shapeName = string.Format("Triangle-{0}", graphDoc.trigleCount++);
                    }
                    else if (type == "rectangle")
                    {
                        shape = new Rect()
                        {
                            UpperTop = ToPoint(node.SelectSingleNode("descendant::a").InnerText),
                            Width = XmlConvert.ToSingle(node.SelectSingleNode("descendant::width").InnerText),
                            Height = XmlConvert.ToSingle(node.SelectSingleNode("descendant::height").InnerText)
                        };

                        limitPoints.Add(((Rect)(shape)).UpperTop);
                        limitPoints.Add(new PointF(((Rect)(shape)).UpperTop.X - ((Rect)(shape)).Width,
                            ((Rect)(shape)).UpperTop.Y - ((Rect)(shape)).Height));
                        shapeName = string.Format("Rect-{0}", graphDoc.rectgleCount++);
                    }
                    else
                    {
                    }

                    if (shape != null)
                    {
                        shape.Color = ToColor(node.SelectSingleNode("descendant::color").InnerText);
                        shape.DashStyle = ToDashStyle(node.SelectSingleNode("descendant::lineType").InnerText);
                        shape.Fill = (type == "line") ? false : XmlConvert.ToBoolean(node.SelectSingleNode("descendant::filled").InnerText);
                        shape.Name = shapeName;

                        List<PointF> insctPoints = graphDoc.IntersectPoints;
                        //Find intersect points
                        foreach (Shape sh in graphDoc.Graphs)
                        {
                            shape.Intersect(sh, ref insctPoints);
                        }

                        graphDoc.Graphs.Add(shape);
                        UpdateAxesLimit(limitPoints, ref graphDoc);
                    }
                }
            }
            catch (Exception)
            {
            }
        }
        private static PointF ToPoint(string text)
        {
            PointF point = new PointF();

            try
            {
                text = text.Replace(',', '.').Replace(" ", "");
                Match match = Regex.Matches(text, PATTERN_POINT)[0];

                point.X = Single.Parse(match.Groups[1].Value);
                point.Y = Single.Parse(match.Groups[4].Value);
            }
            catch (Exception)
            {
            }

            return point;
        }

        private static Color ToColor(string text)
        {
            Color color = new Color();

            try
            {
                text = text.Replace(',', '.').Replace(" ", "");
                Match match = Regex.Matches(text, PATTERN_COLOR)[0];

                int A = Int32.Parse(match.Groups[1].Value);
                int R = Int32.Parse(match.Groups[2].Value);
                int G = Int32.Parse(match.Groups[3].Value);
                int B = Int32.Parse(match.Groups[4].Value);

                color = Color.FromArgb(A, R, G, B);
            }
            catch (Exception)
            {
            }

            return color;
        }

        private static DashStyle ToDashStyle(string text)
        {
            DashStyle style = DashStyle.Solid;

            try
            {
                switch (text.ToLower().Trim())
                {
                    case "dashdot":
                        style = DashStyle.DashDot;
                        break;
                    case "dashdotdot":
                        style = DashStyle.DashDotDot;
                        break;
                    case "dot":
                        style = DashStyle.Dot;
                        break;
                    case "dash":
                        style = DashStyle.Dash;
                        break;
                    default:
                        break;
                }
            }
            catch (Exception)
            {
            }

            return style;
        }

        private static void UpdateAxesLimit(List<PointF> points, ref GraphDoc graphDoc)
        {
            foreach (PointF point in points)
            {
                graphDoc.maxX = Math.Max(graphDoc.maxX, point.X);
                graphDoc.minX = Math.Min(graphDoc.minX, point.X);
                graphDoc.maxY = Math.Max(graphDoc.maxY, point.Y);
                graphDoc.minY = Math.Min(graphDoc.minY, point.Y);
            }
        }
    }
}
