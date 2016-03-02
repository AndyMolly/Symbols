using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMA.MICAPS.Box2D.Util;

namespace CMA.MICAPS.Symbols
{
    using CMA.MICAPS.Box2D;
    using Line = CMA.MICAPS.Box2D.LineString2D<float>;
    using SandLib.Math3D;
    class ArrowMidLineSymbol : DoubleLineSymbol
    {
        List<float> _reverse_vertices = new List<float>();
        List<short> _arrow_indices = new List<short>();
        List<float> _arrow_vertices;
        public ArrowMidLineSymbol(Line line,  string label, uint size, System.Drawing.Color label_color, LabelPosition position, bool rotation, int offset, bool split = false)
            : base(line,label,size,label_color,position,rotation,offset,split)
        {
            _arrow_vertices = new List<float>() ;
            isAddLabel = true;
        }
        public ArrowMidLineSymbol(Line line, int offset)
            : base(line, offset)
        {
            _arrow_vertices = new List<float>();
            isAddLabel = false ;
        }

        public override void Render(Box2D.Graphics.SceneManager scene, Box2D.Graphics.Context context)
        {
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

            context.SetRenderState(this.Material.SurfaceState);

            PrepareForDraw(context);

            var color = this.Material.SurfaceState.color;
            var ibuffer = _indices.ToArray();
            StaticBufferDrawHelper.DrawIndex(_vertices.ToArray(), ibuffer,
                color, PrimitiveType.Lines, _indices.Count);
            StaticBufferDrawHelper.DrawIndex(_reverse_vertices.ToArray(), ibuffer,
                color, PrimitiveType.Lines, _indices.Count);
           StaticBufferDrawHelper.DrawIndex(_arrow_vertices.ToArray(), _arrow_indices.ToArray(), color, PrimitiveType.Lines, _arrow_indices.Count);
        }

        public override void PrepareForDraw(Box2D.Graphics.Context context)
        {
            _vertices.Clear();
            _reverse_vertices.Clear();
            _indices.Clear();
            _arrow_vertices.Clear();
            _arrow_indices.Clear();
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
            _reverse_vertices.Add((float)reverse_sx);
            _reverse_vertices.Add((float)reverse_sy);

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
                _reverse_vertices.Add((float)reverse_sx);
                _reverse_vertices.Add((float)reverse_sy);

                //nx_vertices.Add((float)curr_x);
                //nx_vertices.Add((float)curr_y);
                //nx_vertices.Add((float)sx);
                //nx_vertices.Add((float)sy);

                //reverse_nx_vertices.Add((float)curr_x);
                //reverse_nx_vertices.Add((float)curr_y);
                //reverse_nx_vertices.Add((float)reverse_sx);
                //reverse_nx_vertices.Add((float)reverse_sy);
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
            _reverse_vertices.Add((float)reverse_sx);
            _reverse_vertices.Add((float)reverse_sy);

            int count = pts.Length;
            int pt_num = (count >> 1);

            for (int i = 0; i < pt_num; i++)
            {
                _indices.Add(i);
                if(i<pt_num-1)
                _indices.Add(i + 1);
            }

            double line_start_x, line_start_y, line_end_x, line_end_y;
            if (0 == pt_num % 2)
            {
                line_start_x = pts[pt_num - 2];
                line_start_y = pts[pt_num - 1];
                line_end_x = pts[pt_num];
                line_end_y = pts[pt_num + 1];
            }
            else
            {
                line_start_x = pts[pt_num - 1];
                line_start_y = pts[pt_num];
                line_end_x = pts[pt_num + 1];
                line_end_y = pts[pt_num + 2];
            }

            //_test.Add((float)line_start_x);
            //_test.Add((float)line_start_y);
            //_test.Add((float)line_end_x);
            //_test.Add((float)line_end_y);

            //for (int i = 0; i < 4; i++)
            //{
            //    _test_indices.Add((short)i);
            //}

            context.Project(line_start_x, line_start_y, 0, out sx0, out sy0);
            context.Project(line_end_x, line_end_y, 0, out sx1, out sy1);

            float s_line_start_x = (float)sx0;
            float s_line_start_y = (float)sy0;
            float s_line_end_x = (float)sx1;
            float s_line_end_y = (float)sy1;

            float arrow_point_x = (s_line_start_x + s_line_end_x) / 2;
            float arrow_point_y = (s_line_start_y + s_line_end_y) / 2;
            float vector_x = arrow_point_x - s_line_start_x;
            float vector_y = arrow_point_y - s_line_start_y;

            double theta1 = 150;
            double theta2 = 210;

            double cos_theta1 = Math.Cos(theta1 * MathUtil.DEG_TO_RAD);
            double sin_theta1 = Math.Sin(theta1 * MathUtil.DEG_TO_RAD);
            double cos_theta2 = Math.Cos(theta2 * MathUtil.DEG_TO_RAD);
            double sin_theta2 = Math.Sin(theta2 * MathUtil.DEG_TO_RAD);

            double target1_vec_x = vector_x * cos_theta1 - vector_y * sin_theta1;
            double target1_vec_y = vector_x * sin_theta1 + vector_y * cos_theta1;
            double target2_vec_x = vector_x * cos_theta2 - vector_y * sin_theta2;
            double target2_vec_y = vector_x * sin_theta2 + vector_y * cos_theta2;

            double vec_len = Math.Sqrt(target1_vec_x * target1_vec_x + target1_vec_y * target1_vec_y);
            float set_len = 3 * offset;

            double target1_x = arrow_point_x + set_len * target1_vec_x / vec_len;
            double target1_y = arrow_point_y + set_len * target1_vec_y / vec_len;
            double target2_x = arrow_point_x + set_len * target2_vec_x / vec_len;
            double target2_y = arrow_point_y + set_len * target2_vec_y / vec_len;

            double starget1_x, starget1_y, starget2_x, starget2_y, sarrow_point_x, sarrow_point_y;
            context.Unproject(target1_x, target1_y, 0, out starget1_x, out starget1_y, out z);
            context.Unproject(target2_x, target2_y, 0, out starget2_x, out starget2_y, out z);
            context.Unproject(arrow_point_x, arrow_point_y, 0, out sarrow_point_x, out sarrow_point_y, out z);
           
            _arrow_vertices.Add((float)starget1_x);
            _arrow_vertices.Add((float)starget1_y);
            _arrow_vertices.Add((float)sarrow_point_x);
            _arrow_vertices.Add((float)sarrow_point_y);
            _arrow_vertices.Add((float)starget2_x);
            _arrow_vertices.Add((float)starget2_y);

            count = _arrow_vertices.Count;
            pt_num = count >> 1;

            for (int i = 0; i < pt_num; i++)
            {
                _arrow_indices.Add((short)i);
                if (i < pt_num - 1)
                    _arrow_indices.Add((short)(i + 1));
            }
        }
    }
}
