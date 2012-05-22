using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Treefrog.Framework.Model.Support;
using GalaSoft.MvvmLight;
using Treefrog.Framework.Model;
using Treefrog.Framework;
using System.Windows;
using Microsoft.Xna.Framework;
using System.Collections.ObjectModel;

namespace Treefrog.Controls.Layers
{
    public class XnaTileLayer : RenderLayer
    {
        private IEnumerable<LocatedTile> _tileSource;

        public IEnumerable<LocatedTile> TileSource
        {
            get { return _tileSource; }
            set { _tileSource = value; }
        }
    }
}
