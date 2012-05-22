//Copyright (c) 2007-2012, Adolfo Marinucci
//All rights reserved.

//Redistribution and use in source and binary forms, with or without modification, are permitted provided that the 
//following conditions are met:

//* Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.

//* Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following 
//disclaimer in the documentation and/or other materials provided with the distribution.

//* Neither the name of Adolfo Marinucci nor the names of its contributors may be used to endorse or promote products
//derived from this software without specific prior written permission.

//THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES,
//INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. 
//IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, 
//EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
//LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, 
//STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, 
//EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Interop;
using System.Windows.Controls;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Data;
using System.Windows.Media;
using AvalonDock.Layout;
using System.Diagnostics;
using System.Windows.Threading;

namespace AvalonDock.Controls
{
    public class LayoutAutoHideWindowControl : HwndHost, ILayoutControl
    {
        static LayoutAutoHideWindowControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LayoutAutoHideWindowControl), new FrameworkPropertyMetadata(typeof(LayoutAutoHideWindowControl)));
        }

        internal LayoutAutoHideWindowControl(LayoutAnchorControl anchor)
        {
            _model = anchor.Model as LayoutAnchorable;
            _side = (anchor.Model.Parent.Parent as LayoutAnchorSide).Side;
        }

        LayoutAnchorable _model;

        public ILayoutElement Model
        {
            get { return _model; }
        }

        HwndSource _internalHwndSource = null;

        protected override System.Runtime.InteropServices.HandleRef BuildWindowCore(System.Runtime.InteropServices.HandleRef hwndParent)
        {
            _internalHwndSource = new HwndSource(new HwndSourceParameters()
            {
                AcquireHwndFocusInMenuMode = false,
                ParentWindow = hwndParent.Handle,
                WindowStyle = Win32Helper.WS_CHILD | Win32Helper.WS_VISIBLE | Win32Helper.WS_CLIPSIBLINGS | Win32Helper.WS_CLIPCHILDREN | Win32Helper.WS_GROUP,
                Width = 1,
                Height = 1,
            });

            _internalHwndSource.RootVisual = _internalGrid;
            return new HandleRef(this, _internalHwndSource.Handle);
        }

        protected override IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == Win32Helper.WM_WINDOWPOSCHANGING)
            {
                Win32Helper.SetWindowPos(_internalHwndSource.Handle, IntPtr.Zero, 0, 0, 0, 0, Win32Helper.SetWindowPosFlags.IgnoreMove | Win32Helper.SetWindowPosFlags.IgnoreResize);
            }
            else if (msg == Win32Helper.WM_KILLFOCUS)
            {
                Debug.WriteLine("WM_KILLFOCUS");
            }
            else if (msg == Win32Helper.WM_SETFOCUS)
            {
                Debug.WriteLine("WM_SETFOCUS");
            }

            return base.WndProc(hwnd, msg, wParam, lParam, ref handled);
        }

        protected override void DestroyWindowCore(System.Runtime.InteropServices.HandleRef hwnd)
        {
            _closeTimer.Stop();
            _closeTimer = null;

            if (_internalHwndSource != null)
            {
                _internalHwndSource.Dispose();
                _internalHwndSource = null;
            }
        }

        Grid _internalGrid = null;
        LayoutAnchorableControl _internalHost = null;
        AnchorSide _side;
        LayoutGridResizerControl _resizer = null;
        DockingManager _manager;

        protected override void OnInitialized(EventArgs e)
        {
            CreateInternalGrid();
            SetupCloseTimer();

            _manager = _model.Root.Manager;
            _model.PropertyChanged += new System.ComponentModel.PropertyChangedEventHandler(_model_PropertyChanged);

            base.OnInitialized(e);
        }

        void _model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsAutoHidden")
            {
                if (!_model.IsAutoHidden)
                {
                    _model.PropertyChanged -= new System.ComponentModel.PropertyChangedEventHandler(_model_PropertyChanged);
                    _manager.HideAutoHideWindow();
                }
            }
        }

        void CreateInternalGrid()
        {
            var manager = _model.Root.Manager;
            _internalGrid = new Grid();
            _internalGrid.SetBinding(Grid.BackgroundProperty, new Binding("Background") { Source = this });

            _internalHost = new LayoutAnchorableControl() { Model = _model };
            KeyboardNavigation.SetTabNavigation(_internalGrid, KeyboardNavigationMode.Cycle);
            
            _resizer = new LayoutGridResizerControl();

            _resizer.DragStarted += new System.Windows.Controls.Primitives.DragStartedEventHandler(OnResizerDragStarted);
            _resizer.DragDelta += new System.Windows.Controls.Primitives.DragDeltaEventHandler(OnResizerDragDelta);
            _resizer.DragCompleted += new System.Windows.Controls.Primitives.DragCompletedEventHandler(OnResizerDragCompleted);
            if (_side == AnchorSide.Right)
            {
                _internalGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(manager.GridSplitterWidth)});
                _internalGrid.ColumnDefinitions.Add(new ColumnDefinition() {
                    Width = _model.AutoHideWidth == 0.0 ? new GridLength(100.0) : new GridLength(_model.AutoHideWidth, GridUnitType.Pixel),
                    MinWidth = _model.AutoHideMinWidth
                    });

                _internalGrid.Children.Add(_resizer); Grid.SetColumn(_resizer, 0);
                _internalGrid.Children.Add(_internalHost); Grid.SetColumn(_internalHost, 1);
                
                _resizer.Cursor = Cursors.SizeWE;

                HorizontalAlignment = System.Windows.HorizontalAlignment.Right;
            }
            else if (_side == AnchorSide.Left)
            {
                _internalGrid.ColumnDefinitions.Add(new ColumnDefinition()
                {
                    Width = _model.AutoHideWidth == 0.0 ? new GridLength(100.0) : new GridLength(_model.AutoHideWidth, GridUnitType.Pixel),
                    MinWidth = _model.AutoHideMinWidth
                });
                _internalGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(manager.GridSplitterWidth) });

                _internalGrid.Children.Add(_internalHost); Grid.SetColumn(_internalHost, 0);
                _internalGrid.Children.Add(_resizer); Grid.SetColumn(_resizer, 1);

                _resizer.Cursor = Cursors.SizeWE;

                HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
            }
            else if (_side == AnchorSide.Top)
            {
                _internalGrid.RowDefinitions.Add(new RowDefinition()
                {
                    Height = _model.AutoHideHeight == 0.0 ? new GridLength(100.0) : new GridLength(_model.AutoHideHeight, GridUnitType.Pixel),
                    MinHeight = _model.AutoHideMinHeight
                });
                _internalGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(manager.GridSplitterHeight) });

                _internalGrid.Children.Add(_internalHost); Grid.SetRow(_internalHost, 0);
                _internalGrid.Children.Add(_resizer); Grid.SetRow(_resizer, 1);

                _resizer.Cursor = Cursors.SizeNS;

                VerticalAlignment = System.Windows.VerticalAlignment.Top;
            }
            else if (_side == AnchorSide.Bottom)
            {
                _internalGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(manager.GridSplitterHeight) });
                _internalGrid.RowDefinitions.Add(new RowDefinition()
                {
                    Height = _model.AutoHideHeight == 0.0 ? new GridLength(100.0) : new GridLength(_model.AutoHideHeight, GridUnitType.Pixel),
                    MinHeight = _model.AutoHideMinHeight
                });

                _internalGrid.Children.Add(_resizer); Grid.SetRow(_resizer, 0);
                _internalGrid.Children.Add(_internalHost); Grid.SetRow(_internalHost, 1);

                _resizer.Cursor = Cursors.SizeNS;

                VerticalAlignment = System.Windows.VerticalAlignment.Bottom;
            }
            AddLogicalChild(_internalGrid);
        }

        DispatcherTimer _closeTimer = null;
        void SetupCloseTimer()
        {
            _closeTimer = new DispatcherTimer(DispatcherPriority.Background);
            _closeTimer.Interval = TimeSpan.FromMilliseconds(1500);
            _closeTimer.Tick += (s, e) =>
                {
                    if (IsWin32MouseOver ||
                        _model.IsActive)
                        return;

                    Model.Root.Manager.HideAutoHideWindow();
                };
            _closeTimer.Start();
        }

        void OnResizerDragCompleted(object sender, System.Windows.Controls.Primitives.DragCompletedEventArgs e)
        {
            LayoutGridResizerControl splitter = sender as LayoutGridResizerControl;
            var rootVisual = this.FindVisualTreeRoot() as Visual;

            var trToWnd = TransformToAncestor(rootVisual);
            Vector transformedDelta = trToWnd.Transform(new Point(e.HorizontalChange, e.VerticalChange)) -
                trToWnd.Transform(new Point());

            double delta;
            if (_side == AnchorSide.Right || _side == AnchorSide.Left)
                delta = Canvas.GetLeft(_resizerGhost) - _initialStartPoint.X;
            else
                delta = Canvas.GetTop(_resizerGhost) - _initialStartPoint.Y;

            if (_side == AnchorSide.Right)
            {
                if (_model.AutoHideWidth == 0.0)
                    _model.AutoHideWidth = _internalHost.ActualWidth - delta;
                else
                    _model.AutoHideWidth -= delta;

                _internalGrid.ColumnDefinitions[1].Width = new GridLength(_model.AutoHideWidth, GridUnitType.Pixel);
            }
            else if (_side == AnchorSide.Left)
            {
                if (_model.AutoHideWidth == 0.0)
                    _model.AutoHideWidth = _internalHost.ActualWidth + delta;
                else
                    _model.AutoHideWidth += delta;

                _internalGrid.ColumnDefinitions[0].Width = new GridLength(_model.AutoHideWidth, GridUnitType.Pixel);
            }
            else if (_side == AnchorSide.Top)
            {
                if (_model.AutoHideHeight == 0.0)
                    _model.AutoHideHeight = _internalHost.ActualHeight + delta;
                else
                    _model.AutoHideHeight += delta;

                _internalGrid.RowDefinitions[0].Height = new GridLength(_model.AutoHideHeight, GridUnitType.Pixel);
            }
            else if (_side == AnchorSide.Bottom)
            {
                if (_model.AutoHideHeight == 0.0)
                    _model.AutoHideHeight = _internalHost.ActualHeight - delta;
                else
                    _model.AutoHideHeight -= delta;

                _internalGrid.RowDefinitions[1].Height = new GridLength(_model.AutoHideHeight, GridUnitType.Pixel);
            }

            HideResizerOverlayWindow();

            InvalidateMeasure();
            _closeTimer.Start();
        }

        void OnResizerDragDelta(object sender, System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            LayoutGridResizerControl splitter = sender as LayoutGridResizerControl;
            var rootVisual = this.FindVisualTreeRoot() as Visual;

            var trToWnd = TransformToAncestor(rootVisual);
            Vector transformedDelta = trToWnd.Transform(new Point(e.HorizontalChange, e.VerticalChange)) -
                trToWnd.Transform(new Point());

            if (_side == AnchorSide.Right || _side == AnchorSide.Left)
            {
                Canvas.SetLeft(_resizerGhost, MathHelper.MinMax(_initialStartPoint.X + transformedDelta.X, 0.0, _resizerWindowHost.Width - _resizerGhost.Width));
            }
            else
            {
                Canvas.SetTop(_resizerGhost, MathHelper.MinMax(_initialStartPoint.Y + transformedDelta.Y, 0.0, _resizerWindowHost.Height - _resizerGhost.Height));
            }
        }

        void OnResizerDragStarted(object sender, System.Windows.Controls.Primitives.DragStartedEventArgs e)
        {
            var resizer = sender as LayoutGridResizerControl;
            ShowResizerOverlayWindow(resizer);
            _closeTimer.Stop();
        }

        protected override System.Collections.IEnumerator LogicalChildren
        {
            get
            {
                return new UIElement[] { _internalGrid }.GetEnumerator();
            }
        }

        Border _resizerGhost = null;
        Window _resizerWindowHost = null;
        Vector _initialStartPoint;

        void ShowResizerOverlayWindow(LayoutGridResizerControl splitter)
        {
            _resizerGhost = new Border()
            {
                Background = splitter.BackgroundWhileDragging,
                Opacity = splitter.OpacityWhileDragging
            };

            var parentManager = Parent as DockingManager;
            var modelControlActualSize = this._internalHost.TransformActualSizeToAncestor();

            Point ptTopLeftScreen = parentManager.GetAutoHideAreaElement().PointToScreenDPI(new Point());

            var managerSize = parentManager.GetAutoHideAreaElement().TransformActualSizeToAncestor();

            Size windowSize;

            if (_side == AnchorSide.Right || _side == AnchorSide.Left)
            {
                windowSize = new Size(
                    managerSize.Width - _model.AutoHideMinWidth - 25.0 + splitter.ActualWidth,
                    managerSize.Height);

                _resizerGhost.Width = splitter.ActualWidth;
                _resizerGhost.Height = windowSize.Height;
                ptTopLeftScreen.Offset(25, 0.0);
            }
            else
            {
                windowSize = new Size(
                    managerSize.Width,
                    managerSize.Height - _model.AutoHideMinHeight - 25.0 + splitter.ActualHeight);

                _resizerGhost.Height = splitter.ActualHeight;
                _resizerGhost.Width = windowSize.Width;
                ptTopLeftScreen.Offset(0.0, 25.0);
            }

            _initialStartPoint = splitter.PointToScreenDPI(new Point()) - ptTopLeftScreen;

            if (_side == AnchorSide.Right || _side == AnchorSide.Left)
            {
                Canvas.SetLeft(_resizerGhost, _initialStartPoint.X);
            }
            else
            {
                Canvas.SetTop(_resizerGhost, _initialStartPoint.Y);
            }

            Canvas panelHostResizer = new Canvas()
            {
                HorizontalAlignment = System.Windows.HorizontalAlignment.Stretch,
                VerticalAlignment = System.Windows.VerticalAlignment.Stretch
            };

            panelHostResizer.Children.Add(_resizerGhost);


            _resizerWindowHost = new Window()
            {
                ResizeMode = ResizeMode.NoResize,
                WindowStyle = System.Windows.WindowStyle.None,
                ShowInTaskbar = false,
                AllowsTransparency = true,
                Background = null,
                Width = windowSize.Width,
                Height = windowSize.Height,
                Left = ptTopLeftScreen.X,
                Top = ptTopLeftScreen.Y,
                ShowActivated = false,
                Owner = Window.GetWindow(this),
                Content = panelHostResizer
            };

            _resizerWindowHost.Show();
        }

        void HideResizerOverlayWindow()
        {
            if (_resizerWindowHost != null)
            {
                _resizerWindowHost.Close();
                _resizerWindowHost = null;
            }
        }

        protected override Size MeasureOverride(Size constraint)
        {
            _internalGrid.Measure(constraint);
            return _internalGrid.DesiredSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            _internalGrid.Arrange(new Rect(finalSize));
            return finalSize;
        }

        IInputElement _lastFocusedElement = null;
        protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            base.OnGotKeyboardFocus(e);
            if (!e.Handled)
            {
                if (e.NewFocus == _internalGrid)
                {
                    Keyboard.Focus(_lastFocusedElement);
                    e.Handled = true;
                }
                else
                    _lastFocusedElement = e.NewFocus;
            }
        }


        #region Background

        /// <summary>
        /// Background Dependency Property
        /// </summary>
        public static readonly DependencyProperty BackgroundProperty =
            DependencyProperty.Register("Background", typeof(Brush), typeof(LayoutAutoHideWindowControl),
                new FrameworkPropertyMetadata((Brush)null));

        /// <summary>
        /// Gets or sets the Background property.  This dependency property 
        /// indicates background of the autohide childwindow.
        /// </summary>
        public Brush Background
        {
            get { return (Brush)GetValue(BackgroundProperty); }
            set { SetValue(BackgroundProperty, value); }
        }

        #endregion

        bool IsWin32MouseOver
        {
            get
            {
                var ptMouse = new Win32Helper.Win32Point();
                if (!Win32Helper.GetCursorPos(ref ptMouse))
                    return false;

                Point location = this.PointToScreenDPI(new Point());

                Rect rectWindow = this.GetScreenArea();
                if (rectWindow.Contains(new Point(ptMouse.X, ptMouse.Y)))
                    return true;

                var manager = Model.Root.Manager;
                var anchor = manager.FindVisualChildren<LayoutAnchorControl>().Where(c => c.Model == Model).First();
                location = anchor.PointToScreenDPI(new Point());

                if (anchor.IsMouseOver)
                    return true;

                return false;
            }
        }
    }
}
