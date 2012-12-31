using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Treefrog.Presentation.Layers;
using System.Reflection;
using Amphibian.Drawing;
using Treefrog.Presentation;

namespace Treefrog.Windows.Layers
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

        public override IEnumerable<DrawCommand> RenderCommands
        {
            get { yield break; }
        }
    }

    public class LocalRenderLayer : RenderLayer
    {
        private LocalRenderLayerPresenter _model;

        public new LocalRenderLayerPresenter Model
        {
            get { return _model; }
            set
            {
                _model = value;
                base.Model = value;
            }
        }

        protected override void RenderCore (SpriteBatch spriteBatch)
        {
            if (_model.ShouldRenderCoreS)
                _model.RenderCore(this, spriteBatch);
            else
                base.RenderCore(spriteBatch);
        }

        protected override void RenderCore (DrawBatch drawBatch)
        {
            if (_model.ShouldRenderCoreD)
                _model.RenderCore(this, drawBatch);
            else
                base.RenderCore(drawBatch);
        }

        protected override void RenderContent(SpriteBatch spriteBatch)
        {
            if (_model.ShouldRenderContentS)
                _model.RenderContent(this, spriteBatch);
            else
     	        base.RenderContent(spriteBatch);
        }

        protected override void RenderContent (DrawBatch drawBatch)
        {
            if (_model.ShouldRenderContentD)
                _model.RenderContent(this, drawBatch);
            else
                base.RenderContent(drawBatch);
        }
    }
}
