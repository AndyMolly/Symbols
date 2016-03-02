using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMA.MICAPS.Symbols
{
    using CMA.MICAPS.Box2D;
    using Line = CMA.MICAPS.Box2D.LineString2D<float>;
    
    class JiHeLiu:QiangJiangShuiArea
    {
        public JiHeLiu(Line line,int offset)
            :base(line,offset)
        {
           
        }
        public override void PrepareForDraw(Box2D.Graphics.Context context)
        {
            _vertices.Clear();
            quad_vertices.Clear();
            float[] pts = _line.Data;
            int num = pts.Length;

            //取最大最小坐标
            float x_min = pts[0], x_max = pts[0], y_min = pts[1], y_max = pts[1];
            for (int i = 0; i < num; )
            {
                if (pts[i] > x_max) x_max = pts[i];
                if (pts[i] < x_min) x_min = pts[i];
                if (pts[i + 1] > y_max) y_max = pts[i + 1];
                if (pts[i + 1] < y_min) y_min = pts[i + 1];
                i += 2;
            }

            float quadbottomleft_x = x_min;
            float quadbottomleft_y = y_min;

            float quadbottomright_x = x_max;
            float quadbottomright_y = y_min;

            float quadtopleft_x = x_min;
            float quadtopleft_y = y_max;

            float quadtopright_x = x_max;
            float quadtopright_y = y_max;

            double hx = quadbottomright_x - quadbottomleft_x;
            double hy = quadbottomright_y - quadbottomleft_y;
            double quadwidth = Math.Sqrt(hx * hx + hy * hy);
            double vx = quadtopright_x - quadbottomright_x;
            double vy = quadtopright_y - quadbottomright_y;
            double quadheight = Math.Sqrt(vx * vx + vy * vy);
            double triangle_side = quadwidth + quadheight;

            quad_vertices.Add(quadbottomleft_x);
            quad_vertices.Add(quadbottomleft_y);

            quad_vertices.Add(quadbottomright_x);
            quad_vertices.Add(quadbottomright_y);

            quad_vertices.Add(quadtopleft_x);
            quad_vertices.Add(quadtopleft_y);
            quad_vertices.Add(quadtopleft_x);
            quad_vertices.Add(quadtopleft_y);

            quad_vertices.Add(quadbottomright_x);
            quad_vertices.Add(quadbottomright_y);
            quad_vertices.Add(quadtopright_x);
            quad_vertices.Add(quadtopright_y);

            int insert_num = (int)(triangle_side / _offset);
            for (int j = 0; j < insert_num; j++)
            {
                double insert_h_x = quadbottomright_x - (j + 1) * _offset;
                double insert_h_y = quadbottomright_y;

                _vertices.Add((float)insert_h_x);
                _vertices.Add((float)insert_h_y);

                double insert_v_x = quadbottomright_x;
                double insert_v_y = quadbottomright_y + (j + 1) * _offset;

                _vertices.Add((float)insert_v_x);
                _vertices.Add((float)insert_v_y);

                double reverse_h_x = quadbottomleft_x + (j + 1) * _offset;
                double reverse_h_y = quadbottomleft_y;
                _vertices.Add((float)reverse_h_x);
                _vertices.Add((float)reverse_h_y);

                double reverse_v_x = quadbottomleft_x;
                double reverse_v_y = quadbottomleft_y + (j + 1) * _offset;

                _vertices.Add((float)reverse_v_x);
                _vertices.Add((float)reverse_v_y);
            }
           
        }
    }
}
