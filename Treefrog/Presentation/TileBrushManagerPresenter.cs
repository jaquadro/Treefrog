using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Treefrog.Framework.Model;
using Treefrog.Presentation.Commands;
using Treefrog.Windows.Forms;
using System.Windows.Forms;

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
        void ActionEditBrush (int brushId);

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
            _commandManager.Register(CommandKey.TileBrushClone, CommandCanCloneBrush, CommandCloneBrush);
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
            if (CommandCanCreateDynamicBrush()) {
                using (DynamicBrushForm form = new DynamicBrushForm()) {
                    form.BindTileController(_editor.Presentation.TilePoolList);
                    foreach (TileBrush item in TileBrushManager.Brushes)
                        form.ReservedNames.Add(item.Name);

                    if (form.ShowDialog() == DialogResult.OK) {
                        if (TileBrushManager.DynamicBrushes.GetBrush(form.Brush.Id) == null)
                            TileBrushManager.DynamicBrushes.AddBrush(form.Brush);

                        OnSyncTileBrushCollection(EventArgs.Empty);
                        SelectBrush(form.Brush.Id);
                        OnTileBrushSelected(EventArgs.Empty);
                    }
                }
            }
        }

        private bool CommandCanCloneBrush ()
        {
            return SelectedBrush != null;
        }

        private void CommandCloneBrush ()
        {
            if (CommandCanCloneBrush()) {
                string name = FindCloneBrushName(SelectedBrush.Name);

                DynamicBrush brush = SelectedBrush as DynamicBrush;
                if (brush == null)
                    return;

                DynamicBrush newBrush = new DynamicBrush(name, brush.TileWidth, brush.TileHeight, brush.BrushClass);
                for (int i = 0; i < brush.BrushClass.SlotCount; i++)
                    newBrush.SetTile(i, brush.GetTile(i));

                TileBrushManager.DynamicBrushes.AddBrush(newBrush);

                OnSyncTileBrushCollection(EventArgs.Empty);
                SelectBrush(newBrush.Id);
                OnTileBrushSelected(EventArgs.Empty);
            }
        }

        private bool CommandCanDeleteBrush ()
        {
            return SelectedBrush != null;
        }

        private void CommandDeleteBrush ()
        {
            if (CommandCanDeleteBrush()) {
                TileBrushManager.DynamicBrushes.RemoveBrush(SelectedBrush.Name);
                OnSyncTileBrushCollection(EventArgs.Empty);
                SelectBrush(-1);
            }
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
            CommandManager.Invalidate(CommandKey.TileBrushDelete);
            CommandManager.Invalidate(CommandKey.TileBrushClone);

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

        public void ActionEditBrush (int brushId)
        {
            DynamicBrush brush = TileBrushManager.GetBrush(brushId) as DynamicBrush;
            if (brush == null)
                return;

            using (DynamicBrushForm form = new DynamicBrushForm(brush)) {
                form.BindTileController(_editor.Presentation.TilePoolList);
                foreach (TileBrush item in TileBrushManager.Brushes)
                    if (item.Name != brush.Name)
                        form.ReservedNames.Add(item.Name);

                if (form.ShowDialog() == DialogResult.OK) {
                    OnSyncTileBrushCollection(EventArgs.Empty);
                    SelectBrush(form.Brush.Id);
                    OnTileBrushSelected(EventArgs.Empty);
                }
            }
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

        private string FindCloneBrushName (string basename)
        {
            List<string> names = new List<string>();
            foreach (TileBrush brush in TileBrushManager.Brushes) {
                names.Add(brush.Name);
            }

            int i = 0;
            while (true) {
                string name = basename + " (" + ++i + ")";
                if (names.Contains(name)) {
                    continue;
                }
                return name;
            }
        }
    }
}
