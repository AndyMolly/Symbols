using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMA.MICAPS.Box2D;
using CMA.MICAPS.Box2D.Graphics;

namespace CMA.MICAPS.Symbols
{
    using CMA.MICAPS.Box2D.Buffers;
    using CMA.MICAPS.Box2D.Util;
    using Microsoft.Practices.ServiceLocation;
    using SandLib.Math3D;
    using Line = LineString2D<float>;
    class FrontWithLabel:LabelLineSymbol
    {
        public FrontWithLabel(Line line, string label, uint size,
          System.Drawing.Color label_color, LabelPosition position, bool rotation, bool split = false)
            : base(line, label, size, label_color, position, rotation, split)
        {
          
        }
        public override void Render(SceneManager scene, Context context)
        {
            Matrix4 mat;
            context.PushOrtho2D();
            for (int i = 0; i < _render_params.Length; i++)
            {
                _label.Position = new Vec3(_render_params[i].x, _render_params[i].y, 0);
                _label.ScreenOffset = new Vec2(_render_params[i].offx, _render_params[i].offy);
                _label.GetTransformation(out mat);
                _label.Roll(_render_params[i].angle);
                _label.Render(scene, context);
                _label.LoadTransformation(ref mat);
            }
            context.PopOrtho2D();
            context.SetRenderState(this.Material.SurfaceState);

            _line_drawable.Draw(PrimitiveType.LineStrip, 0, _point_count);

            context.PushOrtho2D();

            System.Drawing.Color c1 = this.Material.BackColor;
            System.Drawing.Color c0 = this.Material.ForeColor;

            InterpolatePosition start = new InterpolatePosition();
            start.x = _line.Data[0];
            start.y = _line.Data[1];
            start.next = 2;
            start = this.FindNextPosition(context, start, SymbolSize);

            do
            {
                SymbolMesh symbol = this.MakeSymbol(context, start, SymbolSize, 0);
                if (!symbol.is_completed)
                    break;
                SymbolMesh neighbor_symbol = this.MakeSymbol(context, symbol.tail, SymbolSize, 1);
                if (!neighbor_symbol.is_completed)
                    break;
                start = this.FindNextPosition(context, neighbor_symbol.tail, SymbolDistance);

                context.Project(symbol.vertices);
                context.Project(neighbor_symbol.vertices);

                StaticBufferDrawHelper.DrawIndex(symbol.vertices.ToArray(), symbol.indices.ToArray(),
                   c0, PrimitiveType.TriangleStrip, symbol.indices.Count);

                StaticBufferDrawHelper.DrawIndex(neighbor_symbol.vertices.ToArray(), neighbor_symbol.indices.ToArray(),
                    c1, PrimitiveType.TriangleStrip, neighbor_symbol.indices.Count);

            } while (true);

            context.PopOrtho2D();
        }
        private InterpolatePosition FindNextPosition(Context context, InterpolatePosition start, double distance)
        {
            double sum_len = 0;
            double px = start.x;
            double py = start.y;
            float[] pts = _line.Data;
            int num = pts.Length;
            double sx0, sy0;
            double sx1, sy1;
            context.Project(start.x, start.y, 0, out sx0, out sy0);

            InterpolatePosition symbol_start = new InterpolatePosition() { next = num };
            for (int index = start.next; index < num; index += 2)
            {
                double x1 = pts[index];
                double y1 = pts[index + 1];
                context.Project(x1, y1, 0, out sx1, out sy1);

                double vx = sx1 - sx0;
                double vy = sy1 - sy0;
                double len = Math.Sqrt(vx * vx + vy * vy);
                sum_len += len;

                if (sum_len < distance)
                {
                    sx0 = sx1;
                    sy0 = sy1;
                    px = x1;
                    py = y1;
                    continue;
                }

                //calculate symbol's start point.
                //distance previous symbol SymbolDistance.
                //now sum_len is greater or equals to symbol distance.
                if (sum_len > distance)
                {
                    //start point is not on line's point.
                    //interpolate from previous point.
                    double d0 = distance - (sum_len - len);
                    double t = d0 / len;
                    //get previous point.
                    vx = x1 - px;
                    vy = y1 - py;
                    //interpolate
                    x1 = px + t * vx;
                    y1 = py + t * vy;
                    symbol_start.next = index;
                }
                else
                {
                    symbol_start.next = index + 2;
                }
                symbol_start.x = x1;
                symbol_start.y = y1;
                break;
            }
            return symbol_start;
        }

        private SymbolMesh MakeSymbol(Context context, InterpolatePosition start, int size, byte seq)
        {
            //add symbol data
            SymbolMesh symbol = new SymbolMesh();
            symbol.seq = seq;
            symbol.vertices.Add((float)start.x);
            symbol.vertices.Add((float)start.y);


            double sum_len = 0;
            double px = start.x;
            double py = start.y;
            float[] pts = _line.Data;
            int num = pts.Length;
            double sx0, sy0;
            double sx1, sy1;
            context.Project(start.x, start.y, 0, out sx0, out sy0);

            for (int index = start.next; index < num; index += 2)
            {
                double x1 = pts[index];
                double y1 = pts[index + 1];
                context.Project(x1, y1, 0, out sx1, out sy1);

                double vx = sx1 - sx0;
                double vy = sy1 - sy0;
                double len = Math.Sqrt(vx * vx + vy * vy);
                sum_len += len;

                if (sum_len < size)
                {
                    symbol.vertices.Add((float)x1);
                    symbol.vertices.Add((float)y1);
                    sx0 = sx1;
                    sy0 = sy1;
                    px = x1;
                    py = y1;
                    continue;
                }

                if (sum_len > size)
                {
                    //interpolate from previous point.
                    double d0 = size - (sum_len - len);
                    double t = d0 / len;
                    //get previous point.                   
                    vx = x1 - px;
                    vy = y1 - py;
                    //interpolate
                    x1 = px + t * vx;
                    y1 = py + t * vy;
                    symbol.tail.next = index;
                }
                else
                {
                    symbol.tail.next = index + 2;
                }
                symbol.is_completed = true;
                symbol.tail.x = x1;
                symbol.tail.y = y1;
                symbol.vertices.Add((float)x1);
                symbol.vertices.Add((float)y1);
                break;
            }

            if (symbol.is_completed)
            {
                //assembly vertices into symbols
                this.AssembleSymbol(context, symbol);
            }

            return symbol;
        }
    }

}
