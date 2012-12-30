using Treefrog.Framework.Model;
using Treefrog.Presentation.Commands;
using Treefrog.Presentation.Layers;
using Treefrog.Presentation.Controllers;
using System;
using System.Collections.Generic;
using Treefrog.Framework;

namespace Treefrog.Presentation
{

    public class LevelPresenter2 : IDisposable, IPointerResponderProvider, ICommandSubscriber
    {
        private bool _disposed;
        private EditorPresenter _editor;
        private Level _level;

        private GroupLayerPresenter _rootLayer;
        private GroupLayerPresenter _rootContentLayer;

        private string _selectedLayer;
        private LevelLayerPresenter _selectedLayerRef;

        private Dictionary<string, LevelLayerPresenter> _layerPresenters;

        private CommandHistory _history;

        public LevelPresenter2 (EditorPresenter editor, Level level)
        {
            _editor = editor;
            _level = level;

            _layerPresenters = new Dictionary<string, LevelLayerPresenter>();

            InitializeLayerHierarchy();
            InitializeLayers();
        }

        public void Dispose ()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose (bool disposing)
        {
            if (!_disposed) {
                if (disposing) {
                    foreach (LevelLayerPresenter layer in _layerPresenters.Values) {
                        UnbindLayerEvents(layer);
                        layer.Dispose();
                    }

                    foreach (Layer layer in _level.Layers)
                        UnbindLayerEvents(layer);
                }

                _disposed = true;
            }
        }

        public Level Level
        {
            get { return _level; }
        }

        public GroupLayerPresenter RootLayer
        {
            get { return _rootLayer; }
        }

        public GroupLayerPresenter RootContentLayer
        {
            get { return _rootContentLayer; }
        }

        public LevelLayerPresenter SelectedLayer
        {
            get { return _selectedLayerRef; }
        }

        public TexturePool TexturePool
        {
            get { return _level.Project.TexturePool; }
        }

        public CommandHistory History
        {
            get { return _history; }
        }

        public IPointerResponder PointerEventResponder
        {
            get
            {
                return _selectedLayerRef != null
                    ? _selectedLayerRef.PointerEventResponder : null;
            }
        }

        public event EventHandler PointerEventResponderChanged;

        protected virtual void OnPointerEventResponderChanged (EventArgs e)
        {
            var ev = PointerEventResponderChanged;
            if (ev != null)
                ev(this, e);
        }

        private void InitializeLayerHierarchy ()
        {
            _rootContentLayer = new GroupLayerPresenter();

            _rootLayer = new GroupLayerPresenter();
            _rootLayer.Layers.Add(new WorkspaceLayerPresenter());
            _rootLayer.Layers.Add(_rootContentLayer);
        }

        private void InitializeLayers ()
        {
            foreach (Layer layer in _level.Layers) {
                AddLayer(layer);
            }

            SelectLayer();
        }

        private void AddLayer (Layer layer)
        {
            LevelLayerPresenter layerp = new LevelLayerPresenter(this, layer);

            _layerPresenters[layer.Name] = layerp;
            _rootContentLayer.Layers.Add(layerp);

            BindLayerEvents(layer);
        }

        private void SelectLayer ()
        {
            SelectLayer(null);

            foreach (Layer layer in _level.Layers) {
                SelectLayer(layer.Name);
                return;
            }
        }

        private void SelectLayer (string layerName)
        {
            if (_selectedLayer == layerName)
                return;

            UnbindSelectedLayerEvents(_selectedLayerRef);

            if (layerName == null || !_layerPresenters.ContainsKey(layerName)) {
                _selectedLayer = null;
                _selectedLayerRef = null;

                OnPointerEventResponderChanged(EventArgs.Empty);
                return;
            }

            _selectedLayer = layerName;
            _selectedLayerRef = _layerPresenters[_selectedLayer];

            BindSelectedLayerEvents(_selectedLayerRef);

            OnPointerEventResponderChanged(EventArgs.Empty);
        }

        private void BindLayerEvents (Layer layer)
        {
            if (layer != null) {
                layer.NameChanged += LayerNameChanged;
            }
        }

        private void UnbindLayerEvents (Layer layer)
        {
            if (layer != null) {
                layer.NameChanged -= LayerNameChanged;
            }
        }

        private void UnbindLayerEvents (LevelLayerPresenter layer)
        {
            UnbindSelectedLayerEvents(layer);
        }

        private void BindSelectedLayerEvents (LevelLayerPresenter layer)
        {
            if (layer != null) {
                layer.PointerEventResponderChanged += SelectedLayerPointerEventResponderChanged;
            }
        }

        private void UnbindSelectedLayerEvents (LevelLayerPresenter layer)
        {
            if (layer != null) {
                layer.PointerEventResponderChanged -= SelectedLayerPointerEventResponderChanged;
            }
        }

        private void SelectedLayerPointerEventResponderChanged (object sender, EventArgs e)
        {
            OnPointerEventResponderChanged(EventArgs.Empty);
        }

        private void LayerNameChanged (object sender, NameChangedEventArgs e)
        {
            if (_layerPresenters.ContainsKey(e.OldName)) {
                _layerPresenters[e.NewName] = _layerPresenters[e.OldName];
                _layerPresenters.Remove(e.OldName);
            }

            if (_selectedLayer == e.OldName)
                _selectedLayer = e.NewName;
        }

        private ForwardingCommandManager _commandManager = new ForwardingCommandManager();

        public CommandManager CommandManager
        {
            get { return _commandManager; }
        }
    }
}
