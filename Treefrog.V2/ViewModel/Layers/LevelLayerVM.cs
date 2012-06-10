using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Treefrog.Framework.Model;
using System.Windows;
using Treefrog.Framework;

namespace Treefrog.ViewModel.Layers
{
    public interface ILayerEditTarget
    {
        bool CanCut { get; }
        bool CanCopy { get; }
        bool CanPaste { get; }
        bool CanDelete { get; }

        bool CanSelectAll { get; }
        bool CanSelectNone { get; }

        void Cut ();
        void Copy ();
        void Paste ();
        void Delete ();

        void SelectAll ();
        void SelectNone ();

        event EventHandler CanCutChanged;
        event EventHandler CanCopyChanged;
        event EventHandler CanPasteChanged;
        event EventHandler CanDeleteChanged;

        event EventHandler CanSelectAllChanged;
        event EventHandler CanSelectNoneChanged;
    }

    public class LevelLayerVM : RenderLayerVM, ILayerEditTarget
    {
        private LevelDocumentVM _level;
        private Layer _layer;
        private ViewportVM _viewport;

        public LevelLayerVM (LevelDocumentVM level, Layer layer, ViewportVM viewport)
        {
            _level = level;
            _layer = layer;
            _viewport = viewport ?? new ViewportVM()
            {
                Viewport = new Size(layer.LayerWidth, layer.LayerHeight),
            };

            _layer.NameChanged += HandleNameChanged;
            _layer.VisibilityChanged += HandleVisibilityChanged;
            _layer.OpacityChanged += HandleOpacityChanged;
        }

        public LevelLayerVM (LevelDocumentVM level, Layer layer)
            : this(level, layer, null)
        {
        }

        public virtual void Activate ()
        { }

        public virtual void Deactivate ()
        { }

        protected LevelDocumentVM Level
        {
            get { return _level; }
        }

        protected Layer Layer
        {
            get { return _layer; }
        }

        protected ViewportVM Viewport
        {
            get { return _viewport; }
        }

        public override string LayerName
        {
            get { return _layer.Name; }
            set { _layer.Name = value; }
        }

        public override bool IsVisible
        {
            get { return _layer.IsVisible; }
            set
            {
                if (_layer.IsVisible != value) {
                    _layer.IsVisible = value;
                    RaisePropertyChanged("IsVisible");
                }
            }
        }

        public virtual Vector GetCoordinates (double x, double y)
        {
            return new Vector(x, y);
        }

        public virtual Rect CoordinateBounds
        {
            get { return new Rect(0, 0, LayerWidth, LayerHeight); }
        }

        public override double LayerWidth
        {
            get { return _layer.LayerWidth; }
        }

        public override double LayerHeight
        {
            get { return _layer.LayerHeight; }
        }

        public override IEnumerable<DrawCommand> RenderCommands
        {
            get { yield break; }
        }

        public float Opacity
        {
            get { return _layer.Opacity; }
            set { _layer.Opacity = value; }
        }

        private void HandleNameChanged (object sender, NameChangedEventArgs e)
        {
            RaisePropertyChanged("LayerName");
        }

        private void HandleVisibilityChanged (object sender, EventArgs e)
        {
            RaisePropertyChanged("IsVisible");
        }

        private void HandleOpacityChanged (object sender, EventArgs e)
        {
            RaisePropertyChanged("Opacity");
        }

        #region Pointer Handlers

        public virtual void HandleStartPointerSequence (PointerEventInfo info)
        { }

        public virtual void HandleEndPointerSequence (PointerEventInfo info)
        { }

        public virtual void HandleUpdatePointerSequence (PointerEventInfo info)
        { }

        public virtual void HandlePointerPosition (PointerEventInfo info)
        { }

        public virtual void HandlePointerLeaveField ()
        { }

        #endregion

        #region Edit Handlers

        public virtual bool CanCut
        {
            get { return false; }
        }

        public virtual bool CanCopy
        {
            get { return false; }
        }

        public virtual bool CanPaste
        {
            get { return false; }
        }

        public virtual bool CanDelete
        {
            get { return false; }
        }

        public virtual bool CanSelectAll
        {
            get { return false; }
        }

        public virtual bool CanSelectNone
        {
            get { return false; }
        }

        public virtual void Cut ()
        { }

        public virtual void Copy ()
        { }

        public virtual void Paste ()
        { }

        public virtual void Delete ()
        { }

        public virtual void SelectAll ()
        { }

        public virtual void SelectNone ()
        { }

        public event EventHandler CanCutChanged;
        public event EventHandler CanCopyChanged;
        public event EventHandler CanPasteChanged;
        public event EventHandler CanDeleteChanged;
        public event EventHandler CanSelectAllChanged;
        public event EventHandler CanSelectNoneChanged;

        protected virtual void OnCanCutChanged (EventArgs e)
        {
            if (CanCutChanged != null)
                CanCutChanged(this, e);
        }

        protected virtual void OnCanCopyChanged (EventArgs e)
        {
            if (CanCopyChanged != null)
                CanCopyChanged(this, e);
        }

        protected virtual void OnCanPasteChanged (EventArgs e)
        {
            if (CanPasteChanged != null)
                CanPasteChanged(this, e);
        }

        protected virtual void OnCanDeleteChanged (EventArgs e)
        {
            if (CanDeleteChanged != null)
                CanDeleteChanged(this, e);
        }

        protected virtual void OnCanSelectAllChanged (EventArgs e)
        {
            if (CanSelectAllChanged != null)
                CanSelectAllChanged(this, e);
        }

        protected virtual void OnCanSelectNoneChanged (EventArgs e)
        {
            if (CanSelectNoneChanged != null)
                CanSelectNoneChanged(this, e);
        }

        #endregion
    }
}
