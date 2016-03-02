using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMA.MICAPS.Symbols
{
    using CMA.MICAPS.Box2D;
    using CMA.MICAPS.Box2D.Util;
    using SandLib.Math3D;
    using Line = CMA.MICAPS.Box2D.LineString2D<float>;
    class ShiZhou : LabelLineSymbol
    {     
        private int _distance;
        private int _distance_original;
        private const int _sections = 10;
        private  List<float> _circle_vertices;
        private  List<int> _circle_idices;   
        private List<float> _triangle_vertices;
        private List<short> _triangle_idices;
        
        public ShiZhou(Line line, string label, uint size, System.Drawing.Color label_color, LabelPosition position, bool rotation, int distance, int amplitude, bool split = false)
            :base(line,label,size,label_color,position,rotation,split)
        {
            _vertices = new List<float>(_line.Data.Length << 1);
            _indices = new List<int>((_vertices.Capacity - 1) * 2);
            _circle_vertices = new List<float>();
            _circle_idices = new List<int>();
            _triangle_vertices = new List<float>();
            _triangle_idices = new List<short>();
            this._distance_original = distance;
            this.Material.SurfaceState.point_size = amplitude;
            isAddLabel = true;
        }
        public ShiZhou(Line line, int distance, int amplitude)
            : base(line)
        {
            _vertices = new List<float>(_line.Data.Length << 1);
            _indices = new List<int>((_vertices.Capacity - 1) * 2);
            _circle_vertices = new List<float>();
            _circle_idices = new List<int>();
            _triangle_vertices = new List<float>();
            _triangle_idices = new List<short>();
            this._distance_original = distance;
            this.Material.SurfaceState.point_size = amplitude;
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


            //PrepareForDraw(context);
            //PrepareIndices();

            PrepareForDrawCut(context);
            PrepareIndices();

            var color = this.Material.SurfaceState.color;
          
            StaticBufferDrawHelper.DrawIndex(_circle_vertices.ToArray(), _circle_idices.ToArray(), color, PrimitiveType.LineStrip, _circle_idices.Count);
            StaticBufferDrawHelper.DrawIndex(_triangle_vertices.ToArray(), _triangle_idices.ToArray(),
                   color, PrimitiveType.Triangles, _triangle_idices.Count);
        }
        public void PrepareForDraw(Box2D.Graphics.Context context)
        {
            _circle_vertices.Clear();
            _triangle_vertices.Clear();

            float amplitude = this.Material.SurfaceState.point_size;

            _distance = 0;
            _distance = _distance_original+(int)amplitude;

            float[] pts = _line.Data;
            int num = pts.Length;

            double sx0, sy0;
            double sx1, sy1;
            _circle_vertices.Add(pts[0]);
            _circle_vertices.Add(pts[1]);

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

                double nx = vy;
                double ny = -vx;

                vec_len = Math.Sqrt(vx * vx + vy * vy);
                total_len = vec_len + pre_remain;

                if (total_len >= _distance)
                {
                    int insert_num = (int)(total_len / _distance);
                    for (int point_count = 0; point_count < insert_num; point_count++)
                    {
                        double insert_x = sx0 + ((point_count) * _distance - pre_remain) * vx / vec_len;
                        double insert_y = sy0 + ((point_count) * _distance - pre_remain) * vy / vec_len;

                        double  circle_sx, circle_sy, circle_sz;

                            for (int count = 1; count <= _sections; count++)
                            {
                                double offset = amplitude * Math.Sin(count * 2*Math.PI / _sections);

                                double line_x = insert_x + (count * _distance * vx) / (_sections * vec_len);
                                double line_y = insert_y + (count * _distance * vy) / (_sections * vec_len);

                                double circle_x = line_x + nx * offset / vec_len;
                                double circle_y = line_y + ny * offset / vec_len;

                                context.Unproject(circle_x, circle_y, 0, out circle_sx, out circle_sy, out circle_sz);

                                _circle_vertices.Add((float)circle_sx);
                                _circle_vertices.Add((float)circle_sy);
                            }
                    }
                    pre_remain = total_len - insert_num * _distance;
                }
                else
                {
                    pre_remain += vec_len;
                }
                sx0 = sx1;
                sy0 = sy1;
            }

            //最后点新处理
            int pt_num = _circle_vertices.Count;
            double last_x1, last_y1, last_x2, last_y2;
             //添加点最后一个点
            double _circle_vertices_last_x = _circle_vertices[pt_num - 2];
            double _circle_vertices_last_y = _circle_vertices[pt_num - 1];
            context.Project(_circle_vertices_last_x, _circle_vertices_last_y, 0, out last_x1, out last_y1);
            //初始线上最后一个点
            double _line_last_x = pts[num - 2];
            double _line_last_y = pts[num - 1];
            context.Project(_line_last_x, _line_last_y, 0, out last_x2, out last_y2);
          

            double lastx = last_x2 - last_x1;
            double lasty = last_y2 - last_y1;

            double last_len = Math.Sqrt(lastx * lastx + lasty * lasty);

            double make_last_x_new = last_x1 + last_len * lastx / last_len;
            double make_last_y_new = last_y1 + last_len * lasty / last_len;
            double m_x, m_y, m_z;
            context.Unproject(make_last_x_new, make_last_y_new, 0, out m_x, out m_y, out m_z);
            _circle_vertices.Add((float)m_x);
            _circle_vertices.Add((float)m_y);
            
            //添加箭头
            double last_nx_length = 5;
            int _offset = 5;
            double triangle_side = _offset + last_nx_length;
            int triangle_angle = 60;  

            double point_vx = triangle_side * Math.Sin(triangle_angle * MathUtil.DEG_TO_RAD) * lastx /last_len;
            double point_vy = triangle_side * Math.Sin(triangle_angle * MathUtil.DEG_TO_RAD) * lasty / last_len;
            double top_point_x = make_last_x_new + point_vx;
            double top_point_y = make_last_y_new + point_vy;

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

            double tri_x, tri_y, tri_z;
            context.Unproject(target2_x, target2_y, 0, out tri_x, out tri_y, out tri_z);
            _triangle_vertices.Add((float)tri_x);
            _triangle_vertices.Add((float)tri_y);
            context.Unproject(top_point_x, top_point_y, 0, out tri_x, out tri_y, out tri_z);
            _triangle_vertices.Add((float)tri_x);
            _triangle_vertices.Add((float)tri_y);
            context.Unproject(target1_x, target1_y, 0, out tri_x, out tri_y, out tri_z);
            _triangle_vertices.Add((float)tri_x);
            _triangle_vertices.Add((float)tri_y);
           
            
        }
        
        /// <summary>
        /// 裁剪处理
        /// 只根据屏幕里点进行插值
        /// </summary>
        /// <param name="context"></param>
        private void PrepareForDrawCut(Box2D.Graphics.Context context)
        {
            _circle_vertices.Clear();
            _triangle_vertices.Clear();

            float amplitude = this.Material.SurfaceState.point_size;

            _distance = 0;
            _distance = _distance_original + (int)amplitude;

            //返回裁剪的起始点和终止点
            cut_index pos = FindCutPosition(context);

           
            double sx0, sy0;
            double sx1, sy1;
            //添加第一个点
            _circle_vertices.Add(pos.pts[pos.start]);
            _circle_vertices.Add(pos.pts[pos.start+1]);

            context.Project(pos.pts[pos.start], pos.pts[pos.start+1], 0, out sx0, out sy0);

            double pre_remain = 0.0;
            double total_len = 0.0;
            double vec_len = 0.0;
            for (int i = pos.start+2; i <= pos.end; i += 2)
            {
                context.Project(pos.pts[i], pos.pts[i + 1], 0, out sx1, out sy1);
                double vx, vy;
                vx = sx1 - sx0;
                vy = sy1 - sy0;

                double nx = vy;
                double ny = -vx;

                vec_len = Math.Sqrt(vx * vx + vy * vy);
                total_len = vec_len + pre_remain;

                if (total_len >= _distance)
                {
                    int insert_num = (int)(total_len / _distance);
                    for (int point_count = 0; point_count < insert_num; point_count++)
                    {
                        double insert_x = sx0 + ((point_count) * _distance - pre_remain) * vx / vec_len;
                        double insert_y = sy0 + ((point_count) * _distance - pre_remain) * vy / vec_len;

                        double circle_sx, circle_sy, circle_sz;

                        for (int count = 1; count <= _sections; count++)
                        {
                            double offset = amplitude * Math.Sin(count * 2 * Math.PI / _sections);

                            double line_x = insert_x + (count * _distance * vx) / (_sections * vec_len);
                            double line_y = insert_y + (count * _distance * vy) / (_sections * vec_len);

                            double circle_x = line_x + nx * offset / vec_len;
                            double circle_y = line_y + ny * offset / vec_len;

                            context.Unproject(circle_x, circle_y, 0, out circle_sx, out circle_sy, out circle_sz);

                            _circle_vertices.Add((float)circle_sx);
                            _circle_vertices.Add((float)circle_sy);
                        }
                    }
                    pre_remain = total_len - insert_num * _distance;
                }
                else
                {
                    pre_remain += vec_len;
                }
                sx0 = sx1;
                sy0 = sy1;
            }

            //最后点新处理
            int pt_num = _circle_vertices.Count;
            double last_x1, last_y1, last_x2, last_y2;
            //添加点最后一个点
            double _circle_vertices_last_x = _circle_vertices[pt_num - 2];
            double _circle_vertices_last_y = _circle_vertices[pt_num - 1];
            context.Project(_circle_vertices_last_x, _circle_vertices_last_y, 0, out last_x1, out last_y1);
            //初始线上最后一个点
            double _line_last_x = pos.pts[pos.end];
            double _line_last_y = pos.pts[pos.end+1];
            context.Project(_line_last_x, _line_last_y, 0, out last_x2, out last_y2);


            double lastx = last_x2 - last_x1;
            double lasty = last_y2 - last_y1;

            double last_len = Math.Sqrt(lastx * lastx + lasty * lasty);

            double make_last_x_new = last_x1 + last_len * lastx / last_len;
            double make_last_y_new = last_y1 + last_len * lasty / last_len;
            double m_x, m_y, m_z;
            context.Unproject(make_last_x_new, make_last_y_new, 0, out m_x, out m_y, out m_z);
            _circle_vertices.Add((float)m_x);
            _circle_vertices.Add((float)m_y);

            //添加箭头
            double last_nx_length = 5;
            int _offset = 5;
            double triangle_side = _offset + last_nx_length;
            int triangle_angle = 60;

            double point_vx = triangle_side * Math.Sin(triangle_angle * MathUtil.DEG_TO_RAD) * lastx / last_len;
            double point_vy = triangle_side * Math.Sin(triangle_angle * MathUtil.DEG_TO_RAD) * lasty / last_len;
            double top_point_x = make_last_x_new + point_vx;
            double top_point_y = make_last_y_new + point_vy;

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

            double tri_x, tri_y, tri_z;
            context.Unproject(target2_x, target2_y, 0, out tri_x, out tri_y, out tri_z);
            _triangle_vertices.Add((float)tri_x);
            _triangle_vertices.Add((float)tri_y);
            context.Unproject(top_point_x, top_point_y, 0, out tri_x, out tri_y, out tri_z);
            _triangle_vertices.Add((float)tri_x);
            _triangle_vertices.Add((float)tri_y);
            context.Unproject(target1_x, target1_y, 0, out tri_x, out tri_y, out tri_z);
            _triangle_vertices.Add((float)tri_x);
            _triangle_vertices.Add((float)tri_y);
           
        }
        public void PrepareIndices()
        {
            _circle_idices.Clear();
            _triangle_idices.Clear();
            int circleNum = _circle_vertices.Count >> 1;
            int tri_num = _triangle_vertices.Count >> 1;

            for (int x = 0; x < circleNum; x++)
            {
                _circle_idices.Add(x);
            }
            for (int j = 0; j < tri_num; j++)
            {
                _triangle_idices.Add((short)j);
            }
        }
     
    }
}
