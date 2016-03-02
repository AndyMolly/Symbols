using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMA.MICAPS.Box2D;
using CMA.MICAPS.Box2D.OGL;
using System.Drawing;
using CMA.MICAPS.Box2D.Resources;
using System.IO;

namespace CMA.MICAPS.Symbols
{
    class HatchFullPolygon : LineSymbol
    {
        private SimplePolygonRenderable _polygon;

        public HatchFullPolygon(LineString2D<float> line, System.Drawing.Color fill, HatchStyle hatch)
            : base(line)
        {
            _polygon = new SimplePolygonRenderable(line) { Color = fill };
            string hatchname = hatch.ToString();
            string res_name = string.Format("CMA.MICAPS.Symbols.Resources.{0}.bmp", hatchname);
            try
            {
                System.Reflection.Assembly asm = System.Reflection.Assembly.GetExecutingAssembly();
                 using( var stream = asm.GetManifestResourceStream(res_name))
                {
                    byte[] stipple = new byte[128];
                    stream.Seek(-128, SeekOrigin.End);
                    stream.Read(stipple, 0, 128);
                    _polygon.PolygonStipple = stipple;
                }
            }
            catch (System.Resources.MissingManifestResourceException)
            {
            }
        }

        public override void UpdateLineData()
        {
            base.UpdateLineData();
            if (_polygon != null)
                _polygon.RefreshData();
        }

        public override void Render(Box2D.Graphics.SceneManager scene, Box2D.Graphics.Context context)
        {
            context.SetRenderState(this.Material.SurfaceState);
            _polygon.Render(scene, context);
            _line_drawable.Draw(PrimitiveType.LineLoop, 0, _point_count);
        }

        public override void Dispose()
        {
            base.Dispose();
            _polygon.Dispose();
        }
    }
}
