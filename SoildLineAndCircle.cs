using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMA.MICAPS.Symbols
{
    using CMA.MICAPS.Box2D;
    using SandLib.Math3D;
    using Line = CMA.MICAPS.Box2D.LineString2D<float>;
    class SoildLineAndCircle:LineAndSoildCircle
    {
       private bool isAddQuad = false;
       private const int _sections = 10;
       private int _distance_original;
        public SoildLineAndCircle(Line line, string label, uint size, System.Drawing.Color label_color, LabelPosition position, bool rotation, int distance, int radius, bool split = false)
            : base(line, label, size, label_color, position, rotation,distance, radius,split)
        {
            _vertices = new List<float>(_line.Data.Length << 1);
            _indices = new List<int>((_vertices.Capacity - 1) * 2);
            _circle_vertices = new List<List<float>>();
            _circle_idices = new List<int>();
            //this._radius = radius;
            this._distance_original = distance;
            isAddLabel = true;
        }
        public SoildLineAndCircle(Line line, int distance, int radius)
        :base(line,distance,radius)
        {
            _vertices = new List<float>(_line.Data.Length << 1);
            _indices = new List<int>((_vertices.Capacity - 1) * 2);
            _circle_vertices = new List<List<float>>();
            _circle_idices = new List<int>();
            //this._radius = radius;
            this._distance_original = distance;
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

            StaticBufferDrawHelper.DrawIndex(_vertices.ToArray(), _indices.ToArray(),
                 color, PrimitiveType.Lines, _indices.Count);

            //this.Material.SurfaceState.line_width = 1;
            for (int i = 0; i <_circle_vertices.Count - 1; i++)
            {
                StaticBufferDrawHelper.DrawIndex(_circle_vertices[i].ToArray(), _circle_idices.ToArray(), color, PrimitiveType.TriangleFan,_circle_idices.Count);
            }
        }

        public override void PrepareForDraw(Box2D.Graphics.Context context)
        {

            _vertices.Clear();
            _circle_vertices.Clear();

            int radius = (int)this.Material.SurfaceState.point_size;
            _distance = 0;
            _distance = _distance_original + (int)this.Material.SurfaceState.point_size*2;

            float[] pts = _line.Data;
            int num = pts.Length;

            double sx0, sy0;
            double sx1, sy1;

            _vertices.Add(pts[0]);
            _vertices.Add(pts[1]);

            context.Project(pts[0], pts[1], 0, out sx0, out sy0);

            isAddQuad = true;

            double pre_remain = 0.0;
            double total_len = 0.0;
            double vec_len = 0.0;

            for (int i = 2; i < num - 1; i += 2)
            {
                context.Project(pts[i], pts[i + 1], 0, out sx1, out sy1);
                double vx, vy;
                vx = sx1 - sx0;
                vy = sy1 - sy0;

                double nx = -vy;
                double ny = vx;

                vec_len = Math.Sqrt(vx * vx + vy * vy);
                total_len = vec_len + pre_remain;

                if (total_len >= _distance)
                {
                    int insert_num = (int)(total_len / _distance);
                    for (int point_count = 0; point_count < insert_num; point_count++)
                    {
                        double insert_x = sx0 + ((point_count + 1) * _distance - pre_remain) * vx / vec_len;
                        double insert_y = sy0 + ((point_count + 1) * _distance - pre_remain) * vy / vec_len;

                        int offset = _distance / 2;

                        double circle_center_x = insert_x + offset * vx / vec_len;
                        double circle_center_y = insert_y + offset * vy/ vec_len ;
                        
                        double sx, sy, sz, circle_sx, circle_sy, circle_sz;
                        context.Unproject(insert_x, insert_y, 0, out sx, out sy, out sz);
                        _vertices.Add((float)sx);
                        _vertices.Add((float)sy);

                        if (isAddQuad)
                        {
                            List<float> circle = new List<float>();
                            context.Unproject(circle_center_x, circle_center_y, 0, out sx,out sy, out sz);
                            circle.Add((float)sx);
                            circle.Add((float)sy);
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
                            isAddQuad = true;
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

        }

        public override void PrepareIndices()
        {
            _indices.Clear();
            _circle_idices.Clear();
            int pt_num = (_vertices.Count) >> 1;
            int circleNum = _sections + 2;
            for (int i = 0; i < pt_num; i++)
            {
                _indices.Add(i);
            }

            for (int x = 0; x < circleNum; x++)
            {
               _circle_idices.Add(x);
            }
        }
    }
}
