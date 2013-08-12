using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Treefrog.Aux;
using Treefrog.Framework.Imaging;
using Treefrog.Framework.Model;
using Treefrog.Presentation;
using Treefrog.Windows.Forms;
using Treefrog.Plugins.Tiles.UI;

namespace Treefrog.Plugins.Tiles
{
    public class TilePoolCommandActions
    {
        private PresenterManager _pm;
        //private EditorPresenter _editor;

        /*public TilePoolCommandActions (EditorPresenter editor)
        {
            _editor = editor;
        }*/

        public TilePoolCommandActions (PresenterManager pm)
        {
            _pm = pm;
        }

        private EditorPresenter Editor
        {
            get { return _pm.Lookup<EditorPresenter>(); }
        }

        public bool TilePoolExists (object param)
        {
            if (!(param is Guid))
                return false;

            if (Editor == null || Editor.Project == null)
                return false;

            return Editor.Project.TilePoolManager.Pools.Contains((Guid)param);
        }

        public void CommandImport ()
        {
            List<string> currentNames = new List<string>();
            foreach (TilePool pool in Editor.Project.TilePoolManager.Pools)
                currentNames.Add(pool.Name);

            using (ImportTilePool form = new ImportTilePool()) {
                if (form.ShowDialog() == DialogResult.OK && form.Pool != null)
                    Editor.Project.TilePoolManager.MergePool(form.Pool.Name, form.Pool);
            }
        }

        public void CommandImportMerge (object param)
        {
            if (!TilePoolExists(param))
                return;

            Guid uid = (Guid)param;
            TilePool tilePool = Editor.Project.TilePoolManager.Pools[uid];

            using (ImportTilePool form = new ImportTilePool(tilePool.Name, tilePool.TileWidth, tilePool.TileHeight)) {
                if (form.ShowDialog() == DialogResult.OK && form.Pool != null)
                    tilePool.Merge(form.Pool, TileImportPolicy.SetUnique);
            }
        }

        public void CommandDelete (object param)
        {
            if (!TilePoolExists(param))
                return;

            Editor.Project.TilePoolManager.Pools.Remove((Guid)param);
        }

        public void CommandRename (object param)
        {
            if (!TilePoolExists(param))
                return;

            Guid uid = (Guid)param;
            TilePool tilePool = Editor.Project.TilePoolManager.Pools[uid];

            using (NameChangeForm form = new NameChangeForm(tilePool.Name)) {
                foreach (TilePool pool in Editor.Project.TilePoolManager.Pools) {
                    if (pool.Name != tilePool.Name)
                        form.ReservedNames.Add(pool.Name);
                }

                if (form.ShowDialog() == DialogResult.OK)
                    tilePool.TrySetName(form.Name);
            }
        }

        public void CommandProperties (object param)
        {
            if (!TilePoolExists(param))
                return;

            Guid uid = (Guid)param;
            TilePool pool = Editor.Project.TilePoolManager.Pools[uid];

            Editor.Presentation.PropertyList.Provider = pool;
            Editor.ActivatePropertyPanel();
        }

        public void CommandExport (object param)
        {
            if (!TilePoolExists(param))
                return;

            Guid uid = (Guid)param;
            TilePool tilePool = Editor.Project.TilePoolManager.Pools[uid];

            using (System.Drawing.Bitmap export = tilePool.TileSource.CreateBitmap()) {
                using (SaveFileDialog ofd = new SaveFileDialog()) {
                    ofd.Title = "Export Raw Tileset";
                    ofd.Filter = "Portable Network Graphics (*.png)|*.png|Windows Bitmap (*.bmp)|*.bmp|All Files|*";
                    ofd.OverwritePrompt = true;
                    ofd.RestoreDirectory = false;

                    if (ofd.ShowDialog() == DialogResult.OK) {
                        try {
                            export.Save(ofd.FileName);
                        }
                        catch {
                            MessageBox.Show("Could not save image file.", "Export Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        }
                    }
                }
            }
        }

        public void CommandImportOver (object param)
        {
            if (!TilePoolExists(param))
                return;

            Guid uid = (Guid)param;
            TilePool tilePool = Editor.Project.TilePoolManager.Pools[uid];

            using (OpenFileDialog ofd = new OpenFileDialog()) {
                ofd.Title = "Import Raw Tileset";
                ofd.Filter = "Images Files|*.bmp;*.gif;*.png|All Files|*";
                ofd.Multiselect = false;
                ofd.RestoreDirectory = false;

                if (ofd.ShowDialog() == DialogResult.OK) {
                    try {
                        TextureResource import = TextureResourceBitmapExt.CreateTextureResource(ofd.FileName);

                        TextureResource original = tilePool.TileSource;
                        if (original.Width != import.Width || original.Height != import.Height) {
                            MessageBox.Show("Imported tileset dimensions are incompatible with the selected Tile Pool.", "Import Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            return;
                        }

                        tilePool.Tiles.ReplaceTexture(import);
                    }
                    catch {
                        MessageBox.Show("Could not read selected image file.", "Import Failed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        return;
                    }
                }
            }
        }
    }
}
