using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;
using Treefrog.Presentation.Annotations;
using Treefrog.Framework.Model;
using Treefrog.Presentation.Layers;
using Treefrog.Framework;
using Treefrog.Presentation.Commands;
using Treefrog.Framework.Imaging;
using Treefrog.Framework.Imaging.Drawing;
using Treefrog.Presentation.Controllers;
using Treefrog.Presentation.Tools;

namespace Treefrog.Presentation
{
    public class MinimapPresenter : ILayerContext, IPointerResponderProvider, IPointerResponder
    {
        private EditorPresenter _editor;
        private ObservableCollection<Annotation> _annotations;
        private Dictionary<Guid, LevelLayerPresenter> _layerPresenters;

        private GroupLayerPresenter _rootLayer;
        private GroupLayerPresenter _rootContentLayer;

        private LevelPresenter _levelPresenter;

        private SelectionAnnot _boxAnnot;

        public MinimapPresenter (EditorPresenter editor)
        {
            _editor = editor;
            _editor.SyncCurrentLevel += SyncCurrentLevel;

            _annotations = new ObservableCollection<Annotation>();
            _layerPresenters = new Dictionary<Guid, LevelLayerPresenter>();

            _boxAnnot = new SelectionAnnot() {
                Outline = _boxOutline,
                OutlineGlow = _boxOutlineGlow,
            };
            _annotations.Add(_boxAnnot);
        }

        private void SyncCurrentLevel (object sender, SyncLevelEventArgs e)
        {
            BindLevel(_editor.CurrentLevel);
        }

        private void BindLevel (LevelPresenter levelPresenter)
        {
            if (_levelPresenter == levelPresenter)
                return;

            if (_levelPresenter != null) {
                _levelPresenter.LevelGeometryInvalidated -= LevelGeometryInvalidated;
                ClearLayers();
            }

            _levelPresenter = levelPresenter;

            if (_levelPresenter != null) {
                _levelPresenter.LevelGeometryInvalidated += LevelGeometryInvalidated;
            }

            InitializeLayerHierarchy(levelPresenter);
            InitializeLayers();
            UpdateAreaBox();

            OnCurrentLevelChanged(EventArgs.Empty);
        }

        private void LevelGeometryInvalidated (object sender, EventArgs e)
        {
            UpdateAreaBox();
        }

        private void InitializeLayerHierarchy (LevelPresenter presenter)
        {
            _rootContentLayer = new GroupLayerPresenter();
            _rootLayer = new GroupLayerPresenter();

            if (presenter != null) {
                _rootLayer.Layers.Add(_rootContentLayer);
                _rootLayer.Layers.Add(new AnnotationLayerPresenter() {
                    Annotations = _annotations,
                });
            }
        }

        private void InitializeLayers ()
        {
            if (_levelPresenter != null) {
                foreach (Layer layer in _levelPresenter.Level.Layers)
                    AddLayer(layer);
            }
        }

        private void ClearLayers ()
        {
            foreach (Guid uid in new List<Guid>(_layerPresenters.Keys))
                RemoveLayer(uid);
        }

        private void AddLayer (Layer layer)
        {
            LevelLayerPresenter layerp = LayerPresenterFactory.Default.Create(layer, this);

            _layerPresenters[layer.Uid] = layerp;
            _rootContentLayer.Layers.Add(layerp);
        }

        private void RemoveLayer (Guid layerUid)
        {
            if (layerUid == null || !_layerPresenters.ContainsKey(layerUid))
                return;

            LevelLayerPresenter layerp = _layerPresenters[layerUid];
            _layerPresenters.Remove(layerUid);
            _rootContentLayer.Layers.Remove(layerp);
        }

        public GroupLayerPresenter RootLayer
        {
            get { return _rootLayer; }
        }

        public TexturePool TexturePool
        {
            get { return _levelPresenter != null ? _levelPresenter.TexturePool : null; }
        }

        public Level Level
        {
            get { return _levelPresenter != null ? _levelPresenter.Level : null; }
        }

        public event EventHandler CurrentLevelChanged;

        protected virtual void OnCurrentLevelChanged (EventArgs e)
        {
            var ev = CurrentLevelChanged;
            if (ev != null)
                ev(this, e);
        }

        public ILevelGeometry LevelGeometry { get; set; }

        public void UpdateAreaBox ()
        {
            if (_levelPresenter != null && _levelPresenter.LevelGeometry != null) {
                _boxAnnot.Start = _levelPresenter.LevelGeometry.VisibleBounds.Location;
                _boxAnnot.End = new Point(_levelPresenter.LevelGeometry.VisibleBounds.Right, _levelPresenter.LevelGeometry.VisibleBounds.Bottom);
            }
        }

        #region Layer Context

        public ILevelGeometry Geometry
        {
            get { return LevelGeometry; }
        }

        public CommandHistory History
        {
            get { return _levelPresenter.History; }
        }

        public ObservableCollection<Annotation> Annotations
        {
            get { return _annotations; }
        }

        public void SetPropertyProvider (IPropertyProvider provider)
        { }

        public void ActivatePropertyProvider (IPropertyProvider provider)
        { }

        public void ActivateContextMenu (CommandMenu menu, Point location)
        { }

        #endregion

        private static Pen _boxOutline = new Pen(new SolidColorBrush(Colors.Red), 2);
        private static Pen _boxOutlineGlow = new Pen(new SolidColorBrush(Colors.White), 4);

        public IPointerResponder PointerEventResponder
        {
            get { return this; }
        }

        public event EventHandler PointerEventResponderChanged;

        private double _startX;
        private double _startY;
        private Point _boxStart;
        private bool _drag;

        public void HandleStartPointerSequence (PointerEventInfo info)
        {
            if (info.Type == PointerEventType.Primary) {
                if (_levelPresenter.Geometry.VisibleBounds.Contains(new Point((int)info.X, (int)info.Y))) {
                    _startX = info.X;
                    _startY = info.Y;
                    _boxStart = _boxAnnot.Start;
                    _drag = true;
                }
                else {
                    int transX = (int)info.X - _levelPresenter.Geometry.VisibleBounds.Width / 2;
                    int transY = (int)info.Y - _levelPresenter.Geometry.VisibleBounds.Height / 2;
                    ScrollLevelTo(new Point(transX, transY));
                }
            }
        }

        public void HandleEndPointerSequence (PointerEventInfo info)
        {
            _drag = false;
        }

        public void HandleUpdatePointerSequence (PointerEventInfo info)
        {
            if (!_drag)
                return;

            double diffX = info.X - _startX;
            double diffY = info.Y - _startY;

            int boxX = _boxStart.X + (int)diffX;
            int boxY = _boxStart.Y + (int)diffY;

            ScrollLevelTo(new Point(boxX, boxY));
        }

        public void HandlePointerPosition (PointerEventInfo info)
        { }

        public void HandlePointerLeaveField ()
        { }

        private void ScrollLevelTo (Point point)
        {
            int boxX = point.X;
            int boxY = point.Y;

            boxX = Math.Max(_levelPresenter.Geometry.LevelBounds.Left, boxX);
            boxY = Math.Max(_levelPresenter.Geometry.LevelBounds.Top, boxY);
            boxX = Math.Min(_levelPresenter.Geometry.LevelBounds.Right - _levelPresenter.Geometry.VisibleBounds.Width, boxX);
            boxY = Math.Min(_levelPresenter.Geometry.LevelBounds.Bottom - _levelPresenter.Geometry.VisibleBounds.Height, boxY);

            _levelPresenter.Geometry.ScrollPosition = new Point(boxX, boxY);
        }
    }
}
