using System;

namespace Treefrog.Framework
{
    public class ResourceReleaser : IDisposable
    {
        public void Dispose ()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose (bool disposing)
        { }
    }
}
