using System;
using System.ComponentModel;

namespace Treefrog.ViewModel.Tools
{
    public interface IToolCollection
    {
        bool Enabled { get; }

        event EventHandler Invalidated;
    }
}
