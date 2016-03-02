/* ***********************************************
 * Copyright (c) luoshasha. All rights reserved";
 * CLR version: 4.0.30319.18449"
 * File name:   ISymbolAssembler.cs"
 * Date:        5/14/2014 14:59:32
 * Author :  sand-work
 * Email  :  luoshasha@foxmail.com
 * Description: 
	
 * History:  created by sand-work 5/14/2014 14:59:32
 
 * ***********************************************/
using System;
using System.Collections.Generic;
using System.Text;
using CMA.MICAPS.Box2D.Graphics;

namespace CMA.MICAPS.Symbols
{
    interface ISymbolAssembler
    {
        void Assemble(Context context, SymbolMesh symbol);
    }
}
