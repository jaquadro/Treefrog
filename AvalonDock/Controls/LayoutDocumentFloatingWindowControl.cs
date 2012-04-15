using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using AvalonDock.Layout;
using System.Windows;

namespace AvalonDock.Controls
{
    public class LayoutDocumentFloatingWindowControl : LayoutFloatingWindowControl
    {
        static LayoutDocumentFloatingWindowControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LayoutDocumentFloatingWindowControl), new FrameworkPropertyMetadata(typeof(LayoutDocumentFloatingWindowControl)));
        } 

        internal LayoutDocumentFloatingWindowControl(LayoutDocumentFloatingWindow model)
            :base(model)
        {
            _model = model;
            _model.RootDocumentChanged += (s, e) =>
            {
                if (_model.RootDocument == null)
                    InternalClose(); 
            };
        }


        LayoutDocumentFloatingWindow _model;

        public override ILayoutElement Model
        {
            get { return _model; }
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            var manager = _model.Root.Manager;

            Content = manager.CreateUIElementForModel(_model.RootDocument);

            ContextMenu = _model.Root.Manager.DocumentContextMenu;
            ContextMenu.DataContext = _model.RootDocument;

        }

        protected override IntPtr FilterMessage(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case Win32Helper.WM_NCLBUTTONDOWN: //Left button down on title -> start dragging over docking manager
                    if (wParam.ToInt32() == Win32Helper.HT_CAPTION)
                    {
                        _model.RootDocument.IsActive = true;
                        //FocusElementManager.SetFocusOnLastElement(_model.RootDocument);
                    }
                    break;
            }
               
            return base.FilterMessage(hwnd, msg, wParam, lParam, ref handled);
        }


        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
           
            if (CloseInitiatedByUser)
            {
                var root = Model.Root;
                root.Manager.RemoveFloatingWindow(this);
                root.FloatingWindows.Remove(_model);
                root.CollectGarbage();
            }
            
        }

        
    }
}
