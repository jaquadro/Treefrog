using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Treefrog.Framework.Model;

namespace Treefrog.V2.ViewModel
{
    public class LevelDocumentVM : DocumentVM
    {
        private Level _level;

        public LevelDocumentVM (Level level)
        {
            _level = level;
        }

        public override string Name
        {
            get { return _level.Name; }
        }
    }
}
