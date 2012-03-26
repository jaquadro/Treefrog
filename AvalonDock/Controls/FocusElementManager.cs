using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows;
using System.Diagnostics;
using AvalonDock.Layout;

namespace AvalonDock.Controls
{
    internal static class FocusElementManager
    {
        #region Focus Management
        static List<DockingManager> _managers = new List<DockingManager>();
        internal static void SetupFocusManagement(DockingManager manager)
        {
            if (_managers.Count == 0)
            {
                InputManager.Current.EnterMenuMode += new EventHandler(InputManager_EnterMenuMode);
                InputManager.Current.LeaveMenuMode += new EventHandler(InputManager_LeaveMenuMode);
                _focusHandler = new FocusHookHandler();
                _focusHandler.FocusChanged += new EventHandler<FocusChangeEventArgs>(_focusHandler_FocusChanged);
                _focusHandler.Attach();

            }

            manager.PreviewGotKeyboardFocus += new KeyboardFocusChangedEventHandler(manager_PreviewGotKeyboardFocus);
            manager.LayoutChanged += new EventHandler(manager_LayoutChanged);
            _managers.Add(manager);
        }

        internal static void FinalizeFocusManagement(DockingManager manager)
        {
            manager.PreviewGotKeyboardFocus -= new KeyboardFocusChangedEventHandler(manager_PreviewGotKeyboardFocus);
            manager.LayoutChanged -= new EventHandler(manager_LayoutChanged);
            _managers.Remove(manager);

            if (_managers.Count == 0)
            {
                InputManager.Current.EnterMenuMode -= new EventHandler(InputManager_EnterMenuMode);
                InputManager.Current.LeaveMenuMode -= new EventHandler(InputManager_LeaveMenuMode);
                _focusHandler.FocusChanged -= new EventHandler<FocusChangeEventArgs>(_focusHandler_FocusChanged);
                _focusHandler.Detach();
                _focusHandler = null;
            }

            RefreshDetachedElements();
        }

        static void manager_PreviewGotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            var focusedElement = e.NewFocus as DependencyObject;
            if (focusedElement != null && !(focusedElement is LayoutAnchorableTabItem || focusedElement is LayoutDocumentTabItem))
            {
                var parentAnchorable = focusedElement.FindVisualAncestor<LayoutAnchorableControl>();
                if (parentAnchorable != null)
                {
                    if (_modelFocusedWindowHandle.ContainsKey(parentAnchorable.Model))
                        _modelFocusedWindowHandle.Remove(parentAnchorable.Model);
                    _modelFocusedElement[parentAnchorable.Model] = e.NewFocus;
                }
                else
                {
                    var parentDocument = focusedElement.FindVisualAncestor<LayoutDocumentControl>();
                    if (parentDocument != null)
                    {
                        if (_modelFocusedWindowHandle.ContainsKey(parentDocument.Model))
                            _modelFocusedWindowHandle.Remove(parentDocument.Model);
                        _modelFocusedElement[parentDocument.Model] = e.NewFocus;
                    }
                }
            }
        }

        static void manager_LayoutChanged(object sender, EventArgs e)
        {
            RefreshDetachedElements();
        }

        static void RefreshDetachedElements()
        {
            var detachedElements = _modelFocusedElement.Where(d => d.Key.Root == null || d.Key.Root.Manager == null || !_managers.Contains(d.Key.Root.Manager)).Select(d => d.Key).ToArray();
            foreach (var detachedElement in detachedElements)
                _modelFocusedElement.Remove(detachedElement);
            detachedElements = _modelFocusedWindowHandle.Where(d => d.Key.Root == null || d.Key.Root.Manager == null || !_managers.Contains(d.Key.Root.Manager)).Select(d => d.Key).ToArray();
            foreach (var detachedElement in detachedElements)
                _modelFocusedWindowHandle.Remove(detachedElement);
        }

        static Dictionary<ILayoutElement, IInputElement> _modelFocusedElement = new Dictionary<ILayoutElement, IInputElement>();
        static Dictionary<ILayoutElement, IntPtr> _modelFocusedWindowHandle = new Dictionary<ILayoutElement, IntPtr>();

        internal static IInputElement GetLastFocusedElement(ILayoutElement model)
        {
            if (_modelFocusedElement.ContainsKey(model))
                return _modelFocusedElement[model];

            return null;
        }



        internal static IntPtr GetLastWindowHandle(ILayoutElement model)
        {
            if (_modelFocusedWindowHandle.ContainsKey(model))
                return _modelFocusedWindowHandle[model];

            return IntPtr.Zero;
        }

        internal static void SetFocusOnLastElement(ILayoutElement model)
        {
            if (_modelFocusedElement.ContainsKey(model))
                Keyboard.Focus(_modelFocusedElement[model]);

            if (_modelFocusedWindowHandle.ContainsKey(model))
                Win32Helper.SetFocus(_modelFocusedWindowHandle[model]);
        }

        static FocusHookHandler _focusHandler = null;

        static void _focusHandler_FocusChanged(object sender, FocusChangeEventArgs e)
        {
            Debug.WriteLine("_focusHandler_FocusChanged(Got={0}, Lost={1})", e.GotFocusWinHandle, e.LostFocusWinHandle);

            foreach (var manager in _managers)
            {
                var hostContainingFocusedHandle = manager.FindLogicalChildren<HwndHost>().FirstOrDefault(hw => Win32Helper.IsChild(hw.Handle, e.GotFocusWinHandle));

                if (hostContainingFocusedHandle != null)
                {
                    var parentAnchorable = hostContainingFocusedHandle.FindVisualAncestor<LayoutAnchorableControl>();
                    if (parentAnchorable != null)
                    {
                        if (_modelFocusedElement.ContainsKey(parentAnchorable.Model))
                            _modelFocusedElement.Remove(parentAnchorable.Model);
                        _modelFocusedWindowHandle[parentAnchorable.Model] = e.GotFocusWinHandle;
                    }
                    else
                    {
                        var parentDocument = hostContainingFocusedHandle.FindVisualAncestor<LayoutDocumentControl>();
                        if (parentDocument != null)
                        {
                            if (_modelFocusedElement.ContainsKey(parentDocument.Model))
                                _modelFocusedElement.Remove(parentDocument.Model);

                            _modelFocusedWindowHandle[parentDocument.Model] = e.GotFocusWinHandle;
                        }
                    }
                }


            }
        }

        static IInputElement _lastFocusedElement = null;
        static void InputManager_EnterMenuMode(object sender, EventArgs e)
        {
            _lastFocusedElement = Keyboard.FocusedElement;

            if (_lastFocusedElement != null)
            {
                var lastfocusDepObj = _lastFocusedElement as DependencyObject;
                if (lastfocusDepObj.FindLogicalAncestor<DockingManager>() == null)
                    _lastFocusedElement = null;
            }

            Debug.WriteLine(string.Format("Current_EnterMenuMode({0})", Keyboard.FocusedElement));
        }
        static void InputManager_LeaveMenuMode(object sender, EventArgs e)
        {
            Debug.WriteLine(string.Format("Current_LeaveMenuMode({0})", Keyboard.FocusedElement));
            if (_lastFocusedElement != null)
            {
                if (_lastFocusedElement != Keyboard.Focus(_lastFocusedElement))
                    Debug.WriteLine("Unable to activate the element");
            }
        }

        #endregion

    }
}
