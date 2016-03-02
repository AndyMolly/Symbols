using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMA.MICAPS.Symbols
{
    using CMA.MICAPS.Box2D;
    using Line = CMA.MICAPS.Box2D.LineString2D<float>;
    class BrokenLine : LineSymbol
    {
        private List<float> _vertices;
        private List<int> _indices;
        public BrokenLine(Line line)
         :base(line)
        {
            _vertices = new List<float>(_line.Data.Length << 1);
            _indices = new List<int>((_vertices.Capacity - 1) * 2);
        }
        public override void Render(Box2D.Graphics.SceneManager scene, Box2D.Graphics.Context context)
        {
            context.SetRenderState(this.Material.SurfaceState);
            PrepareVertices();
            PrepareIndices();

            var color = this.Material.SurfaceState.color;

            StaticBufferDrawHelper.DrawIndex(_vertices.ToArray(), _indices.ToArray(),
                 color, PrimitiveType.LineStrip, _indices.Count);

        }
        protected virtual void PrepareVertices()
        {
            _vertices.Clear();
            float[] pts = _line.Data;
            int num = pts.Length;
            for (int i = 0; i < num;)
            {
                _vertices.Add(pts[i]);
             
                i++;
            }
        }
        public virtual void PrepareIndices()
        {
            _indices.Clear();
            int pt_num =_vertices.Count>>1;
            for (int i = 0; i < pt_num; i++)
            {
                _indices.Add(i);
            }
        }
    }
}
