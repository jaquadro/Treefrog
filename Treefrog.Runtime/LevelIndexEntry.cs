using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;

namespace Treefrog.Runtime
{
    public class LevelIndexEntry : IDisposable
    {
        private bool _disposed;
        private ContentManager _manager;

        internal LevelIndexEntry (ContentManager manager)
        {
            _manager = manager;
            Properties = new PropertyCollection();
        }

        public Guid Id { get; internal set; }
        public string Name { get; internal set; }
        public string Asset { get; internal set; }

        public PropertyCollection Properties { get; internal set; }

        public Level Load ()
        {
            if (_disposed) {
                return null;
            }

            return _manager.Load<Level>(Asset);
        }

        #region IDisposable Members

        public void Dispose ()
        {
            _disposed = true;
        }

        #endregion
    }
}
