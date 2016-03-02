/* ***********************************************
 * Copyright (c) 2012-2013 luoshasha. All rights reserved";
 * CLR version: 4.0.30319.18444
 * File name:   TemperatureLineSymbol
 * Date:        2014-10-31 11:20:58 AM
 * Author :  luoshasha(sand)
 * Email  :  luoshasha@foxmail.com
 * Description: 
	
 * History:  created by luoshasha(sand) 2014-10-31 11:20:58 AM
 
 * ***********************************************/
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CMA.MICAPS.Box2D;
using SandLib.Math3D;

namespace CMA.MICAPS.Symbols
{
    using System.Diagnostics;
    using CMA.MICAPS.Box2D.Graphics;
    using CMA.MICAPS.Box2D.Text;
    using CMA.MICAPS.Box2D.Util;
    using Line = LineString2D<float>;

    class LabelLineSymbol : LineSymbol
    {
        protected Label _label;
        private bool _split;
        private LabelPosition _position;
        protected render_param[] _render_params;
        protected bool isAddLabel = false;
        protected List<float> _vertices;
        protected List<int> _indices;
        protected struct render_param
        {
            public float x, y;
            public float angle;
            public float offx, offy;
        }

        public LabelLineSymbol(Line line)
            :base(line)
        { 
           
        }
        public LabelLineSymbol(Line line, string label, uint size, System.Drawing.Color label_color, LabelPosition position, bool rotation, bool split = false)
            : base(line)
        {
            //if (string.IsNullOrEmpty(label))
            //    throw new ArgumentNullException("Can't create label line with empty label!");

            _split = split;

            var c = Color4F.FromSystemColor(label_color);

            _label = new Label();
            _label.Text = new Box2D.Text.TextBlock(FontManager.DefaultFont, new FontSize(size), label, false);
            _label.Color = c;
            _label.Text.Font.LoadAllGlyphs();
            _label.ScreenFixed =_split;
           

            //calculate label position.
            int len = line.Data.Length;
            Debug.Assert(len >= 4);

            _position = position;
            float x = 0, y = 0;
            if (!_split)
            {
                switch (position)
                {
                    case LabelPosition.Tail:
                        float ex = line.Data[len - 2];
                        float ey = line.Data[len - 1];
                       _render_params = new render_param[1];
                       _render_params[0].x = ex;
                       _render_params[0].y = ey;
                  
                       y = _label.Text.Bounds.Size.y * .5;
                        if (rotation)
                        {
                            float px = line.Data[len - 4];
                            float py = line.Data[len - 3];
                            double angle = MathUtil.RAD_TO_DEG * Math.Atan2(ey - py, ex - px);

                            if (px > ex)
                            {
                                angle += 180;
                                x=-_label.Text.Bounds.Size.x;  
                            }
                            _render_params[0].angle = (float)(-angle);
                            Quaternion q = Quaternion.FromEulerAnglesInDegrees(0, 0, -angle);
                            var offset = q * new Vec3(x, y, 0);
                            _render_params[0].offx = offset.x;
                            _render_params[0].offy=offset.y; 
                        }
                        break;
                    case LabelPosition.Head:
                        float sx = line.Data[0];
                        float sy = line.Data[1];
                        _render_params = new render_param[1];
                         _render_params[0].x = sx;
                        _render_params[0].y = sy;
                        y=_label.Text.Bounds.Size.y * .5;           
                        if (rotation)
                        {
                            float px = line.Data[2];
                            float py = line.Data[3];
                            double angle = MathUtil.RAD_TO_DEG * Math.Atan2(py - sy, px - sx);

                            if (px < sx)
                            {
                                angle += 180;
                            }
                            else
                            {
                                x= -_label.Text.Bounds.Size.x;
                            }
                            
                            _render_params[0].angle = (float)(-angle);

                            Quaternion q = Quaternion.FromEulerAnglesInDegrees(0, 0, -angle);
                            var offset = q * new Vec3(x, y, 0);
                            _render_params[0].offx =offset.x;
                            _render_params[0].offy = offset.y;
                        }
                        break;
                    case LabelPosition.HeadAndTail:

                        _render_params = new render_param[2];
                        //处理头
                        float ix = line.Data[0];
                        float iy = line.Data[1];
                        _render_params[0].x = ix;
                        _render_params[0].y = iy;
                       
                        y=_label.Text.Bounds.Size.y * .5;
                        if (rotation)
                        {
                            float px = line.Data[2];
                            float py = line.Data[3];
                            
                            double angle = MathUtil.RAD_TO_DEG * Math.Atan2(py - iy, px - ix);
                           
                            //if (_line.Data[0] > _line.Data[len - 2])
                            //{
                            //    angle += 180;
                            //}
                            //else
                            //{
                            //    x = -_label.Text.Bounds.Size.x; 
                               
                            //}
                            if (angle < 0) angle += 360;
                            if (_line.Data[0] < _line.Data[len - 2])
                            {
                                if (angle < 90 || angle > 270)
                                {
                                    //1，4象限
                                    // if(_line.Data[0]>_line.Data[2])
                                    x = -_label.Text.Bounds.Size.x;
                                }
                                else
                                {
                                    //2,3象限
                                    //  angle += 180;
                                    if (_line.Data[0] > _line.Data[2])
                                        x = -_label.Text.Bounds.Size.x;
                                }
                            }
                            else
                            {
                                if (angle < 90 || angle > 270)
                                {
                                    if(_line.Data[0]<_line.Data[2])
                                    x = -_label.Text.Bounds.Size.x;
                                }
                                else
                                {
                                    angle += 180;
                                }
                              
                            }
                           
                         
                            _render_params[0].angle = (float)(-angle);

                            Quaternion q = Quaternion.FromEulerAnglesInDegrees(0, 0, -angle);
                            var offset = q * new Vec3(x,y , 0);

                            _render_params[0].offx = offset.x;
                            _render_params[0].offy = offset.y;
                        }

                        //处理尾部
                        float cx = line.Data[len - 2];
                        float cy = line.Data[len - 1];
                         _render_params[1].x = cx;
                        _render_params[1].y = cy;
                        x = y = 0;
                        y = _label.Text.Bounds.Size.y * .5;
                        if (rotation)
                        {
                            float mx = line.Data[len - 4];
                            float my = line.Data[len - 3];
                            double angle = MathUtil.RAD_TO_DEG * Math.Atan2(cy - my, cx - mx);

                            //判断第一个点和最后个点的位置关系
                            if (_line.Data[0] < _line.Data[len - 2])
                            {
                                //从左向右画，此时不旋转
                            }
                            else
                            {
                                //从右向左画，此时旋转
                                angle += 180;
                                x = -_label.Text.Bounds.Size.x;
                            }
               
                            
                            _render_params[1].angle = (float)(-angle);
                            Quaternion q = Quaternion.FromEulerAnglesInDegrees(0, 0, -angle);
                            var offset = q * new Vec3(x, y, 0);
                            _render_params[1].offx = offset.x;
                            _render_params[1].offy = offset.y; 
                        }
                        break;                   
                }
            }
            else
            {
                _label.ScreenOffset = new SandLib.Math3D.Vec2(0, _label.Text.Bounds.Size.y * .5); 
            }
        }

        public override void UpdateLineData()
        {
            base.UpdateLineData();
            if (!_split && _label != null)
            {
                switch (_position)
                {
                    case LabelPosition.Tail:
                        int len = _line.Data.Length;
                        float lx = _line.Data[len - 2];
                        float ly = _line.Data[len - 1];
                       // _label.Position = new Vec3(lx, ly, 0);
                        _render_params[0].x = lx;
                        _render_params[0].y = ly;
                        break;
                    case LabelPosition.Head:
                       // _label.Position = new Vec3(_line.Data[0], _line.Data[1], 0);
                        _render_params[0].x = _line.Data[0];
                        _render_params[0].y = _line.Data[1];
                        break;
                    case LabelPosition.HeadAndTail:
                       // _label.Position = new Vec3(_line.Data[0], _line.Data[1], 0);
                        _render_params[0].x = _line.Data[0];
                        _render_params[0].y = _line.Data[1];
                        int len1 = _line.Data.Length;
                        float lx1 = _line.Data[len1 - 2];
                        float ly1 = _line.Data[len1 - 1];
                       // _label1.Position = new Vec3(lx1, ly1, 0);
                        _render_params[1].x = lx1;
                        _render_params[1].y = ly1;
                        break;
                }
            }
        }

        public override Label Label
        {
            get
            {
                return _label;
            }
        }

        struct index_pos
        {
            public double sx, sy;
            public int next;  //-1 distance is not enough, -2 out of screen bounds.
            public int ret;
        }

        public override void Render(Box2D.Graphics.SceneManager scene, Box2D.Graphics.Context context)
        {
            Matrix4 mat;
            context.SetRenderState(this.Material.SurfaceState);
            int len1 = _line.Data.Length;

            double x1, y1, x2, y2;
            float first_x = _line.Data[0];
            float first_y = _line.Data[1];
            context.Project(first_x, first_y, 0,out  x1,out  y1);
            float last_x = _line.Data[len1 - 2];
            float last_y = _line.Data[len1 - 1];
            context.Project(last_x, last_y, 0, out x2, out y2);
            double nx=x2-x1;
            double ny=y2-y1;
            double len2 = Math.Sqrt(nx * nx + ny * ny);
          
            if (!_split)
            {
                _line_drawable.Draw(PrimitiveType.LineStrip, 0, _point_count);
                if (len2 > _label.Text.Bounds.Size.x * 2)
                {
                   // double beishu = (len2) /( _label.Text.Bounds.Size.x);
                    //if (beishu > 10)
                    //{
                        context.PushOrtho2D();
                        for (int i = 0; i < _render_params.Length; i++)
                        {
                            _label.Position = new Vec3(_render_params[i].x, _render_params[i].y, 0);
                            _label.ScreenOffset = new Vec2(_render_params[i].offx, _render_params[i].offy);
                            _label.GetTransformation(out mat);
                            _label.Roll(_render_params[i].angle);
                            _label.Render(scene, context);
                            _label.LoadTransformation(ref mat);
                        }
                        context.PopOrtho2D();
                   // }
                    //else
                    //{
                    //    float an = -_render_params[0].angle;
                    //    float x = 0;
                    //    if (_line.Data[0] > _line.Data[_line.Data.Length - 2])
                    //    {
                           
                    //        x = -_label.Text.Bounds.Size.x;
                    //    }
                    //    else
                    //    {
                    //        if (an < 90 || an > 270)
                    //        {
                    //            x = -_label.Text.Bounds.Size.x;
                    //        }
                    //        else
                    //        {
                    //            an += 180;
                    //        }

                    //    }
                    //    Quaternion q = Quaternion.FromEulerAnglesInDegrees(0, 0, -an);
                    //    var off = q * new Vec3(x, _label.Text.Bounds.Size.y * .5, 0);

                    //    _label.Position = new Vec3(_line.Data[0], _line.Data[1], 0);
                    //    _label.ScreenOffset = new Vec2(off.x, off.y);
                    //    _label.GetTransformation(out mat);
                    //    _label.Roll(-an);
                    //    _label.Render(scene, context);
                    //    _label.LoadTransformation(ref mat);
                    //}
                  
                }
                else
                {
                    float an = -_render_params[0].angle;
                    float x = 0;
                    if (_line.Data[_line.Data.Length - 2] > _line.Data[0]) //判断线的方向从左往右还是从右往左
                    {
                        //x = -_label.Text.Bounds.Size.x;
                        if (an < 90 || an > 270)
                        { x = -_label.Text.Bounds.Size.x; }
                        else
                        { an += 180; }
                        
                        Quaternion q = Quaternion.FromEulerAnglesInDegrees(0, 0, -an);
                        var off = q * new Vec3(x, _label.Text.Bounds.Size.y * .5, 0);

                        _label.Position = new Vec3(_line.Data[0], _line.Data[1], 0);
                        _label.ScreenOffset = new Vec2(off.x, off.y);
                        _label.GetTransformation(out mat);
                        _label.Roll(-an);
                        _label.Render(scene, context);
                        _label.LoadTransformation(ref mat);
                    }
                    else
                    {
                        _label.Position = new Vec3(_render_params[0].x, _render_params[0].y, 0);
                        _label.ScreenOffset = new Vec2(_render_params[0].offx, _render_params[0].offy);
                        _label.GetTransformation(out mat);
                        _label.Roll(_render_params[0].angle);
                        _label.Render(scene, context);
                        _label.LoadTransformation(ref mat);
                    }
                  
                }
                                  
            }
            else
            {
                double sx, sy = 0;
                int interval = Math.Min(20 * (int)scene.ZoomLevel, context.SceneState.viewport[2]);
                interval = Math.Max(interval, 120);

                //Matrix4 mat;

                Line line = _line;

                context.Project(line.Data[0], line.Data[1], 0, out sx, out sy);

                index_pos next = new index_pos() { next = 0, ret = -1, sx = sx, sy = sy };
                var end = next;

                int len = line.Data.Length;

                do
                {
                    float width = _label.Text.Bounds.Size.x;
                    end = this.FindNextPosition(context, line, next, width);
                    if (end.ret < 0)
                        break;

                    _label.Position = new SandLib.Math3D.Vec3(next.sx, next.sy, 0);
                    _label.GetTransformation(out mat);
                    float angle = 180 + (float)(Math.Atan2(next.sy - end.sy, next.sx - end.sx) * CMA.MICAPS.Box2D.Util.MathUtil.RAD_TO_DEG);
                    _label.Roll(angle);
                    _label.Render(scene, context);
                    _label.LoadTransformation(ref mat);

                    next = this.FindNextPosition(context, line, end, interval, false);

                    if (next.ret < 0)
                        break;
                    //if (next.ret == -1)
                    //    next.next = len - 2;
                    //else if (next.ret == -2)
                    //    return;

                    ////draw split line.
                    //int offset = end.next >> 1;
                    //int count = (next.next >> 1) - offset + 1;

                    //if (count > 1)
                    //{
                    //    StaticBufferDrawHelper.DrawArray(line.Data, PrimitiveType.LineStrip, offset, count);
                    //}
                } while (true);

                StaticBufferDrawHelper.DrawArray(line.Data, PrimitiveType.LineStrip, 0, len >> 1);
            }
        }

        private index_pos FindNextPosition(Context context, Line line, index_pos start, double distance, bool screen_clipping = false)
        {
            double sum_len = 0;
            float[] pts = line.Data;
            int num = pts.Length;
            double sx0 = start.sx, sy0 = start.sy;
            double sx1 = sx0, sy1 = sx1;
            index_pos end = new index_pos() { next = num - 2, ret = -1 };

            double min_sx = sx0, max_sx = sx0;
            double min_sy = sy0, max_sy = sy0;

            for (int index = start.next + 2; index < num; index += 2)
            {
                double x1 = pts[index];
                double y1 = pts[index + 1];

                context.Project(x1, y1, 0, out sx1, out sy1);

                double vx = sx1 - sx0;
                double vy = sy1 - sy0;
                double len = Math.Sqrt(vx * vx + vy * vy);
                sum_len += len;

                if (screen_clipping)
                {
                    if (sx1 > max_sx)
                        max_sx = sx1;
                    if (sx1 < min_sx)
                        min_sx = sx1;

                    if (sy1 > max_sy)
                        max_sy = sy1;
                    if (sy1 < min_sy)
                        min_sy = sy1;
                }

                if (sum_len < distance)
                {
                    sx0 = sx1;
                    sy0 = sy1;
                    continue;
                }

                //now sum_len is greater or equals to symbol distance.
                end.next = index;
                end.ret = 0;

                if (screen_clipping)
                {
                    var vp = context.SceneState.viewport;
                    bool intersects = ((min_sx <= vp[2]) && (min_sy <= vp[3]) && max_sx >= 0 && max_sy >= 0);
                    if (!intersects)
                        end.ret = -2;
                }
                break;
            }

            end.sx = sx1;
            end.sy = sy1;

            return end;
        }
      
        public override void Dispose()
        {
            base.Dispose();
            if (_label != null) 
                _label.Dispose();
            //if (_label1 != null)
            //    _label1.Dispose();
        }
    }
}
