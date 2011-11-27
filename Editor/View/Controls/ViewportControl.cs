using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Treefrog.View.Controls
{
    public interface IScrollableControl
    {
        event ScrollEventHandler Scroll;
        event EventHandler VirtualSizeChanged;
        event EventHandler ScrollPropertyChanged;

        Control Control { get; }
        Rectangle VirtualSize { get; }
        float Zoom { get; }

        int GetScrollValue (ScrollOrientation orientation);
        int GetScrollSmallChange (ScrollOrientation orientation);
        int GetScrollLargeChange (ScrollOrientation orientation);

        bool GetScrollEnabled (ScrollOrientation orientation);

        void SetScrollValue (ScrollOrientation orientation, int value);
    }

    [Designer(typeof(ViewportControlDesigner))] 
    public partial class ViewportControl : UserControl
    {
        private IScrollableControl _control;

        private int _inScrollHandler;

        public ViewportControl ()
        {
            InitializeComponent();

            _hScrollBar.Scroll += ScrollbarScrollHandler;
            _vScrollBar.Scroll += ScrollbarScrollHandler;
        }

        internal ViewportPanel ContentPanel
        {
            get { return panel1; }
        }

        public IScrollableControl Control
        {
            get { return _control; }
            set
            {
                if (_control != null) {
                    _control.Scroll -= ControlScrollHandler;
                    _control.ScrollPropertyChanged -= ControlVirtualSizeChangedHandler;
                    _control.VirtualSizeChanged -= ControlVirtualSizeChangedHandler;
                    _control.Control.Resize -= ControlVirtualSizeChangedHandler;
                }

                _control = value;

                if (_control != null) {
                    panel1.Controls.Clear();
                    panel1.Controls.Add(_control.Control);

                    _control.Scroll += ControlScrollHandler;
                    _control.ScrollPropertyChanged += ControlVirtualSizeChangedHandler;
                    _control.VirtualSizeChanged += ControlVirtualSizeChangedHandler;
                    _control.Control.Resize += ControlVirtualSizeChangedHandler;
                }
            }
        }

        protected override ControlCollection CreateControlsInstance ()
        {
            return new ControlCollection(this);
        }

        public void ScrollbarScrollHandler (object sender, ScrollEventArgs e)
        {
            if (Interlocked.Exchange(ref _inScrollHandler, 1) != 0) {
                return;
            }

            if (_control != null) {
                switch (e.ScrollOrientation) {
                    case ScrollOrientation.HorizontalScroll:
                        _control.SetScrollValue(ScrollOrientation.HorizontalScroll, e.NewValue);
                        break;

                    case ScrollOrientation.VerticalScroll:
                        _control.SetScrollValue(ScrollOrientation.VerticalScroll, e.NewValue);
                        break;
                }
            }

            Interlocked.Exchange(ref _inScrollHandler, 0);
        }

        public void ControlScrollHandler (object sender, ScrollEventArgs e)
        {
            if (Interlocked.Exchange(ref _inScrollHandler, 1) != 0) {
                return;
            }

            switch (e.ScrollOrientation) {
                case ScrollOrientation.HorizontalScroll:
                    _hScrollBar.Value = Math.Max(0, Math.Min(_hScrollBar.Maximum, e.NewValue));
                    break;

                case ScrollOrientation.VerticalScroll:
                    _vScrollBar.Value = Math.Max(0, Math.Min(_vScrollBar.Maximum, e.NewValue));
                    break;
            }

            Interlocked.Exchange(ref _inScrollHandler, 0);
        }

        public void ControlVirtualSizeChangedHandler (object sender, EventArgs e)
        {
            Rectangle vsize = _control.VirtualSize;
            float zoom = _control.Zoom;

            int hLargeChange = _control.GetScrollLargeChange(ScrollOrientation.HorizontalScroll);
            int vLargeChange = _control.GetScrollLargeChange(ScrollOrientation.VerticalScroll);

            int hSmallChange = _control.GetScrollSmallChange(ScrollOrientation.HorizontalScroll);
            int vSmallChange = _control.GetScrollSmallChange(ScrollOrientation.VerticalScroll);

            bool hEnabled = _control.GetScrollEnabled(ScrollOrientation.HorizontalScroll);
            bool vEnabled = _control.GetScrollEnabled(ScrollOrientation.VerticalScroll);

            int width = _control.Control.Width;
            int height = _control.Control.Height;

            // Update scrollbar properties

            _hScrollBar.Minimum = 0;
            _vScrollBar.Minimum = 0;
            _hScrollBar.Maximum = (width > vsize.Width * zoom) ? 0
                : (int)(vsize.Width * zoom) - width + (int)(hLargeChange * zoom) -(int)(1 * zoom);
            _vScrollBar.Maximum = (height > vsize.Height * zoom) ? 0
                : (int)(vsize.Height * zoom) - height + (int)(vLargeChange * zoom) -(int)(1 * zoom);

            _hScrollBar.Maximum = (int)(_hScrollBar.Maximum / zoom);
            _vScrollBar.Maximum = (int)(_vScrollBar.Maximum / zoom);

            _hScrollBar.SmallChange = hSmallChange;
            _vScrollBar.SmallChange = vSmallChange;
            _hScrollBar.LargeChange = hLargeChange;
            _vScrollBar.LargeChange = vLargeChange;

            _hScrollBar.Value = Math.Min(_hScrollBar.Value, Math.Max(0, _hScrollBar.Maximum - _hScrollBar.LargeChange));
            _vScrollBar.Value = Math.Min(_vScrollBar.Value, Math.Max(0, _vScrollBar.Maximum - _vScrollBar.LargeChange));

            if (_hScrollBar.Maximum <= 0) {
                _hScrollBar.Maximum = 0;
                _hScrollBar.Value = 0;
                _hScrollBar.Enabled = false;
            }
            else {
                _hScrollBar.Enabled = true & hEnabled;
            }

            if (_vScrollBar.Maximum <= 0) {
                _vScrollBar.Maximum = 0;
                _vScrollBar.Value = 0;
                _vScrollBar.Enabled = false;
            }
            else {
                _vScrollBar.Enabled = true & vEnabled;
            }
        }
    }

    internal class ViewportControlDesigner : ParentControlDesigner
    {
        private ViewportControl _viewportControl;

        public override void Initialize (IComponent component)
        {
            base.Initialize(component);

            _viewportControl = component as ViewportControl;

            EnableDesignMode(_viewportControl.ContentPanel, "ScrollableControl1");
        }
    }

    public sealed class ViewportPanel : Panel
    {
        private class ViewportPanelControlCollection : ControlCollection
        {
            public ViewportPanelControlCollection (Control owner)
                : base(owner)
            {
            }

            public override void Add (Control value)
            {
                if (value as IScrollableControl == null) {
                    throw new NotSupportedException("ViewportPanel only accepts controls implementing IScrollableControl");
                }

                base.Clear();
                base.Add(value);
            }
        }

        private ViewportControl _owner;

        #region Events

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public new event EventHandler AutoSizeChanged
        {
            add { base.AutoSizeChanged += value; }
            remove { base.AutoSizeChanged -= value; }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event EventHandler LocationChanged
        {
            add { base.LocationChanged += value; }
            remove { base.LocationChanged -= value; }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event EventHandler TabIndexChanged
        {
            add { base.TabIndexChanged += value; }
            remove { base.TabIndexChanged -= value; }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event EventHandler TabStopChanged
        {
            add { base.TabStopChanged += value; }
            remove { base.TabStopChanged -= value; }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new event EventHandler VisibleChanged
        {
            add { base.VisibleChanged += value; }
            remove { base.VisibleChanged -= value; }
        }

        #endregion

        public ViewportPanel (UserControl owner)
            : this(owner as ViewportControl)
        {
        }

        public ViewportPanel (ViewportControl owner)
        {
            _owner = owner;

            base.SetStyle(ControlStyles.ResizeRedraw, true);
        }

        #region Methods

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new AnchorStyles Anchor
        {
            get { return base.Anchor; }
            set { base.Anchor = value; }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public override AutoSizeMode AutoSizeMode
        {
            get { return AutoSizeMode.GrowOnly; }
            set { }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new BorderStyle BorderStyle
        {
            get { return base.BorderStyle; }
            set { base.BorderStyle = value; }
        }

        protected override Padding DefaultMargin
        {
            get { return new Padding(0, 0, 0, 0); }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new DockStyle Dock
        {
            get { return base.Dock; }
            set { base.Dock = value; }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new ScrollableControl.DockPaddingEdges DockPadding
        {
            get { return base.DockPadding; }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new Point Location
        {
            get { return base.Location; }
            set { base.Location = value; }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new Size MaximumSize
        {
            get { return base.MaximumSize; }
            set { base.MaximumSize = value; }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new Size MinimumSize
        {
            get { return base.MinimumSize; }
            set { base.MinimumSize = value; }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new string Name
        {
            get { return base.Name; }
            set { base.Name = value; }
        }

        internal ViewportControl Owner
        {
            get { return _owner; }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new Control Parent
        {
            get { return base.Parent; }
            set { base.Parent = value; }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new Size Size
        {
            get { return base.Size; }
            set { base.Size = value; }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new int TabIndex
        {
            get { return base.TabIndex; }
            set { base.TabIndex = value; }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new bool TabStop
        {
            get { return base.TabStop; }
            set { base.TabStop = value; }
        }

        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public new bool Visible
        {
            get { return base.Visible; }
            set { base.Visible = value; }
        }

        #endregion

    }
}
