/* ***********************************************
 * Copyright (c) 2014-2015 luoshasha. All rights reserved";
 * CLR version: 4.0.30319.34014"
 * File name:   SlashFillPolygonSymbol.cs"
 * Date:        2/10/2015 11:30:23 AM
 * Author :  sand
 * Email  :  luoshasha@foxmail.com
 * Description: 
	
 * History:  created by sand 2/10/2015 11:30:23 AM
 
 * ***********************************************/
using System;
using System.Collections.Generic;
using System.Text;
using CMA.MICAPS.Box2D;
using CMA.MICAPS.Box2D.OGL;

namespace CMA.MICAPS.Symbols
{
    class SlashFillPolygonSymbol : PolygonSymbol
    {
        private SymbolMesh _mesh;

        public SlashFillPolygonSymbol(LineString2D<float> line, System.Drawing.Color line_color, System.Drawing.Color fill)
            : base(line, fill)
        {
            _mesh = new SymbolMesh();

            //calculate slash fill mesh.
            int len = line.Data.Length;
            float minx = line.Data[0], miny = line.Data[1];
            float maxx = minx, maxy = miny;

            for (int i = 2; i < len; i += 2)
            {
                float x = line.Data[i];
                float y = line.Data[i + 1];

                if (x < minx)
                    minx = x;
                else if (x > maxx)
                    maxx = x;

                if (y < miny)
                    miny = y;
                else if (y > maxy)
                    maxy = y;
            }
        }
    }
}
