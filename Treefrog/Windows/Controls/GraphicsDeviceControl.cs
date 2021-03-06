﻿using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Xna.Framework.Graphics;
using OpenTK;
using Treefrog.Framework;

namespace Treefrog.Windows.Controls
{
    using Color = System.Drawing.Color;
    using Rectangle = Microsoft.Xna.Framework.Rectangle;
    using XnaColor = Microsoft.Xna.Framework.Color;
    
    public abstract class GraphicsDeviceControl : GLControl
    {
        #region Fields

        //private OpenTK.GLControl _tkControl;

        private bool _designMode;

        Form _mainForm;

        GraphicsDeviceService _deviceService;

        ServiceContainer _services = new ServiceContainer();

        #endregion

        #region Properties

        public Form MainForm
        {
            get { return _mainForm; }
            internal set { _mainForm = value; }
        }

        public GraphicsDevice GraphicsDevice
        {
            get { return _deviceService.GraphicsDevice; }
        }

        public GraphicsDeviceService GraphicsDeviceService
        {
            get { return _deviceService; }
        }

        public ServiceContainer Services
        {
            get { return _services; }
        }

        #endregion

        #region Events

        public event EventHandler<EventArgs> ControlInitialized;
        public event EventHandler<EventArgs> ControlInitializing;

        #endregion

        #region Initialization

        protected GraphicsDeviceControl ()
        {
            _designMode = DesignMode || LicenseManager.UsageMode == LicenseUsageMode.Designtime;

            //_tkControl.Dock = DockStyle.Fill;
            Load += _tkControl_Load;

            //Controls.Add(_tkControl);
        }

        private void _tkControl_Load (object sender, EventArgs e)
        {
            if (!DesignMode) {
                _deviceService = GraphicsDeviceService.AddRef(Handle, ClientSize.Width, ClientSize.Height);

                _services.AddService<IGraphicsDeviceService>(_deviceService);

                if (ControlInitializing != null) {
                    ControlInitializing(this, EventArgs.Empty);
                }

                Initialize();

                if (ControlInitialized != null) {
                    ControlInitialized(this, EventArgs.Empty);
                }
            }
        }

        protected override void OnCreateControl ()
        {
            base.OnCreateControl();
        }

        protected override void Dispose (bool disposing)
        {
            if (_deviceService != null) {
                try {
                    _deviceService.Release();
                }
                catch { }

                _deviceService = null;
            }

            base.Dispose(disposing);
        }

        protected new bool DesignMode
        {
            get { return _designMode; }
        }

        #endregion

        #region Paint

        protected override void OnPaint (PaintEventArgs e)
        {
            string beginDrawError = BeginDraw();

            if (string.IsNullOrEmpty(beginDrawError)) {
                Draw();
                //GraphicsDevice.Clear(XnaColor.Aqua);
                EndDraw();
            }
            else {
                PaintUsingSystemDrawing(e.Graphics, beginDrawError);
            }
        }

        private string BeginDraw ()
        {
            if (_deviceService == null) {
                return Text + "\n\n" + GetType();
            }

            string deviceResetError = HandleDeviceReset();

            if (!string.IsNullOrEmpty(deviceResetError)) {
                return deviceResetError;
            }

            //MakeCurrent();
            GLControl control = GLControl.FromHandle(_deviceService.GraphicsDevice.PresentationParameters.DeviceWindowHandle) as GLControl;
            if (control != null) {
                control.Context.MakeCurrent(WindowInfo);
                _deviceService.GraphicsDevice.PresentationParameters.BackBufferHeight = ClientSize.Height;
                _deviceService.GraphicsDevice.PresentationParameters.BackBufferWidth = ClientSize.Width;
            }     

            Viewport viewport = new Viewport();

            viewport.X = 0;
            viewport.Y = 0;

            viewport.Width = ClientSize.Width;
            viewport.Height = ClientSize.Height;

            viewport.MinDepth = 0;
            viewport.MaxDepth = 1;

            if (GraphicsDevice.Viewport.Equals(viewport) == false)
                GraphicsDevice.Viewport = viewport;

            return null;
        }

        private static Random rand = new Random();

        private void EndDraw ()
        {
            try {
                //Rectangle srcRect = new Rectangle(0, 0, ClientSize.Width, ClientSize.Height);
                //GraphicsDevice.Clear(XnaColor.Aqua);
                //GraphicsDevice.Present(srcRect, null, Handle);

                SwapBuffers();
            }
            catch {
            }
        }

        private string HandleDeviceReset ()
        {
            bool needsReset = false;

            switch (GraphicsDevice.GraphicsDeviceStatus) {
                case GraphicsDeviceStatus.Lost:
                    return "Graphics device lost";

                case GraphicsDeviceStatus.NotReset:
                    needsReset = true;
                    break;

                default:
                    PresentationParameters pp = GraphicsDevice.PresentationParameters;
                    needsReset = (ClientSize.Width > pp.BackBufferWidth) || (ClientSize.Height > pp.BackBufferHeight);
                    break;
            }

            if (needsReset) {
                try {
                    _deviceService.ResetDevice(ClientSize.Width, ClientSize.Height);
                }
                catch (Exception e) {
                    return "Graphics device reset failed\n\n" + e;
                }
            }

            return null;
        }

        protected virtual void PaintUsingSystemDrawing (Graphics graphics, string text)
        {
            graphics.Clear(Color.Black);

            using (Brush brush = new SolidBrush(Color.White)) {
                using (StringFormat format = new StringFormat()) {
                    format.Alignment = StringAlignment.Center;
                    format.LineAlignment = StringAlignment.Center;

                    graphics.DrawString(text, Font, brush, ClientRectangle, format);
                }
            }
        }

        protected override void OnPaintBackground (PaintEventArgs pevent)
        {
        }

        #endregion

        #region Abstract Methods

        protected abstract void Initialize ();
        protected abstract void Draw ();

        #endregion

        public XnaColor GetPixel (int x, int y)
        {
            RenderTarget2D target = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.None);

            GraphicsDevice.SetRenderTarget(target);
            HandleDeviceReset();

            Draw();

            GraphicsDevice.SetRenderTarget(null);

#if false
            XnaColor[] data = new XnaColor[1];
            target.GetData(0, new Rectangle(x, y, 1, 1), data, 0, data.Length);
            return data[0];
#else
            XnaColor[] data = new XnaColor[GraphicsDevice.Viewport.Width * GraphicsDevice.Viewport.Height];
            target.GetData(data);

            return data[GraphicsDevice.Viewport.Width * y + x];
#endif
        }
    }
}
