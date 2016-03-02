using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMA.MICAPS.Symbols
{
    using CMA.MICAPS.Box2D;
    using Line = CMA.MICAPS.Box2D.LineString2D<float>;
    class QiangJiangShuiArea:LineSymbol
    {
        protected List<float> _vertices;
        protected List<int> _indices;
        protected List<float> quad_vertices;
        protected List<int> quad_indices;
        protected List<int> line_indices;
        protected int _offset;
        public QiangJiangShuiArea(Line line,int offset)
          :base(line)
        {
            _vertices = new List<float>(_line.Data.Length << 1);
            _indices = new List<int>((_vertices.Capacity - 1) * 2);
            quad_vertices = new List<float>();
            quad_indices = new List<int>();
            line_indices = new List<int>();
            _offset = 20;
        }
        public override void Render(Box2D.Graphics.SceneManager scene, Box2D.Graphics.Context context)
        {
            context.SetRenderState(this.Material.SurfaceState);

            PrepareForDraw(context);
            PrepareIndices();

            var color =SymbolColor0;
            var color1 = SymbolColor1;
            //StaticBufferDrawHelper.DrawIndex(quad_vertices.ToArray(), quad_indices.ToArray(),
            //     color, PrimitiveType.Triangles, quad_indices.Count);
            StaticBufferDrawHelper.DrawIndex(_vertices.ToArray(), _indices.ToArray(),
                 color1, PrimitiveType.Lines, _indices.Count);

        }
        public virtual void PrepareForDraw(Box2D.Graphics.Context context)
        {
            //_vertices.Clear();
            //float[] pts = _line.Data;
            //int num = pts.Length;
            //float x = (pts[0] + pts[num - 4]) / 2;
            //float y = (pts[1] + pts[num - 3]) / 2;
            //_vertices.Add(x);
            //_vertices.Add(y);
            //for (int i = 0; i < num - 1; i++)
            //{
            //    _vertices.Add(pts[i]);
            //}
            ////取最大最小坐标
            //float x_min = pts[0], x_max = pts[0], y_min = pts[1], y_max = pts[1];
            //for (int i = 0; i < num; )
            //{
            //    if (pts[i] > x_max) x_max = pts[i];
            //    if (pts[i] < x_min) x_min = pts[i];
            //    if (pts[i + 1] > y_max) y_max = pts[i + 1];
            //    if (pts[i + 1] < y_min) y_min = pts[i + 1];
            //    i += 2;
            //}

            _vertices.Clear();

            float[] pts = _line.Data;
            int num = pts.Length;

            double sx0, sy0;
            double sx1, sy1;

            _vertices.Add(pts[0]);
            _vertices.Add(pts[1]);

            context.Project(pts[0], pts[1], 0, out sx0, out sy0);

            double pre_remain = 0.0;
            double total_len = 0.0;
            double vec_len = 0.0;

            for (int i = 2; i < num - 1; i += 2)
            {
                context.Project(pts[i], pts[i + 1], 0, out sx1, out sy1);
                double vx, vy;
                vx = sx1 - sx0;
                vy = sy1 - sy0;
                double nvx = -vy;
                double nvy = vx;
                double reverse_nvx = vy;
                double reverse_nvy = -vx;

                vec_len = Math.Sqrt(vx * vx + vy * vy);
                total_len = vec_len + pre_remain;

                if (total_len >= _offset)
                {
                    int insert_num = (int)(total_len / _offset);
                    for (int point_count = 0; point_count < insert_num; point_count++)
                    {
                        double insert_x = sx0 + ((point_count + 1) * _offset - pre_remain) * vx / vec_len;
                        double insert_y = sy0 + ((point_count + 1) * _offset - pre_remain) * vy / vec_len;

                        double sx, sy, sz;
                        context.Unproject(insert_x, insert_y, 0, out sx, out sy, out sz);
                        _vertices.Add((float)sx);
                        _vertices.Add((float)sy);

                    }
                    pre_remain = total_len - insert_num * _offset;
                }
                else
                {
                    pre_remain += vec_len;
                }
                sx0 = sx1;
                sy0 = sy1;
            }

            //_vertices.Clear();
            //quad_vertices.Clear();
            //float[] pts = _line.Data;
            //int num = pts.Length;

            ////取最大最小坐标
            //float x_min = pts[0], x_max = pts[0], y_min = pts[1], y_max = pts[1];
            //for (int i = 0; i < num; )
            //{
            //    if (pts[i] > x_max) x_max = pts[i];
            //    if (pts[i] < x_min) x_min = pts[i];
            //    if (pts[i + 1] > y_max) y_max = pts[i + 1];
            //    if (pts[i + 1] < y_min) y_min = pts[i + 1];
            //    i += 2;
            //}

            //float quadbottomleft_x = x_min;
            //float quadbottomleft_y = y_min;

            //float quadbottomright_x = x_max;
            //float quadbottomright_y = y_min;

            //float quadtopleft_x = x_min;
            //float quadtopleft_y = y_max;

            //float quadtopright_x = x_max;
            //float quadtopright_y = y_max;

            //double hx = quadbottomright_x - quadbottomleft_x;
            //double hy = quadbottomright_y - quadbottomleft_y;
            //double quadwidth = Math.Sqrt(hx * hx + hy * hy);
            //double vx = quadtopright_x - quadbottomright_x;
            //double vy = quadtopright_y - quadbottomright_y;
            //double quadheight = Math.Sqrt(vx * vx + vy * vy);
            //double triangle_side = quadwidth + quadheight;

            //quad_vertices.Add(quadbottomleft_x);
            //quad_vertices.Add(quadbottomleft_y);

            //quad_vertices.Add(quadbottomright_x);
            //quad_vertices.Add(quadbottomright_y);

            //quad_vertices.Add(quadtopleft_x);
            //quad_vertices.Add(quadtopleft_y);
            //quad_vertices.Add(quadtopleft_x);
            //quad_vertices.Add(quadtopleft_y);

            //quad_vertices.Add(quadbottomright_x);
            //quad_vertices.Add(quadbottomright_y);
            //quad_vertices.Add(quadtopright_x);
            //quad_vertices.Add(quadtopright_y);

            //int insert_num = (int)(triangle_side / _offset);
            //for (int j = 0; j < insert_num; j++)
            //{
            //    double insert_h_x = quadbottomright_x - (j + 1) * _offset;
            //    double insert_h_y = quadbottomright_y;

            //    _vertices.Add((float)insert_h_x);
            //    _vertices.Add((float)insert_h_y);

            //    double insert_v_x = quadbottomright_x;
            //    double insert_v_y = quadbottomright_y + (j + 1) * _offset;

            //    _vertices.Add((float)insert_v_x);
            //    _vertices.Add((float)insert_v_y);
            //}
        }
        public virtual void PrepareIndices()
        {
            _indices.Clear();
            quad_indices.Clear();
            line_indices.Clear();
            int num=_vertices.Count>>1;
            //for (int i = 0; i < 6; i++)
            //{
            //    quad_indices.Add((short)i);
            //}
            for (int j = 0; j < num; j++)
            {
                _indices.Add(j);
            }
            //if (num%2!=0)num=num-1;
            //for (int m = 0; m < num/2; m++)
            //{
            //    line_indices.Add((short)(2*m));
            //    line_indices.Add((short)(2*m + 1));
            //    line_indices.Add((short)(2 * num - 2));
            //    line_indices.Add((short)(2 * num - 1));
            //}
        }
    }
}
