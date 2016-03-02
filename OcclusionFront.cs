/* ***********************************************
 * Copyright (c) luoshasha. All rights reserved";
 * CLR version: 4.0.30319.18449"
 * File name:   OcclusionSymbol.cs"
 * Date:        5/14/2014 11:49:10
 * Author :  sand-work
 * Email  :  luoshasha@foxmail.com
 * Description: 
	
 * History:  created by sand-work 5/14/2014 11:49:10
 
 * ***********************************************/
using System;
using System.Collections.Generic;
using System.Text;

namespace CMA.MICAPS.Symbols
{
    class OcclusionFront : ISymbolAssembler
    {
        private ColdFront _cold;
        private WarmFront _warm;

        public OcclusionFront()
        {
            _cold = new ColdFront();
            _warm = new WarmFront();
        }

        public void Assemble(Box2D.Graphics.Context context, SymbolMesh symbol)
        {
            if (symbol.seq == 0)
                _cold.Assemble(context, symbol);
            else
                _warm.Assemble(context, symbol);
        }
    }
}
