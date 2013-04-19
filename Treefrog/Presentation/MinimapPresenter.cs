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

namespace Treefrog.Presentation
{
    public class MinimapPresenter : ILayerContext
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
            foreach (Layer layer in _levelPresenter.Level.Layers)
                AddLayer(layer);
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
            get { return _levelPresenter.TexturePool; }
        }

        public Level Level
        {
            get { return _levelPresenter.Level; }
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
            if (_levelPresenter.LevelGeometry != null) {
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
    }
}
