using System;

namespace Treefrog.Presentation
{
    public interface IContentInfoPresenter
    {
        bool CanZoom { get; }

        float Zoom { get; }
        string CoordinateString { get; }

        void ActionZoom (float zoom);
        void ActionUpdateCoordinates (string coords);

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

        public float Zoom
        {
            get { return (_curPresenter != null) ? _curPresenter.Zoom : 1f; }
        }

        public string CoordinateString
        {
            get { return (_curPresenter != null) ? _curPresenter.CoordinateString : ""; }
        }

        public void ActionZoom (float zoom)
        {
            if (_curPresenter != null)
                _curPresenter.ActionZoom(zoom);
        }

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

    class LevelInfoPresenter : IContentInfoPresenter
    {
        private LevelPresenter _level;

        private string _coordinates = "";

        public LevelInfoPresenter (LevelPresenter level)
        {
            _level = level;
        }

        #region IContentInfoPresenter Members

        public bool CanZoom
        {
            get { return true; }
        }

        public float Zoom
        {
            get { return _level.LayerControl.Zoom; }
        }

        public string CoordinateString
        {
            get { return _coordinates; }
        }

        public void ActionZoom (float zoom)
        {
            _level.LayerControl.Zoom = zoom;

            OnSyncZoomLevel(EventArgs.Empty);
        }

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
