using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Treefrog.Framework.Tests
{
    class Runner
    {
        public static void Main ()
        {
            NUnit.Gui.AppEntry.Main(new string[] { "..\\..\\..\\Treefrog.Framework.Tests.csproj", "/run" });
        }
    }
}
