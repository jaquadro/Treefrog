using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Treefrog.Presentation;
using Treefrog.Windows.Forms;
using System.Windows.Forms;
using Treefrog.Framework.Model;

namespace Treefrog
{
    public class LibraryCommandActions
    {
        private EditorPresenter _editor;

        public LibraryCommandActions (EditorPresenter editor)
        {
            _editor = editor;
        }

        public bool LibraryExists (object param)
        {
            if (!(param is Guid))
                return false;

            if (_editor.Project == null)
                return false;

            return _editor.Project.LibraryManager.Libraries.Contains((Guid)param);
        }

        public void CommandCreate ()
        {
            using (NameChangeForm form = new NameChangeForm(FindDefaultLibraryName("New Library"), "Add New Library")) {
                foreach (var library in _editor.Project.LibraryManager.Libraries)
                    form.ReservedNames.Add(library.Name);

                if (form.ShowDialog() == DialogResult.OK) {
                    Library library = new Library(form.Name);
                    _editor.Project.LibraryManager.Libraries.Add(library);
                }
            }
        }

        public void CommandSetDefault (object param)
        {
            if (!LibraryExists(param))
                return;

            Guid uid = (Guid)param;
            Library library = _editor.Project.LibraryManager.Libraries[uid];

            _editor.Project.DefaultLibrary = library;
        }

        private string FindDefaultLibraryName (string basename)
        {
            List<string> names = new List<string>();
            foreach (var library in _editor.Project.LibraryManager.Libraries)
                names.Add(library.Name);

            if (!names.Contains(basename))
                return basename;

            int i = 0;
            while (true) {
                string name = basename + " " + ++i;
                if (names.Contains(name)) {
                    continue;
                }
                return name;
            }
        }
    }
}
