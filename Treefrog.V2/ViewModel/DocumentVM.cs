using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GalaSoft.MvvmLight;
using Treefrog.ViewModel.Commands;
using Treefrog.ViewModel.Tools;

namespace Treefrog.ViewModel
{
    public abstract class DocumentVM : ViewModelBase, IEditTarget
    {
        public abstract string Name { get; }

        private bool _isActive;
        public bool IsActive
        {
            get { return _isActive; }
            set
            {
                if (_isActive != value) {
                    _isActive = value;
                    RaisePropertyChanged("IsActive");
                }
            }
        }

        public virtual bool SupportsZoom
        {
            get { return false; }
        }

        public virtual double ZoomLevel
        {
            get { return 1; }
            set { }
        }

        public virtual string Coordinates
        {
            get { return ""; }
            set { }
        }

        public virtual CommandHistory CommandHistory
        {
            get { return null; }
        }

        public virtual IEnumerable<IToolCollection> RegisteredToolCollections
        {
            get { yield break; }
        }

        public T LookupToolCollection<T> () 
            where T : IToolCollection 
        {
            foreach (IToolCollection tc in RegisteredToolCollections)
                if (tc is T)
                    return (T)tc;
            return default(T);
        }

        #region IEditCommandProvider Members

        public virtual bool CanUndo
        {
            get { return false; }
        }

        public virtual bool CanRedo
        {
            get { return false; }
        }

        public virtual bool CanCut
        {
            get { return false; }
        }

        public virtual bool CanCopy
        {
            get { return false; }
        }

        public virtual bool CanPaste
        {
            get { return false; }
        }

        public virtual bool CanDelete
        {
            get { return false; }
        }

        public virtual bool CanSelectAll
        {
            get { return false; }
        }

        public virtual bool CanSelectNone
        {
            get { return false; }
        }

        public virtual void Undo ()
        { }

        public virtual void Redo ()
        { }

        public virtual void Cut ()
        { }

        public virtual void Copy ()
        { }

        public virtual void Paste ()
        { }

        public virtual void Delete ()
        { }

        public virtual void SelectAll ()
        { }

        public virtual void SelectNone ()
        { }

        public event EventHandler CanUndoChanged;
        public event EventHandler CanRedoChanged;
        public event EventHandler CanCutChanged;
        public event EventHandler CanCopyChanged;
        public event EventHandler CanPasteChanged;
        public event EventHandler CanDeleteChanged;
        public event EventHandler CanSelectAllChanged;
        public event EventHandler CanSelectNoneChanged;

        protected virtual void OnCanUndoChanged (EventArgs e)
        {
            if (CanUndoChanged != null)
                CanUndoChanged(this, e);
        }

        protected virtual void OnCanRedoChanged (EventArgs e)
        {
            if (CanRedoChanged != null)
                CanRedoChanged(this, e);
        }

        protected virtual void OnCanCutChanged (EventArgs e)
        {
            if (CanCutChanged != null)
                CanCutChanged(this, e);
        }

        protected virtual void OnCanCopyChanged (EventArgs e)
        {
            if (CanCopyChanged != null)
                CanCopyChanged(this, e);
        }

        protected virtual void OnCanPasteChanged (EventArgs e)
        {
            if (CanPasteChanged != null)
                CanPasteChanged(this, e);
        }

        protected virtual void OnCanDeleteChanged (EventArgs e)
        {
            if (CanDeleteChanged != null)
                CanDeleteChanged(this, e);
        }

        protected virtual void OnCanSelectAllChanged (EventArgs e)
        {
            if (CanSelectAllChanged != null)
                CanSelectAllChanged(this, e);
        }

        protected virtual void OnCanSelectNoneChanged (EventArgs e)
        {
            if (CanSelectNoneChanged != null)
                CanSelectNoneChanged(this, e);
        }

        #endregion
    }

    public interface IEditTarget
    {
        bool CanUndo { get; }
        bool CanRedo { get; }

        bool CanCut { get; }
        bool CanCopy { get; }
        bool CanPaste { get; }
        bool CanDelete { get; }

        bool CanSelectAll { get; }
        bool CanSelectNone { get; }

        void Undo ();
        void Redo ();

        void Cut ();
        void Copy ();
        void Paste ();
        void Delete ();

        void SelectAll ();
        void SelectNone ();

        event EventHandler CanUndoChanged;
        event EventHandler CanRedoChanged;

        event EventHandler CanCutChanged;
        event EventHandler CanCopyChanged;
        event EventHandler CanPasteChanged;
        event EventHandler CanDeleteChanged;

        event EventHandler CanSelectAllChanged;
        event EventHandler CanSelectNoneChanged;
    }
}
