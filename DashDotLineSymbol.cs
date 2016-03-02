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
    class DashDotLineSymbol : LabelLineSymbol
    {
        private List<int> _point_indices;
        private int _time;
        private int _dash_len;
        private int _dash_interval;
        public DashDotLineSymbol(Line line, int dash_len, int dash_interval)
            : base(line)
        {
            _vertices = new List<float>();
            _indices = new List<int>();
            _point_indices = new List<int>();
            _time = 0;
            _dash_len = dash_len;
            _dash_interval = dash_interval;
            isAddLabel = false;
        }
        public DashDotLineSymbol(Line line, string label, uint size, System.Drawing.Color label_color, LabelPosition position, bool rotation, int dash_len, int dash_interval, bool split = false)
            : base(line, label, size, label_color, position, rotation, split)
        {
            _vertices = new List<float>();
            _indices = new List<int>();
            _point_indices = new List<int>();
            _time = 0;
            _dash_len = dash_len;
            _dash_interval = dash_interval;
            isAddLabel = true;
        }
        public override void UpdateLineData()
        {
            base.UpdateLineData();
            _time = 0;
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

            var color = this.Material.SurfaceState.color;

            StaticBufferDrawHelper.DrawIndex(_vertices.ToArray(), _indices.ToArray(),
                color, PrimitiveType.Lines, _indices.Count);
            StaticBufferDrawHelper.DrawIndex(_vertices.ToArray(), _point_indices.ToArray(),
                color, PrimitiveType.Points, _point_indices.Count);

        }
        public void PrepareForDraw(Box2D.Graphics.Context context)
        {
            _vertices.Clear();
            _indices.Clear();
            _point_indices.Clear();
            double distance = 1.0;
            int dash_len = _dash_len;
            int dash_interval = _dash_interval;

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
                }
                sx0 = sx1;
                sy0 = sy1;
            }

            int count = _vertices.Count;
            int pt_num = (count >> 1);


            for (int i = 0; i < pt_num; i += (2 * dash_interval + dash_len))
            {
                int flag = 0;
                _indices.Add(i);
                for (int n = 1; n < dash_len; n++)
                {
                    if (i + n > pt_num - 1)
                    {
                        flag++;
                        break;
                    }
                    _indices.Add(n + i);
                    _indices.Add(n + i);
                }

                if (0 == flag && dash_len != 1 && i + dash_len < pt_num)
                    _indices.Add(i + dash_len);

                if(i + dash_interval + dash_len < pt_num)
                    _point_indices.Add(i + dash_interval + dash_len);
            }

            _time++;
        }
    }
}
