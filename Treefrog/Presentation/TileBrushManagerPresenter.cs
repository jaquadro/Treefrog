using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Treefrog.Framework.Model;
using Treefrog.Presentation.Commands;

namespace Treefrog.Presentation
{
    public class SyncTileBrushEventArgs : EventArgs
    {
        public TileBrush PreviousBrush { get; private set; }

        public SyncTileBrushEventArgs (TileBrush brush)
        {
            PreviousBrush = brush;
        }
    }

    public interface ITileBrushManagerPresenter : ICommandSubscriber
    {
        TileBrushManager TileBrushManager { get; }

        TileBrush SelectedBrush { get; }

        event EventHandler SyncTileBrushManager;
        event EventHandler SyncTileBrushCollection;

        event EventHandler<SyncTileBrushEventArgs> SyncCurrentBrush;

        event EventHandler TileBrushSelected;

        void ActionSelectBrush (int brushId);

        void RefreshTileBrushCollection ();
    }

    public class TileBrushManagerPresenter : ITileBrushManagerPresenter
    {
        private IEditorPresenter _editor;

        private int _selectedBrush;
        private TileBrush _selectedBrushRef;

        public TileBrushManagerPresenter (IEditorPresenter editor)
        {
            _editor = editor;
            _editor.SyncCurrentProject += SyncCurrentProjectHandler;

            InitializeCommandManager();
        }

        private void SyncCurrentProjectHandler (object sender, SyncProjectEventArgs e)
        {
            SelectBrush(-1);

            OnSyncTileBrushManager(EventArgs.Empty);
            OnSyncTileBrushCollection(EventArgs.Empty);
        }

        #region Commands

        private CommandManager _commandManager;

        private void InitializeCommandManager ()
        {
            _commandManager = new CommandManager();

            _commandManager.Register(CommandKey.NewDynamicTileBrush, CommandCanCreateDynamicBrush, CommandCreateDynamicBrush);
            _commandManager.Register(CommandKey.TileBrushDelete, CommandCanDeleteBrush, CommandDeleteBrush);

            //_commandManager.Register(CommandKey.TileBrushFilter, CommandCanToggleFilter, CommandToggleFilter);
        }

        public CommandManager CommandManager
        {
            get { return _commandManager; }
        }

        private bool CommandCanCreateDynamicBrush ()
        {
            return TileBrushManager != null && TileBrushManager.DynamicBrushes != null;
        }

        private void CommandCreateDynamicBrush ()
        {

        }

        private bool CommandCanDeleteBrush ()
        {
            return false;
        }

        private void CommandDeleteBrush ()
        {

        }

        #endregion

        public TileBrushManager TileBrushManager
        {
            get { return _editor.Project.TileBrushManager; }
        }

        public TileBrush SelectedBrush
        {
            get { return _selectedBrushRef; }
        }

        public event EventHandler SyncTileBrushManager;

        public event EventHandler SyncTileBrushCollection;

        public event EventHandler<SyncTileBrushEventArgs> SyncCurrentBrush;

        public event EventHandler TileBrushSelected;

        protected virtual void OnSyncTileBrushManager (EventArgs e)
        {
            if (SyncTileBrushManager != null)
                SyncTileBrushManager(this, e);
        }

        protected virtual void OnSyncTileBrushCollection (EventArgs e)
        {
            if (SyncTileBrushCollection != null)
                SyncTileBrushCollection(this, e);
        }

        protected virtual void OnSyncCurrentBrush (SyncTileBrushEventArgs e)
        {
            if (SyncCurrentBrush != null)
                SyncCurrentBrush(this, e);
        }

        protected virtual void OnTileBrushSelected (EventArgs e)
        {
            if (TileBrushSelected != null)
                TileBrushSelected(this, e);
        }

        public void ActionSelectBrush (int brushId)
        {
            SelectBrush(brushId);
            OnTileBrushSelected(EventArgs.Empty);
        }

        public void RefreshTileBrushCollection ()
        {
            OnSyncTileBrushCollection(EventArgs.Empty);
        }

        private void SelectBrush (int brushId)
        {
            if (_selectedBrush == brushId)
                return;

            TileBrush prevBrush = _selectedBrushRef;
            TileBrush newBrush = TileBrushManager.GetBrush(brushId);

            _selectedBrushRef = newBrush;

            OnSyncCurrentBrush(new SyncTileBrushEventArgs(prevBrush));
        }
    }
}
