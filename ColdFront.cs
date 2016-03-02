
using System;
using System.Collections.Generic;
using System.Text;
using CMA.MICAPS.Box2D.Util;

namespace CMA.MICAPS.Symbols
{
    using Line = CMA.MICAPS.Box2D.LineString2D<float>;
    class ColdFront : ISymbolAssembler
    {
        public ColdFront()
        {
            this.Direction = 1;
        }

        internal int Direction
        {
            get;
            set;
        }
        public void Assemble(Box2D.Graphics.Context context, SymbolMesh symbol)
        {
            int count = symbol.vertices.Count;
            if (count < 4)
                return;

            //make a triangle symbol
            float x0 = symbol.vertices[0];
            float y0 = symbol.vertices[1];

            float x1 = symbol.vertices[count - 2];
            float y1 = symbol.vertices[count - 1];

            float cx = (x0 + x1) / 2;
            float cy = (y0 + y1) / 2;

            //calculate in screen coordinate space.
            double sx, sy;
            context.Project(cx, cy, 0, out sx, out sy);
            cx = (float)sx;
            cy = (float)sy;

            context.Project(x1, y1, 0, out sx, out sy);
            x1 = (float)sx;
            y1 = (float)sy;

            float vx = x1 - cx;
            float vy = y1 - cy;

            int dir = this.Direction;
            float nx = vy * dir;
            float ny = -vx * dir;

            double d = Math.Sin(30 * MathUtil.DEG_TO_RAD) * LineSymbol.SymbolSize;
            double len = Math.Sqrt(nx * nx + ny * ny);

            double tx = cx + d * nx / len;
            double ty = cy + d * ny / len;

            double z;
            context.Unproject(tx, ty, 0, out sx, out sy, out z);

            symbol.vertices.Add((float)sx);
            symbol.vertices.Add((float)sy);

            int pt_num = (count >> 1);

            int triangle_v_index = pt_num;

            for (int i = 0; i < pt_num; i++)
            {
                symbol.indices.Add(i);
                if ((i & 0x1) == 0 && i != pt_num - 1)
                {
                    symbol.indices.Add(triangle_v_index);
                }
            }
        }
    }
}
