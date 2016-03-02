
using System;
using System.Collections.Generic;
using System.Text;

namespace CMA.MICAPS.Symbols
{
    class WarmFront : ISymbolAssembler
    {
        public WarmFront()
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

            float wcx = cx;
            float wcy = cy;

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

            double len = Math.Sqrt(vx * vx + vy * vy);

            int segments = 8;
            double a = Math.PI / segments;
            float radius = LineSymbol.SymbolSize * 0.5F;
            for (int i = 0; i < segments; ++i)
            {
                double angle = (i + 1) * a;
                double vd = radius * Math.Sin(angle);
                double hd = radius * Math.Cos(angle);

                double dx = hd / len;
                double px = cx + vx * dx;
                double py = cy + vy * dx;

                double dy = vd / len;
                px = px + nx * dy;
                py = py + ny * dy;

                double dummyz;
                context.Unproject(px, py, 0, out px, out py, out dummyz);

                symbol.vertices.Add((float)px);
                symbol.vertices.Add((float)py);
            }

            symbol.vertices.Add(wcx);
            symbol.vertices.Add(wcy);

            int circle_num = (symbol.vertices.Count >> 1) - 1;
            int center_v_index = circle_num;

            for (int i = 0; i < circle_num; i++)
            {
                symbol.indices.Add(i);
                if ((i & 0x1) == 0 && i != circle_num - 1)
                {
                    symbol.indices.Add(center_v_index);
                }
            }

        }
    }
}
