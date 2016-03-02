using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMA.MICAPS.Symbols
{
    using CMA.MICAPS.Box2D;
    using SandLib.Math3D;
    using Line = CMA.MICAPS.Box2D.LineString2D<float>;
    class LineAndCross : LabelLineSymbol
    {
        private int _offset = 0;
        private int _distance;
        private int _distance_original;
        private bool isAddQuad = false;
        private List<float> _quard_vertices;
        private List<int> _quad_idices;
        private float _line_width = 0;
        public LineAndCross(Line line, string label, uint size, System.Drawing.Color label_color, LabelPosition position, bool rotation, int distance, int cubeLength, bool split = false)
            : base(line, label, size, label_color, position, rotation, split)
        {
            _vertices = new List<float>(_line.Data.Length << 1);
            _indices = new List<int>((_vertices.Capacity - 1) * 2);
            _quad_idices = new List<int>();
            _distance_original = distance;
            this.Material.SurfaceState.point_size = cubeLength;
            _quard_vertices = new List<float>();
            isAddLabel = true;
        }
        public LineAndCross(Line line, int distance, int cubeLength)
            : base(line)
        {
            _vertices = new List<float>(_line.Data.Length << 1);
            _indices = new List<int>((_vertices.Capacity - 1) * 2);
            _quad_idices = new List<int>();
            _distance_original = distance;
            this.Material.SurfaceState.point_size = cubeLength;
           
            _quard_vertices = new List<float>();
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

            _line_width = this.Material.SurfaceState.line_width;

            PrepareForDraw(context);
            PrepareIndices();

            var color = this.Material.SurfaceState.color;

            this.Material.SurfaceState.line_width = 2;
            context.SetRenderState(this.Material.SurfaceState);
            StaticBufferDrawHelper.DrawIndex(_quard_vertices.ToArray(), _quad_idices.ToArray(), color, PrimitiveType.Lines, _quad_idices.Count);

            this.Material.SurfaceState.line_width = _line_width;
            context.SetRenderState(this.Material.SurfaceState);
            var ibuffer = _indices.ToArray();
            StaticBufferDrawHelper.DrawIndex(_vertices.ToArray(), ibuffer,
                color, PrimitiveType.Lines, _indices.Count);
         
        }

        public void PrepareForDraw(Box2D.Graphics.Context context)
        {
            _vertices.Clear();
            _quard_vertices.Clear();
            float cubeLength = this.Material.SurfaceState.point_size;

            _distance = 0;
            _distance = _distance_original + (int)this.Material.SurfaceState.point_size;

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

                        float offset = (_distance - cubeLength) / 2;

                        double quadtopleft_x = insert_x+offset*vx/vec_len;
                        double quadtopleft_y = insert_y+offset*vy/vec_len ;
                        double quadbottomleft_x = quadtopleft_x + cubeLength * nx / vec_len;
                        double quadbottomleft_y = quadtopleft_y +cubeLength *ny/vec_len;
                        double quadtopright_x = insert_x + (offset+cubeLength)*vx/vec_len;
                        double quadtopright_y = insert_y + (offset + cubeLength) * vy / vec_len;
                        double quadbottomright_x = quadtopright_x+cubeLength*nx/vec_len;
                        double quadbottomright_y = quadtopright_y+cubeLength*ny/vec_len;

                        double sx, sy, sz, quad_x, quad_y, quad_z;
                        if (isAddQuad)
                        {
                            context.Unproject(insert_x, insert_y, 0, out sx, out sy, out sz);
                            _vertices.Add((float)sx);
                            _vertices.Add((float)sy);

                            context.Unproject(quadbottomleft_x, quadbottomleft_y, 0, out quad_x, out quad_y, out quad_z);
                            _quard_vertices.Add((float)quad_x);
                            _quard_vertices.Add((float)quad_y);

                            context.Unproject(quadtopright_x, quadtopright_y, 0, out quad_x, out quad_y, out quad_z);
                            _quard_vertices.Add((float)quad_x);
                            _quard_vertices.Add((float)quad_y);

                            context.Unproject(quadtopleft_x, quadtopleft_y, 0, out quad_x, out quad_y, out quad_z);
                            _quard_vertices.Add((float)quad_x);
                            _quard_vertices.Add((float)quad_y);

                            context.Unproject(quadbottomright_x, quadbottomright_y, 0, out quad_x, out quad_y, out quad_z);
                            _quard_vertices.Add((float)quad_x);
                            _quard_vertices.Add((float)quad_y);
                            isAddQuad = false;
                        }
                        else
                        {
                            context.Unproject(insert_x, insert_y, 0, out sx, out sy, out sz);
                            _vertices.Add((float)sx);
                            _vertices.Add((float)sy);
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
        protected virtual void PrepareVertices()
        {
            _vertices.Clear();

            int offset = _offset;
            float cubeLength = this.Material.SurfaceState.point_size;

            float[] pts = _line.Data;
            int num = pts.Length;

            float x0 = pts[0];
            float y0 = pts[1];
            float x1 = pts[2];
            float y1 = pts[3];

            float vx = x1 - x0;
            float vy = y1 - y0;

            float nx = vy;
            float ny = -vx;

            double len = Math.Sqrt(nx * nx + ny * ny);

            double tx = x0 + offset * nx / len;
            double ty = y0 + offset * ny / len;         

            _vertices.Add((float)tx);
            _vertices.Add((float)ty);
            isAddQuad = true;

            float nnx = nx, nny = ny;
            for (int i = 2; i <num-2 ; i += 2)
            {

                float curr_x = pts[i];
                float curr_y = pts[i + 1];
                float next_x = pts[i + 2];
                float next_y = pts[i + 3];

                nnx = next_y - curr_y;
                nny = -(next_x - curr_x);

                float inx = nnx + nx;
                float iny = nny + ny;

                len = Math.Sqrt(inx * inx + iny * iny);

                tx = curr_x + offset * inx / len;
                ty = curr_y + offset * iny / len;

                double quadtopleft_x = tx;
                double quadtopleft_y = ty + cubeLength / 2;
                double quadbottomleft_x = tx;
                double quadbottomleft_y = ty - cubeLength / 2;
                double quadtopright_x = tx + cubeLength;
                double quadtopright_y = ty + cubeLength / 2;
                double quadbottomright_x = tx + cubeLength;
                double quadbottomright_y = ty - cubeLength / 2;

                if (isAddQuad)
                {
                    _vertices.Add((float)tx);
                    _vertices.Add((float)ty);
                    _vertices.Add((float)quadbottomleft_x);
                    _vertices.Add((float)quadbottomleft_y);
                    _vertices.Add((float)quadtopright_x);
                    _vertices.Add((float)quadtopright_y);
                    _vertices.Add((float)quadtopleft_x);
                    _vertices.Add((float)quadtopleft_y);
                    _vertices.Add((float)quadbottomright_x);
                    _vertices.Add((float)quadbottomright_y);
                    isAddQuad = false;
                }else
                {
                    _vertices.Add((float)tx);
                    _vertices.Add((float)ty);
                    isAddQuad = true;
                }

                nx = nnx;
                ny = nny;
                
            }
        }

        protected virtual void PrepareIndices()
        {
            _indices.Clear();
            _quad_idices.Clear();
            int pt_num = (_vertices.Count) >> 1;
            int quadNum = (_quard_vertices.Count) >> 1;

            for (int i = 0; i < pt_num; i++)
            {
                _indices.Add(i);

            }
            for (int j = 0; j < quadNum; j++)
            {
                _quad_idices.Add(j);
            }
        }
    }
}
