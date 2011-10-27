using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Editor.Model.Controls;

namespace Editor.Views
{
    public partial class LevelPanel : UserControl, IZoomable, IDataReporter
    {
        #region Fields

        private LevelController _controller;
        private LevelPresentation _levelInfo;

        #endregion

        #region Constructors

        public LevelPanel ()
        {
            InitializeComponent();

            layerControl1.ZoomChanged += LayerControlZoomChangedHandler;
            
        }

        #endregion

        #region Properties

        public LayerControl LayerControl
        {
            get { return layerControl1; }
        }

        #endregion

        #region Event Handlers

        private void LayerControlTileMouseMove (object sender, TileMouseEventArgs e)
        {

        }

        #endregion

        #region IZoomable Members

        public float Zoom
        {
            get { return layerControl1.Zoom; }
            set
            {
                if (Zoom != value) {
                    _inSetZoom = true;

                    layerControl1.Zoom = value;
                    OnZoomChanged(EventArgs.Empty);

                    _inSetZoom = false;
                }
            }
        }

        public event EventHandler ZoomChanged;

        #region Support

        private bool _inSetZoom;

        protected virtual void OnZoomChanged (EventArgs e)
        {
            if (ZoomChanged != null) {
                ZoomChanged(this, e);
            }
        }

        private void LayerControlZoomChangedHandler (object sender, EventArgs e)
        {
            if (!_inSetZoom) {
                OnZoomChanged(e);
            }
        }

        #endregion

        #endregion

        #region IDataReporter Members

        public event EventHandler<DataReportEventArgs> DataReport;

        #region Support

        protected virtual void OnDataReport (DataReportEventArgs e)
        {
            if (DataReport != null) {
                DataReport(this, e);
            }
        }

        #endregion

        #endregion
    }
}
