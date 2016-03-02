using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMA.MICAPS.Symbols
{
    using CMA.MICAPS.Box2D;
    using Line = CMA.MICAPS.Box2D.LineString2D<float>;
    class CoveredArea:LineSymbol
    {
        private List<float> _vertices;
        private List<int> _indices;
        public CoveredArea(Line line)
            :base(line)
        {
            _vertices = new List<float>(_line.Data.Length << 1);
            _indices = new List<int>((_vertices.Capacity - 1) * 2);
        }
        public override void Render(Box2D.Graphics.SceneManager scene, Box2D.Graphics.Context context)
        {
            context.SetRenderState(this.Material.SurfaceState);

            PrepareForDraw(context);
            PrepareIndices();

            var color = this.Material.SurfaceState.color;

            StaticBufferDrawHelper.DrawIndex(_vertices.ToArray(), _indices.ToArray(),
                 color, PrimitiveType.TriangleFan, _indices.Count);

        }
        public virtual void PrepareForDraw(Box2D.Graphics.Context context)
        {
            _vertices.Clear();
            float[] pts = _line.Data;
            int num = pts.Length;
            float x = (pts[0] + pts[num - 4]) / 2;
            float y = (pts[1] + pts[num - 3]) / 2;
            _vertices.Add(x);
            _vertices.Add(y);
            for(int i=0;i<num-1;i++)
            {
                _vertices.Add(pts[i]); 
             }
        }
        public virtual void PrepareIndices()
        {
            _indices.Clear();
            int tran_num=(_vertices.Count>>1);
            for (int i = 0; i < tran_num; i++)
            {
                _indices.Add(i);
            }
        }
    }
}
