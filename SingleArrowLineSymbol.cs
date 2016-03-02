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
    class SingleArrowLineSymbol : LabelLineSymbol
    {
        public SingleArrowLineSymbol(Line line)
            : base(line)
        {
            _vertices = new List<float>();
            _indices = new List<int>();       
            isAddLabel = false;
        }
        public SingleArrowLineSymbol(Line line, string label, uint size, System.Drawing.Color label_color, LabelPosition position, bool rotation,  bool split = false)
            : base(line,label,size,label_color,position,rotation,split)
        {
            _vertices = new List<float>();
            _indices = new List<int>(); 
            isAddLabel = true;
        }
        public override void UpdateLineData()
        {
            base.UpdateLineData();          
        }

        public override void Render(Box2D.Graphics.SceneManager scene, Box2D.Graphics.Context context)
        {
            Matrix4 mat;
            context.SetRenderState(this.Material.SurfaceState);
            _line_drawable.Draw(PrimitiveType.LineStrip, 0, _point_count);
            //_line_drawable.Draw(System.Drawing.Color.Green, PrimitiveType.Points, 0, _point_count);
            //_point_count = number of DataArray

            //if (0 == _time)
            //{
                PrepareForDraw(context);
            //}

            //StaticBufferDrawHelper.DrawIndex(_final.ToArray(), _indices.ToArray(), System.Drawing.Color.Red, PrimitiveType.LineLoop, _indices.Count);
            StaticBufferDrawHelper.DrawIndex(_vertices.ToArray(),
                _indices.ToArray(), this.Material.SurfaceState.color, 
                PrimitiveType.Triangles, _indices.Count);
            //StaticBufferDrawHelper.DrawIndex(_reverse_vertices.ToArray(), _indices.ToArray(), System.Drawing.Color.Red, PrimitiveType.LineStrip, _indices.Count);
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
        }
        public void PrepareForDraw(Box2D.Graphics.Context context)
        {
            _vertices.Clear();
            _indices.Clear();
            float[] pts = _line.Data;
            int num = pts.Length;
            float penult_x = pts[num - 4];
            float penult_y = pts[num - 3];
            float last_x = pts[num - 2];
            float last_y = pts[num - 1];

            double sx0, sx1, sy0, sy1;
            context.Project(penult_x, penult_y, 0, out sx0, out sy0);
            context.Project(last_x, last_y, 0, out sx1, out sy1);

            double vx = sx1 - sx0;
            double vy = sy1 - sy0;

            double vec_len = Math.Sqrt(vx * vx + vy * vy);

            int triangle_side = 25;
            int triangle_angle = 10;

            double arrow_height = triangle_side * Math.Cos(triangle_angle * MathUtil.DEG_TO_RAD);
            double vec_height_x = arrow_height * vx / vec_len;
            double vec_height_y = arrow_height * vy / vec_len;
            double arrow_x = vec_height_x + sx1;
            double arrow_y = vec_height_y + sy1;

            double theta1 = 170;
            double theta2 = 190;

            double cos_theta1 = Math.Cos(theta1 * MathUtil.DEG_TO_RAD);
            double sin_theta1 = Math.Sin(theta1 * MathUtil.DEG_TO_RAD);
            double cos_theta2 = Math.Cos(theta2 * MathUtil.DEG_TO_RAD);
            double sin_theta2 = Math.Sin(theta2 * MathUtil.DEG_TO_RAD);

            double target1_vec_x = vec_height_x * cos_theta1 - vec_height_y * sin_theta1;
            double target1_vec_y = vec_height_x * sin_theta1 + vec_height_y * cos_theta1;
            double target2_vec_x = vec_height_x * cos_theta2 - vec_height_y * sin_theta2;
            double target2_vec_y = vec_height_x * sin_theta2 + vec_height_y * cos_theta2;

            double target_vec_len = Math.Sqrt(target1_vec_x * target1_vec_x + target1_vec_y * target1_vec_y);

            double target1_x = arrow_x + triangle_side * target1_vec_x / target_vec_len;
            double target1_y = arrow_y + triangle_side * target1_vec_y / target_vec_len;
            double target2_x = arrow_x + triangle_side * target2_vec_x / target_vec_len;
            double target2_y = arrow_y + triangle_side * target2_vec_y / target_vec_len;

            double s_top_point_x, s_top_point_y, s_top, starget1_x, starget1_y, starget2_x, starget2_y;
            context.Unproject(arrow_x, arrow_y, 0, out s_top_point_x, out s_top_point_y, out s_top);
            context.Unproject(target1_x, target1_y, 0, out starget1_x, out starget1_y, out s_top);
            context.Unproject(target2_x, target2_y, 0, out starget2_x, out starget2_y, out s_top);

            _vertices.Add((float)starget2_x);
            _vertices.Add((float)starget2_y);
            _vertices.Add((float)s_top_point_x);
            _vertices.Add((float)s_top_point_y);
            _vertices.Add((float)starget1_x);
            _vertices.Add((float)starget1_y);

            for (int i = 0; i < 3; i++)
            {
                _indices.Add(i);
            }
        }
    }
}
