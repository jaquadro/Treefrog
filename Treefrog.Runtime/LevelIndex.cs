using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Content;

namespace Treefrog.Runtime
{
    public class LevelIndex : IEnumerable<LevelIndexEntry>, IDisposable
    {
        private bool _disposed;
        private ContentManager _manager;

        private Dictionary<int, LevelIndexEntry> _index;
        private Dictionary<string, LevelIndexEntry> _nameIndex;
        private Dictionary<string, LevelIndexEntry> _assetIndex;

        internal LevelIndex ()
        {
            _index = new Dictionary<int, LevelIndexEntry>();
            _nameIndex = new Dictionary<string, LevelIndexEntry>();
            _assetIndex = new Dictionary<string, LevelIndexEntry>();
        }

        internal LevelIndex (ContentReader reader)
            : this()
        {
            _manager = reader.ContentManager;

            int lieCount = reader.ReadInt32();

            for (int i = 0; i < lieCount; i++) {
                LevelIndexEntry entry = new LevelIndexEntry(reader.ContentManager)
                {
                    Id = reader.ReadInt32(),
                    Name = reader.ReadString(),
                    Asset = reader.ReadString(),
                    Properties = new PropertyCollection(reader),
                };

                _index[i] = entry;
                _nameIndex[entry.Name] = entry;
                _assetIndex[entry.Asset] = entry;
            }
        }

        public LevelIndexEntry ById (int id)
        {
            LevelIndexEntry entry;
            if (_index.TryGetValue(id, out entry)) {
                return entry;
            }
            return null;
        }

        public LevelIndexEntry ByName (string name)
        {
            LevelIndexEntry entry;
            if (_nameIndex.TryGetValue(name, out entry)) {
                return entry;
            }
            return null;
        }

        public LevelIndexEntry ByAsset (string assetName)
        {
            LevelIndexEntry entry;
            if (_assetIndex.TryGetValue(assetName, out entry)) {
                return entry;
            }
            return null;
        }

        public IEnumerable<LevelIndexEntry> ByProperty (string propertyName)
        {
            foreach (LevelIndexEntry entry in _index.Values) {
                Property prop;
                if (entry.Properties.TryGetValue(propertyName, out prop)) {
                    yield return entry;
                }
            }
        }

        public IEnumerable<LevelIndexEntry> ByProperty (string propertyName, string propertyValue)
        {
            foreach (LevelIndexEntry entry in _index.Values) {
                Property prop;
                if (entry.Properties.TryGetValue(propertyName, out prop)) {
                    if (prop.Value == propertyValue) {
                        yield return entry;
                    }
                }
            }
        }

        private PropertyCollection ReadProperties (ContentReader reader, PropertyCollection properties)
        {
            int propCount = reader.ReadInt16();

            for (int i = 0; i < propCount; i++) {
                string name = reader.ReadString();
                string value = reader.ReadString();

                properties.Add(new Property(name, value));
            }

            return properties;
        }

        #region IEnumerable<LevelIndexEntry> Members

        public IEnumerator<LevelIndexEntry> GetEnumerator ()
        {
            return _index.Values.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator ()
        {
            return _index.Values.GetEnumerator();
        }

        #endregion

        #region IDisposable Members

        public void Dispose ()
        {
            if (!_disposed) {
                foreach (LevelIndexEntry entry in _index.Values) {
                    entry.Dispose();
                }
                _index.Clear();
                _nameIndex.Clear();
                _assetIndex.Clear();

                _disposed = true;
            }
        }

        #endregion
    }
}
