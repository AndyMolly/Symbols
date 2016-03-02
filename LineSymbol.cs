using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMA.MICAPS.Box2D;
using CMA.MICAPS.Box2D.Graphics;

namespace CMA.MICAPS.Symbols
{
    using CMA.MICAPS.Box2D.Buffers;
    using CMA.MICAPS.Box2D.Util;
    using Microsoft.Practices.ServiceLocation;
    using SandLib.Math3D;
    using Line = LineString2D<float>;

    public class LineSymbol : MRenderableBase
    {
        /// <summary>
        /// Symbol distance between distance.
        /// </summary>
        public const int SymbolDistance = 100;
        public const int SymbolSize = 30;
        protected Line _line;
        protected StaticBufferDrawHelper _line_drawable;
        protected int _point_count;
        private ISymbolAssembler _asm;
        private CMA.MICAPS.Infrastructures.Configurations.Configuration _config = new Infrastructures.Configurations.Configuration(); 

        internal LineSymbol(Line line)
        {
            _line = line;
            Material2D ma = new Material2D();
            ma.ForeColor = System.Drawing.Color.Blue;
            ma.SurfaceState.line_width = 2.0F;
            ma.SurfaceState.point_size = 4;
            ma.SurfaceState.color = System.Drawing.Color.Yellow;
            ma.BackColor = System.Drawing.Color.Blue;
            this.SetMaterial(ma);

            this.UpdateLineData();
        }

        protected LineSymbol()
        {
            Material2D ma = new Material2D();
            ma.ForeColor = System.Drawing.Color.Blue;
            ma.SurfaceState.line_width = 2.0F;
            ma.SurfaceState.point_size = 4;
            ma.SurfaceState.color = System.Drawing.Color.Yellow;
            ma.BackColor = System.Drawing.Color.Blue;
            this.SetMaterial(ma);

            _line = new Line();
        }

        protected struct cut_index
        {
            public int start;
            public int end;
            public float[] pts;
        }

        public string GetConfiguration(string key)
        {
            return _config.GetString(key);
        }

        public void SetConfiguration(string key, string value)
        {
            this._config.SetString(key, value);
        }

        public System.Drawing.Color LineColor
        {
            get {return this.Material.SurfaceState.color; }
            set { this.Material.SurfaceState.color = value;}
        }

        public System.Drawing.Color SymbolColor0
        {
            get { return this.Material.ForeColor; }
            set { this.Material.ForeColor = value; }
        }

        public System.Drawing.Color SymbolColor1
        {
            get { return this.Material.BackColor; }
            set { this.Material.BackColor = value; }
        }

        /// <summary>
        /// rebuild line buffer;
        /// </summary>
        public virtual void UpdateLineData()
        {
            _point_count = _line.NumOfPoints;
            var line_buffer = new VertexBuffer(VertexFormat.V2f, _point_count);
            line_buffer.WriteData(0, _point_count * 8, _line.Data);
            line_buffer.SetSize(_point_count * 8);
            if (_line_drawable != null)
            {
                _line_drawable.Dispose();
            }
            _line_drawable = new StaticBufferDrawHelper(line_buffer);
        }

        public override void Render(SceneManager scene, Context context)
        {
            context.SetRenderState(this.Material.SurfaceState);
            _line_drawable.Draw(PrimitiveType.LineStrip, 0, _point_count);

            //_line_drawable.Draw(System.Drawing.Color.Green, PrimitiveType.Lines, 0, _point_count);

            context.PushOrtho2D();

            System.Drawing.Color c1 = this.Material.BackColor;
            System.Drawing.Color c0 = this.Material.ForeColor;

            InterpolatePosition start = new InterpolatePosition();
            start.x = _line.Data[0];
            start.y = _line.Data[1];
            start.next = 2;
            start = this.FindNextPosition(context, start, SymbolSize);

            do
            {
                SymbolMesh symbol = this.MakeSymbol(context, start, SymbolSize, 0);
                if (!symbol.is_completed)
                    break;
                SymbolMesh neighbor_symbol = this.MakeSymbol(context, symbol.tail, SymbolSize, 1);
                if (!neighbor_symbol.is_completed)
                    break;
                start = this.FindNextPosition(context, neighbor_symbol.tail, SymbolDistance);

                context.Project(symbol.vertices);
                context.Project(neighbor_symbol.vertices);

                StaticBufferDrawHelper.DrawIndex(symbol.vertices.ToArray(), symbol.indices.ToArray(),
                   c0, PrimitiveType.TriangleStrip, symbol.indices.Count);

                StaticBufferDrawHelper.DrawIndex(neighbor_symbol.vertices.ToArray(), neighbor_symbol.indices.ToArray(),
                    c1, PrimitiveType.TriangleStrip, neighbor_symbol.indices.Count);

            } while (true);

            context.PopOrtho2D();
        }

        /// <summary>
        /// Gets original line.
        /// </summary>
        public Line Line
        {
            get { return _line; }
        }

        /// <summary>
        /// Gets or sets the origin line before interpolate.
        /// </summary>
        public Line ControlLine
        {
            get;
            set;
        }

        /// <summary>
        /// Gets label of the line symbol.
        /// </summary>
        /// <returns></returns>
        public virtual Label Label
        {
            get
            {
                return null;
            }
        }
        protected cut_index FindCutPosition(Context context)
        {
            int num = _line.Data.Length;
            cut_index cut = new cut_index { start = 0, end = 0 ,pts=new float[num]};
           
            var vp = context.SceneState.viewport;
            double sx, sy,sx0,sy0,sx1, sy1;
            bool isStart = true;

            Array.Copy(_line.Data, cut.pts, num);
           
            context.Project(cut.pts[0], cut.pts[1], 0, out sx0, out sy0);
            context.Project(cut.pts[num - 2], cut.pts[num - 1], 0, out sx1, out sy1);//最后一个点
          
            //两个点时从左往右画
            //if (num < 6 && sx0 < 0)
            //{
            //    //处理第一个点
            //    double add_x = 0;
            //    double offset_x = add_x - sx0;
            //    double angle = Math.Atan2(Math.Abs(vy0), Math.Abs(vx0));
            //    double offset = offset_x / Math.Cos(angle);
            //    double add_y = sy0 + offset * vy0 / len;
            //    context.Unproject(add_x, add_y, 0, out x, out y, out z);
            //    cut.pts[0] = (float)x;
            //    cut.pts[1] = (float)y;
             
            //}
            ////两个点时从右往左画 
            //if (num < 6 && sx0 > vp[2])
            //{
            //    double add_x = vp[2];
            //    double offset_x = sx0 - add_x;
            //    double angle = Math.Atan2(Math.Abs(vy0), Math.Abs(vx0));
            //    double offset = offset_x / Math.Cos(angle);
            //    double add_y = sy0 + offset * vy0 / len;
            //    context.Unproject(add_x, add_y, 0, out x, out y, out z);
            //    cut.pts[0] = (float)x;
            //    cut.pts[1] = (float)y;
            //}

            for (int i = 0; i < num; i += 2)
            {
                context.Project(cut.pts[i], cut.pts[i + 1], 0, out sx, out sy);

                if (isStart)//找到起始点后接着找结束点,为了确保符号不因起始点而改变形状，起始点统一为第一个点
                {
                    //从左往右画
                    //if (sx0 < sx1)
                    //{
                    //    if (sx >= 0)
                    //    {
                    //        cut.start = i == 0 ? i : i - 2;
                    //        isStart = false;
                    //    }
                    //}
                    //else//从右往左画
                    //{
                    //    if (sx <=vp[2])
                    //    {
                    //        cut.start = i == 0 ? i : i - 2;
                    //        isStart = false;
                    //    }
                    //}
                    cut.start = 0;
                    isStart = false;
                }
                else
                {
                    //从左往右画,最后一个点在屏幕外
                    if (sx0 < sx1)
                    {
                        if (sx >=vp[2])
                        {
                            cut.end = i;
                            break;
                        }
                    }
                    else//从右往左
                    {
                        if (sx <= vp[0])
                        {
                            cut.end = i;
                            break;
                        }                      
                    }
                }
            }
           //如果start在屏幕外或者只有两个点时
            if (cut.start == num - 4)
            {
                cut.end = num - 2;
            }
            //最后一个点在屏幕内
            if (sx1 > 0 && sx1 < vp[2])
            {
                cut.end = num - 2;
            }
            return cut;
        }
        private InterpolatePosition FindNextPosition(Context context, InterpolatePosition start, double distance)
        {
            double sum_len = 0;
            double px = start.x;
            double py = start.y;
            float[] pts = _line.Data;
            int num = pts.Length;
            double sx0, sy0;
            double sx1, sy1;
            context.Project(start.x, start.y, 0, out sx0, out sy0);

            InterpolatePosition symbol_start = new InterpolatePosition() { next = num };
            for (int index = start.next; index < num; index += 2)
            {
                double x1 = pts[index];
                double y1 = pts[index + 1];
                context.Project(x1, y1, 0, out sx1, out sy1);

                double vx = sx1 - sx0;
                double vy = sy1 - sy0;
                double len = Math.Sqrt(vx * vx + vy * vy);
                sum_len += len;

                if (sum_len < distance)
                {
                    sx0 = sx1;
                    sy0 = sy1;
                    px = x1;
                    py = y1;
                    continue;
                }

                //calculate symbol's start point.
                //distance previous symbol SymbolDistance.
                //now sum_len is greater or equals to symbol distance.
                if (sum_len > distance)
                {
                    //start point is not on line's point.
                    //interpolate from previous point.
                    double d0 = distance - (sum_len - len);
                    double t = d0 / len;
                    //get previous point.
                    vx = x1 - px;
                    vy = y1 - py;
                    //interpolate
                    x1 = px + t * vx;
                    y1 = py + t * vy;
                    symbol_start.next = index;
                }
                else
                {
                    symbol_start.next = index + 2;
                }
                symbol_start.x = x1;
                symbol_start.y = y1;
                break;
            }
            return symbol_start;
        }

        private SymbolMesh MakeSymbol(Context context, InterpolatePosition start, int size, byte seq)
        {
            //add symbol data
            SymbolMesh symbol = new SymbolMesh();
            symbol.seq = seq;
            symbol.vertices.Add((float)start.x);
            symbol.vertices.Add((float)start.y);


            double sum_len = 0;
            double px = start.x;
            double py = start.y;
            float[] pts = _line.Data;
            int num = pts.Length;
            double sx0, sy0;
            double sx1, sy1;
            context.Project(start.x, start.y, 0, out sx0, out sy0);

            for (int index = start.next; index < num; index += 2)
            {
                double x1 = pts[index];
                double y1 = pts[index + 1];
                context.Project(x1, y1, 0, out sx1, out sy1);

                double vx = sx1 - sx0;
                double vy = sy1 - sy0;
                double len = Math.Sqrt(vx * vx + vy * vy);
                sum_len += len;

                if (sum_len < size)
                {
                    symbol.vertices.Add((float)x1);
                    symbol.vertices.Add((float)y1);
                    sx0 = sx1;
                    sy0 = sy1;
                    px = x1;
                    py = y1;
                    continue;
                }

                if (sum_len > size)
                {
                    //interpolate from previous point.
                    double d0 = size - (sum_len - len);
                    double t = d0 / len;
                    //get previous point.                   
                    vx = x1 - px;
                    vy = y1 - py;
                    //interpolate
                    x1 = px + t * vx;
                    y1 = py + t * vy;
                    symbol.tail.next = index;
                }
                else
                {
                    symbol.tail.next = index + 2;
                }
                symbol.is_completed = true;
                symbol.tail.x = x1;
                symbol.tail.y = y1;
                symbol.vertices.Add((float)x1);
                symbol.vertices.Add((float)y1);
                break;
            }

            if (symbol.is_completed)
            {
                //assembly vertices into symbols
                this.AssembleSymbol(context, symbol);
            }

            return symbol;
        }

        /// <summary>
        /// assemble symbol's shape, fill mesh using triangle strip primitive type.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="symbol"></param>
        protected virtual void AssembleSymbol(Context context, SymbolMesh symbol)
        {
            _asm.Assemble(context, symbol);
        }

        internal ISymbolAssembler Assembler
        {
            get { return _asm; }
            set { _asm = value; }
        }

        public override void Dispose()
        {
            base.Dispose();
            _line_drawable.Dispose();
        }
    }
}
