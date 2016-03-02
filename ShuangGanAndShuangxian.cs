using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMA.MICAPS.Symbols
{
    using CMA.MICAPS.Box2D;
    using SandLib.Math3D;
    using Line = CMA.MICAPS.Box2D.LineString2D<float>;
    class ShuangGanAndShuangxian:DoubleLineSymbol
    {       
        protected int _toatal_num = 0;
        protected int _distance;
        private int _distance_original;
        public ShuangGanAndShuangxian(Line line, string label, uint size, System.Drawing.Color label_color, LabelPosition position, bool rotation, int offset, int distance, bool split = false)
            :base(line,label,size,label_color,position,rotation,offset)
        {
            _distance_original = distance;
            isAddLabel = true;
        }
        public ShuangGanAndShuangxian(Line line,int offset, int distance)
            : base(line, offset)
        {
            _distance_original = distance;
            isAddLabel = false;
        }
        public override void PrepareForDraw(Box2D.Graphics.Context context)
        {
            _vertices.Clear();
            _indices.Clear();
            float offset = this.Material.SurfaceState.point_size;

            _distance = 0;
            _distance = _distance_original + (int)this.Material.SurfaceState.point_size;

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

            double len = Math.Sqrt(vx * vx + vy * vy);

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
            _toatal_num++;

            double pre_remain = 0.0;
            double total_len = 0.0;
            double vec_len = 0.0;
            double tx1, ty1, reverse_tx1, reverse_ty1;
            for (int i = 2; i < pts.Length - 1; i += 2)
            {

                //double curr_x = pts[i];
                //double curr_y = pts[i + 1];
                //double next_x = pts[i + 2];
                //double next_y = pts[i + 3];
                double curr_x = pts[i - 2];
                double curr_y = pts[i-1];
                double next_x = pts[i];
                double next_y = pts[i + 1];

                context.Project(curr_x, curr_y, 0, out sx1, out sy1);
                context.Project(next_x, next_y, 0, out sx2, out sy2);

                x1 = (float)sx1;
                y1 = (float)sy1;
                x2 = (float)sx2;
                y2 = (float)sy2;

                float ovx = x2 - x1;
                float ovy = y2 - y1;

                float onx = ovy;
                float ony = -ovx;
                float reverse_onx = -ovy;
                float reverse_ony = ovx;

                len = Math.Sqrt(ovx * ovx + ovy * ovy);
                float pvx = x1 - x0;
                float pvy = y1 - y0;
                vx = x2 - x1;
                vy = y2 - y1;

                //tx1 = sx1 + offset * onx / len;
                //ty1 = sy1 + offset * ony / len;
                //reverse_tx1 = sx1 + offset * reverse_onx / len;
                //reverse_ty1 = sy1 + offset * reverse_ony / len;
                tx1 = sx2 + offset * onx / len;
                ty1 = sy2 + offset * ony / len;
                reverse_tx1 = sx2 + offset * reverse_onx / len;
                reverse_ty1 = sy2 + offset * reverse_ony / len;

                double dx, dy,reverse_dx,reverse_dy;
                dx = tx1 - tx;
                dy = ty1 - ty;
                reverse_dx = reverse_tx1 - reverse_tx;
                reverse_dy = reverse_ty1 - reverse_ty;


                vec_len = Math.Sqrt(dx * dx + dy * dy);
                double reverse_vec_len = Math.Sqrt(reverse_dx * reverse_dx + reverse_dy * reverse_dy);
                total_len = vec_len + pre_remain;
                if (total_len >= _distance)
                {
                    int insert_num = (int)(total_len / _distance);
                    _toatal_num += insert_num;
                    for (int point_count = 0; point_count < insert_num; point_count++)
                    {
                        double insert_x = tx + ((point_count + 1) * _distance - pre_remain) * dx / vec_len;
                        double insert_y = ty + ((point_count + 1) * _distance - pre_remain) * dy / vec_len;
                        double insert_reverse_x = reverse_tx + ((point_count + 1) * _distance - pre_remain) * reverse_dx / reverse_vec_len;
                        double insert_reverse_y = reverse_ty + ((point_count + 1) * _distance - pre_remain) * reverse_dy / reverse_vec_len;

                        context.Unproject(insert_x, insert_y, 0, out sx, out sy, out z);
                        context.Unproject(insert_reverse_x, insert_reverse_y, 0, out reverse_sx, out reverse_sy, out z);

                        _vertices.Add((float)sx);
                        _vertices.Add((float)sy);

                        _vertices.Add((float)reverse_sx);
                        _vertices.Add((float)reverse_sy);
                    }
                    pre_remain = total_len - insert_num * _distance;
                }
                else if (total_len < _distance)
                {
                    pre_remain += vec_len;
                }

                tx = tx1;
                ty = ty1;
                reverse_tx = reverse_tx1;
                reverse_ty = reverse_ty1;
            }

            //context.Project(penult_x, penult_y, 0, out sx0, out sy0);
            //context.Project(last_x, last_y, 0, out sx1, out sy1);

            //x0 = (float)sx0;
            //y0 = (float)sy0;
            //x1 = (float)sx1;
            //y1 = (float)sy1;

            //vx = x1 - x0;
            //vy = y1 - y0;

            //nx = vy;
            //ny = -vx;
            //reverse_nx = -vy;
            //reverse_ny = vx;

            //len = Math.Sqrt(nx * nx + ny * ny);

            //tx = sx1 + offset * nx / len;
            //ty = sy1 + offset * ny / len;
            //reverse_tx = sx1 + offset * reverse_nx / len;
            //reverse_ty = sy1 + offset * reverse_ny / len;

            //context.Unproject(tx, ty, 0, out sx, out sy, out z);
            //context.Unproject(reverse_tx, reverse_ty, 0, out reverse_sx, out reverse_sy, out z);

            //_vertices.Add((float)sx);
            //_vertices.Add((float)sy);

            //_vertices.Add((float)reverse_sx);
            //_vertices.Add((float)reverse_sy);
            //_toatal_num++;

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
        public override void PrepareIndices()
        {
            _indices.Clear();
            List<int> next_indices = new List<int>((_toatal_num - 1) * 2);
            List<int> near_indices = new List<int>((_toatal_num - 1) * 2);
            for (int i = 0; i <_toatal_num - 1; i++)
            {
                int first_line_index = (i << 1);
                int next_line_index = first_line_index + 1;

                //0--2--4 
                //1--3--5
                //0-1   2-3  ...
                _indices.Add(first_line_index);
                next_indices.Add(next_line_index);
                near_indices.Add(first_line_index);
                near_indices.Add(next_line_index);
                if (i == _toatal_num - 2)
                {
                    near_indices.Add(first_line_index + 2);
                    near_indices.Add(next_line_index + 2);
                }
                _indices.Add(first_line_index + 2);
                next_indices.Add(next_line_index + 2);

            }

            _indices.AddRange(next_indices);
            _indices.AddRange(near_indices);
            _toatal_num = 0;
            _primitive_type = PrimitiveType.Lines;
        }
    }
}
