using System;

namespace Treefrog.Presentation.Commands
{
    public abstract class Command
    {
        public abstract void Execute ();

        public abstract void Undo ();

        public abstract void Redo ();
    }
}
