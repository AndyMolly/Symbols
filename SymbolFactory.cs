
using System;

namespace CMA.MICAPS.Symbols
{
    using System.Drawing;
    using Line = CMA.MICAPS.Box2D.LineString2D<float>;
    using CMA.MICAPS.Algorithms.Contours;
    using CMA.MICAPS.GMap.Data;
    using CMA.MICAPS.GMap.Presenter;
    using CMA.MICAPS.Box2D;
    using System.Collections.Generic;

    public static class SymbolFactory
    {

        public static LineSymbol CreateColdFront(Line line)
        {
            LineSymbol s = new LineSymbol(line);
            s.Assembler = new ColdFront();
            s.SymbolColor0 = Color.Blue;
            s.SymbolColor1 = Color.Blue;
            return s;
        }
        public static LineSymbol CreateColdFront(Line line, string label, uint size,
           System.Drawing.Color color, LabelPosition position, bool rotation, bool split = false)
        {
            FrontWithLabel s = new FrontWithLabel(line, label, size, color, position, rotation, split);
            s.Assembler = new ColdFront();
            s.SymbolColor0 = Color.Blue;
            s.SymbolColor1 = Color.Blue;
            return s;
        }
        public static LineSymbol CreateWarmFront(Line line)
        {
            LineSymbol s = new LineSymbol(line);
            s.Assembler = new WarmFront();
            s.SymbolColor0 = Color.Red;
            s.SymbolColor1 = Color.Red;
            return s;
        }
        public static LineSymbol CreateWarmFront(Line line, string label, uint size,
       System.Drawing.Color color, LabelPosition position, bool rotation, bool split = false)
        {
            FrontWithLabel s = new FrontWithLabel(line, label, size, color, position, rotation, split);
            s.Assembler = new WarmFront();
            s.SymbolColor0 = Color.Blue;
            s.SymbolColor1 = Color.Blue;
            return s;
        }
        public static LineSymbol CreateStationaryFront(Line line)
        {
            LineSymbol s = new LineSymbol(line);
            s.Assembler = new StationaryFront();
            s.SymbolColor0 = Color.Blue;
            s.SymbolColor1 = Color.Red;
            return s;
        }
        public static LineSymbol CreateStationaryFront(Line line, string label, uint size,
        System.Drawing.Color color, LabelPosition position, bool rotation, bool split = false)
        {
            FrontWithLabel s = new FrontWithLabel(line, label, size, color, position, rotation, split);
            s.Assembler = new StationaryFront();
            s.SymbolColor0 = Color.Blue;
            s.SymbolColor1 = Color.Blue;
            return s;
        }
        public static LineSymbol CreateOcclusionFront(Line line)
        {
            LineSymbol s = new LineSymbol(line);
            s.Assembler = new OcclusionFront();
            s.SymbolColor0 = Color.Blue;
            s.SymbolColor1 = Color.Red;
            return s;
        }
        public static LineSymbol CreateOcclusionFront(Line line, string label, uint size,
        System.Drawing.Color color, LabelPosition position, bool rotation, bool split = false)
        {
            FrontWithLabel s = new FrontWithLabel(line, label, size, color, position, rotation, split);
            s.Assembler = new OcclusionFront();
            s.SymbolColor0 = Color.Blue;
            s.SymbolColor1 = Color.Blue;
            return s;
        }
        public static LineSymbol CreateLabelLine(Line line, string label, uint size,
            System.Drawing.Color color, LabelPosition position, bool rotation, bool split = false)
        {
            LineSymbol symbol = new LabelLineSymbol(line, label, size, color, position, rotation, split);
            return symbol;
        }
        public static LineSymbol CreateLabelSplitLine(Line line, string label, uint size,
            System.Drawing.Color color, bool rotation,bool cross)
        {
            LineSymbol symbol = new LabelSplitLine(line, label, size, color, rotation,cross);
            return symbol;
        }

        public static LineSymbol CreateSolidLine(Line line)
        {
            return new SolidLineSymbol(line);
        }

        public static LineSymbol CreateFillPolygon(Line line, System.Drawing.Color fill)
        {
            return new PolygonSymbol(line, fill);
        }

        public static LineSymbol CreateDoubleLine(Line line, string label, uint size, System.Drawing.Color label_color, LabelPosition position, bool rotation, int offset=2, bool split = false)
        {
            DoubleLineSymbol symbol = new DoubleLineSymbol(line, label, size, label_color, position, rotation, offset, split); ;
            return symbol;
        }
        /// <summary>
        /// Create a double line symbol
        /// </summary>
        /// <param name="line"></param>
        /// <param name="offset">offset in world units (usually meters)</param>
        /// <returns></returns>
        public static LineSymbol CreateDoubleLine(Line line,int offset=2)
        {
            DoubleLineSymbol symbol = new DoubleLineSymbol(line, offset);
            return symbol;
        }

        public static LineSymbol CreateShuangGanAndShuangxian(Line line, string label, uint size, System.Drawing.Color label_color, LabelPosition position, bool rotation, int offset = 5, int distance = 20, bool split = false)
        {
            ShuangGanAndShuangxian symbol = new ShuangGanAndShuangxian(line,label,size,label_color,position,rotation,offset,distance);
            return symbol;
        }
        public static LineSymbol CreateShuangGanAndShuangxian(Line line, int offset = 5, int distance = 20)
        {
            ShuangGanAndShuangxian symbol = new ShuangGanAndShuangxian(line, offset, distance);
            return symbol;
        }
        public static LineSymbol CreateLineAndAuads(Line line, string label, uint size, System.Drawing.Color label_color, LabelPosition position, bool rotation, int distance = 20, int cubeLength = 8, bool split = false)
        {
            LineAndQuads symbol = new LineAndQuads(line,label,size,label_color,position,rotation,distance,cubeLength,split);
            return symbol;
        }
        public static LineSymbol CreateLineAndAuads(Line line, int distance = 20, int cubeLength = 8)
        {
            LineAndQuads symbol = new LineAndQuads(line,distance, cubeLength);
            return symbol;
        }
        public static LineSymbol CreateLineAndCross(Line line,  string label, uint size, System.Drawing.Color label_color, LabelPosition position, bool rotation,int distance = 20, int cubeLength = 8, bool split = false)
        {
            LineAndCross symbol = new LineAndCross(line,label,size,label_color,position,rotation,distance,cubeLength,split);
            return symbol;
        }
        public static LineSymbol CreateLineAndCross(Line line, int distance = 20, int cubeLength = 8)
        {
            LineAndCross symbol = new LineAndCross(line, distance, cubeLength);
            return symbol;
        }
        public static LineSymbol CreateSoildCircle(Line line, string label, uint size, System.Drawing.Color label_color, LabelPosition position, bool rotation, int distance = 20, int radius = 8)
        {
            SoildCircle symbol = new SoildCircle(line,label,size,label_color,position,rotation, distance, radius);
            return symbol;
        }
        public static LineSymbol CreateSoildCircle(Line line,int distance = 20, int radius = 8)
        {
            SoildCircle symbol = new SoildCircle(line, distance, radius);
            return symbol;
        }
        public static LineSymbol CreateEmptyCircle(Line line,  string label, uint size, System.Drawing.Color label_color, LabelPosition position, bool rotation, int distance = 20, int radius = 8, bool split = false)
        {
            EmptyCircle symbol = new EmptyCircle(line,label,size,label_color,position,rotation,distance,radius);
            return symbol;
        }
        public static LineSymbol CreateEmptyCircle(Line line,int distance = 20, int radius = 8)
        {
            EmptyCircle symbol = new EmptyCircle(line,distance, radius);
            return symbol;
        }
        public static LineSymbol CreateLineAndSoildCircle(Line line, string label, uint size, System.Drawing.Color label_color, LabelPosition position, bool rotation, int distance, int radius)
        {
            LineAndSoildCircle symbol = new LineAndSoildCircle(line,label,18,label_color,position,rotation,distance, radius);
            return symbol;
        }
        public static LineSymbol CreateLineAndSoildCircle(Line line, int distance, int radius)
        {
            LineAndSoildCircle symbol = new LineAndSoildCircle(line, distance, radius);
            return symbol;
        }
        public static LineSymbol CreateLineAndHalfCircle(Line line, string label, uint size, System.Drawing.Color label_color, LabelPosition position, bool rotation, int radius = 10, bool split = false)
        {
            LineAndHalfCircle symbol = new LineAndHalfCircle(line,label,size,label_color,position,rotation,radius,split);
            return symbol;
        }
        public static LineSymbol CreateLineAndHalfCircle(Line line, int radius = 10)
        {
            LineAndHalfCircle symbol = new LineAndHalfCircle(line, radius);
            return symbol;
        }
        public static LineSymbol CreateBrokenLine(Line line)
        {
            BrokenLine symbol = new BrokenLine(line);
            return symbol;
        }
        public static LineSymbol CreateLineAndTriangle(Line line, string label, uint size, System.Drawing.Color label_color, LabelPosition position, bool rotation, int distance = 10, int offset = 8, bool split = false)
        {
            LineAndTriangle symbol = new LineAndTriangle(line,label,size,label_color,position,rotation,distance,offset,split);
            return symbol;
        }
        public static LineSymbol CreateLineAndTriangle(Line line, int distance = 10, int offset = 8)
        {
            LineAndTriangle symbol = new LineAndTriangle(line, distance, offset);
            return symbol;
        }
        public static LineSymbol Create24hourWeather(Line line, string label, uint size, System.Drawing.Color label_color, LabelPosition position, bool rotation, int offset = 5, int distance = 30, bool split = false)
        {
            _24hourWeather symbol = new _24hourWeather(line, label, size, label_color, position, rotation, offset, distance, split);
            return symbol;
        }
        public static LineSymbol Create24hourWeather(Line line, int offset = 5, int distance = 30)
        {
            _24hourWeather symbol = new _24hourWeather(line, offset, distance);
            return symbol;
        }
        public static LineSymbol CreateCoveredArea(Line line)
        {
            CoveredArea symbol = new CoveredArea(line);
            return symbol;
        }
        public static LineSymbol CreateQiangJiangShuiArea(Line line,int offset=50000)
        {
            QiangJiangShuiArea symbol = new QiangJiangShuiArea(line, offset);
            //QiangJiangShuiNew symbol = new QiangJiangShuiNew(line,offset);
            return symbol;
        }
        public static LineSymbol CreateHatchFullPolygon(Line line, System.Drawing.Color fill, HatchStyle hatch)
        {
            HatchFullPolygon symbol = new HatchFullPolygon(line,fill,hatch);
            return symbol;
        }
        public static LineSymbol CreatePolygonWeather(Line line, System.Drawing.Color fill, string label, uint size, System.Drawing.Color label_color, bool rotation)
        {
            return new PolygonWeather(line, fill, label, size, label_color, rotation);
        }
        public static LineSymbol CreateJiLiuHe(Line line, string label, uint size,
            System.Drawing.Color label_color, LabelPosition position, bool rotation, int offset = 5, int distance = 5, bool split = false)
        {
            JiLiuHeNew symbol = new JiLiuHeNew(line,label,size,label_color,position,rotation,offset,distance,split);
            return symbol;
        }
        public static LineSymbol CreateJiLiuHe(Line line, int offset = 5, int distance = 5)
        {
            JiLiuHeNew symbol = new JiLiuHeNew(line, offset, distance);
            return symbol;
        }
        public static LineSymbol CreateGanQinRu(Line line, string label, uint size,
            System.Drawing.Color label_color, LabelPosition position, bool rotation, int distance = 20, int radius = 5, int offset = 5, bool split = false)
        {
            GanQinRu symbol = new GanQinRu(line,label,size,label_color,position,rotation,distance,radius,offset,split);
            return symbol;
        }
        public static LineSymbol CreateGanQinRu(Line line, int distance = 20, int radius = 5, int offset = 5)
        {
            GanQinRu symbol = new GanQinRu(line,distance, radius, offset);
            return symbol;
        }
        public static LineSymbol CreateSoildLineAndCircle(Line line, string label, uint size, 
            System.Drawing.Color label_color, LabelPosition position, bool rotation, int distance = 20, int radius = 3,bool split=false)
        {
            SoildLineAndCircle symbol = new SoildLineAndCircle(line,label,size,label_color,position,rotation, distance, radius,split);
            return symbol;
        }
        public static LineSymbol CreateSoildLineAndCircle(Line line,  int distance = 20, int radius = 3)
        {
            SoildLineAndCircle symbol = new SoildLineAndCircle(line, distance, radius);
            return symbol;
        }
        public static LineSymbol CreateGanShe(Line line, string label, uint size, 
            System.Drawing.Color label_color, LabelPosition position, bool rotation, int offset = 5, int distance = 10, bool split = false)
        {
            GanShe symbol = new GanShe(line, label, size, label_color, position, rotation, offset, distance, split);
            return symbol;
        }
        public static LineSymbol CreateGanShe(Line line, int offset = 5, int distance = 10)
        {
            GanShe symbol = new GanShe(line, offset, distance);
            return symbol;
        }
        public static LineSymbol CreateShiZhou(Line line, string label, uint size, 
            System.Drawing.Color label_color, LabelPosition position, bool rotation, int distance = 30, int amplitude = 10, bool split = false)
        {
            ShiZhou symbol = new ShiZhou(line,label,size,label_color,position,rotation,distance,amplitude,split);
            return symbol;
        }
        public static LineSymbol CreateShiZhou(Line line, int distance = 30, int amplitude = 10)
        {
            ShiZhou symbol = new ShiZhou(line,distance, amplitude);
            return symbol;
        }
        public static LineSymbol CreateFenLiuQu(Line line, string label, uint size, 
            System.Drawing.Color label_color, LabelPosition position, bool rotation, int distance = 30, int amplitude = 10, bool split = false)
        {
            FenLiuQu symbol = new FenLiuQu(line,label,size,label_color,position,rotation,distance,amplitude,split);
            return symbol;
        }
        public static LineSymbol CreateFenLiuQu(Line line, int distance = 30, int amplitude = 10)
        {
            FenLiuQu symbol = new FenLiuQu(line, distance, amplitude);
            return symbol;
        }
        public static LineSymbol CreateArrowHeadFilledLine(Line line, string label, uint size,
            System.Drawing.Color label_color, LabelPosition position, bool rotation, int offset=5, bool split = false)
        {
            ArrowHeadFilledLineSymbol symbol = new ArrowHeadFilledLineSymbol(line,label,size,label_color,position,rotation,offset,split);
            return symbol;
        }
        public static LineSymbol CreateArrowHeadFilledLine(Line line, int offset = 5)
        {
            ArrowHeadFilledLineSymbol symbol = new ArrowHeadFilledLineSymbol(line,offset);
            return symbol;
        }

        public static LineSymbol CreateArrowHeadLine(Line line, string label, uint size, 
            System.Drawing.Color label_color, LabelPosition position, bool rotation, int offset = 5, bool split = false)
        {
            ArrowHeadLineSymbol symbol = new ArrowHeadLineSymbol(line, label, size, label_color, position, rotation, offset, split);
            return symbol;
        }

        public static LineSymbol CreateArrowHeadLine(Line line, int offset = 5)
        {
            ArrowHeadLineSymbol symbol = new ArrowHeadLineSymbol(line, offset);
            return symbol;
        }

        public static LineSymbol CreateArrowHeadTailFilledLine(Line line, string label, uint size, 
            System.Drawing.Color label_color, LabelPosition position, bool rotation, int offset = 5, bool split = false)
        {
            ArrowHeadTailFilledLineSymbol symbol = new ArrowHeadTailFilledLineSymbol(line, label, size, label_color, position, rotation, offset, split);
            return symbol;
        }
        public static LineSymbol CreateArrowHeadTailFilledLine(Line line,int offset = 5)
        {
            ArrowHeadTailFilledLineSymbol symbol = new ArrowHeadTailFilledLineSymbol(line, offset);
            return symbol;
        }
        public static LineSymbol CreateArrowHeadTailLine(Line line, string label, uint size, 
            System.Drawing.Color label_color, LabelPosition position, bool rotation, int offset = 5, bool split = false)
        {
            ArrowHeadTailLineSymbol symbol = new ArrowHeadTailLineSymbol(line, label, size, label_color, position, rotation, offset, split);
            return symbol;
        }
        public static LineSymbol CreateArrowHeadTailLine(Line line,int offset = 5)
        {
            ArrowHeadTailLineSymbol symbol = new ArrowHeadTailLineSymbol(line, offset);
            return symbol;
        }

        public static LineSymbol CreateArrowLine(Line line, string label, uint size, 
            System.Drawing.Color label_color, LabelPosition position, bool rotation, int offset = 5, bool split = false)
        {
            ArrowMidLineSymbol symbol = new ArrowMidLineSymbol(line, label, size, label_color, position, rotation, offset, split);
            return symbol;
        }
        public static LineSymbol CreateArrowLine(Line line, int offset = 5)
        {
            ArrowMidLineSymbol symbol = new ArrowMidLineSymbol(line, offset);
            return symbol;
        }

        public static LineSymbol CreateDashDotDotLine(Line line, string label, uint size, System.Drawing.Color label_color, LabelPosition position, bool rotation, int dash_len=20, int dash_interval=8, bool split = false)
        {
            DashDotDotLineSymbol symbol = new DashDotDotLineSymbol(line,label,size,label_color,position,rotation,dash_len,dash_interval,split);
            return symbol;
        }
        public static LineSymbol CreateDashDotDotLine(Line line, int dash_len = 20, int dash_interval = 8)
        {
            DashDotDotLineSymbol symbol = new DashDotDotLineSymbol(line, dash_len, dash_interval);
            return symbol;
        }
        public static LineSymbol CreateDashDotLine(Line line, string label, uint size, System.Drawing.Color label_color, LabelPosition position, bool rotation, int dash_len=20, int dash_interval=8, bool split = false)
        {
            DashDotLineSymbol symbol = new DashDotLineSymbol(line, label, size, label_color, position, rotation, dash_len, dash_interval, split);
            return symbol;
        }
        public static LineSymbol CreateDashDotLine(Line line, int dash_len = 20, int dash_interval = 8)
        {
            DashDotLineSymbol symbol = new DashDotLineSymbol(line, dash_len, dash_interval);
            return symbol;
        }
        public static LineSymbol CreateDashLine(Line line, string label, uint size, System.Drawing.Color label_color, LabelPosition position, bool rotation, int dash_len=20, int dash_interval=8, bool split = false)
        {
            DashLineSymbol symbol = new DashLineSymbol(line,label,size,label_color,position,rotation,dash_len,dash_interval,split);
            return symbol;
        }
        public static LineSymbol CreateDashLine(Line line, int dash_len = 20, int dash_interval = 8)
        {
            DashLineSymbol symbol = new DashLineSymbol(line, dash_len, dash_interval);
            return symbol;
        }
        public static LineSymbol CreateDotLine(Line line, string label, uint size, System.Drawing.Color label_color, LabelPosition position, bool rotation, int distance=20, bool split = false)
        {
            DotLineSymbol symbol = new DotLineSymbol(line,label,size,label_color,position,rotation,distance,split);
            return symbol;
        }
        public static LineSymbol CreateDotLine(Line line, int distance = 20)
        {
            DotLineSymbol symbol = new DotLineSymbol(line, distance);
            return symbol;
        }
        public static LineSymbol CreateFrostLine(Line line, string label, uint size, System.Drawing.Color label_color, LabelPosition position, bool rotation, int distance=20, bool split = false)
        {
            FrostLineSymbol symbol = new FrostLineSymbol(line,label,size,label_color,position,rotation,distance,split);
            return symbol;
        }
        public static LineSymbol CreateFrostLine(Line line, int distance = 20)
        {
            FrostLineSymbol symbol = new FrostLineSymbol(line, distance);
            return symbol;
        }
        public static LineSymbol CreateSingleArrowLine(Line line, string label, uint size, System.Drawing.Color label_color, LabelPosition position, bool rotation, bool split = false)
        {
            SingleArrowLineSymbol symbol = new SingleArrowLineSymbol(line,label,size,label_color,position,rotation,split);
            return symbol;
        }
        public static LineSymbol CreateSingleArrowLine(Line line)
        {
            SingleArrowLineSymbol symbol = new SingleArrowLineSymbol(line);
            return symbol;
        }
    }
}
