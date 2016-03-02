using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMA.MICAPS.Symbols
{
    using CMA.MICAPS.Box2D;
    using SandLib.Math3D;
    using Line = CMA.MICAPS.Box2D.LineString2D<float>;
    class DoubleLineSymbol : LabelLineSymbol
    {
        protected PrimitiveType _primitive_type;
        public DoubleLineSymbol(Line line,string label, uint size, System.Drawing.Color label_color, LabelPosition position,bool rotation, int offset, bool split = false)
            : base(line,label,size,label_color,position,rotation,split)
        {
            _vertices = new List<float>(_line.Data.Length << 1);
            _indices = new List<int>((_vertices.Capacity - 1) * 2);
            this.Material.SurfaceState.point_size = offset;
            isAddLabel = true;
        }
        public DoubleLineSymbol(Line line, int offset)
            :base(line)
        {
            _vertices = new List<float>(_line.Data.Length << 1);
            _indices = new List<int>((_vertices.Capacity - 1) * 2);
            this.Material.SurfaceState.point_size = offset;
            isAddLabel = false;
        }

        public override void Render(Box2D.Graphics.SceneManager scene, Box2D.Graphics.Context context)
        {
            context.SetRenderState(this.Material.SurfaceState);
             
            PrepareForDraw(context);
            PrepareIndices();

            var color = this.Material.SurfaceState.color;
            var ibuffer = _indices.ToArray();
            StaticBufferDrawHelper.DrawIndex(_vertices.ToArray(), ibuffer,
                color, _primitive_type, _indices.Count);

            Matrix4 mat;
            if (isAddLabel)
            {
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
            }
        }

        protected virtual void PrepareVertices()
        {
            _vertices.Clear();

            int offset = (int)this.Material.SurfaceState.point_size;
            float[] pts = _line.Data;
            int num = pts.Length;

            float x0 = pts[0];
            float y0 = pts[1];
            float x1 = pts[2];
            float y1 = pts[3];

            float vx = x1 - x0;
            float vy = y1 - y0;

            float nx = vy;
            float ny = -vx;
            float reverse_nx = -vy;
            float reverse_ny = vx;

            double len = Math.Sqrt(nx * nx + ny * ny);

            double tx = x0 + offset * nx / len;
            double ty = y0 + offset * ny / len;
            double reverse_tx = x0 + offset * reverse_nx / len;
            double reverse_ty = y0 + offset * reverse_ny / len;

            _vertices.Add((float)tx);
            _vertices.Add((float)ty);
            _vertices.Add((float)reverse_tx);
            _vertices.Add((float)reverse_ty);

            float nnx = nx, nny = ny;
            for (int i = 2; i < num - 1; i += 2)
            {
                float curr_x = pts[i-2];
                float curr_y = pts[i -1];
                float next_x = pts[i];
                float next_y = pts[i + 1];

                nnx = next_y - curr_y;
                nny = -(next_x - curr_x);
                float reverse_pnx = -nnx;
                float reverse_pny = -nny;

                float inx = nnx + nx;
                float iny = nny + ny;
                float reverse_inx = -inx;
                float reverse_iny = -iny;

                len = Math.Sqrt(inx * inx + iny * iny);

                tx = next_x + offset * inx / len;
                ty = next_x + offset * iny / len;
                reverse_tx = next_x + offset * reverse_inx / len;
                reverse_ty = next_x + offset * reverse_iny / len;

                _vertices.Add((float)tx);
                _vertices.Add((float)ty);
                _vertices.Add((float)reverse_tx);
                _vertices.Add((float)reverse_ty);

                nx = nnx;
                ny = nny;
            }
        }
        
        public virtual void PrepareIndices()
        {
            _indices.Clear();
            List<int> next_indices = new List<int>((_point_count - 1) * 2);
            for (int i = 0; i < _point_count - 1; i++)
            {
                int first_line_index = i << 1;
                int next_line_index = first_line_index + 1;

                //0--2--4 
                //1--3--5
                _indices.Add(first_line_index);
                next_indices.Add(next_line_index);

                _indices.Add(first_line_index + 2);
                next_indices.Add(next_line_index + 2);
            }

            _indices.AddRange(next_indices);

            _primitive_type = PrimitiveType.Lines;
        }

        public virtual void PrepareForDraw(Box2D.Graphics.Context context)
        {
            _vertices.Clear();

            float offset = this.Material.SurfaceState.point_size;

            float[] pts = _line.Data;
            double start_x = pts[0];
            double start_y = pts[1];
            double second_x = pts[2];
            double second_y = pts[3];
            int num = pts.Length;
            double penult_x = pts[num - 4];
            double penult_y = pts[num - 3];
            double last_x = pts[num - 2];
            double last_y = pts[num - 1];
            double sx0, sy0;
            double sx1, sy1;
            double sx2, sy2;
            float x0, y0;
            float x1, y1;
            float x2, y2;

            context.Project(start_x, start_y, 0, out sx0, out sy0);
            context.Project(second_x, second_y, 0, out sx1, out sy1);

            x0 = (float)sx0;
            y0 = (float)sy0;
            x1 = (float)sx1;
            y1 = (float)sy1;

            float vx = x1 - x0;
            float vy = y1 - y0;

            float nx = vy;
            float ny = -vx;
            float reverse_nx = -vy;
            float reverse_ny = vx;

            double len = Math.Sqrt(nx * nx + ny * ny);

            double tx = sx0 + offset * nx / len;
            double ty = sy0 + offset * ny / len;
            double reverse_tx = sx0 + offset * reverse_nx / len;
            double reverse_ty = sy0 + offset * reverse_ny / len;

            double sx, sy, reverse_sx, reverse_sy, z;
            context.Unproject(tx, ty, 0, out sx, out sy, out z);
            context.Unproject(reverse_tx, reverse_ty, 0, out reverse_sx, out reverse_sy, out z);

            _vertices.Add((float)sx);
            _vertices.Add((float)sy);

            _vertices.Add((float)reverse_sx);
            _vertices.Add((float)reverse_sy);
            for (int i = 2; i < pts.Length - 2; i += 2)
            {
                double prev_x = pts[i - 2];
                double prev_y = pts[i - 1];
                double curr_x = pts[i];
                double curr_y = pts[i + 1];
                double next_x = pts[i + 2];
                double next_y = pts[i + 3];

                context.Project(prev_x, prev_y, 0, out sx0, out sy0);
                context.Project(curr_x, curr_y, 0, out sx1, out sy1);
                context.Project(next_x, next_y, 0, out sx2, out sy2);

                x0 = (float)sx0;
                y0 = (float)sy0;
                x1 = (float)sx1;
                y1 = (float)sy1;
                x2 = (float)sx2;
                y2 = (float)sy2;

                float pvx = x1 - x0;
                float pvy = y1 - y0;
                vx = x2 - x1;
                vy = y2 - y1;

                nx = vy;
                ny = -vx;
                reverse_nx = -vy;
                reverse_ny = vx;

                float pnx = pvy;
                float pny = -pvx;
                float reverse_pnx = -pvy;
                float reverse_pny = pvx;

                float inx = pnx + nx;
                float iny = pny + ny;
                float reverse_inx = -inx;
                float reverse_iny = -iny;

                len = Math.Sqrt(inx * inx + iny * iny);

                tx = sx1 + offset * inx / len;
                ty = sy1 + offset * iny / len;
                reverse_tx = sx1 + offset * reverse_inx / len;
                reverse_ty = sy1 + offset * reverse_iny / len;

                context.Unproject(tx, ty, 0, out sx, out sy, out z);
                context.Unproject(reverse_tx, reverse_ty, 0, out reverse_sx, out reverse_sy, out z);

                _vertices.Add((float)sx);
                _vertices.Add((float)sy);

               _vertices.Add((float)reverse_sx);
                _vertices.Add((float)reverse_sy);

            }

            context.Project(penult_x, penult_y, 0, out sx0, out sy0);
            context.Project(last_x, last_y, 0, out sx1, out sy1);

            x0 = (float)sx0;
            y0 = (float)sy0;
            x1 = (float)sx1;
            y1 = (float)sy1;

            vx = x1 - x0;
            vy = y1 - y0;

            nx = vy;
            ny = -vx;
            reverse_nx = -vy;
            reverse_ny = vx;

            len = Math.Sqrt(nx * nx + ny * ny);

            tx = sx1 + offset * nx / len;
            ty = sy1 + offset * ny / len;
            reverse_tx = sx1 + offset * reverse_nx / len;
            reverse_ty = sy1 + offset * reverse_ny / len;

            context.Unproject(tx, ty, 0, out sx, out sy, out z);
            context.Unproject(reverse_tx, reverse_ty, 0, out reverse_sx, out reverse_sy, out z);

            _vertices.Add((float)sx);
            _vertices.Add((float)sy);

           _vertices.Add((float)reverse_sx);
            _vertices.Add((float)reverse_sy);

        }
    }
}
