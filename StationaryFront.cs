
using System;
using System.Collections.Generic;
using System.Text;

namespace CMA.MICAPS.Symbols
{
    class StationaryFront : ISymbolAssembler
    {
        private ColdFront _cold;
        private WarmFront _warm;

        public StationaryFront()
        {
            _cold = new ColdFront() { Direction = 1 };
            _warm = new WarmFront() { Direction=-1};
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
