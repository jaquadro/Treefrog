using System;
using System.Reflection;
using LilyPath;
using Microsoft.Xna.Framework.Graphics;
using Treefrog.Presentation.Layers;

namespace Treefrog.Render.Layers
{
    public class LocalRenderCore
    {
        public virtual void RenderCore (RenderLayer context, SpriteBatch spriteBatch)
        { }

        public virtual void RenderCore (RenderLayer context, DrawBatch drawBatch)
        { }

        public virtual void RenderContent (RenderLayer context, SpriteBatch spriteBatch)
        { }

        public virtual void RenderContent (RenderLayer context, DrawBatch drawBatch)
        { }
    }

    public class LocalRenderLayerPresenter : RenderLayerPresenter
    {
        private LocalRenderCore _renderCore;

        public LocalRenderLayerPresenter (LocalRenderCore renderCore)
        {
            _renderCore = renderCore;

            MethodInfo infoRenderCoreS = _renderCore.GetType().GetMethod("RenderCore", new Type[] { typeof(RenderLayer), typeof(SpriteBatch) });
            MethodInfo infoRenderCoreD = _renderCore.GetType().GetMethod("RenderCore", new Type[] { typeof(RenderLayer), typeof(DrawBatch) });
            MethodInfo infoRenderContentS = _renderCore.GetType().GetMethod("RenderContent", new Type[] { typeof(RenderLayer), typeof(SpriteBatch) });
            MethodInfo infoRenderContentD = _renderCore.GetType().GetMethod("RenderContent", new Type[] { typeof(RenderLayer), typeof(DrawBatch) });

            ShouldRenderCoreS = IsOverride(infoRenderCoreS);
            ShouldRenderCoreD = IsOverride(infoRenderCoreD);
            ShouldRenderContentS = IsOverride(infoRenderContentS);
            ShouldRenderContentD = IsOverride(infoRenderContentD);
        }

        private bool IsOverride (MethodInfo info)
        {
            MethodInfo baseInfo = info.GetBaseDefinition();
            return info.DeclaringType != baseInfo.DeclaringType;
        }

        public bool ShouldRenderCoreS { get; private set; }
        public bool ShouldRenderCoreD { get; private set; }
        public bool ShouldRenderContentS { get; private set; }
        public bool ShouldRenderContentD { get; private set; }

        public void RenderCore (RenderLayer context, SpriteBatch spriteBatch)
        {
            _renderCore.RenderCore(context, spriteBatch);
        }

        public void RenderCore (RenderLayer context, DrawBatch drawBatch)
        {
            _renderCore.RenderCore(context, drawBatch);
        }

        public void RenderContent (RenderLayer context, SpriteBatch spriteBatch)
        {
            _renderCore.RenderContent(context, spriteBatch);
        }

        public void RenderContent (RenderLayer context, DrawBatch drawBatch)
        {
            _renderCore.RenderContent(context, drawBatch);
        }
    }

    public class LocalRenderLayer : RenderLayer
    {
        public LocalRenderLayer (LocalRenderLayerPresenter model)
            : base(model)
        { }

        protected new LocalRenderLayerPresenter Model
        {
            get { return ModelCore as LocalRenderLayerPresenter; }
        }

        protected override void RenderCore (SpriteBatch spriteBatch)
        {
            if (Model.ShouldRenderCoreS)
                Model.RenderCore(this, spriteBatch);
            else
                base.RenderCore(spriteBatch);
        }

        protected override void RenderCore (DrawBatch drawBatch)
        {
            if (Model.ShouldRenderCoreD)
                Model.RenderCore(this, drawBatch);
            else
                base.RenderCore(drawBatch);
        }

        protected override void RenderContent(SpriteBatch spriteBatch)
        {
            if (Model.ShouldRenderContentS)
                Model.RenderContent(this, spriteBatch);
            else
     	        base.RenderContent(spriteBatch);
        }

        protected override void RenderContent (DrawBatch drawBatch)
        {
            if (Model.ShouldRenderContentD)
                Model.RenderContent(this, drawBatch);
            else
                base.RenderContent(drawBatch);
        }
    }
}
