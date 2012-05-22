using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Windows.Input;
using System.ComponentModel;
using Treefrog.ViewModel.Tools;

namespace Treefrog.ViewModel.Menu
{
    public class TileMenuVM : ViewModelBase
    {
        EditorVM _editor;

        public TileMenuVM (EditorVM editor)
        {
            _editor = editor;
            _editor.PropertyChanged += HandleEditorPropertyChanged;

            _selectedTool = TileTool.Select;
        }

        private void HandleEditorPropertyChanged (object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName) {
                case "ActiveDocument":
                    RaisePropertyChanged("Enabled");
                    RaisePropertyChanged("SelectToolEnabled");
                    RaisePropertyChanged("DrawToolEnabled");
                    RaisePropertyChanged("EraseToolEnabled");
                    RaisePropertyChanged("FillToolEnabled");
                    RaisePropertyChanged("StampToolEnabled");
                    break;
            }
        }

        private DocumentVM ActiveDocument
        {
            get { return _editor.ActiveDocument; }
        }

        private ITileToolCollection ToolCollection
        {
            get 
            {
                if (ActiveDocument == null)
                    return null;
                return ActiveDocument.LookupToolCollection<ITileToolCollection>();
            }
        }

        public bool Enabled
        {
            get { return ToolCollection != null && ToolCollection.Enabled; }
        }

        #region Tool Group

        private TileTool _selectedTool;

        private void ChangeTool (TileTool tool)
        {
            if (_selectedTool != tool) {
                _selectedTool = tool;

                if (ToolCollection != null)
                    ToolCollection.SelectedTool = tool;

                RaisePropertyChanged("SelectToolSelected");
                RaisePropertyChanged("DrawToolSelected");
                RaisePropertyChanged("EraseToolSelected");
                RaisePropertyChanged("FillToolSelected");
                RaisePropertyChanged("StampToolSelected");
            }
        }

        public bool SelectToolSelected
        {
            get { return _selectedTool == TileTool.Select; }
            set { ChangeTool(TileTool.Select); }
        }

        public bool DrawToolSelected
        {
            get { return _selectedTool == TileTool.Draw; }
            set { ChangeTool(TileTool.Draw); }
        }

        public bool EraseToolSelected
        {
            get { return _selectedTool == TileTool.Erase; }
            set { ChangeTool(TileTool.Erase); }
        }

        public bool FillToolSelected
        {
            get { return _selectedTool == TileTool.Fill; }
            set { ChangeTool(TileTool.Fill); }
        }

        public bool StampToolSelected
        {
            get { return _selectedTool == TileTool.Stamp; }
            set { ChangeTool(TileTool.Stamp); }
        }

        public bool SelectToolEnabled
        {
            get { return ToolCollection != null && ToolCollection.SelectEnabled; }
        }

        public bool DrawToolEnabled
        {
            get { return ToolCollection != null && ToolCollection.DrawEnabled; }
        }

        public bool EraseToolEnabled
        {
            get { return ToolCollection != null && ToolCollection.EraseEnabled; }
        }

        public bool FillToolEnabled
        {
            get { return ToolCollection != null && ToolCollection.FillEnabled; }
        }

        public bool StampToolEnabled
        {
            get { return ToolCollection != null && ToolCollection.StampEnabled; }
        }

        #endregion

        #region Flip Horizontal Tool

        public bool FlipHToolEnabled
        {
            get { return false; }
        }

        #endregion

        #region Flip Vertical Tool

        public bool FlipVToolEnabled
        {
            get { return false; }
        }

        #endregion

        #region Rotate Left Tool

        public bool RotateLeftToolEnabled
        {
            get { return false; }
        }

        #endregion

        #region Rotate Right Tool

        public bool RotateRightToolEnabled
        {
            get { return false; }
        }

        #endregion
    }
}
