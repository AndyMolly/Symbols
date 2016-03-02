
using System;
using System.Collections.Generic;
using System.Text;
using CMA.MICAPS.Box2D;

namespace CMA.MICAPS.Symbols
{
    class SolidLineSymbol : LineSymbol
    {
        public SolidLineSymbol(LineString2D<float> line)
            : base(line)
        {

        }

        public override void Render(Box2D.Graphics.SceneManager scene, Box2D.Graphics.Context context)
        {
            context.SetRenderState(this.Material.SurfaceState);

            //Tao.OpenGl.Gl.glEnable(Tao.OpenGl.Gl.GL_LINE_STIPPLE);
            //Tao.OpenGl.Gl.glLineStipple(4,0x1F1F);
            _line_drawable.Draw(PrimitiveType.LineStrip, 0, _point_count);
        }
    }
}
