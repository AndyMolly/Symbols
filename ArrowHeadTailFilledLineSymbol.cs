using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMA.MICAPS.Box2D.Util;

namespace CMA.MICAPS.Symbols
{
    using CMA.MICAPS.Box2D;
    using Line = CMA.MICAPS.Box2D.LineString2D<float>;
    class ArrowHeadTailFilledLineSymbol : ArrowHeadFilledLineSymbol
    {
        private float[] _tail_fill_vertices;
        private short[] _tail_fill_indices;

        public ArrowHeadTailFilledLineSymbol(Line line, string label, uint size, System.Drawing.Color label_color, LabelPosition position, bool rotation, int offset, bool split = false)
            : base(line,label,size,label_color,position,rotation,offset,split)
        {
            //two triangles.
            _tail_fill_vertices = new float[10];
            _tail_fill_indices = new short[6];
        }
        public ArrowHeadTailFilledLineSymbol(Line line, int offset)
            : base(line, offset)
        {
            _tail_fill_vertices = new float[10];
            _tail_fill_indices = new short[6];
        }
        public override void PrepareForDraw(Box2D.Graphics.Context context)
        {
            base.PrepareForDraw(context);
            float offset = this.Material.SurfaceState.point_size;
            //build tail arrow vertices.
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

            //1-----0
            //-    -
            //  -  -
            //   - -2
            //  -  -
            //3-----4
            double sx, sy, sz;

            _tail_fill_vertices[0] = _vertices[0];
            _tail_fill_vertices[1] = _vertices[1];

            context.Unproject(tail_target2_x, tail_target2_y, 0, out sx, out sy, out sz);
            _tail_fill_vertices[2] = (float)sx;
            _tail_fill_vertices[3] = (float)sy;

            _tail_fill_vertices[4] = (float)start_x;
            _tail_fill_vertices[5] = (float)start_y;

            context.Unproject(tail_target1_x, tail_target1_y, 0, out sx, out sy, out sz);
            _tail_fill_vertices[6] = (float)sx;
            _tail_fill_vertices[7] = (float)sy;

            _tail_fill_vertices[8] = _vertices[2];
            _tail_fill_vertices[9] = _vertices[3];

            _tail_fill_indices[0] = 1;
            _tail_fill_indices[1] = 0;
            _tail_fill_indices[2] = 2;

            _tail_fill_indices[3] = 3;
            _tail_fill_indices[4] = 2;
            _tail_fill_indices[5] = 4;
        }
        public override void Render(Box2D.Graphics.SceneManager scene, Box2D.Graphics.Context context)
        {
            var color = this.Material.SurfaceState.color;
            //draw tail fill arrow.
            StaticBufferDrawHelper.DrawIndex(_tail_fill_vertices, _tail_fill_indices, color, PrimitiveType.Triangles, 6);
            base.Render(scene, context);
        }

        protected override void PrepareVertices()
        {
            base.PrepareVertices();

            //build tail arrow vertices.
            var pts = _line.Data;
            float offset = this.Material.SurfaceState.point_size; 
                
            //这里加入三个点形成箭尾
            double start_x = pts[0];
            double start_y = pts[1];
            double second_x = pts[2];
            double second_y = pts[3];

            double tail_vector_x = second_x - start_x;
            double tail_vector_y = second_y - start_y;

            double tail_theta1 = 145;
            double tail_theta2 = 215;
            double last_nx_length = 20000;
            double triangle_side = 2 *offset + 2 * last_nx_length;
            double tail_triangle_side = triangle_side + 10000;

            double cos_theta1 = Math.Cos(tail_theta1 * MathUtil.DEG_TO_RAD);
            double sin_theta1 = Math.Sin(tail_theta1 * MathUtil.DEG_TO_RAD);
            double cos_theta2 = Math.Cos(tail_theta2 * MathUtil.DEG_TO_RAD);
            double sin_theta2 = Math.Sin(tail_theta2 * MathUtil.DEG_TO_RAD);

            double tail_target1_vec_x = tail_vector_x * cos_theta1 - tail_vector_y * sin_theta1;
            double tail_target1_vec_y = tail_vector_x * sin_theta1 + tail_vector_y * cos_theta1;
            double tail_target2_vec_x = tail_vector_x * cos_theta2 - tail_vector_y * sin_theta2;
            double tail_target2_vec_y = tail_vector_x * sin_theta2 + tail_vector_y * cos_theta2;

            double tail_target_vec_len = Math.Sqrt(tail_vector_x * tail_vector_x + tail_vector_y * tail_vector_y);

            double tail_target1_x = start_x + tail_triangle_side * tail_target1_vec_x / tail_target_vec_len;
            double tail_target1_y = start_y + tail_triangle_side * tail_target1_vec_y / tail_target_vec_len;
            double tail_target2_x = start_x + tail_triangle_side * tail_target2_vec_x / tail_target_vec_len;
            double tail_target2_y = start_y + tail_triangle_side * tail_target2_vec_y / tail_target_vec_len;

            //1-----0
            //-    -
            //  -  -
            //   - -2
            //  -  -
            //3-----4
            _tail_fill_vertices[0] = _vertices[0];
            _tail_fill_vertices[1] = _vertices[1];

            _tail_fill_vertices[2] = (float)tail_target2_x;
            _tail_fill_vertices[3] = (float)tail_target2_y;

            _tail_fill_vertices[4] = (float)start_x;
            _tail_fill_vertices[5] = (float)start_y;

            _tail_fill_vertices[6] = (float)tail_target1_x;
            _tail_fill_vertices[7] = (float)tail_target1_y;

            _tail_fill_vertices[8] = _vertices[2];
            _tail_fill_vertices[9] = _vertices[3];

            _tail_fill_indices[0] = 1;
            _tail_fill_indices[1] = 0;
            _tail_fill_indices[2] = 2;

            _tail_fill_indices[3] = 3;
            _tail_fill_indices[4] = 2;
            _tail_fill_indices[5] = 4;
        }
      
    }
}
