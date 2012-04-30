using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Treefrog.V2.ViewModel.Commands;
using Treefrog.Framework.Model;
using System.Collections.ObjectModel;
using Treefrog.V2.ViewModel.Annotations;
using Treefrog.Framework.Imaging.Drawing;
using Treefrog.Framework.Imaging;

namespace Treefrog.V2.ViewModel.Tools
{
    public class ObjectDrawTool : PointerTool
    {
        private ObjectPoolManagerService _poolManager;

        private CommandHistory _history;
        private ObjectLayer _layer;

        private ObservableCollection<Annotation> _annots;

        private SelectionAnnot _previewMarker;

        public ObjectDrawTool (CommandHistory history, ObjectLayer layer, ObservableCollection<Annotation> annots)
        {
            _history = history;
            _layer = layer;
            _annots = annots;
        }

        protected ObjectLayer Layer
        {
            get { return _layer; }
        }

        protected CommandHistory History
        {
            get { return _history; }
        }

        protected ObjectClass ActiveObjectClass
        {
            get
            {
                if (_poolManager == null) {
                    _poolManager = GalaSoft.MvvmLight.ServiceContainer.Default.GetService<ObjectPoolManagerService>();
                    if (_poolManager == null)
                        return null;
                }

                ObjectPoolItemVM activeClass = _poolManager.ActiveObjectClass;
                if (activeClass == null)
                    return null;
                return activeClass.ObjectClass;
            }
        }

        public override void StartPointerSequence (PointerEventInfo info)
        {
            switch (info.Type) {
                case PointerEventType.Primary:
                    StartDrawObjectSequence(info);
                    break;
            }

            UpdatePointerSequence(info);
        }

        public override void PointerPosition (PointerEventInfo info)
        {
            ShowPreviewMarker(info);
        }

        public override void PointerLeaveField ()
        {
            HidePreviewMarker();
        }

        #region Preview Marker

        private bool _previewMarkerVisible;
        private ObjectClass _activeClass;

        private void ShowPreviewMarker (PointerEventInfo info)
        {
            if (ActiveObjectClass == null)
                return;

            if (ActiveObjectClass != _activeClass) {
                HidePreviewMarker();
                _previewMarker = null;
                _activeClass = ActiveObjectClass;
            }

            if (!_previewMarkerVisible) {
                if (_previewMarker == null) {
                    _previewMarker = new SelectionAnnot();
                    _previewMarker.Fill = new PatternBrush(ActiveObjectClass.Image, 0.5);
                }

                _annots.Add(_previewMarker);
                _previewMarkerVisible = true;
            }

            _previewMarker.Start = new Point((int)info.X, (int)info.Y);
            _previewMarker.End = new Point((int)info.X + _activeClass.ImageBounds.Width, (int)info.Y + _activeClass.ImageBounds.Height);
        }

        private void HidePreviewMarker ()
        {
            _annots.Remove(_previewMarker);
            _previewMarkerVisible = false;
        }

        #endregion

        #region Draw Object Sequence

        private TileReplace2DCommand _drawCommand;

        private void StartDrawObjectSequence (PointerEventInfo info)
        {
            if (ActiveObjectClass == null)
                return;

            ObjectInstance inst = new ObjectInstance(ActiveObjectClass, (int)info.X, (int)info.Y);
            Layer.AddObject(inst);
        }

        #endregion
    }
}
