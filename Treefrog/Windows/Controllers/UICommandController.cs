using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Treefrog.Presentation.Commands;
using Treefrog.Utility;

namespace Treefrog.Windows.Controllers
{
    public class UICommandController
    {
        private CommandManager _commandManager;

        private Mapper<CommandKey, ToolStripButton> _buttonMap = new Mapper<CommandKey, ToolStripButton>();
        private Mapper<CommandKey, ToolStripMenuItem> _menuMap = new Mapper<CommandKey, ToolStripMenuItem>();

        public void BindCommandManager (CommandManager commandManager)
        {
            if (_commandManager != null) {
                _commandManager.CommandInvalidated -= HandleCommandInvalidated;
                _commandManager.ManagerInvalidated -= HandleManagerInvalidated;
            }

            _commandManager = commandManager;

            if (_commandManager != null) {
                _commandManager.CommandInvalidated += HandleCommandInvalidated;
                _commandManager.ManagerInvalidated += HandleManagerInvalidated;
            }

            ResetComponent();
        }

        public void MapButtons (IEnumerable<KeyValuePair<CommandKey, ToolStripButton>> mappings)
        {
            foreach (var item in mappings) {
                _buttonMap.Add(item.Key, item.Value);
                item.Value.Click += BoundButtonClickHandler;

                Invalidate(item.Key);
            }
        }

        public void MapMenuItems (IEnumerable<KeyValuePair<CommandKey, ToolStripMenuItem>> mappings)
        {
            foreach (var item in mappings)
                MapMenuItem(item.Key, item.Value);
        }

        public void MapMenuItems (IEnumerable<ToolStripMenuItem> items)
        {
            foreach (ToolStripMenuItem item in items) {
                if (item != null && item.Tag is CommandKey)
                    MapMenuItem((CommandKey)item.Tag, item);

                if (item.DropDownItems != null)
                    MapMenuItems(item.DropDownItems);
            }
        }

        private void MapMenuItems (ToolStripItemCollection items)
        {
            foreach (ToolStripItem item in items) {
                ToolStripMenuItem menuItem = item as ToolStripMenuItem;
                if (menuItem != null) {
                    if (item != null && item.Tag is CommandKey)
                        MapMenuItem((CommandKey)menuItem.Tag, menuItem);

                    if (menuItem.DropDownItems != null)
                        MapMenuItems(menuItem.DropDownItems);
                }
            }
        }

        private void MapMenuItem (CommandKey key, ToolStripMenuItem item)
        {
            if (key != CommandKey.Unknown && item != null) {
                _menuMap.Add(key, item);
                item.Click += BoundMenuClickHandler;

                Invalidate(key);
            }
        }

        private bool CanPerformCommand (CommandKey key)
        {
            return _commandManager != null && _commandManager.CanPerform(key);
        }

        private void PerformCommand (CommandKey key)
        {
            if (CanPerformCommand(key))
                _commandManager.Perform(key);
        }

        private bool IsCommandSelected (CommandKey key)
        {
            return _commandManager != null && _commandManager.IsSelected(key);
        }

        private void HandleCommandInvalidated (object sender, CommandSubscriberEventArgs e)
        {
            Invalidate(e.CommandKey);
        }

        private void HandleManagerInvalidated (object sender, EventArgs e)
        {
            ResetComponent();
        }

        private void Invalidate (CommandKey key)
        {
            if (_buttonMap.ContainsKey(key)) {
                ToolStripButton item = _buttonMap[key];
                item.Enabled = CanPerformCommand(key);
                item.Checked = IsCommandSelected(key);
            }
            if (_menuMap.ContainsKey(key)) {
                ToolStripMenuItem item = _menuMap[key];
                item.Enabled = CanPerformCommand(key);
                item.Checked = IsCommandSelected(key);
            }
        }

        private void BoundButtonClickHandler (object sender, EventArgs e)
        {
            ToolStripButton item = sender as ToolStripButton;
            if (_commandManager != null && _buttonMap.ContainsValue(item))
                _commandManager.Perform(_buttonMap[item]);
        }

        private void BoundMenuClickHandler (object sender, EventArgs e)
        {
            ToolStripMenuItem item = sender as ToolStripMenuItem;
            if (_commandManager != null && _menuMap.ContainsValue(item))
                _commandManager.Perform(_menuMap[item]);
        }

        private void ResetComponent ()
        {
            foreach (CommandKey key in _buttonMap.Keys)
                Invalidate(key);
            foreach (CommandKey key in _menuMap.Keys)
                Invalidate(key);
        }
    }
}
