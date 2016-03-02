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
    class FenLiuQu : LabelLineSymbol
    {
        private int _distance;
        private int _distance_original;
        private bool isAddCircle = false;
        private const int _sections = 10;
        private List<float> _triangle_vertices;
        private List<short> _triangle_idices;

        public FenLiuQu(Line line, string label, uint size, System.Drawing.Color label_color, LabelPosition position, bool rotation, int distance, int amplitude, bool split = false)
            : base(line,label,size,label_color,position,rotation,split)
        {
            _vertices = new List<float>(_line.Data.Length << 1);
            _indices = new List<int>((_vertices.Capacity - 1) * 2);
            _triangle_vertices = new List<float>();
            _triangle_idices = new List<short>();
            this._distance_original = distance;
            this.Material.SurfaceState.point_size = amplitude;
            isAddLabel = true;
        }
        public FenLiuQu(Line line, int distance, int amplitude)
            : base(line)
        {
            _vertices = new List<float>(_line.Data.Length << 1);
            _indices = new List<int>((_vertices.Capacity - 1) * 2);
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

            PrepareForDrawCut(context);
            PrepareIndices();

            var color = this.Material.SurfaceState.color;

            StaticBufferDrawHelper.DrawIndex(_vertices.ToArray(), _indices.ToArray(),
                 color, PrimitiveType.LineStrip,_indices.Count);
            StaticBufferDrawHelper.DrawIndex(_triangle_vertices.ToArray(), _triangle_idices.ToArray(),
                color, PrimitiveType.Triangles, _triangle_idices.Count);
        }
        private void PrepareForDraw(Box2D.Graphics.Context context)
        {
            _vertices.Clear();
            _triangle_vertices.Clear();

            float amplitude = this.Material.SurfaceState.point_size;
            _distance = 0;
            _distance = _distance_original ;

            float[] pts = _line.Data;
            int num = pts.Length;

            double sx0, sy0;
            double sx1, sy1;

            _vertices.Add(pts[0]);
            _vertices.Add(pts[1]);

            context.Project(pts[0], pts[1], 0, out sx0, out sy0);

            isAddCircle = true;

            double pre_remain = 0.0;
            double total_len = 0.0;
            double vec_len = 0.0;

            int flag = pts[num - 2] > pts[0] ? 1 : -1;
            bool isLinePoint = false; //判断最后添加的点是不是在线上
            
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
                double sx, sy, sz;
                if (total_len >= _distance)
                {
                    int insert_num = (int)(total_len / _distance);
                    for (int point_count = 0; point_count < insert_num; point_count++)
                    {
                        double insert_x = sx0 + ((point_count + 1) * _distance - pre_remain) * vx / vec_len;
                        double insert_y = sy0 + ((point_count + 1) * _distance - pre_remain) * vy / vec_len;

                        double offset_x = insert_x + amplitude * nx*flag / vec_len;
                        double offset_y = insert_y + amplitude * ny *flag/ vec_len;

                        if (isAddCircle)
                        {
                            context.Unproject(offset_x, offset_y, 0, out sx, out sy, out sz);
                            _vertices.Add((float)sx);
                            _vertices.Add((float)sy);
                            flag = -flag;
                            isAddCircle = false;
                            isLinePoint = false;
                        }
                        else
                        {
                            context.Unproject(insert_x, insert_y, 0, out sx, out sy, out sz);
                            _vertices.Add((float)sx);
                            _vertices.Add((float)sy);
                            isAddCircle = true;
                            isLinePoint = true;
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

     

            //处理箭头
            int pt_num = _vertices.Count;

            double last_nx_length = 5;
            int _offset = 5;
            double triangle_side = _offset + last_nx_length;
            int triangle_angle = 60;
         
            double m_x, m_y, m_z;
            if (!isLinePoint)
            {
                if (pt_num > 4)
                {
                    //移除最后一个点
                    _vertices.RemoveAt(pt_num - 1);
                    _vertices.RemoveAt(pt_num - 2);
                }
            }

            double last_x1, last_y1, last_x2, last_y2;
            int pt_num_new = _vertices.Count;
            double _vertices_last_x = _vertices[pt_num_new - 2];
            double _vertices_last_y = _vertices[pt_num_new - 1];
            context.Project(_vertices_last_x, _vertices_last_y, 0, out last_x1, out last_y1);
            //初始点里的最后一个点
            double _line_last_x = pts[num - 2];
            double _line_last_y = pts[num - 1];
            context.Project(_line_last_x, _line_last_y, 0, out last_x2, out last_y2);

            double lasx = last_x2 - last_x1;
            double lasy = last_y2 - last_y1;
            double las_len = Math.Sqrt(lasx * lasx + lasy * lasy);

            double make_last_x_new = last_x1 + las_len * lasx / las_len;
            double make_last_y_new = last_y1 + las_len * lasy / las_len;

            context.Unproject(make_last_x_new, make_last_y_new, 0, out m_x, out m_y, out m_z);
            _vertices.Add((float)m_x);
            _vertices.Add((float)m_y);


            double point_vx = triangle_side * Math.Sin(triangle_angle * MathUtil.DEG_TO_RAD) * lasx / las_len;
            double point_vy = triangle_side * Math.Sin(triangle_angle * MathUtil.DEG_TO_RAD) * lasy / las_len;

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
        private void PrepareForDrawCut(Box2D.Graphics.Context context)
        {
            _vertices.Clear();
            _triangle_vertices.Clear();

            float amplitude = this.Material.SurfaceState.point_size;
            _distance = 0;
            _distance = _distance_original+(int)amplitude;

            //返回裁剪的起始点和终止点
            cut_index pos = FindCutPosition(context);

            double sx0, sy0;
            double sx1, sy1;

            _vertices.Add(pos.pts[pos.start]);
            _vertices.Add(pos.pts[pos.start+1]);

            context.Project(pos.pts[pos.start], pos.pts[pos.start+1], 0, out sx0, out sy0);

            isAddCircle = true;

            double pre_remain = 0.0;
            double total_len = 0.0;
            double vec_len = 0.0;

            int flag = _line.Data[_line.Data.Length - 2] > _line.Data[0] ? 1 : -1;
            bool isLinePoint = false; //判断最后添加的点是不是在线上

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
                double sx, sy, sz;
                if (total_len >= _distance)
                {
                    int insert_num = (int)(total_len / _distance);
                    for (int point_count = 0; point_count < insert_num; point_count++)
                    {
                        double insert_x = sx0 + ((point_count + 1) * _distance - pre_remain) * vx / vec_len;
                        double insert_y = sy0 + ((point_count + 1) * _distance - pre_remain) * vy / vec_len;

                        double offset_x = insert_x + amplitude * nx * flag / vec_len;
                        double offset_y = insert_y + amplitude * ny * flag / vec_len;

                        if (isAddCircle)
                        {
                            context.Unproject(offset_x, offset_y, 0, out sx, out sy, out sz);
                            _vertices.Add((float)sx);
                            _vertices.Add((float)sy);
                            flag = -flag;
                            isAddCircle = false;
                            isLinePoint = false;
                        }
                        else
                        {
                            context.Unproject(insert_x, insert_y, 0, out sx, out sy, out sz);
                            _vertices.Add((float)sx);
                            _vertices.Add((float)sy);
                            isAddCircle = true;
                            isLinePoint = true;
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



            //处理箭头
            int pt_num = _vertices.Count;

            double last_nx_length = 5;
            int _offset = 5;
            double triangle_side = _offset + last_nx_length;
            int triangle_angle = 60;

            double m_x, m_y, m_z;
            if (!isLinePoint)
            {
                if (pt_num > 4)
                {
                    //移除最后一个点
                    _vertices.RemoveAt(pt_num - 1);
                    _vertices.RemoveAt(pt_num - 2);
                }
            }

            double last_x1, last_y1, last_x2, last_y2;
            int pt_num_new = _vertices.Count;
            double _vertices_last_x = _vertices[pt_num_new - 2];
            double _vertices_last_y = _vertices[pt_num_new - 1];
            context.Project(_vertices_last_x, _vertices_last_y, 0, out last_x1, out last_y1);
            //初始点里的最后一个点
            double _line_last_x = pos.pts[pos.pts.Length - 2];
            double _line_last_y = pos.pts[pos.pts.Length - 1];
            context.Project(_line_last_x, _line_last_y, 0, out last_x2, out last_y2);

            double lasx = last_x2 - last_x1;
            double lasy = last_y2 - last_y1;
            double las_len = Math.Sqrt(lasx * lasx + lasy * lasy);

            double make_last_x_new = last_x1 + las_len * lasx / las_len;
            double make_last_y_new = last_y1 + las_len * lasy / las_len;

            context.Unproject(make_last_x_new, make_last_y_new, 0, out m_x, out m_y, out m_z);
            _vertices.Add((float)m_x);
            _vertices.Add((float)m_y);


            double point_vx = triangle_side * Math.Sin(triangle_angle * MathUtil.DEG_TO_RAD) * lasx / las_len;
            double point_vy = triangle_side * Math.Sin(triangle_angle * MathUtil.DEG_TO_RAD) * lasy / las_len;

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
        private void PrepareIndices()
        {
            _indices.Clear();
            _triangle_idices.Clear();
            int pt_num = (_vertices.Count) >> 1;
            int tri_num=_triangle_vertices.Count>>1;
            for (int i = 0; i < pt_num; i++)
            {
                _indices.Add(i);
            }

            for (int j = 0; j < tri_num; j++)
            {
                _triangle_idices.Add((short)j);
            }
        }
    }
}
