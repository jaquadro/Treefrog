using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Treefrog.Controls.Xna;
using System.Windows.Interop;
using Microsoft.Xna.Framework.Graphics;

namespace Treefrog.Controls.XnaD3D
{
    /// <summary>
    /// Interaction logic for XnaPanel.xaml
    /// </summary>
    public partial class XnaPanel : UserControl, IDisposable
    {
        private GraphicsDeviceService _graphicsService;
        private D3DSurface _surface;

        private object _lock;
        private bool _disposed;
        private bool _active;
        private bool _appHasFocus = false;

        public XnaPanel ()
        {
            InitializeComponent();

            _lock = new object();
            _surface = new D3DSurface(_d3dImage);

            Loaded += HandleLoaded;
            Unloaded += HandleUnloaded;
            SizeChanged += HandleSizeChanged;

            Application.Current.Activated += HandleApplicationActivated;
            Application.Current.Deactivated += HandleApplicationDeactivated;

            _appHasFocus = Application.Current.MainWindow.IsActive;

            Activate();
        }

        public void Dispose ()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose (bool disposing)
        {
            if (!_disposed) {
                Loaded -= HandleLoaded;
                Unloaded -= HandleUnloaded;
                SizeChanged -= HandleSizeChanged;

                if (_graphicsService != null) {
                    _surface.SetBackBuffer(null);

                    _graphicsService.Release(disposing);
                    _graphicsService = null;
                }

                Deactivate();

                Application.Current.Activated -= HandleApplicationActivated;
                Application.Current.Deactivated -= HandleApplicationDeactivated;

                _disposed = true;
            }
        }

        public void Activate ()
        {
            if (!_disposed && !_active) {
                CompositionTarget.Rendering += HandleRendering;
                _active = true;
            }
        }

        public void Deactivate ()
        {
            if (!_disposed && _active) {
                CompositionTarget.Rendering -= HandleRendering;
                _active = false;
            }
        }

        private void HandleLoaded (object sender, RoutedEventArgs e)
        {
            lock (_lock) {
                HwndSource hwndSource = (HwndSource)HwndSource.FromVisual(this);
                if (_graphicsService == null && hwndSource != null && hwndSource.Handle != null) {
                    _graphicsService = GraphicsDeviceService.AddRef(hwndSource.Handle, (int)ActualWidth, (int)ActualHeight);

                    if (_graphicsService != null) {
                        //SetD3DImageBackBuffer(CreateRenderTarget(1, 1));
                        _surface.SetBackBuffer(_graphicsService.GraphicsDevice);
                    }

                    // Invoke the LoadContent event
                    OnLoadContent(new GraphicsDeviceEventArgs(_graphicsService.GraphicsDevice));
                }
            }
        }

        private void HandleUnloaded (object sender, RoutedEventArgs e)
        {
            Dispose();
        }

        private void HandleSizeChanged (object sender, SizeChangedEventArgs e)
        {
            if ((int)ActualWidth <= 0 || (int)ActualHeight <= 0)
                return;

            lock (_lock) {
                if (_graphicsService != null) {
                    _surface.SetBackBuffer(null);

                    _graphicsService.ResetDevice((int)ActualWidth, (int)ActualHeight);

                    //RenderTarget2D renderTarget = CreateRenderTarget((int)_image.ActualWidth, (int)_image.ActualHeight);
                    /*if (renderTarget != null) {
                        _graphicsService.GraphicsDevice.SetRenderTarget(renderTarget);
                        SetD3DImageBackBuffer(renderTarget);
                    }*/
                    _surface.SetBackBuffer(_graphicsService.GraphicsDevice);
                }
            }
        }

        private void HandleApplicationActivated (object sender, EventArgs e)
        {
            _appHasFocus = true;
        }

        private void HandleApplicationDeactivated (object sender, EventArgs e)
        {
            _appHasFocus = false;
        }

        private void HandleRendering (object sender, EventArgs e)
        {
            if (_disposed || _graphicsService == null)
                return;

            int width = (int)ActualWidth;
            int height = (int)ActualHeight;

            if (width < 1 || height < 1)
                return;

            lock (_lock) {
                _d3dImage.Lock();

                HandleDeviceReset();

                Viewport viewport = new Viewport(0, 0, width, height);
                _graphicsService.GraphicsDevice.Viewport = viewport;

                OnRenderXna(new GraphicsDeviceEventArgs(_graphicsService.GraphicsDevice));

                _d3dImage.AddDirtyRect(new Int32Rect(0, 0, _d3dImage.PixelWidth, _d3dImage.PixelHeight));
                _d3dImage.Unlock();
            }
        }

        public event EventHandler<GraphicsDeviceEventArgs> LoadContent;

        protected virtual void OnLoadContent (GraphicsDeviceEventArgs e)
        {
            var ev = LoadContent;
            if (ev != null)
                ev(this, e);
        }

        public event EventHandler<GraphicsDeviceEventArgs> RenderXna;

        protected virtual void OnRenderXna (GraphicsDeviceEventArgs e)
        {
            var ev = RenderXna;
            if (ev != null)
                ev(this, e);
        }

        private void HandleDeviceReset ()
        {
            if (_graphicsService == null)
                return;

            switch (_graphicsService.GraphicsDevice.GraphicsDeviceStatus) {
                case GraphicsDeviceStatus.NotReset:
                    _graphicsService.ResetDevice((int)ActualWidth, (int)ActualHeight);
                    break;

                default:
                    PresentationParameters pp = _graphicsService.GraphicsDevice.PresentationParameters;
                    if ((int)ActualWidth > pp.BackBufferWidth || (int)ActualHeight > pp.BackBufferHeight)
                        _graphicsService.ResetDevice((int)ActualWidth, (int)ActualHeight);
                    break;
            }
        }

        private RenderTarget2D CreateRenderTarget (int width, int height)
        {
            return new RenderTarget2D(_graphicsService.GraphicsDevice, width, height, false, SurfaceFormat.Color, DepthFormat.None);
        }

        private void SetD3DImageBackBuffer (RenderTarget2D renderTarget)
        {
            _d3dImage.Lock();
            _d3dImage.SetBackBuffer(D3DResourceType.IDirect3DSurface9, GameReflector.GetRenderTargetSurface(renderTarget));
            _d3dImage.Unlock();
        }

        private void SetD3DImageBackBuffer (GraphicsDevice device)
        {
            _d3dImage.Lock();
            _d3dImage.SetBackBuffer(D3DResourceType.IDirect3DSurface9, GameReflector.GetGraphicsDeviceSurface(device));
            _d3dImage.Unlock();
        }

        private class D3DSurface
        {
            private D3DImage _image;
            private IntPtr _resource;

            public D3DSurface (D3DImage image)
            {
                _image = image;
            }

            public void SetBackBuffer (GraphicsDevice device)
            {
                _image.Lock();

                if (_resource != IntPtr.Zero) {
                    _image.SetBackBuffer(D3DResourceType.IDirect3DSurface9, IntPtr.Zero);
                    GameReflector.ReleaseGraphicsDeviceSurface(_resource);
                    _resource = IntPtr.Zero;
                }

                if (device != null) {
                    _resource = GameReflector.GetGraphicsDeviceSurface(device);
                    _image.SetBackBuffer(D3DResourceType.IDirect3DSurface9, _resource);
                }

                _image.Unlock();
            }
        }
    }
}
