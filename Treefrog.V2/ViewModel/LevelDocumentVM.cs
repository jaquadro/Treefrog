using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Treefrog.Framework.Model;
using System.Collections.ObjectModel;

using Treefrog.Controls.Layers;
using System.Windows.Media;
using Treefrog.ViewModel.Layers;
using GalaSoft.MvvmLight.Command;
using System.Windows.Input;
using System.Windows;
using Treefrog.ViewModel.Commands;
using Treefrog.ViewModel.Tools;
using Treefrog.ViewModel.Annotations;
using System.ComponentModel;

namespace Treefrog.ViewModel
{
    public class LevelDocumentVM : DocumentVM
    {
        private Level _level;
        private LevelLayerVM _activeLayer;

        private ViewportVM _viewport;
        private LevelGroupLayerVM _root;

        private LayerCollectionVM _layerCollection;

        private CommandHistory _commandHistory;

        private ITileToolCollection _tileToolCollection;
   
        public LevelDocumentVM (Level level)
        {
            _level = level;
            _commandHistory = new CommandHistory();
            _commandHistory.HistoryChanged += HandleHistoryChanged;

            _viewport = new ViewportVM();
            _root = new LevelGroupLayerVM(this, _level.Layers, _viewport);

            _layerCollection = new LayerCollectionVM(_level.Layers, _root, _level);
            _layerCollection.PropertyChanged += HandleLayerCollectionPropertyChanged;

            _tileToolCollection = new TileTools(this);

            IsInitialized = true;
            OnInitialized(EventArgs.Empty);
        }

        public bool IsInitialized { get; private set; }

        public event EventHandler Initialized;

        protected virtual void OnInitialized (EventArgs e)
        {
            if (Initialized != null)
                Initialized(this, e);
        }

        private void HandleLayerCollectionPropertyChanged (object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName) {
                case "SelectedLayer":
                    SetActiveLayer(ActiveLayer);
                    RaisePropertyChanged("ActiveLayer");
                    RaisePropertyChanged("GridTileHeight");
                    RaisePropertyChanged("GridTileWidth");
                    break;
            }
        }

        private void SetActiveLayer (LevelLayerVM layer)
        {
            if (_activeLayer == layer)
                return;

            if (_activeLayer != null) {
                _activeLayer.CanCutChanged -= HandleLayerCutChanged;
                _activeLayer.CanCopyChanged -= HandleLayerCopyChanged;
                _activeLayer.CanPasteChanged -= HandleLayerPasteChanged;
                _activeLayer.CanDeleteChanged -= HandleLayerDeleteChanged;
                _activeLayer.CanSelectAllChanged -= HandleLayerSelectAllChanged;
                _activeLayer.CanSelectNoneChanged -= HandleLayerSelectNoneChanged;
                _activeLayer.Deactivate();
            }

            _activeLayer = layer;

            if (_activeLayer != null) {
                _activeLayer.CanCutChanged += HandleLayerCutChanged;
                _activeLayer.CanCopyChanged += HandleLayerCopyChanged;
                _activeLayer.CanPasteChanged += HandleLayerPasteChanged;
                _activeLayer.CanDeleteChanged += HandleLayerDeleteChanged;
                _activeLayer.CanSelectAllChanged += HandleLayerSelectAllChanged;
                _activeLayer.CanSelectNoneChanged += HandleLayerSelectNoneChanged;
                _activeLayer.Activate();
            }
        }

        public override string Name
        {
            get { return _level.Name; }
        }

        public override bool SupportsZoom
        {
            get { return true; }
        }

        public override double ZoomLevel
        {
            get { return _viewport.ZoomFactor; }
            set {
                if (_viewport.ZoomFactor != value) {
                    _viewport.ZoomFactor = (float)value;
                    RaisePropertyChanged("ZoomLevel");
                }
            }
        }

        private string _coordinates = "";
        public override string Coordinates
        {
            get { return _coordinates; }
            set
            {
                if (_coordinates != value) {
                    _coordinates = value;
                    RaisePropertyChanged("Coordinates");
                }
            }
        }

        public override CommandHistory CommandHistory
        {
            get { return _commandHistory; }
        }

        public override IEnumerable<Tools.IToolCollection> RegisteredToolCollections
        {
            get
            {
                yield return _tileToolCollection;
            }
        }

        public ViewportVM Viewport
        {
            get { return _viewport; }
        }

        public LevelGroupLayerVM Root
        {
            get { return _root; }
        }

        public double LevelHeight
        {
            get { return _level.PixelsHigh; }
        }

        public double LevelWidth
        {
            get { return _level.PixelsWide; }
        }

        public double GridTileHeight
        {
            get
            {
                LevelLayerVM layer = ActiveLayer;
                return layer != null ? layer.GridTileHeight : 0;
            }
        }

        public double GridTileWidth
        {
            get
            {
                LevelLayerVM layer = ActiveLayer;
                return layer != null ? layer.GridTileWidth : 0;
            }
        }

        public Color BackgroundColor
        {
            get { return Colors.Gray; }
        }

        public LevelDocumentVM Model
        {
            get { return this; }
        }

        public LayerCollectionVM LayerCollection
        {
            get { return _layerCollection; }
        }

        public LevelLayerVM ActiveLayer
        {
            get
            {
                if (_layerCollection.SelectedLayer == null)
                    return null;
                foreach (LayerVM layer in _root.Layers)
                    if (layer.LayerName == _layerCollection.SelectedLayer.LayerName)
                        return layer as LevelLayerVM;
                return null;
            }
        }

        // XXX: Do I really want this here?
        private ObservableCollection<Annotation> _annotations = new ObservableCollection<Annotation>();

        public ObservableCollection<Annotation> Annotations
        {
            get { return _annotations; }
        }

        #region Pointer Commands

        #region Start Pointer Sequence Command

        private RelayCommand<PointerEventInfo> _startPointerSeqCommand;

        public ICommand StartPointerSequenceCommand
        {
            get
            {
                if (_startPointerSeqCommand == null)
                    _startPointerSeqCommand = new RelayCommand<PointerEventInfo>(OnExecuteStartPointerSeq);
                return _startPointerSeqCommand;
            }
        }

        private void OnExecuteStartPointerSeq (PointerEventInfo info)
        {
            LevelLayerVM layer = ActiveLayer;
            if (layer != null)
                layer.HandleStartPointerSequence(info);
        }

        #endregion

        #region End Pointer Sequence Command

        private RelayCommand<PointerEventInfo> _EndPointerSeqCommand;

        public ICommand EndPointerSequenceCommand
        {
            get
            {
                if (_EndPointerSeqCommand == null)
                    _EndPointerSeqCommand = new RelayCommand<PointerEventInfo>(OnExecuteEndPointerSeq);
                return _EndPointerSeqCommand;
            }
        }

        private void OnExecuteEndPointerSeq (PointerEventInfo info)
        {
            LevelLayerVM layer = ActiveLayer;
            if (layer != null)
                layer.HandleEndPointerSequence(info);
        }

        #endregion

        #region Update Pointer Sequence Command

        private RelayCommand<PointerEventInfo> _UpdatePointerSeqCommand;

        public ICommand UpdatePointerSequenceCommand
        {
            get
            {
                if (_UpdatePointerSeqCommand == null)
                    _UpdatePointerSeqCommand = new RelayCommand<PointerEventInfo>(OnExecuteUpdatePointerSeq);
                return _UpdatePointerSeqCommand;
            }
        }

        private void OnExecuteUpdatePointerSeq (PointerEventInfo info)
        {
            LevelLayerVM layer = ActiveLayer;
            if (layer != null)
                layer.HandleUpdatePointerSequence(info);
        }

        #endregion

        #region Pointer Position Command

        private RelayCommand<PointerEventInfo> _pointerPositionCommand;

        public ICommand PointerPositionCommand
        {
            get
            {
                if (_pointerPositionCommand == null)
                    _pointerPositionCommand = new RelayCommand<PointerEventInfo>(OnExecutePointerPosition);
                return _pointerPositionCommand;
            }
        }

        private void OnExecutePointerPosition (PointerEventInfo info)
        {
            LevelLayerVM layer = ActiveLayer;
            if (layer != null) {
                layer.HandlePointerPosition(info);

                Vector coords = layer.GetCoordinates(info.X, info.Y);
                if (coords.X < layer.CoordinateBounds.X || coords.Y < layer.CoordinateBounds.Y ||
                    coords.X >= layer.CoordinateBounds.Right || coords.Y >= layer.CoordinateBounds.Bottom)
                    Coordinates = "";
                else
                    Coordinates = coords.X + ", " + coords.Y;
            }
        }

        #endregion

        #region Pointer Leave Field Command

        private RelayCommand _pointerLeaveFieldCommand;

        public ICommand PointerLeaveFieldCommand
        {
            get
            {
                if (_pointerLeaveFieldCommand == null)
                    _pointerLeaveFieldCommand = new RelayCommand(OnExecutePointerLeaveField);
                return _pointerLeaveFieldCommand;
            }
        }

        private void OnExecutePointerLeaveField ()
        {
            LevelLayerVM layer = ActiveLayer;
            if (layer != null) {
                layer.HandlePointerLeaveField();
            }
        }

        #endregion

        #endregion

        #region IEditCommandProvider Members

        public override bool CanUndo
        {
            get { return _commandHistory.CanUndo; }
        }

        public override bool CanRedo
        {
            get { return _commandHistory.CanRedo; }
        }

        public override bool CanCut
        {
            get
            {
                LevelLayerVM vm = ActiveLayer;
                return (vm == null) ? false : vm.CanCut;
            }
        }

        public override bool CanCopy
        {
            get
            {
                LevelLayerVM vm = ActiveLayer;
                return (vm == null) ? false : vm.CanCopy;
            }
        }

        public override bool CanPaste
        {
            get
            {
                LevelLayerVM vm = ActiveLayer;
                return (vm == null) ? false : vm.CanPaste;
            }
        }

        public override bool CanDelete
        {
            get
            {
                LevelLayerVM vm = ActiveLayer;
                return (vm == null) ? false : vm.CanDelete;
            }
        }

        public override bool CanSelectAll
        {
            get
            {
                LevelLayerVM vm = ActiveLayer;
                return (vm == null) ? false : vm.CanSelectAll;
            }
        }

        public override bool CanSelectNone
        {
            get
            {
                LevelLayerVM vm = ActiveLayer;
                return (vm == null) ? false : vm.CanSelectNone;
            }
        }

        public override void Undo ()
        {
            _commandHistory.Undo();
        }

        public override void Redo ()
        {
            _commandHistory.Redo();
        }

        public override void Cut ()
        {
            LevelLayerVM vm = ActiveLayer;
            if (vm != null)
                vm.Cut();
        }

        public override void Copy ()
        {
            LevelLayerVM vm = ActiveLayer;
            if (vm != null)
                vm.Copy();
        }

        public override void Paste ()
        {
            LevelLayerVM vm = ActiveLayer;
            if (vm != null)
                vm.Paste();
        }

        public override void Delete ()
        {
            LevelLayerVM vm = ActiveLayer;
            if (vm != null)
                vm.Delete();
        }

        public override void SelectAll ()
        {
            LevelLayerVM vm = ActiveLayer;
            if (vm != null)
                vm.SelectAll();
        }

        public override void SelectNone ()
        {
            LevelLayerVM vm = ActiveLayer;
            if (vm != null)
                vm.SelectNone();
        }

        private void HandleHistoryChanged (object sender, EventArgs e)
        {
            OnCanUndoChanged(e);
            OnCanRedoChanged(e);
        }

        private void HandleLayerCutChanged (object sender, EventArgs e)
        {
            OnCanCutChanged(e);
        }

        private void HandleLayerCopyChanged (object sender, EventArgs e)
        {
            OnCanCopyChanged(e);
        }

        private void HandleLayerPasteChanged (object sender, EventArgs e)
        {
            OnCanPasteChanged(e);
        }

        private void HandleLayerDeleteChanged (object sender, EventArgs e)
        {
            OnCanDeleteChanged(e);
        }

        private void HandleLayerSelectAllChanged (object sender, EventArgs e)
        {
            OnCanSelectAllChanged(e);
        }

        private void HandleLayerSelectNoneChanged (object sender, EventArgs e)
        {
            OnCanSelectNoneChanged(e);
        }

        #endregion

        private class TileTools : ITileToolCollection
        {
            private LevelDocumentVM _level;
            private TileTool _selectedTool = TileTool.Select;

            public TileTools (LevelDocumentVM level)
            {
                _level = level;
                _level.PropertyChanged += HandleLevelDocumentPropertyChanged;
            }

            private void HandleLevelDocumentPropertyChanged (object sender, PropertyChangedEventArgs e)
            {
                switch (e.PropertyName) {
                    case "ActiveLayer":
                        OnInvalidated(EventArgs.Empty);
                        break;
                }
            }

            private TileLayerVM TileLayer
            {
                get { return _level.ActiveLayer as TileLayerVM; }
            }

            public bool Enabled
            {
                get { return _level.ActiveLayer is TileLayerVM; }
            }

            public bool SelectEnabled
            {
                get { return Enabled; }
            }

            public bool DrawEnabled
            {
                get { return Enabled; }
            }

            public bool EraseEnabled
            {
                get { return Enabled; }
            }

            public bool FillEnabled
            {
                get { return Enabled; }
            }

            public bool StampEnabled
            {
                get { return Enabled; }
            }

            public TileTool SelectedTool
            {
                get { return _selectedTool; }
                set
                {
                    if (_selectedTool != value) {
                        _selectedTool = value;
                        OnSelectedToolChanged(EventArgs.Empty);
                        OnInvalidated(EventArgs.Empty);
                    }
                }
            }

            public event EventHandler Invalidated;

            protected virtual void OnInvalidated (EventArgs e)
            {
                if (Invalidated != null)
                    Invalidated(this, e);
            }

            public event EventHandler SelectedToolChanged;

            protected virtual void OnSelectedToolChanged (EventArgs e)
            {
                if (SelectedToolChanged != null)
                    SelectedToolChanged(this, e);
            }
        }
    }

    public enum PointerEventType
    {
        None,
        Primary,
        Secondary,
    }

    public struct PointerEventInfo
    {
        public PointerEventType Type;
        public double X;
        public double Y;

        public PointerEventInfo (PointerEventType type, double x, double y)
        {
            Type = type;
            X = x;
            Y = y;
        }
    }
}
