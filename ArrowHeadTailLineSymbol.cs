using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMA.MICAPS.Box2D.Util;

namespace CMA.MICAPS.Symbols
{
    using CMA.MICAPS.Box2D;
    using Line = CMA.MICAPS.Box2D.LineString2D<float>;
    class ArrowHeadTailLineSymbol : ArrowHeadLineSymbol
    {

        public ArrowHeadTailLineSymbol(Line line, string label, uint size, System.Drawing.Color label_color, LabelPosition position, bool rotation, int offset, bool split = false)
            : base(line,label,size,label_color,position,rotation,offset,split)
        {
         
        }
        public ArrowHeadTailLineSymbol(Line line, int offset)
         :base(line,offset)
        {
        
        }
        public override void PrepareForDraw(Box2D.Graphics.Context context)
        {
            base.PrepareForDraw(context);

            float offset = this.Material.SurfaceState.point_size;
            var pts = _line.Data;

            //这里加入三个点形成箭尾
            double start_x = pts[0];
            double start_y = pts[1];
            double second_x = pts[2];
            double second_y = pts[3];

            double sx0, sy0;
            double sx1, sy1;
            float x0, y0;
            float x1, y1;

            context.Project(start_x, start_y, 0, out sx0, out sy0);
            context.Project(second_x, second_y, 0, out sx1, out sy1);

            x0 = (float)sx0;
            y0 = (float)sy0;
            x1 = (float)sx1;
            y1 = (float)sy1;
            float tail_vector_x = x1 - x0;
            float tail_vector_y = y1 - y0;

            double tail_theta1 = 145;
            double tail_theta2 = 215;
            double last_nx_length = 5;
            double triangle_side = 2 * offset + 2 * last_nx_length;
            double tail_triangle_side = triangle_side + 5;

            double cos_theta1 = Math.Cos(tail_theta1 * MathUtil.DEG_TO_RAD);
            double sin_theta1 = Math.Sin(tail_theta1 * MathUtil.DEG_TO_RAD);
            double cos_theta2 = Math.Cos(tail_theta2 * MathUtil.DEG_TO_RAD);
            double sin_theta2 = Math.Sin(tail_theta2 * MathUtil.DEG_TO_RAD);

            double tail_target1_vec_x = tail_vector_x * cos_theta1 - tail_vector_y * sin_theta1;
            double tail_target1_vec_y = tail_vector_x * sin_theta1 + tail_vector_y * cos_theta1;
            double tail_target2_vec_x = tail_vector_x * cos_theta2 - tail_vector_y * sin_theta2;
            double tail_target2_vec_y = tail_vector_x * sin_theta2 + tail_vector_y * cos_theta2;

            double tail_target_vec_len = Math.Sqrt(tail_vector_x * tail_vector_x + tail_vector_y * tail_vector_y);

            double tail_target1_x = sx0 + tail_triangle_side * tail_target1_vec_x / tail_target_vec_len;
            double tail_target1_y = sy0 + tail_triangle_side * tail_target1_vec_y / tail_target_vec_len;
            double tail_target2_x = sx0 + tail_triangle_side * tail_target2_vec_x / tail_target_vec_len;
            double tail_target2_y = sy0 + tail_triangle_side * tail_target2_vec_y / tail_target_vec_len;

            double sx, sy, sz;
            context.Unproject(tail_target1_x, tail_target1_y, 0, out sx, out sy, out sz);
            _vertices.Add((float)sx);
            _vertices.Add((float)sy);
         
            _vertices.Add((float)start_x);
            _vertices.Add((float)start_y);
            context.Unproject(tail_target2_x, tail_target2_y, 0, out sx, out sy, out sz);
            _vertices.Add((float)sx);
            _vertices.Add((float)sy);
        } 

        public override void PrepareIndices()
        {

            //double line indices
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

            //head arrow indices
            int first_line_end = (_point_count - 1) * 2;
            int next_line_end = first_line_end + 1;


            int vnum = _vertices.Count >> 1;

            //build indices for head arrow
            //-t0
            //      -t1
            //-t3
            int t0 = vnum - 6;
            int t1 = vnum - 5;//top
            int t2 = vnum - 4;

            _indices.Add(first_line_end);
            _indices.Add(t0);

            _indices.Add(t0);
            _indices.Add(t1);

            _indices.Add(t1);
            _indices.Add(t2);

            _indices.Add(t2);
            _indices.Add(next_line_end);

            //build indices for tail arrow
            t0 = vnum - 1;
            t1 = vnum - 2;
            t2 = vnum - 3;

            _indices.Add(0);
            _indices.Add(t0);

            _indices.Add(t0);
            _indices.Add(t1);

            _indices.Add(t1);
            _indices.Add(t2);

            _indices.Add(t2);
            _indices.Add(1);
        }

    }
}
