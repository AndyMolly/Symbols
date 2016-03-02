using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CMA.MICAPS.Symbols
{
    using CMA.MICAPS.Box2D;
    using SandLib.Math3D;
    using Line = CMA.MICAPS.Box2D.LineString2D<float>;
    class SoildCircle : LineAndSoildCircle
    {
        public SoildCircle(Line line, string label, uint size, System.Drawing.Color label_color, LabelPosition position, bool rotation, int distance, int radius, bool split=false)
            : base(line, label, size, label_color, position, rotation, distance, radius, split)
        {
            isAddLabel = true;
        }
        public SoildCircle(Line line, int distance, int radius)
            : base(line, distance, radius)
        {
            isAddLabel = false;
        }
        public override void Render(Box2D.Graphics.SceneManager scene, Box2D.Graphics.Context context)
        {
            Matrix4 mat;
            if (isAddLabel)
            {
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
            }

            context.SetRenderState(this.Material.SurfaceState);

            PrepareForDraw(context);
            PrepareIndices();

            var color = this.Material.SurfaceState.color;

            for (int i = 0; i < _circle_vertices.Count; i++)
            {
                StaticBufferDrawHelper.DrawIndex(_circle_vertices[i].ToArray(),_circle_idices.ToArray(), color, PrimitiveType.TriangleFan, _circle_idices.Count);
            }
        }
    }
}
