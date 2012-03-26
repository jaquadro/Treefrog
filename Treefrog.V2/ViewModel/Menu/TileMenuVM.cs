using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using System.Windows.Input;

namespace Treefrog.V2.ViewModel.Menu
{
    public class TileMenuVM : ViewModelBase
    {
        private enum TileTool {
            Select,
            Draw,
            Erase,
            Fill,
            Stamp,
        }

        EditorVM _editor;

        public TileMenuVM (EditorVM editor)
        {
            _editor = editor;

            _selectedTool = TileTool.Select;
        }

        #region Tool Group

        private TileTool _selectedTool;

        private void ChangeTool (TileTool tool)
        {
            if (_selectedTool != tool) {
                _selectedTool = tool;

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

        #endregion
    }
}
