using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Editor.Controls
{
    public class SubItemEventArgs : EventArgs
    {
        public int SubItem { get; private set; }

        public ListViewItem Item { get; private set; }

        public SubItemEventArgs (ListViewItem item, int subItem)
        {
            Item = item;
            SubItem = subItem;
        }
    }

    public class SubItemEndEditingEventArgs : SubItemEventArgs
    {
        public string DisplayText { get; set; }

        public bool Cancel { get; set; }

        public SubItemEndEditingEventArgs (ListViewItem item, int subItem, string display, bool cancel)
            : base(item, subItem)
        {
            DisplayText = display;
            Cancel = cancel;
        }
    }

    public class ListViewSubItemEx : ListViewItem.ListViewSubItem
    {
        public ListViewSubItemEx ()
            : base()
        {
        }

        public ListViewSubItemEx (ListViewItem owner, string text)
            : base(owner, text)
        {
        }

        public ListViewSubItemEx (ListViewItem owner, string text, Color foreColor, Color backColor, Font font)
            : base(owner, text, foreColor, backColor, font)
        {
        }

        public bool ReadOnly { get; set; }
    }

    public class EditableListView : ListView
    {
        // ListView messages
        private const int LVM_FIRST                 = 0x1000;
        private const int LVM_GETCOLUMNORDERARRAY   = (LVM_FIRST + 59);

        // Windows Messages that will abort editing
        private const int WM_HSCROLL = 0x114;
        private const int WM_VSCROLL = 0x115;
        private const int WM_SIZE    = 0x05;
        private const int WM_NOTIFY  = 0x4E;

        private const int HDN_FIRST = -300;
        private const int HDN_BEGINDRAG = (HDN_FIRST-10);
        private const int HDN_ITEMCHANGINGA = (HDN_FIRST-0);
        private const int HDN_ITEMCHANGINGW = (HDN_FIRST-20);

        #region Fields

        private System.ComponentModel.Container _components = null;

        private Control _editControl;
        private ListViewItem _editItem;
        private int _editSubItem;

        private bool _doubleClickActivation = true;

        #endregion

        #region Constructors

        public EditableListView ()
        {
            InitializeComponent();

            base.FullRowSelect = true;
            base.View = View.Details;
            base.AllowColumnReorder = false;
        }

        #endregion

        #region Properties

        public bool DoubleClickActivation
        {
            get { return _doubleClickActivation; }
            set { _doubleClickActivation = value; }
        }

        public bool Editing
        {
            get { return _editControl != null; }
        }

        #endregion

        #region Events

        public event EventHandler<SubItemEventArgs> SubItemClicked;
        public event EventHandler<SubItemEventArgs> SubItemBeginEditing;
        public event EventHandler<SubItemEndEditingEventArgs> SubItemEndEditing;

        #endregion

        #region Event Dispatchers

        protected void OnSubItemBeginEditing (SubItemEventArgs e)
        {
            if (SubItemBeginEditing != null) {
                SubItemBeginEditing(this, e);
            }
        }

        protected void OnSubItemEndEditing (SubItemEndEditingEventArgs e)
        {
            if (SubItemEndEditing != null) {
                SubItemEndEditing(this, e);
            }
        }

        protected void OnSubItemClicked (SubItemEventArgs e)
        {
            if (SubItemClicked != null) {
                SubItemClicked(this, e);
            }
        }

        protected override void OnMouseUp (MouseEventArgs e)
        {
            base.OnMouseUp(e);

            if (DoubleClickActivation) {
                return;
            }

            EditSubItemAt(Location);
        }

        protected override void OnDoubleClick (EventArgs e)
        {
            base.OnDoubleClick(e);

            if (!DoubleClickActivation) {
                return;
            }

            Point p = PointToClient(Cursor.Position);
            EditSubItemAt(p);
        }

        #endregion

        #region Event Handlers

        private void EditControlLeaveHandler (object sender, EventArgs e)
        {
            EndEditing(true);
        }

        private void EditControlKeyPressHandler (object sender, KeyPressEventArgs e)
        {
            switch (e.KeyChar) {
                case (char)Keys.Escape:
                    EndEditing(false);
                    break;
                case (char)Keys.Enter:
                    EndEditing(true);
                    break;
            }
        }

        #endregion

        private void InitializeComponent ()
        {
            _components = new System.ComponentModel.Container();
        }

        private void EditSubItemAt (Point p)
        {
            ListViewItem item;
            int index = GetSubItemAt(p.X, p.Y, out item);

            if (index > 0) {
                OnSubItemClicked(new SubItemEventArgs(item, index));
            }
        }

        public int GetSubItemAt (int x, int y, out ListViewItem item)
        {
            item = this.GetItemAt(x, y);

            if (item != null) {
                int[] order = GetColumnOrder();
                Rectangle itemBounds = item.GetBounds(ItemBoundsPortion.Entire);

                int subItemX = itemBounds.Left;
                for (int i = 0; i < order.Length; i++) {
                    ColumnHeader h = Columns[order[i]];
                    if (x < subItemX + h.Width) {
                        return h.Index;
                    }

                    subItemX += h.Width;
                }
            }

            return -1;
        }

        public int[] GetColumnOrder ()
        {
            IntPtr lParam = Marshal.AllocHGlobal(Marshal.SizeOf(typeof(int)) * Columns.Count);
            IntPtr result = SendMessage(Handle, LVM_GETCOLUMNORDERARRAY, new IntPtr(Columns.Count), lParam);

            if (result.ToInt32() == 0) {
                Marshal.FreeHGlobal(lParam);
                return null;
            }

            int[] order = new int[Columns.Count];
            Marshal.Copy(lParam, order, 0, Columns.Count);
            Marshal.FreeHGlobal(lParam);

            return order;
        }

        public Rectangle GetSubItemBounds (ListViewItem item, int subItem)
        {
            int[] order = GetColumnOrder();

            if (subItem >= order.Length) {
                throw new IndexOutOfRangeException("SubItem '" + subItem + "' is out of range.");
            }

            if (item == null) {
                throw new ArgumentNullException("item");
            }

            Rectangle itemBounds = item.GetBounds(ItemBoundsPortion.Entire);

            int subItemX = itemBounds.Left;
            for (int i = 0; i < order.Length; i++) {
                ColumnHeader col = Columns[order[i]];
                if (col.Index == subItem) {
                    return new Rectangle(subItemX, itemBounds.Top, col.Width, itemBounds.Height);
                }
                subItemX += col.Width;
            }

            return Rectangle.Empty;
        }

        public void StartEditing (Control editor, ListViewItem item, int subItem)
        {
            OnSubItemBeginEditing(new SubItemEventArgs(item, subItem));

            Rectangle rectSubItem = GetSubItemBounds(item, subItem);

            if (rectSubItem.X < 0) {
                rectSubItem.Width += rectSubItem.X;
                rectSubItem.X = 0;
            }

            if (rectSubItem.X + rectSubItem.Width > Width) {
                rectSubItem.Width = Width - rectSubItem.Left;
            }

            rectSubItem.Offset(Left, Top);

            Point origin = new Point(0, 0);
            Point controlOrigin = Parent.PointToScreen(origin);
            Point editorOrigin = editor.Parent.PointToScreen(origin);

            rectSubItem.Offset(controlOrigin.X - editorOrigin.X, controlOrigin.Y - editorOrigin.Y);

            editor.Bounds = rectSubItem;
            editor.Text = item.SubItems[subItem].Text;
            editor.Visible = true;
            editor.BringToFront();
            editor.Focus();

            _editControl = editor;
            _editControl.Leave += EditControlLeaveHandler;
            _editControl.KeyPress += EditControlKeyPressHandler;

            _editItem = item;
            _editSubItem = subItem;
        }

        public void EndEditing (bool acceptChanges)
        {
            if (_editControl == null) {
                return;
            }

            SubItemEndEditingEventArgs e = new SubItemEndEditingEventArgs(_editItem, _editSubItem,
                acceptChanges ? _editControl.Text : _editItem.SubItems[_editSubItem].Text, !acceptChanges);

            OnSubItemEndEditing(e);

            _editItem.SubItems[_editSubItem].Text = e.DisplayText;

            _editControl.Leave -= EditControlLeaveHandler;
            _editControl.KeyPress -= EditControlKeyPressHandler;

            _editControl.Visible = false;

            _editControl = null;
            _editItem = null;
            _editSubItem = -1;
        }

        protected override void WndProc (ref Message m)
        {
            switch (m.Msg) {
                case WM_VSCROLL:
                case WM_HSCROLL:
                case WM_SIZE:
                    EndEditing(false);
                    break;
                case WM_NOTIFY:
                    NMHDR h = (NMHDR)Marshal.PtrToStructure(m.LParam, typeof(NMHDR));
                    switch (h.code) {
                        case HDN_BEGINDRAG:
                        case HDN_ITEMCHANGINGA:
                        case HDN_ITEMCHANGINGW:
                            EndEditing(false);
                            break;
                    }
                    break;
            }

            base.WndProc(ref m);
        }

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage (
            IntPtr hWnd, 
            int msg,  
            IntPtr wPar, 
            IntPtr lPar
            );

        [DllImport("user32.dll", CharSet=CharSet.Ansi)]
        private static extern IntPtr SendMessage (
            IntPtr hWnd, 
            int msg, 
            int len, 
            ref int [] order
            );

        private struct NMHDR 
        { 
            public IntPtr hwndFrom; 
            public Int32  idFrom; 
            public Int32  code; 
        }
    }
}
