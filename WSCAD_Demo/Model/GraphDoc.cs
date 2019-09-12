using System;
using System.Collections.Generic;
using System.Drawing;

namespace WSCAD_Demo.Model
{
    public struct GraphDoc
    {
        public string FileName { get; set; }
        public string FilePath { get; set; }
        public List<Shape> Graphs { get; set; }
        public List<PointF> IntersectPoints { get; set; }
        public Single maxX { get; set; }
        public Single minX { get; set; }
        public Single maxY { get; set; }
        public Single minY { get; set; }
        public int lineCount { get; set; }
        public int circleCount { get; set; }
        public int rectgleCount { get; set; }
        public int trigleCount { get; set; }
    }
}
