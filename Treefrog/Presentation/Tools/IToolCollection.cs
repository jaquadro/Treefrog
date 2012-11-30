using System;
using System.ComponentModel;

namespace Treefrog.Presentation.Tools
{
    public interface IToolCollection
    {
        bool Enabled { get; }

        event EventHandler Invalidated;
    }
}
