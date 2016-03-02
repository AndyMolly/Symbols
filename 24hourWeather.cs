using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMA.MICAPS.Symbols
{
    using CMA.MICAPS.Box2D;
    using SandLib.Math3D;
    using Line = CMA.MICAPS.Box2D.LineString2D<float>;
    class _24hourWeather:ShuangGanAndShuangxian
    {
        private List<float> _center_vertices;
        private List<int> _center_indices;
        public _24hourWeather(Line line, string label, uint size, System.Drawing.Color label_color, LabelPosition position, bool rotation, int offset, int distance, bool split = false)
            : base(line, label, size, label_color, position, rotation, offset, distance, split)
        {
            _center_vertices = new List<float>();
            _center_indices = new List<int>();
            isAddLabel = true;
        }
        public _24hourWeather(Line line, int offset, int distance)
            : base(line, offset, distance)
        {
            _center_vertices = new List<float>();
            _center_indices = new List<int>();
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
          
            StaticBufferDrawHelper.DrawIndex(_vertices.ToArray(), _indices.ToArray(), color, PrimitiveType.Lines, _indices.Count);
            StaticBufferDrawHelper.DrawIndex(_center_vertices.ToArray(), _center_indices.ToArray(), color, PrimitiveType.Lines, _center_indices.Count); 
        }
        public override void PrepareForDraw(Box2D.Graphics.Context context)
        {
             base.PrepareForDraw(context);

             _center_vertices.Clear();
             if (_toatal_num % 2 == 0) _toatal_num = _toatal_num;
             else _toatal_num = _toatal_num - 1;

             for (int i = 0; i <_toatal_num; )
             {
                 float center_x = (_vertices[4*i] + _vertices[4*i + 4]) / 2;
                 float center_y = (_vertices[4 * i + 1] + _vertices[4 * i + 5]) / 2;
                 float reverse_cen_x = (_vertices[4 * i + 2] + _vertices[4 * i + 6]) / 2;
                 float reverse_cen_y = (_vertices[4 * i + 3] + _vertices[4 * i + 7]) / 2;
                 _center_vertices.Add(center_x);
                 _center_vertices.Add(center_y);
                 _center_vertices.Add(reverse_cen_x);
                 _center_vertices.Add(reverse_cen_y);
                 i += 2;
             }
        }
        public override void PrepareIndices()
        {
            _indices.Clear();
            _center_indices.Clear();
            List<int> next_indices = new List<int>();
            List<int> near_indices = new List<int>();
            if (_toatal_num % 2 == 0) _toatal_num = _toatal_num;
            else _toatal_num = _toatal_num - 1;
            for (int i = 0; i < _toatal_num; i++)
            {
                int first_line_index = i << 1;
                int next_line_index =first_line_index + 1;

                //0--2--4 
                //1--3--5
                //0-1   2-3  ...
                _indices.Add(first_line_index);
                next_indices.Add(next_line_index);
                near_indices.Add(first_line_index);
                near_indices.Add(next_line_index);
            }

            _indices.AddRange(next_indices);
            _indices.AddRange(near_indices);
            _toatal_num = 0;
            for (int j = 0; j < _center_vertices.Count / 2; j++)
            {
                _center_indices.Add(j);
            }
        }
    }
}
