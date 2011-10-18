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

        #region XML Import / Export

        public static Layer FromXml (XmlReader reader, IServiceProvider services, Level level)
        {
            Dictionary<string, string> attribs = XmlHelper.CheckAttributes(reader, new List<string> { 
                "name", "type",
            });

            Layer layer = null;
            switch (attribs["type"]) {
                case "tilemulti":
                    layer = new MultiTileGridLayer(attribs["name"], level.TileWidth, level.TileHeight, level.TilesWide, level.TilesHigh);
                    break;
            }

            XmlHelper.SwitchAllAdvance(reader, (xmlr, s) => {
                return layer.ReadXmlElement(xmlr, s, services);
            });

            return layer;
        }

        public abstract void WriteXml(XmlWriter writer);

        protected virtual bool ReadXmlElement (XmlReader reader, string name, IServiceProvider services)
        {
            return true;
        }

        #endregion

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
