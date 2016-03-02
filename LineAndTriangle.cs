using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMA.MICAPS.Symbols
{
    using CMA.MICAPS.Box2D;
    using SandLib.Math3D;
    using Line = CMA.MICAPS.Box2D.LineString2D<float>;
    class LineAndTriangle : LabelLineSymbol
    {
        private int _distance;
        private int _distance_original;
        private bool isAddQuad = false;
        private List<float> _triangle_vertices;
        private List<int> _triangle_idices;
        public LineAndTriangle(Line line, string label, uint size, System.Drawing.Color label_color, LabelPosition position, bool rotation, int distance, int offset, bool split = false)
            : base(line,label,size,label_color,position,rotation,split)
        {
            _vertices = new List<float>(_line.Data.Length << 1);
            _indices = new List<int>((_vertices.Capacity - 1) * 2);
            _triangle_idices = new List<int>();
            _triangle_vertices = new List<float>();
            _distance_original = distance;
            this.Material.SurfaceState.point_size=offset;
            isAddLabel = true;
        }
        public LineAndTriangle(Line line, int distance, int offset)
            : base(line)
        {
            _vertices = new List<float>(_line.Data.Length << 1);
            _indices = new List<int>((_vertices.Capacity - 1) * 2);
            _triangle_idices = new List<int>();
            _triangle_vertices = new List<float>();
            this._distance_original = distance;
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

            StaticBufferDrawHelper.DrawIndex(_triangle_vertices.ToArray(), _triangle_idices.ToArray(), color, PrimitiveType.Triangles, _triangle_idices.Count);
        }

        public void PrepareForDraw(Box2D.Graphics.Context context)
        {
            _triangle_vertices.Clear();

            float offset = this.Material.SurfaceState.point_size;

            _distance = 0;
            _distance = _distance_original + (int)(offset / Math.Sin(Math.PI / 3));

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

                double nx = vy;
                double ny = -vx;

                vec_len = Math.Sqrt(vx * vx + vy * vy);
                total_len = vec_len + pre_remain;

                if (total_len >= _distance)
                {
                    int insert_num = (int)(total_len / _distance);
                    for (int point_count = 0; point_count < insert_num; point_count++)
                    {
                        double insert_x = sx0 + ((point_count + 1) * _distance - pre_remain) * vx / vec_len;
                        double insert_y = sy0 + ((point_count + 1) * _distance - pre_remain) * vy / vec_len;

                        double triangle_side = offset / Math.Sin(Math.PI / 3);

                        double triangle1_x = insert_x + triangle_side * vx / vec_len;
                        double triangle1_y = insert_y + triangle_side * vy / vec_len;

                        double triangle_bottomcenter_x = insert_x + (triangle_side / 2) * vx / vec_len;
                        double triangle_bottomcenter_y = insert_y + (triangle_side / 2) * vy / vec_len;
                        
                        double triangle_top_x = triangle_bottomcenter_x + offset * nx / vec_len;
                        double triangle_top_y = triangle_bottomcenter_y + offset * ny / vec_len;
                     
                        double sx, sy, sz, triangle_x, triangle_y, triangle_z;
                        if (isAddQuad)
                        {
                             context.Unproject(insert_x, insert_y, 0, out sx, out sy, out sz);
                             _triangle_vertices.Add((float)sx);
                             _triangle_vertices.Add((float)sy);

                            context.Unproject(triangle1_x,triangle1_y,0,out triangle_x,out triangle_y,out triangle_z);
                            _triangle_vertices.Add((float)triangle_x);
                            _triangle_vertices.Add((float)triangle_y);

                            context.Unproject(triangle_top_x, triangle_top_y, 0, out triangle_x, out triangle_y, out triangle_z);
                            _triangle_vertices.Add((float)triangle_x);
                            _triangle_vertices.Add((float)triangle_y);
                            isAddQuad = false;
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

        public void PrepareIndices()
        {
            _triangle_idices.Clear();
            int triangleNum = _triangle_vertices.Count >> 1;

            for (int i = 0; i < triangleNum; i++)
            {
                _triangle_idices.Add(i);
            }
           
        }
    }
}
