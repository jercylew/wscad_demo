using PdfSharp.Drawing;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace WSCAD_Demo.Model
{
    public abstract class Shape
    {
        protected Guid id;
        protected static readonly float FloatEpsilon = 2.0f; //Using float.Epsilon or double.Epsilon is too precise for this app 

        public string Name { get; set; }
        public DashStyle DashStyle { get; set; }
        public bool Fill { get; set; }
        public Color Color { get; set; }
        public Single Scale { get; set; }
        public bool IsSelected { get; set; }

        public string Id
        {
            get => id.ToString();
        }

        public Shape()
        {
            id = new Guid();
        }


        public abstract void Intersect(Shape shape, ref List<PointF> inscts);
        public abstract void Draw(Graphics graphics);
        public abstract void Draw(XGraphics graphics);
        public abstract bool ContainsPoint(PointF point);
        public override string ToString()
        {
            return string.Format("Color: {0}, lineType: {1}, Filled: {2}",
                Color, DashStyle, Fill);
        }
    }
}
