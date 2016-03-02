using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMA.MICAPS.Symbols
{
    using CMA.MICAPS.Box2D;
    using SandLib.Math3D;
    using Line = CMA.MICAPS.Box2D.LineString2D<float>;
    class JiLiuHeNew : DoubleLineSymbol
    {
        private List<int> _quad_indices;
        private List<float> _quad_vectices;
        private List<int> _line_indices;
        private List<float> _line_vectices;
        private List<float> _reverse_line_vectices;
        private int _distance;
        private int _distance_original;
        public JiLiuHeNew(Line line, string label, uint size, System.Drawing.Color label_color, LabelPosition position, bool rotation, int offset,int distance ,bool split = false)
         :base(line,label,size,label_color,position,rotation,offset)
        {
            _quad_indices = new List<int>();
            _quad_vectices = new List<float>();
            _line_vectices = new List<float>();
            _reverse_line_vectices = new List<float>();
            _line_indices = new List<int>();
            _distance_original = distance;
            isAddLabel = true;
        }
        public JiLiuHeNew(Line line, int offset, int distance)
            :base(line,offset)
        {
            _quad_indices = new List<int>();
            _quad_vectices = new List<float>();
            _line_vectices = new List<float>();
            _reverse_line_vectices = new List<float>();
            _line_indices = new List<int>();
            _distance_original = distance;
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

            var color = SymbolColor0;
            var color1 = SymbolColor1;
            var ibuffer = _indices.ToArray();
            StaticBufferDrawHelper.DrawIndex(_vertices.ToArray(), ibuffer,
                color, PrimitiveType.TriangleStrip, _indices.Count);

            StaticBufferDrawHelper.DrawIndex(_line_vectices.ToArray(), _line_indices.ToArray(), color1, PrimitiveType.LineStrip, _line_indices.Count);
            StaticBufferDrawHelper.DrawIndex(_reverse_line_vectices.ToArray(), _line_indices.ToArray(), color1, PrimitiveType.LineStrip, _line_indices.Count);
            StaticBufferDrawHelper.DrawIndex(_quad_vectices.ToArray(), _quad_indices.ToArray(),
             color1, PrimitiveType.Lines, _quad_indices.Count);
        }
        public override void PrepareForDraw(Box2D.Graphics.Context context)
        {
            _vertices.Clear();
            //_indices.Clear();
            _line_vectices.Clear();
            _reverse_line_vectices.Clear();

            float offset = this.Material.SurfaceState.point_size;

            _distance = 0;
            _distance = _distance_original + (int)this.Material.SurfaceState.point_size*2;

            float[] pts = _line.Data;
            double start_x = pts[0];
            double start_y = pts[1];
            double second_x = pts[2];
            double second_y = pts[3];
            int num = pts.Length;
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
            _line_vectices.Add((float)sx);
            _line_vectices.Add((float)sy);

            _vertices.Add((float)reverse_sx);
            _vertices.Add((float)reverse_sy);
            _reverse_line_vectices.Add((float)reverse_sx);
            _reverse_line_vectices.Add((float)reverse_sy);

            double pre_remain = 0.0;
            double total_len = 0.0;
            double vec_len = 0.0;
            double tx1, ty1, reverse_tx1, reverse_ty1;
            for (int i = 2; i < pts.Length - 1; i += 2)
            {
                double curr_x = pts[i-2];
                double curr_y = pts[i -1];
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

                tx1 = sx2 + offset * onx / len;
                ty1 = sy2 + offset * ony / len;
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
                        _line_vectices.Add((float)sx);
                        _line_vectices.Add((float)sy);

                        _vertices.Add((float)reverse_sx);
                        _vertices.Add((float)reverse_sy);
                        _reverse_line_vectices.Add((float)reverse_sx);
                        _reverse_line_vectices.Add((float)reverse_sy);
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

            _quad_vectices.Clear();
            List<float> next1_Vec = new List<float>();
            List<float> next2_Vec = new List<float>();

            for (int j = 0; j < _vertices.Count; )
            {
                if (j < _vertices.Count - 7)
                {
                    next1_Vec.Add(_vertices[j]);
                    next1_Vec.Add(_vertices[j + 1]);

                    next1_Vec.Add(_vertices[j + 6]);
                    next1_Vec.Add(_vertices[j + 7]);

                    next2_Vec.Add(_vertices[j + 2]);
                    next2_Vec.Add(_vertices[j + 3]);
                    next2_Vec.Add(_vertices[j + 4]);
                    next2_Vec.Add(_vertices[j + 5]);
                }
                j += 4;
            }
            _quad_vectices.AddRange(next1_Vec);
            _quad_vectices.AddRange(next2_Vec);
        }
       public override void PrepareIndices()
        {
            _indices.Clear();
            _quad_indices.Clear();
            _line_indices.Clear();

            int quad_point = _vertices.Count >> 1;
            for (int i = 0; i < quad_point; i++)
            {
                _indices.Add(i);
            }

            int num = _quad_vectices.Count >> 1;
            for (int j = 0; j < num; j++)
            {
                _quad_indices.Add(j);
            }
            for (int m = 0; m < _line_vectices.Count / 2; m++)
            {
                _line_indices.Add(m);
            }
            //List<short> next_indices = new List<short>();
            //for (int i = 0; i < quad_point/2; i++)
            //{
            //    short first_line_index = (short)(i << 1);
            //    short next_line_index = (short)(first_line_index + 1);

            //    //0--2--4 
            //    //1--3--5
            //    _line_indices.Add(first_line_index);
            //    next_indices.Add(next_line_index);

            //    _line_indices.Add((short)(first_line_index + 2));
            //    next_indices.Add((short)(next_line_index + 2));
            //}

            //_line_indices.AddRange(next_indices);
        }
    }
}
