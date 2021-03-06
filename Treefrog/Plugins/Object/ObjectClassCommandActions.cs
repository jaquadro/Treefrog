﻿using System;
using System.Windows.Forms;
using Treefrog.Framework.Imaging;
using Treefrog.Framework.Model;
using Treefrog.Plugins.Object.UI;
using Treefrog.Presentation;
using Treefrog.Windows.Forms;

namespace Treefrog.Plugins.Object
{
    public class ObjectClassCommandActions
    {
        private PresenterManager _pm;
        //private EditorPresenter Editor;

        /*public ObjectClassCommandActions (EditorPresenter editor)
        {
            _editor = editor;
        }*/

        public ObjectClassCommandActions (PresenterManager pm)
        {
            _pm = pm;
        }

        private EditorPresenter Editor
        {
            get { return _pm.Lookup<EditorPresenter>(); }
        }

        public bool ObjectExists (object param)
        {
            if (!(param is Guid))
                return false;

            if (Editor == null || Editor.Project == null)
                return false;

            return Editor.Project.ObjectPoolManager.Contains((Guid)param);
        }

        public void CommandEdit (object param)
        {
            if (!ObjectExists(param))
                return;

            Guid uid = (Guid)param;
            ObjectPool objPool = Editor.Project.ObjectPoolManager.PoolFromItemKey(uid);
            ObjectClass objClass = objPool.GetObject(uid);

            using (ImportObject form = new ImportObject(objClass)) {
                foreach (ObjectClass obj in objPool.Objects) {
                    if (obj.Name != objClass.Name)
                        form.ReservedNames.Add(obj.Name);
                }

                if (form.ShowDialog() == DialogResult.OK) {
                    using (objClass.BeginModify()) {
                        if (form.SourceImage != null)
                            objClass.Image = form.SourceImage;
                        objClass.TrySetName(form.ObjectName);
                        objClass.MaskBounds = new Rectangle(form.MaskLeft ?? 0, form.MaskTop ?? 0,
                            (form.MaskRight ?? 0) - (form.MaskLeft ?? 0), (form.MaskBottom ?? 0) - (form.MaskTop ?? 0));
                        objClass.Origin = new Point(form.OriginX ?? 0, form.OriginY ?? 0);
                    }
                }
            }
        }

        public void CommandClone (object param)
        {
            if (!ObjectExists(param))
                return;

            Guid uid = (Guid)param;
            ObjectPool objPool = Editor.Project.ObjectPoolManager.PoolFromItemKey(uid);
            ObjectClass objClass = objPool.GetObject(uid);

            ObjectClass newClass = new ObjectClass(objPool.Objects.CompatibleName(objClass.Name), objClass);
            objPool.AddObject(newClass);
        }

        public void CommandDelete (object param)
        {
            if (!ObjectExists(param))
                return;

            Guid uid = (Guid)param;
            ObjectPool objPool = Editor.Project.ObjectPoolManager.PoolFromItemKey(uid);

            objPool.RemoveObject(uid);
        }

        public void CommandRename (object param)
        {
            if (!ObjectExists(param))
                return;

            Guid uid = (Guid)param;
            ObjectPool objPool = Editor.Project.ObjectPoolManager.PoolFromItemKey(uid);
            ObjectClass objClass = objPool.GetObject(uid);

            using (NameChangeForm form = new NameChangeForm(objClass.Name)) {
                foreach (ObjectClass obj in objPool.Objects) {
                    if (obj.Name != objClass.Name)
                        form.ReservedNames.Add(obj.Name);
                }

                if (form.ShowDialog() == DialogResult.OK)
                    objClass.TrySetName(form.Name);
            }
        }

        public void CommandProperties (object param)
        {
            if (!ObjectExists(param))
                return;

            Guid uid = (Guid)param;
            ObjectPool objPool = Editor.Project.ObjectPoolManager.PoolFromItemKey(uid);
            ObjectClass objClass = objPool.GetObject(uid);

            Editor.Presentation.PropertyList.Provider = objClass;
            Editor.ActivatePropertyPanel();
        }
    }
}
