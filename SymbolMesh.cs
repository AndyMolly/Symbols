
using System;
using System.Collections.Generic;
using System.Text;

namespace CMA.MICAPS.Symbols
{
    /// <summary>
    /// a simple class to hold symbol info.
    /// </summary>
    public class SymbolMesh
    {
        public List<float> vertices = new List<float>();
        public List<int> indices = new List<int>();
        public InterpolatePosition tail;
        public byte seq;
        public bool is_completed;
    }

    public struct InterpolatePosition
    {
        /// <summary>
        /// next find position in source line vertices.
        /// </summary>
        public int next;
        /// <summary>
        /// postion which interpolate lies on line. it may be a interpolated position between line segment.
        /// </summary>
        public double x;
        public double y;
    }
}
