/* ***********************************************
 * Copyright (c) 2009-2010 luoshasha. All rights reserved";
 * CLR version: 4.0.30319.34014"
 * File name:   PolygonSymbol.cs"
 * Date:        11/19/2014 3:00:31 PM
 * Author :  sand
 * Email  :  luoshasha@foxmail.com
 * Description: 
	
 * History:  created by sand 11/19/2014 3:00:31 PM
 
 * ***********************************************/
using System;
using System.Collections.Generic;
using System.Text;
using CMA.MICAPS.Box2D;
using CMA.MICAPS.Box2D.OGL;

namespace CMA.MICAPS.Symbols
{
    class PolygonSymbol : LineSymbol
    {
        private SimplePolygonRenderable _polygon;
        public PolygonSymbol(LineString2D<float> line, System.Drawing.Color fill)
            : base(line)
        {
            _polygon = new SimplePolygonRenderable(line) { Color = fill };
        }

        public override void UpdateLineData()
        {
            base.UpdateLineData();
            if (_polygon != null)
                _polygon.RefreshData();
        }

        public override void Render(Box2D.Graphics.SceneManager scene, Box2D.Graphics.Context context)
        {
            _polygon.Render(scene, context);
            context.SetRenderState(this.Material.SurfaceState);
            _line_drawable.Draw(PrimitiveType.LineStrip, 0, _point_count);
        }

        public override void Dispose()
        {
            base.Dispose();
            _polygon.Dispose();
        }
    }
}
