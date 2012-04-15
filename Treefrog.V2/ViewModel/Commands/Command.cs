using System;

namespace Treefrog.V2.ViewModel.Commands
{
    public abstract class Command
    {
        public abstract void Execute ();

        public abstract void Undo ();

        public abstract void Redo ();
    }
}
