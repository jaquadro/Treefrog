using System.Collections.ObjectModel;
using Treefrog.Framework.Imaging;
using Treefrog.Framework.Imaging.Drawing;
using Treefrog.Framework.Model;
using Treefrog.ViewModel.Annotations;
using Treefrog.ViewModel.Commands;

namespace Treefrog.ViewModel.Tools
{
    public class ObjectDrawTool : ObjectPointerTool
    {
        private ObservableCollection<Annotation> _annots;

        private SelectionAnnot _previewMarker;

        public ObjectDrawTool (CommandHistory history, ObjectLayer layer, Size gridSize, ObservableCollection<Annotation> annots)
            : base(history, layer, gridSize)
        {
            _annots = annots;
        }

        protected override void DisposeManaged ()
        {
            HidePreviewMarker();
        }

        protected override void StartPointerSequenceCore (PointerEventInfo info)
        {
            switch (info.Type) {
                case PointerEventType.Primary:
                    StartDrawObjectSequence(info);
                    break;
                case PointerEventType.Secondary:
                    Cancel();
                    break;
            }

            UpdatePointerSequence(info);
        }

        protected override void PointerPositionCore (PointerEventInfo info)
        {
            base.PointerPositionCore(info);
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
                    _previewMarker = new SelectionAnnot();
                    _previewMarker.Fill = new PatternBrush(ActiveObjectClass.Image, 0.5);
                }

                _annots.Add(_previewMarker);
                _previewMarkerVisible = true;
            }

            Point xlat = new Point((int)info.X - _activeClass.ImageBounds.Width / 2, (int)info.Y - _activeClass.ImageBounds.Height / 2);
            if (SnapManager != null)
                xlat = SnapManager.Translate(xlat, SnappingTarget);

            _previewMarker.Start = xlat;
            _previewMarker.End = new Point(xlat.X + _activeClass.ImageBounds.Width, xlat.Y + _activeClass.ImageBounds.Height);
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

            ObjectInstance inst = new ObjectInstance(ActiveObjectClass, xlat.X, xlat.Y);
            History.Execute(new ObjectAddCommand(Layer, inst));
        }

        #endregion
    }
}
