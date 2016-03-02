using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMA.MICAPS.Box2D.Util;

namespace CMA.MICAPS.Symbols
{
    using CMA.MICAPS.Box2D;
    using Line = CMA.MICAPS.Box2D.LineString2D<float>;
    class ArrowHeadFilledLineSymbol : ArrowHeadLineSymbol
    {
        public ArrowHeadFilledLineSymbol(Line line, string label, uint size, System.Drawing.Color label_color, LabelPosition position, bool rotation, int offset, bool split = false)
            : base(line,label,size,label_color,position,rotation,offset,split)
        {
            
        }
        public ArrowHeadFilledLineSymbol(Line line,int offset)
            :base(line,offset)
        {

        }
        public override void PrepareForDraw(Box2D.Graphics.Context context)
        {
            base.PrepareForDraw(context);
        }
        public override void PrepareIndices()
        {
            int vertice_count = _vertices.Count >> 1;

            _indices.Clear();
            for (int i = 0; i < vertice_count; i++)
            {
                _indices.Add(i);
            }
            _primitive_type = PrimitiveType.TriangleStrip;
        }
    }
}
