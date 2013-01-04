using System;
using Treefrog.Framework.Model;
using Treefrog.Presentation.Layers;
using Treefrog.Presentation.Controllers;
using Treefrog.Framework.Imaging;

namespace Treefrog.Presentation
{
    public interface IContentInfoPresenter
    {
        bool CanZoom { get; }

        //float Zoom { get; }
        string CoordinateString { get; }
        ZoomState Zoom { get; }

        string InfoString { get; set; }

        //void ActionZoom (float zoom);
        void ActionUpdateCoordinates (string coords);

        LevelLayerPresenter CurrentLayer { get; }

        event EventHandler SyncContentInfoActions;
        event EventHandler SyncZoomLevel;
        event EventHandler SyncStatusInfo;

        void RefreshContentInfo ();
    }

    public class ContentInfoArbitrationPresenter : IContentInfoPresenter
    {
        private EditorPresenter _editor;
        private IContentInfoPresenter _curPresenter;

        public ContentInfoArbitrationPresenter (EditorPresenter editor)
        {
            _editor = editor;
        }

        public void BindInfoPresenter (IContentInfoPresenter pres)
        {
            if (_curPresenter != null) {
                _curPresenter.SyncContentInfoActions -= SyncContentInfoActionsHandler;
                _curPresenter.SyncStatusInfo -= SyncStatusInfoHandler;
                _curPresenter.SyncZoomLevel -= SyncZoomLevelHandler;
            }

            _curPresenter = pres;

            if (_curPresenter != null) {
                _curPresenter.SyncContentInfoActions += SyncContentInfoActionsHandler;
                _curPresenter.SyncStatusInfo += SyncStatusInfoHandler;
                _curPresenter.SyncZoomLevel += SyncZoomLevelHandler;
            }
        }

        #region IContentInfoPresenter Members

        public bool CanZoom
        {
            get { return (_curPresenter != null) ? _curPresenter.CanZoom : false; }
        }

        public ZoomState Zoom
        {
            get { return (_curPresenter != null) ? _curPresenter.Zoom : null; }
        }

        public string CoordinateString
        {
            get { return (_curPresenter != null) ? _curPresenter.CoordinateString : ""; }
        }

        public LevelLayerPresenter CurrentLayer
        {
            get { return (_curPresenter != null) ? _curPresenter.CurrentLayer : null; }
        }

        public string InfoString
        {
            get { return (_curPresenter != null) ? _curPresenter.InfoString : ""; }
            set
            {
                if (_curPresenter != null)
                    _curPresenter.InfoString = value;
            }
        }

        /*public void ActionZoom (float zoom)
        {
            if (_curPresenter != null)
                _curPresenter.ActionZoom(zoom);
        }*/

        public void ActionUpdateCoordinates (string coords)
        {
            if (_curPresenter != null)
                _curPresenter.ActionUpdateCoordinates(coords);
        }

        public event EventHandler SyncContentInfoActions;

        public event EventHandler SyncZoomLevel;

        public event EventHandler SyncStatusInfo;

        protected void OnSyncContentInfoActions (EventArgs e)
        {
            if (SyncContentInfoActions != null) {
                SyncContentInfoActions(this, e);
            }
        }

        protected void OnSyncZoomLevel (EventArgs e)
        {
            if (SyncZoomLevel != null) {
                SyncZoomLevel(this, e);
            }
        }

        protected void OnSyncStatusInfo (EventArgs e)
        {
            if (SyncStatusInfo != null) {
                SyncStatusInfo(this, e);
            }
        }

        public void RefreshContentInfo ()
        {
            if (_curPresenter != null) {
                _curPresenter.RefreshContentInfo();
            }
            else {
                OnSyncContentInfoActions(EventArgs.Empty);
                OnSyncStatusInfo(EventArgs.Empty);
                OnSyncZoomLevel(EventArgs.Empty);
            }
        }

        #endregion

        private void SyncContentInfoActionsHandler (object sender, EventArgs e)
        {
            OnSyncContentInfoActions(e);
        }

        private void SyncZoomLevelHandler (object sender, EventArgs e)
        {
            OnSyncZoomLevel(e);
        }

        private void SyncStatusInfoHandler (object sender, EventArgs e)
        {
            OnSyncStatusInfo(e);
        }
    }

    public class LevelInfoPresenter : IContentInfoPresenter
    {
        private LevelPresenter _level;

        private string _coordinates = "";
        private string _info = "";

        public LevelInfoPresenter (LevelPresenter level)
        {
            _level = level;
            _level.Zoom.ZoomLevelChanged += ZoomStateChanged;
        }

        private void ZoomStateChanged (object sender, EventArgs e)
        {
            OnSyncZoomLevel(EventArgs.Empty);
        }

        #region IContentInfoPresenter Members

        public bool CanZoom
        {
            get { return true; }
        }

        public ZoomState Zoom
        {
            get { return _level.Zoom; }
        }

        public string CoordinateString
        {
            get { return _coordinates; }
        }

        public LevelLayerPresenter CurrentLayer
        {
            get { return _level.SelectedLayer ; }
        }

        public string InfoString
        {
            get { return _info; }
            set
            {
                _info = value;
                OnSyncStatusInfo(EventArgs.Empty);
            }
        }

        /*public void ActionZoom (float zoom)
        {
            //_level.LayerControl.Zoom = zoom;

            OnSyncZoomLevel(EventArgs.Empty);
        }*/

        public void ActionUpdateCoordinates (string text)
        {
            _coordinates = text;

            OnSyncStatusInfo(EventArgs.Empty);
        }

        public event EventHandler SyncContentInfoActions;

        public event EventHandler SyncZoomLevel;

        public event EventHandler SyncStatusInfo;

        protected void OnSyncContentInfoActions (EventArgs e)
        {
            if (SyncContentInfoActions != null) {
                SyncContentInfoActions(this, e);
            }
        }

        protected void OnSyncZoomLevel (EventArgs e)
        {
            if (SyncZoomLevel != null) {
                SyncZoomLevel(this, e);
            }
        }

        protected void OnSyncStatusInfo (EventArgs e)
        {
            if (SyncStatusInfo != null) {
                SyncStatusInfo(this, e);
            }
        }

        public void RefreshContentInfo ()
        {
            OnSyncContentInfoActions(EventArgs.Empty);
            OnSyncStatusInfo(EventArgs.Empty);
            OnSyncZoomLevel(EventArgs.Empty);
        }

        #endregion
    }
}
