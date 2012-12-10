using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace Treefrog.Framework.Model
{
    [XmlRoot("TileBrush")]
    public class TileBrushXmlProxy
    {
        [XmlAttribute]
        public int Id { get; set; }
    }

    public abstract class TileBrush : IKeyProvider<string>
    {
        private int _id;
        private string _name;

        private int _tileHeight;
        private int _tileWidth;

        protected TileBrush (string name, int tileWidth, int tileHeight)
        {
            _name = name;
            _tileWidth = tileWidth;
            _tileHeight = tileHeight;
        }

        public int TileHeight
        {
            get { return _tileHeight; }
        }

        public int TileWidth
        {
            get { return _tileWidth; }
        }

        public int Id
        {
            get { return _id; }
            set { _id = value; }
        }

        public string GetKey ()
        {
            return Name;
        }

        public string Name
        {
            get { return _name; }
        }

        public bool SetName (string name)
        {
            if (_name != name) {
                KeyProviderEventArgs<string> e = new KeyProviderEventArgs<string>(_name, name);
                try {
                    OnKeyChanging(e);
                }
                catch (KeyProviderException) {
                    return false;
                }

                _name = name;
                OnKeyChanged(e);
                //OnPropertyProviderNameChanged(EventArgs.Empty);
                //RaisePropertyChanged("Name");
            }

            return true;
        }

        public event EventHandler<KeyProviderEventArgs<string>> KeyChanging;

        public event EventHandler<KeyProviderEventArgs<string>> KeyChanged;

        protected virtual void OnKeyChanging (KeyProviderEventArgs<string> e)
        {
            if (KeyChanging != null)
                KeyChanging(this, e);
        }

        protected virtual void OnKeyChanged (KeyProviderEventArgs<string> e)
        {
            if (KeyChanged != null)
                KeyChanged(this, e);
        }
    }
}
