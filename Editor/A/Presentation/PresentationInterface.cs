using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Treefrog.Framework.Model;

namespace Editor.A.Presentation
{
    public interface IStandardToolsPresenter
    {
        bool CanCreateProject { get; }
        bool CanOpenProject { get; }
        bool CavSaveProject { get; }

        void ActionCreateProject ();
        void ActionOpenProject (string path);
        void ActionSaveProject (string path);

        event EventHandler SyncStandardToolsActions;

        void RefreshStandardTools ();
    }

    public interface IDocumentToolsPresenter
    {
        bool CanCut { get; }
        bool CanCopy { get; }
        bool CanPaste { get; }

        bool CanUndo { get; }
        bool CanRedo { get; }

        void ActionCut ();
        void ActionCopy ();
        void ActionPaste ();

        void ActionUndo ();
        void ActionRedo ();

        event EventHandler SyncDocumentToolsActions;

        void RefreshDocumentTools ();
    }

    

    public class StandardToolsPresenter : IStandardToolsPresenter
    {

        #region IStandardToolsPresenter Members

        public bool CanCreateProject
        {
            get { throw new NotImplementedException(); }
        }

        public bool CanOpenProject
        {
            get { throw new NotImplementedException(); }
        }

        public bool CavSaveProject
        {
            get { throw new NotImplementedException(); }
        }

        public void ActionCreateProject ()
        {
            throw new NotImplementedException();
        }

        public void ActionOpenProject (string path)
        {
            throw new NotImplementedException();
        }

        public void ActionSaveProject (string path)
        {
            throw new NotImplementedException();
        }

        public event EventHandler SyncStandardToolsActions;

        public void RefreshStandardTools ()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
