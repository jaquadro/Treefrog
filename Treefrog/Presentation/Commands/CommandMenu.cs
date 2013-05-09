using System.Collections.Generic;

namespace Treefrog.Presentation.Commands
{
    public class CommandMenuEntry
    {
        public CommandKey Key { get; set; }
        public object Param { get; set; }
        public CommandMenu SubMenu { get; set; }
        public bool Default { get; set; }

        public CommandMenuEntry (CommandKey key)
        {
            Key = key;
        }

        public CommandMenuEntry (CommandKey key, object param)
            : this(key)
        {
            Param = param;
        }

        public CommandMenuEntry (CommandMenu subMenu)
        {
            SubMenu = subMenu;
        }

        public static implicit operator CommandMenuEntry (CommandKey key)
        {
            return new CommandMenuEntry(key);
        }
    }

    public class CommandMenu
    {
        public string Name { get; set; }
        public IEnumerable<CommandMenuGroup> Groups { get; set; }

        public CommandMenu (string name)
        {
            Name = name;
        }

        public CommandMenu (string name, IEnumerable<CommandMenuGroup> groups)
            : this(name)
        {
            Groups = groups;
        }

        public CommandMenu (string name, CommandMenuGroup group)
            : this(name)
        {
            List<CommandMenuGroup> groups = new List<CommandMenuGroup>();
            groups.Add(group);

            Groups = groups;
        }
    }

    public class CommandMenuGroup : List<CommandMenuEntry>
    {
        public CommandMenuGroup ()
            : base()
        { }

        public CommandMenuGroup (IEnumerable<CommandMenuEntry> entries)
            : base(entries)
        { }

        public CommandMenuGroup (IEnumerable<CommandKey> keys)
            : base(BuildEntryList(keys))
        { }

        private static IEnumerable<CommandMenuEntry> BuildEntryList (IEnumerable<CommandKey> keys)
        {
            List<CommandMenuEntry> entries = new List<CommandMenuEntry>();
            foreach (CommandKey key in keys)
                entries.Add(new CommandMenuEntry(key));

            return entries;
        }
    }
}
