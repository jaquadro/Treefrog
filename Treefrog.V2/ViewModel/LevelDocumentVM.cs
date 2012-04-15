using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Treefrog.Framework.Model;
using System.Collections.ObjectModel;

using Treefrog.V2.Controls.Layers;
using System.Windows.Media;
using Treefrog.V2.ViewModel.Layers;
using GalaSoft.MvvmLight.Command;
using System.Windows.Input;
using System.Windows;
using Treefrog.V2.ViewModel.Commands;
using Treefrog.V2.ViewModel.Tools;
using Treefrog.V2.ViewModel.Annotations;

namespace Treefrog.V2.ViewModel
{
    public class LevelDocumentVM : DocumentVM
    {
        private Level _level;

        private ViewportVM _viewport;
        private LevelGroupLayerVM _root;

        private LayerCollectionVM _layerCollection;

        private CommandHistory _commandHistory;

        private ITileToolCollection _tileToolCollection;
   
        public LevelDocumentVM (Level level)
        {
            _level = level;
            _commandHistory = new CommandHistory();

            _viewport = new ViewportVM();
            _root = new LevelGroupLayerVM(this, _level.Layers, _viewport);

            _layerCollection = new LayerCollectionVM(_level.Layers, _root);

            _tileToolCollection = new TileTools(this);
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
                Coordinates = coords.X + ", " + coords.Y;
            }
        }

        #endregion

        #endregion

        private class TileTools : ITileToolCollection
        {
            private LevelDocumentVM _level;
            private TileTool _selectedTool = TileTool.Draw;

            public TileTools (LevelDocumentVM level)
            {
                _level = level;
                if (TileLayer != null)
                    TileLayer.SetTool(_selectedTool);
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
                        if (TileLayer != null)
                            TileLayer.SetTool(_selectedTool);
                    }
                }
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
