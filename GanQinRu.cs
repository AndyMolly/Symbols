using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMA.MICAPS.Symbols
{
    using CMA.MICAPS.Box2D;
    using SandLib.Math3D;
    using Line = CMA.MICAPS.Box2D.LineString2D<float>;
    class GanQinRu : LabelLineSymbol
    {
        private int _distance;
        private int _distance_original;
        private int _toatal_num = 0;
        private const int _sections = 10;
        private bool isAddQuad = false;
        private List<List<float>> _circle_vertices;
        private List<int> _circle_idices;
        public GanQinRu(Line line, string label, uint size, System.Drawing.Color label_color, LabelPosition position, bool rotation, int distance, int radius, int offset, bool split = false)
            : base(line,label,size,label_color,position,rotation,split)
        {
            _vertices = new List<float>(_line.Data.Length << 1);
            _indices = new List<int>((_vertices.Capacity - 1) * 2);
            _distance_original = distance;
            this.Material.SurfaceState.point_size = radius;
            _circle_vertices = new List<List<float>>();
            _circle_idices = new List<int>();
            this.Material.SurfaceState.point_size = offset;
            isAddLabel = true;
        }
        public GanQinRu(Line line, int distance, int radius, int offset)
            : base(line)
        {
            _vertices = new List<float>(_line.Data.Length << 1);
            _indices = new List<int>((_vertices.Capacity - 1) * 2);
            _distance_original = distance;
            this.Material.SurfaceState.point_size = radius;
            _circle_vertices = new List<List<float>>();
            _circle_idices = new List<int>();
            this.Material.SurfaceState.point_size = offset;
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
            var ibuffer = _indices.ToArray();
            StaticBufferDrawHelper.DrawIndex(_vertices.ToArray(), _indices.ToArray(), color, PrimitiveType.Lines, _indices.Count);

            for (int i = 0; i < _circle_vertices.Count-1; i++)
            {
                StaticBufferDrawHelper.DrawIndex(_circle_vertices[i].ToArray(), _circle_idices.ToArray(), color, PrimitiveType.LineLoop, _circle_idices.Count);
            }
        }

        public void PrepareForDraw(Box2D.Graphics.Context context)
        {

            _vertices.Clear();
            _circle_vertices.Clear();

            float offset = this.Material.SurfaceState.point_size;
            float radius = this.Material.SurfaceState.point_size;

            _distance = 0;
            _distance =_distance_original + (int)this.Material.SurfaceState.point_size *4;

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

            _vertices.Add((float)reverse_sx);
            _vertices.Add((float)reverse_sy);
            _toatal_num++;
            isAddQuad = true;
            double pre_remain = 0.0;
            double total_len = 0.0;
            double vec_len = 0.0;
            double tx1, ty1, reverse_tx1, reverse_ty1;
            for (int i = 2; i < pts.Length - 1; i += 2)
            {

                double curr_x = pts[i-2];
                double curr_y = pts[i-1];
                double next_x = pts[i];
                double next_y = pts[i +1];

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
                    _toatal_num += insert_num;
                    for (int point_count = 0; point_count < insert_num; point_count++)
                    {

                        double insert_x = tx + ((point_count + 1) * _distance - pre_remain) * dx / vec_len;
                        double insert_y = ty + ((point_count + 1) * _distance - pre_remain) * dy / vec_len;
                        double insert_reverse_x = reverse_tx + ((point_count + 1) * _distance - pre_remain) * reverse_dx / reverse_vec_len;
                        double insert_reverse_y = reverse_ty + ((point_count + 1) * _distance - pre_remain) * reverse_dy / reverse_vec_len;

                        int offt = _distance / 2;
                        double circle_center_x = (insert_x+insert_reverse_x)/2 + offt * ovx /len;
                        double circle_center_y = (insert_y+insert_reverse_y)/2 + offt * ovy / len;
                        double  circle_sx, circle_sy, circle_sz;
                        if (isAddQuad)
                        {
                            context.Unproject(insert_x, insert_y, 0, out sx, out sy, out z);
                            context.Unproject(insert_reverse_x, insert_reverse_y, 0, out reverse_sx, out reverse_sy, out z);

                            _vertices.Add((float)sx);
                            _vertices.Add((float)sy);

                            _vertices.Add((float)reverse_sx);
                            _vertices.Add((float)reverse_sy);

                            List<float> circle = new List<float>();
                            for (int count = 0; count <= _sections; count++)
                            {
                                double circle_x = circle_center_x + radius * Math.Cos(count * 2 * Math.PI / _sections);
                                double circle_y = circle_center_y + radius * Math.Sin(count * 2 * Math.PI / _sections);

                                context.Unproject(circle_x, circle_y, 0, out circle_sx, out circle_sy, out circle_sz);

                                circle.Add((float)circle_sx);
                                circle.Add((float)circle_sy);
                            }
                            isAddQuad = false;
                            _circle_vertices.Add(circle);
                        }
                        else
                        {
                            context.Unproject(insert_x, insert_y, 0, out sx, out sy, out z);
                            context.Unproject(insert_reverse_x, insert_reverse_y, 0, out reverse_sx, out reverse_sy, out z);

                            _vertices.Add((float)sx);
                            _vertices.Add((float)sy);

                            _vertices.Add((float)reverse_sx);
                            _vertices.Add((float)reverse_sy);
                            isAddQuad = true;
                        }

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
       public virtual void PrepareIndices()
        {
            _indices.Clear();
            _circle_idices.Clear();
            List<int> next_indices = new List<int>((_toatal_num - 1) * 2);
            List<int> near_indices = new List<int>((_toatal_num - 1) * 2);
            if (_toatal_num % 2 != 0) 
             _toatal_num = _toatal_num - 1;
            for (int i = 0; i < _toatal_num; i++)
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
            }

            _indices.AddRange(next_indices);
            _indices.AddRange(near_indices);
            _toatal_num = 0;
          
          
            int circleNum = _sections + 1;

            for (int x = 0; x < circleNum; x++)
            {
                _circle_idices.Add(x);
            }
        }
    }
}
