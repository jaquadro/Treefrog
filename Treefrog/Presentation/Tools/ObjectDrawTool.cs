using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using Treefrog.Presentation.Commands;
using Treefrog.Framework.Model;
using Treefrog.Framework.Imaging;
using Treefrog.Framework.Imaging.Drawing;
using Treefrog.Presentation.Annotations;

namespace Treefrog.Presentation.Tools
{
    public class ObjectDrawTool : ObjectPointerTool
    {
        private ObservableCollection<Annotation> _annots;

        private ImageAnnot _previewMarker;

        public ObjectDrawTool (CommandHistory history, ObjectLayer layer, Size gridSize, ObservableCollection<Annotation> annots)
            : base(history, layer, gridSize)
        {
            _annots = annots;
        }

        protected override void DisposeManaged ()
        {
            HidePreviewMarker();
        }

        protected override void StartPointerSequenceCore (PointerEventInfo info, ILevelGeometry viewport)
        {
            switch (info.Type) {
                case PointerEventType.Primary:
                    StartDrawObjectSequence(info);
                    break;
                case PointerEventType.Secondary:
                    Cancel();
                    break;
            }

            UpdatePointerSequence(info, viewport);
        }

        protected override void PointerPositionCore (PointerEventInfo info, ILevelGeometry viewport)
        {
            base.PointerPositionCore(info, viewport);
            ShowPreviewMarker(info);
        }

        protected override void PointerLeaveFieldCore ()
        {
            base.PointerLeaveFieldCore();
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
                    _previewMarker = new ImageAnnot() {
                        Image = ActiveObjectClass.Image,
                        BlendColor = new Color(255, 255, 255, 128),
                    };
                }

                _annots.Add(_previewMarker);
                _previewMarkerVisible = true;
            }

            Point xlat = new Point((int)info.X - _activeClass.ImageBounds.Width / 2, (int)info.Y - _activeClass.ImageBounds.Height / 2);
            if (SnapManager != null)
                xlat = SnapManager.Translate(xlat, SnappingTarget);

            _previewMarker.Position = xlat;
        }

        private void HidePreviewMarker ()
        {
            _annots.Remove(_previewMarker);
            _previewMarkerVisible = false;
        }

        #endregion

        #region Draw Object Sequence

        private void StartDrawObjectSequence (PointerEventInfo info)
        {
            if (ActiveObjectClass == null)
                return;

            Point xlat = new Point((int)info.X - _activeClass.ImageBounds.Width / 2, (int)info.Y - _activeClass.ImageBounds.Height / 2);
            if (SnapManager != null)
                xlat = SnapManager.Translate(xlat, SnappingTarget);

            ObjectInstance inst = new ObjectInstance(ActiveObjectClass, xlat.X + ActiveObjectClass.Origin.X, xlat.Y + ActiveObjectClass.Origin.Y);
            History.Execute(new ObjectAddCommand(Layer, inst));
        }

        #endregion
    }
}
