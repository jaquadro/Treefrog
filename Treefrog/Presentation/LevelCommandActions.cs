using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Treefrog.Framework.Model;
using Treefrog.Windows.Forms;

namespace Treefrog.Presentation
{
    class LevelCommandActions
    {
        private PresenterManager _pm;

        public LevelCommandActions (PresenterManager pm)
        {
            _pm = pm;
        }

        private EditorPresenter Editor
        {
            get { return _pm.Lookup<EditorPresenter>(); }
        }

        public bool LevelExists (object param)
        {
            if (!(param is Guid))
                return false;

            if (Editor == null || Editor.Project == null)
                return false;

            return Editor.Project.Levels.Contains((Guid)param);
        }

        public bool CanOpenLevel (object param)
        {
            if (!(param is Guid))
                return false;

            Guid uid = (Guid)param;
            if (Editor.ContentWorkspace.IsContentOpen(uid))
                return false;

            return Editor.ContentWorkspace.IsContentValid(uid);
        }

        public void CommandOpen (object param)
        {
            if (!CanOpenLevel(param))
                return;

            Editor.ContentWorkspace.OpenContent((Guid)param);
        }

        public void CommandDelete (object param)
        {
            if (!LevelExists(param))
                return;

            if (MessageBox.Show("Are you sure you want to delete this level?\nThis operation cannot be undone.",
                "Confirm Delete", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) != DialogResult.OK) {
                return;
            }

            Guid uid = (Guid)param;
            if (Editor.ContentWorkspace.IsContentOpen(uid))
                Editor.ContentWorkspace.CloseContent(uid);

            Editor.Project.Levels.Remove(uid);
        }

        public void CommandClone (object param)
        {
            if (!LevelExists(param))
                return;

            Level level = Editor.Project.Levels[(Guid)param];

            Level clone = new Level(level, Editor.Project);
            clone.TrySetName(Editor.Project.Levels.CompatibleName(level.Name));

            Editor.Project.Levels.Add(clone);
        }

        public void CommandRename (object param)
        {
            if (!LevelExists(param))
                return;

            Level level = Editor.Project.Levels[(Guid)param];

            using (NameChangeForm form = new NameChangeForm(level.Name)) {
                foreach (Level lev in level.Project.Levels)
                    form.ReservedNames.Add(lev.Name);

                if (form.ShowDialog() == DialogResult.OK)
                    level.TrySetName(form.Name);
            }
        }

        public void CommandProperties (object param)
        {
            if (!LevelExists(param))
                return;

            Editor.Presentation.PropertyList.Provider = Editor.Project.Levels[(Guid)param];
            Editor.ActivatePropertyPanel();
        }
    }
}
