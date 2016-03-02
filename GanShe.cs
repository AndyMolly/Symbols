using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMA.MICAPS.Symbols
{
    using CMA.MICAPS.Box2D;
    using SandLib.Math3D;
    using Line = CMA.MICAPS.Box2D.LineString2D<float>;
    class GanShe : LabelLineSymbol
    {
        private int _distance;
        public GanShe(Line line, string label, uint size, System.Drawing.Color label_color, LabelPosition position, bool rotation, int offset, int distance, bool split = false)
            : base(line, label, size, label_color, position, rotation, split)
        {
            _vertices = new List<float>(_line.Data.Length << 1);
            _indices = new List<int>((_vertices.Capacity - 1) * 2);
            this.Material.SurfaceState.point_size = offset;
            this._distance = distance;
            isAddLabel = true;
        }
        public GanShe(Line line, int offset, int distance)
         :base(line)
        {
            _vertices = new List<float>(_line.Data.Length << 1);
            _indices = new List<int>((_vertices.Capacity - 1) * 2);
            this.Material.SurfaceState.point_size = offset;
            this._distance = distance;
            isAddLabel = false;
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
            PrepareIndices();

            var color = this.Material.SurfaceState.color;
            _line_drawable.Draw(PrimitiveType.LineStrip, 0, _point_count);
            StaticBufferDrawHelper.DrawIndex(_vertices.ToArray(), _indices.ToArray(),
                 color, PrimitiveType.Lines, _indices.Count);

        }
        public void PrepareForDraw(Box2D.Graphics.Context context)
        {
            _vertices.Clear();

            float offset = this.Material.SurfaceState.point_size*2;//特殊处理保证偏移point_size

            float[] pts = _line.Data;
            int num = pts.Length;

            double pre_remain = 0.0;
            double total_len = 0.0;
            double vec_len = 0.0;
            double tx,ty,tx1, ty1,reverse_tx, reverse_ty, reverse_tx1, reverse_ty1;
            double sx1, sy1;
            double sx2, sy2;
            double len;
            float vx, vy;
            double sx, sy, reverse_sx, reverse_sy, z;
            for (int i = 2; i < num - 1; i += 2)
            {
                double curr_x = pts[i-2];
                double curr_y = pts[i -1];
                double next_x = pts[i];
                double next_y = pts[i+1];

                context.Project(curr_x, curr_y, 0, out sx1, out sy1);
                context.Project(next_x, next_y, 0, out sx2, out sy2);

               float x1 = (float)sx1;
               float y1 = (float)sy1;
               float x2 = (float)sx2;
               float y2 = (float)sy2;

                float ovx = x2 - x1;
                float ovy = y2 - y1;

                float onx = -ovy;
                float ony = ovx;
                float reverse_onx =ovy;
                float reverse_ony = -ovx;

                len = Math.Sqrt(ovx * ovx + ovy * ovy);

                vx = x2 - x1;
                vy = y2 - y1;

                //add
                tx = sx1 + offset * onx / len;
                ty = sy1 + offset * ony / len;
                tx1 = sx2 + offset * onx / len;
                ty1 = sy2 + offset * ony / len;

                reverse_tx = sx1 + offset * reverse_onx / len;
                reverse_ty = sy1 + offset * reverse_ony / len;
                reverse_tx1 = sx2 + offset * reverse_onx / len;
                reverse_ty1 = sy2 + offset * reverse_ony / len;

                double dx, dy, reverse_dx, reverse_dy;
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
                    //_toatal_num += insert_num;
                    for (int point_count = 0; point_count < insert_num; point_count++)
                    {

                        double insert_x = tx + ((point_count + 1) * _distance - pre_remain) * dx / vec_len;
                        double insert_y = ty + ((point_count + 1) * _distance - pre_remain) * dy / vec_len;
                        double insert_reverse_x = reverse_tx + ((point_count + 1) * _distance - pre_remain) * reverse_dx / reverse_vec_len;
                        double insert_reverse_y = reverse_ty + ((point_count + 1) * _distance - pre_remain) * reverse_dy / reverse_vec_len;
                        double line_x = (insert_x + insert_reverse_x) / 2;
                        double line_y = (insert_y + insert_reverse_y) / 2;
                        context.Unproject(line_x, line_y, 0, out sx, out sy, out z);
                        _vertices.Add((float)sx);
                        _vertices.Add((float)sy);

                        context.Unproject(insert_x, insert_y, 0, out sx, out sy, out z);
                        context.Unproject(insert_reverse_x, insert_reverse_y, 0, out reverse_sx, out reverse_sy, out z);

                        _vertices.Add((float)sx);
                        _vertices.Add((float)sy);

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

        }
        public void PrepareIndices()
        {
            _indices.Clear();
            int pt_num = (_vertices.Count) >> 1;

            for (int i = 0; i < pt_num; i++)
            {
                _indices.Add(i);
            }
        }
    }
}
