using CMA.MICAPS.Box2D;
using CMA.MICAPS.Box2D.Graphics;
using CMA.MICAPS.Box2D.Text;
using CMA.MICAPS.Box2D.Util;
using CMA.MICAPS.GMap;
using SandLib.Math3D;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace CMA.MICAPS.Symbols
{
    class PolygonWeather : PolygonSymbol
    {
        private Label _label;
        private double x, y;
        public PolygonWeather(LineString2D<float> line, System.Drawing.Color fill, string label, uint size, System.Drawing.Color label_color, bool rotation)
            : base(line, fill)
        {
            CMA.MICAPS.Box2D.Text.Font font = FontManager.Instance.GetFont("res:wind.ttf");
            CMA.MICAPS.Box2D.Text.Font font2 = FontManager.Instance.GetFont(AppDomain.CurrentDomain.BaseDirectory + "Fonts\\Helvetica Bold.ttf");
            _label = new Label();
            _label.Text = new Box2D.Text.TextBlock(font, new FontSize(size), label, false);
            var c = Color4F.FromSystemColor(label_color);
            _label.Color = c;
         
            _label.Text.Font.LoadAllGlyphs();
            font.LoadAllGlyphs();
            font2.LoadAllGlyphs();
            FontManager.DefaultFont.LoadAllGlyphs();


            double sx=0,sy=0;
            x = y = 0;
            for (int i = 0; i < _line.Data.Length; i += 2)
            {
                sx += _line.Data[i];
                sy += _line.Data[i + 1];
            }
            int num=_line.Data.Length >> 1;
            x = sx /num;
            y = sy / num;
        }
        public override void Render(Box2D.Graphics.SceneManager scene, Box2D.Graphics.Context context)
        {
            base.Render(scene, context);
            _label.Position = new Vec3(x, y, 0);
            //_label.ScreenOffset = new Vec2(0, _label.Text.Bounds.Size.y * .5);
            _label.Render(scene, context);
        }
        public override void Dispose()
        {
            base.Dispose();
            if (_label != null)
                _label.Dispose();
        }
    }
}
