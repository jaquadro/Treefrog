using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Win32;

namespace Treefrog.V2
{
    public abstract class FileOptions
    {
        public string InitialDirectory { get; set; }
        public string Filter { get; set; }
        public int FilterIndex { get; set; }
    }

    public class OpenFileOptions : FileOptions
    {
    }

    public class SaveFileOptions : FileOptions
    {
    }

    public interface IOService
    {
        string OpenFileDialog (OpenFileOptions options);

        string SaveFileDialog (SaveFileOptions options);
    }

    public class DefaultIOService : IOService
    {
        public string OpenFileDialog (OpenFileOptions options)
        {
            OpenFileDialog dlg = new OpenFileDialog()
            {
                InitialDirectory = options.InitialDirectory,
                Filter = options.Filter,
                FilterIndex = options.FilterIndex,
                Multiselect = false,
            };

            if (dlg.ShowDialog() == true)
                return dlg.FileName;
            else
                return null;
        }

        public string SaveFileDialog (SaveFileOptions options)
        {
            SaveFileDialog dlg = new SaveFileDialog()
            {
                InitialDirectory = options.InitialDirectory,
                Filter = options.Filter,
                FilterIndex = options.FilterIndex,
            };

            if (dlg.ShowDialog() == true)
                return dlg.FileName;
            else
                return null;
        }
    }
}
