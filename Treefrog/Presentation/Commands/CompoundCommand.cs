using System.Collections.Generic;

namespace Treefrog.Presentation.Commands
{
    public class CompoundCommand : Command
    {
        private List<Command> _commands;

        public CompoundCommand ()
        {
            _commands = new List<Command>();
        }

        public CompoundCommand (Command command)
            : this()
        {
            _commands.Add(command);
        }

        public CompoundCommand (IEnumerable<Command> commands)
            : this()
        {
            _commands.AddRange(commands);
        }

        public void AddCommand (Command command)
        {
            _commands.Add(command);
        }

        public int Count
        {
            get { return _commands.Count; }
        }

        public override void Execute ()
        {
            for (int i = 0; i < _commands.Count; i++)
                _commands[i].Execute();
        }

        public override void Undo ()
        {
            for (int i = _commands.Count - 1; i >= 0; i--)
                _commands[i].Undo();
        }

        public override void Redo ()
        {
            for (int i = 0; i < _commands.Count; i++)
                _commands[i].Redo();
        }
    }
}
