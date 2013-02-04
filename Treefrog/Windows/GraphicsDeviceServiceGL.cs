using System;
using System.Threading;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace Treefrog.Windows
{
    public class GraphicsDeviceService : IGraphicsDeviceService
    {
        #region Fields

        private static readonly GraphicsDeviceService _instance = new GraphicsDeviceService();
        private static int _refCount;

        private GraphicsDevice _device;

        private PresentationParameters _parameters;

        #endregion

        #region Properties

        public GraphicsDevice GraphicsDevice
        {
            get { return _device; }
        }

        #endregion

        #region Events

        public event EventHandler<EventArgs> DeviceCreated;
        public event EventHandler<EventArgs> DeviceDisposing;
        public event EventHandler<EventArgs> DeviceReset;
        public event EventHandler<EventArgs> DeviceResetting;

        #endregion

        protected GraphicsDeviceService ()
        {
        }

        public static GraphicsDeviceService AddRef (IntPtr windowHandle, int width, int height)
        {
            if (Interlocked.Increment(ref _refCount) == 1) {
                _instance.CreateDevice(windowHandle, width, height);
            }

            return _instance;
        }

        public void Release ()
        {
            Release(true);
        }

        protected void Release (bool disposing)
        {
            if (Interlocked.Decrement(ref _refCount) == 0) {
                if (disposing) {
                    if (DeviceDisposing != null) {
                        DeviceDisposing(this, EventArgs.Empty);
                    }

                    _device.Dispose();
                }

                _device = null;
            }
        }

        protected void CreateDevice (IntPtr windowHandle, int width, int height)
        {
            _parameters = new PresentationParameters();

            _parameters.BackBufferWidth = Math.Max(width, 1);
            _parameters.BackBufferHeight = Math.Max(height, 1);
            _parameters.BackBufferFormat = SurfaceFormat.Color;
            _parameters.DepthStencilFormat = DepthFormat.Depth24;
            _parameters.DeviceWindowHandle = windowHandle;
            _parameters.PresentationInterval = PresentInterval.One;
            _parameters.IsFullScreen = false;
            _parameters.RenderTargetUsage = RenderTargetUsage.PreserveContents;

            _device = new GraphicsDevice(GraphicsAdapter.DefaultAdapter, GraphicsProfile.Reach, _parameters);

            if (DeviceCreated != null) {
                DeviceCreated(this, EventArgs.Empty);
            }
        }

        public void ResetDevice (int width, int height)
        {
            if (DeviceResetting != null) {
                DeviceResetting(this, EventArgs.Empty);
            }

            _parameters.BackBufferWidth = Math.Max(_parameters.BackBufferWidth, width);
            _parameters.BackBufferHeight = Math.Max(_parameters.BackBufferHeight, height);

            _device.Reset(_parameters);

            if (DeviceReset != null) {
                DeviceReset(this, EventArgs.Empty);
            }
        }
    }
}
