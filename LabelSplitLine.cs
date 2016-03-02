/* ***********************************************
 * Copyright (c) 2009-2010 luoshasha. All rights reserved";
 * CLR version: 4.0.30319.34014"
 * File name:   LabelSplitContourLines.cs"
 * Date:        12/11/2014 12:41:06 PM
 * Author :  sand
 * Email  :  luoshasha@foxmail.com
 * Description: 
	
 * History:  created by sand 12/11/2014 12:41:06 PM
 
 * ***********************************************/
using System;
using System.Collections.Generic;
using System.Text;
using CMA.MICAPS.Box2D.Graphics;

namespace CMA.MICAPS.Symbols
{
    using CMA.MICAPS.Box2D;
    using CMA.MICAPS.Box2D.Text;
    using CMA.MICAPS.GMap.Presenter;
    using SandLib.Math3D;
    using Line = CMA.MICAPS.Box2D.LineString2D<float>;
    using CMA.MICAPS.Symbols;
    using System.Drawing;
    using CMA.MICAPS.Box2D.Util;

    class LabelSplitLine : LabelLineSymbol
    {
        //protected List<float> _vertices;
        //protected List<short> _indices;
        private List<float> insert_vertices;
       // private List<short> insert_indices;
        //private bool isAddVer = true;
        //private bool isoffset = false;
       private const int Base_Interval = 15;
       // private int _distance = 600;
        private bool _cross = false;
        private int labe_count=0;
        public LabelSplitLine(Line line, string label, uint size, System.Drawing.Color label_color, bool rotation,bool cross=false)
            :base(line,label,size,label_color, LabelPosition.Section, rotation, true)
        {
            //_vertices = new List<float>(_line.Data.Length << 1);
            //_indices = new List<short>((_vertices.Capacity - 1) * 2);
            insert_vertices = new List<float>(_line.Data.Length << 1);
            //insert_indices = new List<short>((insert_vertices.Capacity - 1) * 2);
            this._cross = cross;
        }

        //public override void Render(SceneManager scene, Context context)
        //{
        //    context.SetRenderState(this.Material.SurfaceState);

        //    PrepareForDraw(scene, context);
        //    Matrix4 mat;
        //    double sx0, sy0;
        //    double sx1, sy1;
        //    double label_x=0, label_y=0;
        //        //处理头
        //        float ix = _line.Data[0];
        //        float iy = _line.Data[1];
        //        context.Project(ix, iy, 0, out sx0, out sy0);
          
        //        float px = _line.Data[2];
        //        float py = _line.Data[3];
        //        context.Project(px, py, 0, out sx1, out sy1);
        //        double vx = sx1 - sx0;
        //        double vy = sy1 - sy0;
        //        double len = Math.Sqrt(vx * vx + vy * vy);
        //        double angle = MathUtil.RAD_TO_DEG * Math.Atan2(py - iy, px - ix);
        //        if (px < ix)
        //        {
        //            angle += 180;
        //            label_x = ix;
        //            label_y = iy;
        //        }
        //        else
        //        {
        //            double x = sx0 - width * vx / len;
        //            double y = sy0 - width * vy / len;
        //            double z;
        //            context.Unproject(x, y, 0, out label_x, out label_y, out z);
        //        }
        //        _label.Position = new Vec3(label_x, label_y, 0);
        //        _label.ScreenOffset = new SandLib.Math3D.Vec2(0, _label.Text.Bounds.Size.y * .5);
        //        _label.GetTransformation(out mat);
        //        _label.Roll((float)(-angle));
        //        _label.Render(scene, context);
        //        _label.LoadTransformation(ref mat);

        //        for (int i = 0; i < labels.Count; i++)
        //        {
        //            _label.Position = new Vec3(labels[i].x, labels[i].y, 0);
        //            _label.ScreenOffset = new Vec2(0, labels[i].off);          
        //            _label.GetTransformation(out mat);
        //            _label.Roll(labels[i].angle);
        //            _label.Render(scene, context);
        //            _label.LoadTransformation(ref mat);
        //        }

        //            //int num = _line.Data.Length;

        //            //    float ex = _line.Data[num - 2];
        //            //    float ey = _line.Data[num - 1];
        //            //    //_label.Position = new Vec3(ex, ey, 0);
        //            //    context.Project(ex, ey, 0, out sx0, out sy0);
        //            //    float qx = _line.Data[num - 4];
        //            //    float qy = _line.Data[num - 3];
        //            //    context.Project(qx, qy, 0, out sx1, out sy1);
        //            //    vx = sx1 - sx0;
        //            //    vy = sy1 - sy0;
        //            //    len = Math.Sqrt(vx * vx + vy * vy);
        //            //    angle = MathUtil.RAD_TO_DEG * Math.Atan2(ey - qy, ex - qx);

        //            //    if (qx > ex)
        //            //    {
        //            //        angle += 180;
        //            //        double x = sx0 - width * vx / len;
        //            //        double y = sy0 - width * vy / len;
        //            //        double z;
        //            //        context.Unproject(x, y, 0, out label_x, out label_y, out z);
        //            //    }
        //            //    else
        //            //    {
        //            //        label_x = ex;
        //            //        label_y = ey;
        //            //    }

        //            //    _label.Position = new Vec3(label_x, label_y, 0);
        //            //    _label.ScreenOffset = new SandLib.Math3D.Vec2(0, _label.Text.Bounds.Size.y * .5);
        //            //    _label.GetTransformation(out mat);
        //            //    _label.Roll((float)(-angle));
        //            //    _label.Render(scene, context);
        //            //    _label.LoadTransformation(ref mat);

                 
        //    //PreIndices();
        //    //StaticBufferDrawHelper.DrawIndex(_vertices.ToArray(), _indices.ToArray(), Color.Red, PrimitiveType.Lines, _indices.Count);
        //}
        public override void Render(SceneManager scene, Context context)
        {
            context.SetRenderState(this.Material.SurfaceState);

            labels.Clear();
            insert_vertices.Clear();
            labe_count = 0;
            double sx, sy = 0;
            int interval = Math.Min(Base_Interval * (int)scene.ZoomLevel, context.SceneState.viewport[2]);
            interval = Math.Max(interval, 300);

            Matrix4 mat;

            context.Project(_line.Data[0], _line.Data[1], 0, out sx, out sy);
      
            index_pos next = new index_pos() { next = 0, ret = -1, sx = sx, sy = sy };
            var end = next;

            int len = _line.Data.Length;
           float off_x=0,off_y=0;
            do
            {
                if (_label != null)
                {
                   float width = _label.Text.Bounds.Size.x;

                    end = this.FindNextPosition(context, _line, next, width);
                 
                    if (end.ret < 0)
                    {
                        if (next.next == 0)
                        {
                            //context.SetRenderState(this.Material.SurfaceState);
                            StaticBufferDrawHelper.DrawArray(_line.Data, PrimitiveType.LineStrip, 0, len >> 1);
                        }
                        break;
                    }
                       
                    float angle = (float)(Math.Atan2(next.sy - end.sy, next.sx - end.sx) * CMA.MICAPS.Box2D.Util.MathUtil.RAD_TO_DEG);
                
                    //angle %= 360;
                    if (angle < 0)
                        angle += 360;
                    render_param par = new render_param();
                    //make text's up direction is unit y axis.
                    if (angle < 90 || angle > 270)
                    {
                        //1,4象限不旋转
                        par.x = (float)end.sx;
                        par.y = (float)end.sy;

                        //添加next坐标点（点1）
                        //添加从next点开始偏移len1-width的长度后的点（点2），如下图
                        //       end ------ 2               1(next)
                        //  -------*|      |*  len1-width   *--------- 
                        //           ------              
                        double vx = end.sx - next.sx;
                        double vy = end.sy - next.sy;
                        double len1 = Math.Sqrt(vx * vx + vy * vy);
                        double inn_x = next.sx + (len1 - width) * vx / len1;
                        double inn_y = next.sy + (len1 - width) * vy / len1;
                        double insert_x, insert_y, insert_z;
                        context.Unproject(next.sx, next.sy, 0, out insert_x, out insert_y, out insert_z);
                        insert_vertices.Add((float)insert_x);
                        insert_vertices.Add((float)insert_y);

                        context.Unproject(inn_x, inn_y, 0, out insert_x, out insert_y, out insert_z);
                        insert_vertices.Add((float)insert_x);
                        insert_vertices.Add((float)insert_y);

                        labe_count++;
                    }
                    else
                    {
                        //2,3象限旋转
                        par.x = (float)next.sx;
                        par.y = (float)next.sy;
                        angle += 180;

                        //添加从next开始偏移过width长度后的点(点1)
                        //添加end坐标点（点2），如下图
                        //     next ------- 1        2(end) 
                        //        *| width |*        *----------
                        //          -------
                        double vx = end.sx - next.sx;
                        double vy = end.sy - next.sy;
                        double len1 = Math.Sqrt(vx * vx + vy * vy);
                        double inn_x = next.sx + (width) * vx / len1;
                        double inn_y = next.sy + (width) * vy / len1;
                        double insert_x, insert_y, insert_z;
                        context.Unproject(inn_x, inn_y, 0, out insert_x, out insert_y, out insert_z);
                        insert_vertices.Add((float)insert_x);
                        insert_vertices.Add((float)insert_y);
                        context.Unproject(end.sx, end.sy, 0, out insert_x, out insert_y, out insert_z);
                        insert_vertices.Add((float)insert_x);
                        insert_vertices.Add((float)insert_y);

                        labe_count++;
                    }            
                  
                    par.angle = angle;
                    off_y= _label.Text.Bounds.Size.y * .5;
                    Quaternion q = Quaternion.FromEulerAnglesInDegrees(0, 0, angle);
                    var off = q * new Vec3(off_x, off_y, 0);
                    par.offx = off.x;
                    par.offy = off.y;
                    labels.Add(par);
                }     

                next = this.FindNextPosition(context, _line, end, interval, false);
             
                if (next.ret == -1)
                    next.next = len - 2;
                else if (next.ret == -2)
                    continue;
            

                //draw split line.
                int offset = end.next >> 1;
                int count = (next.next >> 1) - offset + 1;

                if (count > 1)
                {
                   // this.Material.SurfaceState.color = Color.Red;
                    //context.SetRenderState(this.Material.SurfaceState);
                    if(!_cross)
                    {
                        StaticBufferDrawHelper.DrawArray(_line.Data, PrimitiveType.LineStrip, offset, count);
                    }
                    //this.Material.SurfaceState.color = Color.Green;
                    //context.SetRenderState(this.Material.SurfaceState);
                    //StaticBufferDrawHelper.DrawArray(_line.Data, PrimitiveType.Points, offset, count);
                }
            } while (true);

            if (!_cross)
            {
                    int insert_num = insert_vertices.Count >> 1;
                    //绘制添加的点
                    //this.Material.SurfaceState.color = Color.Yellow;
                    //context.SetRenderState(this.Material.SurfaceState);
                    StaticBufferDrawHelper.DrawArray(insert_vertices.ToArray(), PrimitiveType.Lines, 0, insert_num);   
            }
            else
            {
                StaticBufferDrawHelper.DrawArray(_line.Data, PrimitiveType.LineStrip, 0, _point_count);
            }

            //double x1 = _line.Data[0];
            //double y1 = _line.Data[1];
            //float x2 = _line.Data[2];
            //float y2 = _line.Data[3];
            //double angle12 = MathUtil.RAD_TO_DEG * Math.Atan2(y2 - y1, x2 - x1);

            //if (angle12 < 90 || angle12 > 270)
            //{ }
            //else
            //{
            //    angle12 += 180;
            //}
        
                for (int i = 0; i < labels.Count; i++)
                {
                    _label.Position = new Vec3(labels[i].x, labels[i].y, 0);
                    _label.ScreenOffset = new Vec2(labels[i].offx, labels[i].offy);
                    _label.GetTransformation(out mat);     
                    _label.Roll(labels[i].angle);
                    _label.Render(scene, context);
                    _label.LoadTransformation(ref mat);
               }
                    
        }

        List<render_param> labels = new List<render_param>();

        //public void PrepareForDraw(SceneManager scene, Context context)
        //{
        //    labels.Clear();
        //    _vertices.Clear();
        //    width = _label.Text.Bounds.Size.x;
        //    float[] pts = _line.Data;
        //    int num = pts.Length;
        //    Matrix4 mat;
        //    double sx0, sy0;
        //    double sx1, sy1;

        //    _vertices.Add(pts[0]);
        //    _vertices.Add(pts[1]);

        //    context.Project(pts[0], pts[1], 0, out sx0, out sy0);

        //    double pre_remain = 0.0;
        //    double total_len = 0.0;
        //    double vec_len = 0.0;
        //    double last_x = 0.0;
        //    for (int i = 2; i < num - 1; i += 2)
        //    {
        //        context.Project(pts[i], pts[i + 1], 0, out sx1, out sy1);
        //        double vx, vy;
        //        vx = sx1 - sx0;
        //        vy = sy1 - sy0;
        //        double nvx = -vy;
        //        double nvy = vx;
        //        double reverse_nvx = vy;
        //        double reverse_nvy = -vx;

        //        vec_len = Math.Sqrt(vx * vx + vy * vy);
        //        total_len = vec_len + pre_remain;

        //        if (sx1 > sx0)
        //        {
        //            isoffset = false;
        //        }
        //        else
        //        {
        //            isoffset = true;
        //        }

        //        if (total_len >= _distance)
        //        {
        //            int insert_num = (int)(total_len / _distance);
        //            total_num += insert_num;
        //            for (int point_count = 0; point_count < insert_num; point_count++)
        //            {
        //                double insert_x = sx0 + ((point_count + 1) * _distance - pre_remain) * vx / vec_len;
        //                double insert_y = sy0 + ((point_count + 1) * _distance - pre_remain) * vy / vec_len;

        //                double sx, sy, sz;
        //                context.Unproject(insert_x, insert_y, 0, out sx, out sy, out sz);
        //                _vertices.Add((float)sx);
        //                _vertices.Add((float)sy);
        //                last_x = sx;
        //                //line_param lin_pa = new line_param();
                       
        //                PreIndices();
        //                StaticBufferDrawHelper.DrawIndex(_vertices.ToArray(), _indices.ToArray(), this.Material.SurfaceState.color, PrimitiveType.LineStrip, _indices.Count);
        //                _vertices.Clear();
                   
        //                double label_x, label_y;
        //                if (isoffset)
        //                {
        //                    double x = insert_x + width * vx / vec_len;
        //                    double y = insert_y + width * vy / vec_len;
        //                    context.Unproject(x, y, 0, out sx, out sy, out sz);
        //                    label_x = sx;
        //                    label_y = sy;
        //                }
        //                else
        //                {
        //                    label_x = sx;
        //                    label_y = sy;
        //                }
        //                float angle = (float)(Math.Atan2(sy1 - sy0, sx1 - sx0) * CMA.MICAPS.Box2D.Util.MathUtil.RAD_TO_DEG);
        //                if (angle < 0)
        //                    angle += 360;

        //                if (angle < 90 || angle > 270)
        //                {
        //                }
        //                else
        //                {
        //                    angle += 180;
        //                }

        //               // _label.Position = new Vec3(label_x, label_y, 0);
        //               // _label.ScreenOffset = new SandLib.Math3D.Vec2(0, _label.Text.Bounds.Size.y * .5);
        //               // _label.GetTransformation(out mat);
        //               // _label.Roll(angle);
        //               //// _label.Render(scene, context);
        //               // _label.LoadTransformation(ref mat);
        //                render_param par = new render_param();
        //                par.x = (float)label_x;
        //                par.y = (float)label_y;
        //                par.angle = angle;
        //                par.off = _label.Text.Bounds.Size.y * .5;
        //                labels.Add(par);

        //                double offset_x = insert_x + (width) * vx / vec_len;
        //                double offset_y = insert_y + (width) * vy / vec_len;
        //                context.Unproject(offset_x, offset_y, 0, out sx, out sy, out sz);
        //                _vertices.Add((float)sx);
        //                _vertices.Add((float)sy);

        //            }
        //            pre_remain = total_len - insert_num * _distance;
        //        }
        //        else
        //        {
        //            pre_remain += vec_len;
        //            //_vertices.Add(pts[i]);
        //            //_vertices.Add(pts[i + 1]);
        //            //if (!isoffset)
        //            //{
        //            //    if (_vertices[0] <= pts[i])
        //            //    {
        //            //        _vertices.Add(pts[i]);
        //            //        _vertices.Add(pts[i + 1]);
        //            //    }
        //            //}
        //            //else
        //            //{
        //            //    if (_vertices[0] >= pts[i])
        //            //    {
        //            //        _vertices.Add(pts[i]);
        //            //        _vertices.Add(pts[i + 1]);
        //            //    }
        //            //}
                    
        //        }
        //        sx0 = sx1;
        //        sy0 = sy1;
        //    }
        //    if (num < 6||pre_remain>0)
        //    {
        //        //_vertices.Add(pts[num - 2]);
        //        //_vertices.Add(pts[num - 1]);
        //        if (pts[num - 2] > pts[num - 4])
        //        {
        //            if (_vertices[0] < pts[num - 2])
        //            {
        //                _vertices.Add(pts[num - 2]);
        //                _vertices.Add(pts[num - 1]);
        //            }

        //        }
        //        else
        //        {
        //            if (last_x > pts[num - 2])
        //            {
        //                _vertices.Add(pts[num - 2]);
        //                _vertices.Add(pts[num - 1]);
        //            }
        //        }
        //    }
        //    PreIndices();
        //    StaticBufferDrawHelper.DrawIndex(_vertices.ToArray(), _indices.ToArray(), this.Material.SurfaceState.color, PrimitiveType.LineStrip, _indices.Count);
        //    //if (0 == total_num)
        //    //{
        //    //    StaticBufferDrawHelper.DrawArray(_line.Data, PrimitiveType.LineStrip, 0, _point_count);
        //    //}
        //    //total_num = 0;
        //}
        //private void PreIndices()
        //{
        //    _indices.Clear();
        //    int num=_vertices.Count>>1;
        //    for (int i = 0; i < num; i++)
        //    {
        //        _indices.Add((short)i);
        //    }
        //}
        struct index_pos
        {
            public double sx, sy;
            public int next;  //-1 distance is not enough, -2 out of screen bounds.
            public int ret;
        }
       
        private index_pos FindNextPosition(Context context, Line line, index_pos start, double distance, bool screen_clipping = false)
        {
            //double sum_len = 0;
            float[] pts = line.Data;
            int num = pts.Length;

            double sx0 = start.sx, sy0 = start.sy;
            double sx1 = sx0, sy1 = sy0;
            index_pos end = new index_pos() { next = num - 2, ret = -1 };

            double min_sx = sx0, max_sx = sx0;
            double min_sy = sy0, max_sy = sy0;
            double vx=0, vy=0,len=0;
            
            int time = 0;
            for (int index = start.next + 2; index < num; index += 2)
            {
                double x1 = pts[index];
                double y1 = pts[index + 1];
                
                context.Project(x1, y1, 0, out sx1, out sy1);

                 //vx = sx1 - sx0;
                 //vy = sy1 - sy0;
                vx = sx1 - start.sx;
                vy = sy1 - start.sy;
                 len = Math.Sqrt(vx * vx + vy * vy);
               // sum_len += len;

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

                if (len < distance)
                {
                    sx0 = sx1;
                    sy0 = sy1;
                    time++;
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
      
    }
}
