using CMA.MICAPS.Box2D;
using CMA.MICAPS.Box2D.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMA.MICAPS.Symbols
{
    class DengZhiXian : LineSymbol
    {

        private LineSymbol[] _symbols = null;
        public DengZhiXian(LineString2D<float>[] lines)
        {
            _symbols = new LineSymbol[lines.Length];
            for (int i = 0; i < lines.Length; ++i)
                _symbols[i] = new SolidLineSymbol(lines[i]);
        }

        public override void Render(Box2D.Graphics.SceneManager scene, Box2D.Graphics.Context context)
        {
            for (int i = 0; i < _symbols.Length; ++i)
                _symbols[i].Render(scene, context);
       
        }

        public override void Dispose()
        {
            base.Dispose();
            for (int i = 0; i < _symbols.Length; ++i)
                _symbols[i].Dispose();
        }
    }
}
