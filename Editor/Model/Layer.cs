using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using System.Xml;

namespace Editor.Model
{
    public abstract class Layer : INamedResource
    {
        #region Fields

        private string _name;

        private float _opacity;
        private bool _visible;

        #endregion

        #region Constructors

        public Layer (string name)
        {
            _name = name;
        }

        #endregion

        #region Properties

        public bool IsVisible
        {
            get { return _visible; }
            set { _visible = value; }
        }

        public float Opacity
        {
            get { return _opacity; }
            set { _opacity = MathHelper.Clamp(value, 0f, 1f); }
        }

        #endregion

        public static Layer FromXml (XmlReader reader, Level level)
        {
            List<string> reqAttrib = new List<string> {
                "name", "type"
            };

            Dictionary<string, string> attribs = new Dictionary<string, string>();

            if (reader.HasAttributes) {
                while (reader.MoveToNextAttribute()) {
                    attribs[reader.Name] = reader.Value;
                }
                reader.MoveToElement();
            }

            foreach (string name in reqAttrib) {
                if (!attribs.ContainsKey(name)) {
                    throw new Exception("Required attribute '" + name + "' missing in tag 'layer'");
                }
            }

            Layer layer = null;
            switch (attribs["type"]) {
                case "tilemulti":
                    layer = new MultiTileGridLayer(attribs["name"], level.TileWidth, level.TileHeight, level.TilesWide, level.TilesHigh);
                    break;
            }

            /*using (XmlReader subReader = reader.ReadSubtree()) {
                while (subReader.Read()) {
                    if (subReader.IsStartElement()) {
                        switch (reader.Name) {
                            case "tiles":

                                break;
                        }
                    }
                }
            }*/

            return layer;
        }

        public abstract void WriteXml(XmlWriter writer);

        #region INamedResource Members

        public string Name
        {
            get { return _name; }
        }

        public event EventHandler<NameChangedEventArgs> NameChanged;

        protected virtual void OnNameChanged (NameChangedEventArgs e)
        {
            if (NameChanged != null) {
                NameChanged(this, e);
            }
        }

        #endregion
    }
}
