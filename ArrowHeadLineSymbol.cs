using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMA.MICAPS.Box2D.Util;

namespace CMA.MICAPS.Symbols
{
     using CMA.MICAPS.Box2D;
    using Line = CMA.MICAPS.Box2D.LineString2D<float>;

    class ArrowHeadLineSymbol : DoubleLineSymbol
    {
        public ArrowHeadLineSymbol(Line line, string label, uint size, System.Drawing.Color label_color, LabelPosition position, bool rotation, int offset, bool split = false)
            : base(line,label,size,label_color,position,rotation,offset,split)
        {
          
        }
        public ArrowHeadLineSymbol(Line line, int offset)
            :base(line,offset)
        { 
           
        }
        public override void PrepareForDraw(Box2D.Graphics.Context context)
        {
            base.PrepareForDraw(context);

            float offset = this.Material.SurfaceState.point_size;

            var pts = _line.Data;
            int num = pts.Length;

            double penult_x = pts[num - 4];
            double penult_y = pts[num - 3];
            double last_x = pts[num - 2];
            double last_y = pts[num - 1];

            double sx0, sy0;
            double sx1, sy1;
            float x0, y0;
            float x1, y1;

            context.Project(penult_x, penult_y, 0, out sx0, out sy0);
            context.Project(last_x, last_y, 0, out sx1, out sy1);

            x0 = (float)sx0;
            y0 = (float)sy0;
            x1 = (float)sx1;
            y1 = (float)sy1;
            float vx = x1 - x0;
            float vy = y1 - y0;


            double last_nx_length = 5;

            double triangle_side = 2 * offset + 2 * last_nx_length;
            int triangle_angle = 60;
            double vec_len = Math.Sqrt(vx * vx + vy * vy);
            double point_vx = triangle_side * Math.Sin(triangle_angle * MathUtil.DEG_TO_RAD) * vx / vec_len;
            double point_vy = triangle_side * Math.Sin(triangle_angle * MathUtil.DEG_TO_RAD) * vy / vec_len;
            double top_point_x = sx1 + point_vx;
            double top_point_y = sy1 + point_vy;

            double point_vec_len = Math.Sqrt(point_vx * point_vx + point_vy * point_vy);

            double theta1 = 150;
            double theta2 = 210;

            double cos_theta1 = Math.Cos(theta1 * MathUtil.DEG_TO_RAD);
            double sin_theta1 = Math.Sin(theta1 * MathUtil.DEG_TO_RAD);
            double cos_theta2 = Math.Cos(theta2 * MathUtil.DEG_TO_RAD);
            double sin_theta2 = Math.Sin(theta2 * MathUtil.DEG_TO_RAD);

            double target1_vec_x = point_vx * cos_theta1 - point_vy * sin_theta1;
            double target1_vec_y = point_vx * sin_theta1 + point_vy * cos_theta1;
            double target2_vec_x = point_vx * cos_theta2 - point_vy * sin_theta2;
            double target2_vec_y = point_vx * sin_theta2 + point_vy * cos_theta2;

            double target1_x = top_point_x + triangle_side * target1_vec_x / point_vec_len;
            double target1_y = top_point_y + triangle_side * target1_vec_y / point_vec_len;
            double target2_x = top_point_x + triangle_side * target2_vec_x / point_vec_len;
            double target2_y = top_point_y + triangle_side * target2_vec_y / point_vec_len;

            double sx, sy, sz;
            context.Unproject(target2_x, target2_y, 0, out sx, out sy, out sz);
            _vertices.Add((float)sx);
            _vertices.Add((float)sy);
            context.Unproject(top_point_x, top_point_y, 0, out sx, out sy, out sz);
            _vertices.Add((float)sx);
            _vertices.Add((float)sy);
            context.Unproject(target1_x, target1_y, 0, out sx, out sy, out sz);
            _vertices.Add((float)sx);
            _vertices.Add((float)sy);
        }

        public override void PrepareIndices()
        {
            base.PrepareIndices();
            //close line end.
            _indices.Add(0);
            _indices.Add(1);

            //draw head arrow.
            int first_line_end = (_point_count - 1) * 2;
            int next_line_end = first_line_end + 1;

            //triangle_index
            int vcount = _vertices.Count >> 1;

            //-t0
            //      -t1
            //-t3
            int t0 = vcount - 3;
            int t1 = vcount - 2;//top
            int t3 = vcount - 1;

            _indices.Add(first_line_end);
            _indices.Add(t0);

            _indices.Add(t0);
            _indices.Add(t1);

            _indices.Add(t1);
            _indices.Add(t3);

            _indices.Add(t3);
            _indices.Add(next_line_end);
        }
       
    }
}
