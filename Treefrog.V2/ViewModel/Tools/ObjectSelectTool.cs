using System.Collections.Generic;
using System.Collections.ObjectModel;
using Treefrog.Framework.Imaging;
using Treefrog.Framework.Imaging.Drawing;
using Treefrog.Framework.Model;
using Treefrog.V2.ViewModel.Annotations;
using Treefrog.V2.ViewModel.Commands;

namespace Treefrog.V2.ViewModel.Tools
{
    public class ObjectSelectTool : ObjectPointerTool
    {
        private ObservableCollection<Annotation> _annots;

        public ObjectSelectTool (CommandHistory history, ObjectLayer layer, Size gridSize, ObservableCollection<Annotation> annots)
            : base(history, layer, gridSize)
        {
            _annots = annots;
        }

        protected override void DisposeManaged ()
        {
            ClearSelected();
        }

        protected override void StartPointerSequenceCore (PointerEventInfo info)
        {
            switch (info.Type) {
                case PointerEventType.Primary:
                    StartSelectObjectSequence(info);
                    break;
                case PointerEventType.Secondary:
                    ClearSelected();
                    break;
            }

            UpdatePointerSequence(info);
        }

        protected override void UpdatePointerSequenceCore (PointerEventInfo info)
        {
            switch (info.Type) {
                case PointerEventType.Primary:
                    UpdateSelectObjectSequence(info);
                    break;
            }
        }

        #region Start Select Object Sequence

        private class SelectedObjectRecord
        {
            public ObjectInstance Instance { get; set; }
            public SelectionAnnot Annot { get; set; }
            public Point InitialLocation { get; set; }
        }

        private Point _initialLocation;
        private List<SelectedObjectRecord> _selectedObjects = new List<SelectedObjectRecord>();

        private void StartSelectObjectSequence (PointerEventInfo info)
        {
            ClearSelected();

            _initialLocation = new Point((int)info.X, (int)info.Y);

            foreach (ObjectInstance inst in CoarseHitTest((int)info.X, (int)info.Y)) {
                SelectedObjectRecord record = new SelectedObjectRecord()
                {
                    Instance = inst,
                    Annot = new SelectionAnnot(inst.ImageBounds.Location)
                    {
                        End = new Point(inst.ImageBounds.Right, inst.ImageBounds.Bottom),
                        Outline = new Pen(new SolidColorBrush(new Color(64, 255, 255)), 2),
                    },
                    InitialLocation = new Point(inst.X, inst.Y),
                };

                _selectedObjects.Add(record);
                _annots.Add(record.Annot);
            }
        }

        private void UpdateSelectObjectSequence (PointerEventInfo info)
        {
            int diffx = (int)info.X - _initialLocation.X;
            int diffy = (int)info.Y - _initialLocation.Y;

            foreach (SelectedObjectRecord record in _selectedObjects) {
                Point newLoc = new Point(record.InitialLocation.X + diffx, record.InitialLocation.Y + diffy);
                if (SnapManager != null)
                    newLoc = SnapManager.Translate(newLoc, SnappingTarget);

                record.Instance.X = newLoc.X;
                record.Instance.Y = newLoc.Y;
                record.Annot.MoveTo(record.Instance.ImageBounds.Location);
            }
        }

        #endregion

        private List<ObjectInstance> CoarseHitTest (int x, int y)
        {
            List<ObjectInstance> list = new List<ObjectInstance>();
            foreach (ObjectInstance inst in Layer.ObjectsAtPoint(new Point(x, y), ObjectPointTest.Image)) {
                list.Add(inst);
            }

            return list;
        }

        private void ClearSelected ()
        {
            foreach (SelectedObjectRecord record in _selectedObjects) {
                _annots.Remove(record.Annot);
            }

            _selectedObjects.Clear();
        }
    }
}
