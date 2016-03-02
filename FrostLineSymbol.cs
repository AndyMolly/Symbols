using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMA.MICAPS.Box2D.Util;

namespace CMA.MICAPS.Symbols
{
    using CMA.MICAPS.Box2D;
    using Line = CMA.MICAPS.Box2D.LineString2D<float>;
    using SandLib.Math3D;
    class FrostLineSymbol : LabelLineSymbol
    {
        public FrostLineSymbol(Line line, int distance)
            : base(line)
        {
            _vertices = new List<float>();
            _indices = new List<int>();
            this.Material.SurfaceState.point_size = distance;
            isAddLabel = false;
        }
  
        public FrostLineSymbol(Line line, string label, uint size, System.Drawing.Color label_color, LabelPosition position, bool rotation, int distance, bool split = false)
            : base(line, label, size, label_color, position, rotation, split)
        {
            _vertices = new List<float>();
            _indices = new List<int>();
            this.Material.SurfaceState.point_size = distance;
            isAddLabel = true;
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
            _line_drawable.Draw(PrimitiveType.LineStrip, 0, _point_count);

            PrepareForDraw(context);
 
            StaticBufferDrawHelper.DrawIndex(_vertices.ToArray(),
                _indices.ToArray(), this.Material.SurfaceState.color, 
                PrimitiveType.Lines, _indices.Count);
        }

        public void PrepareForDraw(Box2D.Graphics.Context context)
        {
            _vertices.Clear();
            _indices.Clear();

            float distance = this.Material.SurfaceState.point_size;

            float[] pts = _line.Data;
            int num = pts.Length;
            double sx0, sy0;
            double sx1, sy1;

            _vertices.Add(pts[0]);
            _vertices.Add(pts[1]);
            context.Project(pts[0], pts[1], 0, out sx0, out sy0);

            int flag = pts[num - 2] > pts[0] ? -1 : 1;

            double head_x0 = sx0;
            double head_y0 = sy0 + distance;
            double head_x1 = head_x0 + flag *(distance + 5);
            double head_y1 = head_y0;
            double head_x2 = head_x1;
            double head_y2 = sy0;

            double x,y,z;
            context.Unproject(head_x0, head_y0, 0, out x, out y, out z);
            _vertices.Add((float)x);
            _vertices.Add((float)y);
            _vertices.Add((float)x);
            _vertices.Add((float)y);
            context.Unproject(head_x1, head_y1, 0, out x, out y, out z);
            _vertices.Add((float)x);
            _vertices.Add((float)y);
            _vertices.Add((float)x);
            _vertices.Add((float)y);
            context.Unproject(head_x2, head_y2, 0, out x, out y, out z);
            _vertices.Add((float)x);
            _vertices.Add((float)y);

            _vertices.Add((float)(pts[num - 2]));
            _vertices.Add((float)(pts[num - 1]));
            context.Project(pts[num - 2], pts[num - 1], 0, out sx1, out sy1);

            double tail_x0 = sx1;
            double tail_y0 = sy1 + distance;
            double tail_x1 = tail_x0 - flag * (distance + 5);
            double tail_y1 = tail_y0;
            double tail_x2 = tail_x1;
            double tail_y2 = sy1;

            context.Unproject(tail_x0, tail_y0, 0, out x, out y, out z);
            _vertices.Add((float)x);
            _vertices.Add((float)y);
            _vertices.Add((float)x);
            _vertices.Add((float)y);
            context.Unproject(tail_x1, tail_y1, 0, out x, out y, out z);
            _vertices.Add((float)x);
            _vertices.Add((float)y);
            _vertices.Add((float)x);
            _vertices.Add((float)y);
            context.Unproject(tail_x2, tail_y2, 0, out x, out y, out z);
            _vertices.Add((float)x);
            _vertices.Add((float)y);


            double pre_remain = 0.0;
            double total_len = 0.0;
            double vec_len = 0.0;

            //循环一次，前进一段
            for (int i = 2; i < num - 1; i += 2)
            {
                context.Project(pts[i], pts[i + 1], 0, out sx1, out sy1);
                double vx, vy;
                vx = sx1 - sx0;
                vy = sy1 - sy0;

                vec_len = Math.Sqrt(vx * vx + vy * vy);
                total_len = vec_len + pre_remain;

                if (total_len > distance)
                {
                    int insert_num = (int)(total_len / distance);

                    for (int point_count = 0; point_count < insert_num; point_count++)
                    {
                        double insert_x = sx0 + ((point_count + 1) * distance - pre_remain) * vx / vec_len;
                        double insert_y = sy0 + ((point_count + 1) * distance - pre_remain) * vy / vec_len;

                        double sx, sy, sz;
                        context.Unproject(insert_x, insert_y, 0, out sx, out sy, out sz);
                        _vertices.Add((float)sx);
                        _vertices.Add((float)sy);

                        double nx = -vy;
                        double ny = vx;
                        double n_top_x = insert_x + distance * nx / vec_len;
                        double n_top_y = insert_y + distance * ny / vec_len;

                        context.Unproject(n_top_x, n_top_y, 0, out sx, out sy, out sz);
                        _vertices.Add((float)sx);
                        _vertices.Add((float)sy);
                    }
                    pre_remain = total_len - insert_num * distance;

                }
                else if (total_len < distance)
                {
                    pre_remain += vec_len;
                }
                else
                {
                    _vertices.Add(pts[i]);
                    _vertices.Add(pts[i + 1]);
                    pre_remain = 0.0;

                    double pre_x = pts[i - 2];
                    double pre_y = pts[i - 1];
                    double cur_x = pts[i];
                    double cur_y = pts[i + 1];

                    double spre_x, spre_y, scur_x, scur_y, snext_x, snext_y;
                    context.Project(pre_x, pre_y, 0, out spre_x, out spre_y);
                    context.Project(pts[i], pts[i + 1], 0, out scur_x, out scur_y);
                    //如果是最后一个点
                    if(i == num - 1)
                    {
                        double vec_x = scur_x - spre_x;
                        double vec_y = scur_y - spre_y;

                        double nvec_x = -vec_y;
                        double nvec_y = vec_x;

                        double last_vec_len = Math.Sqrt(nvec_x * nvec_x + nvec_y * nvec_y);

                        double ntop_x = scur_x + distance * nvec_x / last_vec_len;
                        double ntop_y = scur_y + distance * nvec_y / last_vec_len;

                        double lx, ly, lz;
                        context.Unproject(ntop_x, ntop_y, 0, out lx, out ly, out lz);
                        _vertices.Add((float)lx);
                        _vertices.Add((float)ly);
                    }
                    else
                    {
                        double next_x = pts[i + 2];
                        double next_y = pts[i + 3];
                        context.Project(next_x, next_y, 0, out snext_x, out snext_y);
                        double pre_vec_x = scur_x - spre_x;
                        double pre_vec_y = scur_y - spre_y;
                        double next_vec_x = snext_x - scur_x;
                        double next_vec_y = snext_y - scur_y;

                        double pre_nx = -pre_vec_y;
                        double pre_ny = pre_vec_x;
                        double next_nx = -next_vec_y;
                        double next_ny = next_vec_x;

                        double nx = next_nx + pre_nx;
                        double ny = next_ny + pre_ny;

                        double vec_n_len = Math.Sqrt(nx * nx + ny * ny);
                        double stop_nx = scur_x + distance * nx / vec_n_len;
                        double stop_ny = scur_y + distance * ny / vec_n_len;

                        double top_x, top_y, top_z;
                        context.Unproject(stop_nx, stop_ny, 0, out top_x, out top_y, out top_z);
                        _vertices.Add((float)top_x);
                        _vertices.Add((float)top_y);
                    }
                }
                sx0 = sx1;
                sy0 = sy1;
            }

            int count = _vertices.Count;
            int pt_num = (count >> 1);

            for (int i = 0; i < pt_num; i++)
            {
                _indices.Add(i);
            }
        }
    }
}
