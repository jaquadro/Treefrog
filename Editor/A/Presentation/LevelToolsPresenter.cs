using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Treefrog.Framework.Model;

namespace Editor.A.Presentation
{
    public interface ILevelToolsPresenter
    {
        bool CanSelect { get; }
        bool CanDraw { get; }
        bool CanErase { get; }
        bool CanFill { get; }
        bool CanStamp { get; }

        TileToolMode ActiveTileTool { get; }

        void ActionToggleSelect ();
        void ActionToggleDraw ();
        void ActionToggleErase ();
        void ActionToggleFill ();
        void ActionToggleStamp ();

        event EventHandler SyncLevelToolsActions;
        event EventHandler SyncLevelToolsState;

        void RefreshLevelTools ();
    }

    public class LevelToolsPresenter : ILevelToolsPresenter
    {
        private EditorPresenter _editor;

        private TileToolMode _tileTool;

        public LevelToolsPresenter (EditorPresenter editor)
        {
            _editor = editor;
        }

        private Layer CurrentLayer
        {
            get
            {
                LevelPresenter level = _editor.CurrentLevel;
                return (level != null)
                    ? level.SelectedLayer
                    : null;
            }
        }

        private bool IsTileLayer (Layer layer)
        {
            return layer != null && layer is MultiTileGridLayer;
        }

        #region ILevelToolsPresenter Members

        public bool CanSelect
        {
            get { return IsTileLayer(CurrentLayer); }
        }

        public bool CanDraw
        {
            get { return IsTileLayer(CurrentLayer); }
        }

        public bool CanErase
        {
            get { return IsTileLayer(CurrentLayer); }
        }

        public bool CanFill
        {
            get { return IsTileLayer(CurrentLayer); }
        }

        public bool CanStamp
        {
            get { return IsTileLayer(CurrentLayer); }
        }

        public TileToolMode ActiveTileTool
        {
            get { return _tileTool; }
        }

        public void ActionToggleSelect ()
        {
            if (_tileTool != TileToolMode.Select) {
                _tileTool = TileToolMode.Select;
                OnSyncLevelToolsState(EventArgs.Empty);
            }

            OnSyncLevelToolsActions(EventArgs.Empty);
            
        }

        public void ActionToggleDraw ()
        {
            if (_tileTool != TileToolMode.Draw) {
                _tileTool = TileToolMode.Draw;
                OnSyncLevelToolsState(EventArgs.Empty);
            }

            OnSyncLevelToolsActions(EventArgs.Empty);
        }

        public void ActionToggleErase ()
        {
            if (_tileTool != TileToolMode.Erase) {
                _tileTool = TileToolMode.Erase;
                OnSyncLevelToolsState(EventArgs.Empty);
            }

            OnSyncLevelToolsActions(EventArgs.Empty);
        }

        public void ActionToggleFill ()
        {
            if (_tileTool != TileToolMode.Fill) {
                _tileTool = TileToolMode.Fill;
                OnSyncLevelToolsState(EventArgs.Empty);
            }

            OnSyncLevelToolsActions(EventArgs.Empty);
        }

        public void ActionToggleStamp ()
        {
            if (_tileTool != TileToolMode.Stamp) {
                _tileTool = TileToolMode.Stamp;
                OnSyncLevelToolsState(EventArgs.Empty);
            }

            OnSyncLevelToolsActions(EventArgs.Empty);
        }

        public event EventHandler SyncLevelToolsActions;

        public event EventHandler SyncLevelToolsState;

        protected virtual void OnSyncLevelToolsActions (EventArgs e)
        {
            if (SyncLevelToolsActions != null) {
                SyncLevelToolsActions(this, e);
            }
        }

        protected virtual void OnSyncLevelToolsState (EventArgs e)
        {
            if (SyncLevelToolsState != null) {
                SyncLevelToolsState(this, e);
            }
        }

        public void RefreshLevelTools ()
        {
            OnSyncLevelToolsActions(EventArgs.Empty);
            OnSyncLevelToolsState(EventArgs.Empty);
        }

        #endregion
    }
}
