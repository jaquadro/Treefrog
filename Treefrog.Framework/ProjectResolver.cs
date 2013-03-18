using System.IO;

namespace Treefrog.Framework
{
    public abstract class ProjectResolver
    {
        public abstract Stream InputStream (string relativePath);
        public abstract Stream OutputStream (string relativePath);
    }

    public class FileProjectResolver : ProjectResolver
    {
        private string _basePath;

        public FileProjectResolver (string projectFilePath)
        {
            _basePath = Path.GetDirectoryName(projectFilePath);
        }

        public override Stream InputStream (string relativePath)
        {
            return File.OpenRead(Path.Combine(_basePath, relativePath));
        }

        public override Stream OutputStream (string relativePath)
        {
            return File.Open(Path.Combine(_basePath, relativePath), FileMode.Create, FileAccess.Write);
        }
    }
}
